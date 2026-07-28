using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public PaymentsController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index(PaymentState? status)
    {
        var query = _context.Payments.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Enrollment).ThenInclude(x => x.Course)
            .Include(x => x.PaymentTransactions)
            .Where(x => x.Enrollment.Status == EnrollmentState.Approved)
            .AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        ViewBag.Status = status;
        return View(await query.OrderBy(x => x.Status).ThenBy(x => x.Student.Code).ToListAsync());
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(
        int id, decimal paidAmount, PaymentMethod paymentMethod)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var payment = await _context.Payments
                .Include(x => x.Enrollment)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (payment is null)
            {
                return NotFound();
            }

            if (payment.Enrollment.Status != EnrollmentState.Approved)
            {
                TempData["ErrorMessage"] = "Chỉ có thể ghi nhận học phí cho đăng ký đã được duyệt.";
                return RedirectToAction(nameof(Index));
            }

            if (paidAmount < 0 || paidAmount > payment.Amount)
            {
                TempData["ErrorMessage"] =
                    $"Số tiền đã đóng phải từ 0 đến {payment.Amount:N0} đồng.";
                return RedirectToAction(nameof(Index));
            }

            if (paidAmount > 0 && paidAmount < payment.Amount)
            {
                TempData["ErrorMessage"] =
                    $"Hệ thống chỉ ghi nhận chưa thanh toán hoặc đã thanh toán. Vui lòng nhập 0 hoặc {payment.Amount:N0} đồng.";
                return RedirectToAction(nameof(Index));
            }

            var difference = paidAmount - payment.PaidAmount;
            payment.PaidAmount = paidAmount;
            payment.PaymentMethod = paymentMethod;
            payment.PaidDate = paidAmount > 0 ? DateTime.Today : null;
            payment.Status = GetStatus(paidAmount, payment.Amount);

            if (difference != 0)
            {
                _context.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentId = payment.Id,
                    StudentId = payment.StudentId,
                    Amount = Math.Abs(difference),
                    PaymentMethod = paymentMethod,
                    PaidAt = DateTime.Now,
                    RecordedBy = User.Identity?.Name ?? "Nhân viên đào tạo",
                    Note = difference > 0
                        ? "Nhân viên ghi nhận thanh toán"
                        : "Nhân viên điều chỉnh giảm học phí"
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["SuccessMessage"] = "Đã cập nhật học phí.";
            return RedirectToAction(nameof(Index));
        });
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyPayments()
    {
        var studentId = CurrentStudentId();
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        return View(await _context.Payments.AsNoTracking()
            .Where(x => x.StudentId == studentId.Value
                && x.Enrollment.Status == EnrollmentState.Approved)
            .Include(x => x.Enrollment).ThenInclude(x => x.Course)
            .Include(x => x.PaymentTransactions)
            .OrderBy(x => x.Status)
            .ToListAsync());
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id, decimal amount, PaymentMethod paymentMethod)
    {
        var studentId = CurrentStudentId();
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var payment = await _context.Payments.FirstOrDefaultAsync(x =>
                x.Id == id
                && x.StudentId == studentId.Value
                && x.Enrollment.Status == EnrollmentState.Approved);
            if (payment is null)
            {
                return NotFound();
            }

            var remaining = payment.Amount - payment.PaidAmount;
            if (amount <= 0 || amount > remaining)
            {
                TempData["ErrorMessage"] =
                    $"Số tiền thanh toán phải từ 1 đến {remaining:N0} đồng.";
                return RedirectToAction(nameof(MyPayments));
            }

            if (amount != remaining)
            {
                TempData["ErrorMessage"] =
                    $"Hệ thống không hỗ trợ đóng thiếu học phí. Vui lòng thanh toán đủ {remaining:N0} đồng.";
                return RedirectToAction(nameof(MyPayments));
            }

            payment.PaidAmount += amount;
            payment.PaymentMethod = paymentMethod;
            payment.PaidDate = DateTime.Today;
            payment.Status = GetStatus(payment.PaidAmount, payment.Amount);
            _context.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.Id,
                StudentId = payment.StudentId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                PaidAt = DateTime.Now,
                RecordedBy = User.Identity?.Name ?? "Học viên",
                Note = "Học viên thanh toán học phí"
            });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["SuccessMessage"] = "Thanh toán học phí thành công.";
            return RedirectToAction(nameof(MyPayments));
        });
    }

    private static PaymentState GetStatus(decimal paid, decimal amount)
    {
        if (paid <= 0)
        {
            return PaymentState.Unpaid;
        }
        return paid >= amount ? PaymentState.Paid : PaymentState.Unpaid;
    }

    private int? CurrentStudentId()
    {
        return int.TryParse(User.FindFirstValue("StudentId"), out var id) ? id : null;
    }
}
