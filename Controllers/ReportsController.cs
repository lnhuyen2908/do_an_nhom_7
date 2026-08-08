using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web_do_an1.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly EnglishCenterDbContext _context;
    private readonly SimplePdfService _pdfService;

    public ReportsController(EnglishCenterDbContext context, SimplePdfService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> QuarterlyRevenue(int? year, int? quarter)
    {
        var selectedYear = year ?? DateTime.Today.Year;
        var selectedQuarter = quarter is >= 1 and <= 4 ? quarter.Value : ((DateTime.Today.Month - 1) / 3) + 1;
        var monthly = await BuildMonthlyRevenueAsync(selectedYear, selectedQuarter);

        ViewBag.Year = selectedYear;
        ViewBag.Quarter = selectedQuarter;
        ViewBag.TotalRevenue = monthly.Sum(x => x.Revenue);
        return View(monthly);
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> QuarterlyRevenuePdf(int year, int quarter)
    {
        var monthly = await BuildMonthlyRevenueAsync(year, quarter);
        return File(
            _pdfService.BuildRevenueReport(
                year,
                quarter,
                monthly.Select(x => (x.Month, x.Revenue)).ToList()),
            "application/pdf",
            $"bao-cao-doanh-thu-q{quarter}-{year}.pdf");
    }

    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<IActionResult> InvoicePdf(int paymentId)
    {
        var payment = await _context.Payments.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Enrollment).ThenInclude(x => x.Course)
            .Include(x => x.Enrollment).ThenInclude(x => x.CourseClass)
            .Include(x => x.PaymentTransactions)
            .FirstOrDefaultAsync(x => x.Id == paymentId);
        if (payment is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Student")
            && (!int.TryParse(User.FindFirstValue("StudentId"), out var studentId)
                || payment.StudentId != studentId))
        {
            return Forbid();
        }

        if (payment.Status != PaymentState.Paid)
        {
            TempData["ErrorMessage"] = "Chỉ xuất hóa đơn khi học phí đã được duyệt thành công.";
            return RedirectToAction(User.IsInRole("Student") ? "MyPayments" : "Index", "Payments");
        }

        var pdf = _pdfService.BuildInvoice(payment);
        return User.IsInRole("Student")
            ? File(pdf, "application/pdf")
            : File(pdf, "application/pdf", $"hoa-don-{payment.Student.Code}-{payment.Id}.pdf");
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> StudentResultPdf(int studentId, int classId)
    {
        var score = await _context.Scores.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.CourseClass).ThenInclude(x => x.Course)
            .Include(x => x.CourseClass).ThenInclude(x => x.Teacher)
            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.CourseClassId == classId);
        if (score is null)
        {
            return NotFound();
        }

        return File(
            _pdfService.BuildStudentResult(score),
            "application/pdf",
            $"ket-qua-{score.Student.Code}-{score.CourseClass.Code}.pdf");
    }

    private async Task<List<MonthlyRevenueRow>> BuildMonthlyRevenueAsync(int year, int quarter)
    {
        var startMonth = (quarter - 1) * 3 + 1;
        var months = Enumerable.Range(startMonth, 3).ToArray();
        var data = await _context.PaymentTransactions.AsNoTracking()
            .Where(x => x.Status == PaymentTransactionState.Approved
                && x.ApprovedAt.HasValue
                && x.ApprovedAt.Value.Year == year
                && months.Contains(x.ApprovedAt.Value.Month))
            .GroupBy(x => x.ApprovedAt!.Value.Month)
            .Select(x => new { Month = x.Key, Revenue = x.Sum(t => t.Amount) })
            .ToListAsync();

        return months.Select(month => new MonthlyRevenueRow(
            month,
            data.FirstOrDefault(x => x.Month == month)?.Revenue ?? 0))
            .ToList();
    }

    public sealed record MonthlyRevenueRow(int Month, decimal Revenue);
}
