using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly EnglishCenterDbContext _context;
    private readonly NotificationService _notificationService;

    public PaymentsController(EnglishCenterDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index(PaymentState? status, string? keyword, int page = 1)
    {
        const int pageSize = 10;
        keyword = keyword?.Trim();
        page = Math.Max(page, 1);
        var query = _context.Payments.AsNoTracking()
            .Where(x => x.Enrollment.Status != EnrollmentState.Cancelled)
            .Include(x => x.Student)
            .Include(x => x.Enrollment).ThenInclude(x => x.Course)
            .Include(x => x.Enrollment).ThenInclude(x => x.CourseClass)
            .Include(x => x.PaymentTransactions)
            .Where(x => x.Enrollment.Status == EnrollmentState.Approved)
            .AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Student.Code.Contains(keyword)
                || x.Student.FullName.Contains(keyword)
                || x.Enrollment.Course.Code.Contains(keyword)
                || x.Enrollment.Course.Name.Contains(keyword));
        }

        ViewBag.Status = status;
        ViewBag.Keyword = keyword;
        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Min(page, totalPages);
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        return View(await query
            .OrderByDescending(x => x.PaymentTransactions.Any(t => t.Status == PaymentTransactionState.Pending))
            .ThenBy(x => x.PaymentTransactions
                .Where(t => t.Status == PaymentTransactionState.Pending)
                .Select(t => (DateTime?)t.PaidAt)
                .Min() ?? DateTime.MaxValue)
            .ThenBy(x => x.Student.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync());
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
                    Status = PaymentTransactionState.Approved,
                    ApprovedAt = DateTime.Now,
                    ApprovedBy = User.Identity?.Name ?? "Nhân viên đào tạo",
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
            .Where(x => x.StudentId == studentId.Value && x.Status != PaymentState.Cancelled)
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

            if (payment.Status == PaymentState.Cancelled)
            {
                TempData["ErrorMessage"] =
                    "Học phí này đã bị hủy.";
                return RedirectToAction(nameof(MyPayments));
            }

            if (payment.Status == PaymentState.Paid)
            {
                TempData["ErrorMessage"] =
                    "Học phí này đã được thanh toán.";
                return RedirectToAction(nameof(MyPayments));
            }

            var remaining = payment.Amount - payment.PaidAmount;
            if (amount <= 0 || amount > remaining)
            {
                TempData["ErrorMessage"] =
                    $"Số tiền thanh toán phải từ 1 đến {remaining:N0} đồng.";
                return RedirectToAction(nameof(MyPayments));
            }

            var pendingAmount = await _context.PaymentTransactions
                .Where(x => x.PaymentId == payment.Id && x.Status == PaymentTransactionState.Pending)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;
            if (amount + pendingAmount > remaining)
            {
                TempData["ErrorMessage"] =
                    $"Tổng số tiền đang chờ duyệt vượt quá số còn lại {remaining:N0} đồng.";
                return RedirectToAction(nameof(MyPayments));
            }

            payment.PaymentMethod = paymentMethod;
            payment.Status = PaymentState.PendingApproval;
            _context.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.Id,
                StudentId = payment.StudentId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                PaidAt = DateTime.Now,
                Status = PaymentTransactionState.Pending,
                RecordedBy = User.Identity?.Name ?? "Học viên",
                Note = "Học viên gửi yêu cầu thanh toán học phí"
            });
            await _context.SaveChangesAsync();
            await _notificationService.NotifyRolesAsync(
                new[] { "Admin", "Staff" },
                "Thanh toán chờ duyệt",
                $"Học viên {User.Identity?.Name} vừa gửi yêu cầu thanh toán {amount:N0} đồng.",
                Url.Action(nameof(Index), "Payments") ?? string.Empty);
            await transaction.CommitAsync();
            TempData["SuccessMessage"] = "Đã gửi thanh toán. Trạng thái đang chờ Admin/NVDT duyệt.";
            return RedirectToAction(nameof(MyPayments));
        });
    }

    private static PaymentState GetStatus(decimal paid, decimal amount)
    {
        if (paid <= 0)
        {
            return PaymentState.Unpaid;
        }
        return paid >= amount ? PaymentState.Paid : PaymentState.PartiallyPaid;
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveTransaction(int transactionId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var paymentTransaction = await _context.PaymentTransactions
                .Include(x => x.Payment)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == transactionId);
            if (paymentTransaction is null)
            {
                return NotFound();
            }

            if (paymentTransaction.Status != PaymentTransactionState.Pending)
            {
                TempData["ErrorMessage"] = "Giao dịch này đã được xử lý.";
                return RedirectToAction(nameof(Index));
            }

            var payment = paymentTransaction.Payment;
            var remaining = payment.Amount - payment.PaidAmount;
            if (paymentTransaction.Amount > remaining)
            {
                TempData["ErrorMessage"] = "Số tiền giao dịch vượt quá công nợ còn lại.";
                return RedirectToAction(nameof(Index));
            }

            payment.PaidAmount += paymentTransaction.Amount;
            payment.PaymentMethod = paymentTransaction.PaymentMethod;
            payment.PaidDate = DateTime.Today;
            payment.Status = GetStatus(payment.PaidAmount, payment.Amount);
            paymentTransaction.Status = PaymentTransactionState.Approved;
            paymentTransaction.ApprovedAt = DateTime.Now;
            paymentTransaction.ApprovedBy = User.Identity?.Name ?? "Nhân viên đào tạo";

            await _context.SaveChangesAsync();
            var paymentTitle = payment.Status == PaymentState.Paid
                ? "Thanh toán học phí thành công"
                : "Học phí đã được duyệt";
            var paymentUrl = payment.Status == PaymentState.Paid
                ? Url.Action("InvoicePdf", "Reports", new { paymentId = payment.Id })
                : Url.Action(nameof(MyPayments), "Payments");
            await NotifyStudentPaymentAsync(payment.StudentId, paymentTitle,
                $"Khoản thanh toán {paymentTransaction.Amount:N0} đồng đã được xác nhận.", paymentUrl);
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Đã duyệt thanh toán.";
            return RedirectToAction(nameof(Index));
        });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectTransaction(int transactionId)
    {
        var paymentTransaction = await _context.PaymentTransactions
            .Include(x => x.Payment)
            .FirstOrDefaultAsync(x => x.Id == transactionId);
        if (paymentTransaction is null)
        {
            return NotFound();
        }

        if (paymentTransaction.Status != PaymentTransactionState.Pending)
        {
            TempData["ErrorMessage"] = "Giao dịch này đã được xử lý.";
            return RedirectToAction(nameof(Index));
        }

        paymentTransaction.Status = PaymentTransactionState.Rejected;
        paymentTransaction.ApprovedAt = DateTime.Now;
        paymentTransaction.ApprovedBy = User.Identity?.Name ?? "Nhân viên đào tạo";

        var hasPending = await _context.PaymentTransactions
            .AnyAsync(x => x.PaymentId == paymentTransaction.PaymentId
                && x.Id != paymentTransaction.Id
                && x.Status == PaymentTransactionState.Pending);
        paymentTransaction.Payment.Status = hasPending
            ? PaymentState.PendingApproval
            : GetStatus(paymentTransaction.Payment.PaidAmount, paymentTransaction.Payment.Amount);

        await _context.SaveChangesAsync();
        await NotifyStudentPaymentAsync(paymentTransaction.StudentId, "Thanh toán chưa được duyệt",
            $"Khoản thanh toán {paymentTransaction.Amount:N0} đồng chưa được xác nhận. Vui lòng kiểm tra lại.");

        TempData["SuccessMessage"] = "Đã từ chối thanh toán.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> QR(int id, decimal? amount)
    {
        var studentId = CurrentStudentId();
        if (!studentId.HasValue)
            return Forbid();

        var payment = await _context.Payments
            .Include(x => x.Enrollment)
            .ThenInclude(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);

        if (payment == null)
            return NotFound();

        var remaining = payment.Amount - payment.PaidAmount;
        ViewBag.Amount = amount.HasValue && amount.Value > 0 && amount.Value <= remaining
            ? amount.Value
            : remaining;
        return View(payment);
    }

    private int? CurrentStudentId()
    {
        return int.TryParse(User.FindFirstValue("StudentId"), out var id) ? id : null;
    }

    private async Task NotifyStudentPaymentAsync(int studentId, string title, string message, string? url = null)
    {
        var accountId = await _context.UserAccounts.AsNoTracking()
            .Where(x => x.StudentId == studentId && x.IsActive)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
        if (accountId.HasValue)
        {
            await _notificationService.NotifyUserAsync(
                accountId.Value,
                title,
                message,
                url ?? Url.Action(nameof(MyPayments), "Payments") ?? string.Empty);
        }
    }
}
