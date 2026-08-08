using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers;

[Authorize] // Mọi chức năng trong controller đều yêu cầu đã đăng nhập.
public class EnrollmentsController : Controller
{
    private readonly EnglishCenterDbContext _context;
    private readonly NotificationService _notificationService;

    public EnrollmentsController(EnglishCenterDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index(EnrollmentState? status, string? keyword, int page = 1)
    {
        const int pageSize = 10;
        keyword = keyword?.Trim();
        page = Math.Max(page, 1);
        var query = _context.Enrollments.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Course)
            .Include(x => x.CourseClass)
            .AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Student.Code.Contains(keyword)
                || x.Student.FullName.Contains(keyword)
                || x.Course.Code.Contains(keyword)
                || x.Course.Name.Contains(keyword)
                || (x.CourseClass != null && x.CourseClass.Code.Contains(keyword)));
        }

        ViewBag.Status = status;
        ViewBag.Keyword = keyword;
        ViewBag.Classes = await _context.CourseClasses.AsNoTracking()
            .Include(x => x.Course).OrderBy(x => x.Code).ToListAsync();
        ViewBag.ClassSeats = await _context.Enrollments.AsNoTracking()
            .Where(x => x.CourseClassId.HasValue && x.Status == EnrollmentState.Approved)
            .GroupBy(x => x.CourseClassId!.Value)
            .ToDictionaryAsync(x => x.Key, x => x.Count());
        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Min(page, totalPages);
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        return View(await query.OrderByDescending(x => x.RegisteredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync());
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Details(int? id)
    {
        var enrollment = id.HasValue
            ? await _context.Enrollments.AsNoTracking()
                .Include(x => x.Student).Include(x => x.Course)
                .Include(x => x.CourseClass).Include(x => x.Payment)
                .FirstOrDefaultAsync(x => x.Id == id.Value)
            : null;
        return enrollment is null ? NotFound() : View(enrollment);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id, EnrollmentState status, int? courseClassId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment is null)
            {
                return NotFound();
            }

            if (status == EnrollmentState.Approved)
            {
                if (!courseClassId.HasValue)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn lớp trước khi duyệt.";
                    return RedirectToAction(nameof(Index));
                }

                var courseClass = await _context.CourseClasses.FindAsync(courseClassId.Value);
                if (courseClass is null || courseClass.CourseId != enrollment.CourseId)
                {
                    TempData["ErrorMessage"] = "Lớp không thuộc khóa học đã đăng ký.";
                    return RedirectToAction(nameof(Index));
                }

                if (!courseClass.CanRegister)
                {
                    TempData["ErrorMessage"] = $"Lớp {courseClass.Code} hiện đã khóa hoặc đã đóng.";
                    return RedirectToAction(nameof(Index));
                }

                var occupied = await _context.Enrollments.CountAsync(x =>
                    x.Id != id && x.CourseClassId == courseClassId
                    && x.Status == EnrollmentState.Approved);
                if (occupied >= courseClass.Capacity)
                {
                    TempData["ErrorMessage"] = $"Lớp {courseClass.Code} đã đủ sĩ số.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var payment = await _context.Payments.FirstOrDefaultAsync(x => x.EnrollmentId == id);
            if (status == EnrollmentState.Cancelled && payment is not null)
            {
                if (payment.PaidAmount > 0 || payment.Status == PaymentState.Paid)
                {
                    TempData["ErrorMessage"] =
                        "Không thể hủy đăng ký đã phát sinh thanh toán. Vui lòng xử lý học phí trước.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Payments.Remove(payment);
            }

            enrollment.Status = status;
            enrollment.CourseClassId =
                status == EnrollmentState.Approved ? courseClassId : null;

            if (status == EnrollmentState.Approved && payment is null)
            {
                var tuition = await _context.Courses.AsNoTracking()
                    .Where(x => x.Id == enrollment.CourseId)
                    .Select(x => x.Tuition)
                    .FirstAsync();

                _context.Payments.Add(new Payment
                {
                    StudentId = enrollment.StudentId,
                    EnrollmentId = enrollment.Id,
                    Amount = tuition,
                    Status = PaymentState.Unpaid,
                    PaymentMethod = PaymentMethod.Cash
                });
            }

            await _context.SaveChangesAsync();
            var accountId = await _context.UserAccounts.AsNoTracking()
                .Where(x => x.StudentId == enrollment.StudentId && x.IsActive)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
            if (accountId.HasValue)
            {
                var title = status == EnrollmentState.Approved
                    ? "Đăng ký khóa học đã được duyệt"
                    : status == EnrollmentState.Cancelled
                        ? "Đăng ký khóa học đã bị hủy"
                        : "Đăng ký khóa học đang chờ duyệt";
                await _notificationService.NotifyUserAsync(
                    accountId.Value,
                    title,
                    "Trạng thái đăng ký khóa học của bạn vừa được cập nhật.",
                    status == EnrollmentState.Approved
                        ? Url.Action("MyPayments", "Payments") ?? string.Empty
                        : Url.Action(nameof(MyEnrollments), "Enrollments") ?? string.Empty);
            }
            await transaction.CommitAsync();
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái đăng ký.";
            return RedirectToAction(nameof(Index));
        });
    }

    [Authorize(Roles = "Student")] // Chỉ học viên được xem danh sách đăng ký của chính mình.
    public async Task<IActionResult> MyEnrollments()
    {
        var studentId = CurrentStudentId(); // Lấy mã học viên từ claim trong cookie đăng nhập.
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        return View(await _context.Enrollments.AsNoTracking()
            .Where(x => x.StudentId == studentId.Value) // Chỉ lấy dữ liệu của học viên đang đăng nhập.
            .Include(x => x.Course) // Lấy kèm thông tin khóa học.
            .Include(x => x.CourseClass) // Lấy kèm lớp học đã chọn.
                .ThenInclude(x => x!.Teacher) // Lấy tiếp giáo viên của lớp.
            .Include(x => x.Payment) // Lấy kèm khoản học phí nếu đăng ký đã được duyệt.
            .OrderByDescending(x => x.RegisteredAt) // Đăng ký mới nhất hiển thị trước.
            .ToListAsync()); // Chạy truy vấn và gửi danh sách sang View MyEnrollments.
    }

    [Authorize(Roles = "Student")] // Chỉ học viên mới được tự hủy đăng ký.
    [HttpPost] // Nhận yêu cầu từ nút Hủy đăng ký.
    [ValidateAntiForgeryToken] // Chống yêu cầu giả mạo.
    public async Task<IActionResult> Cancel(int id)
    {
        //var studentId = CurrentStudentId();
        //var enrollment = studentId.HasValue
        //    ? await _context.Enrollments.FirstOrDefaultAsync(x =>
        //        x.Id == id && x.StudentId == studentId.Value
        //        && x.Status == EnrollmentState.Pending)
        //    : null;
        //if (enrollment is null)
        //{
        //    TempData["ErrorMessage"] =
        // "Không thể hủy đăng ký này.";
        //    return RedirectToAction(nameof(MyEnrollments));
        //}

        //enrollment.Status = EnrollmentState.Cancelled;
        //enrollment.CourseClassId = null;
        //await _context.SaveChangesAsync();
        //TempData["SuccessMessage"] = "Đã hủy đăng ký.";
        //return RedirectToAction(nameof(MyEnrollments));

        var studentId = CurrentStudentId(); // Xác định học viên đang thao tác.
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var enrollment = await _context.Enrollments
                .Include(x => x.Payment) // Lấy kèm học phí để không hủy nhầm đăng ký đã thanh toán.
                .FirstOrDefaultAsync(x =>
                    // Phải đúng phiếu, đúng chủ sở hữu và vẫn đang Chờ duyệt.
                    x.Id == id && x.StudentId == studentId.Value
                    && x.Status == EnrollmentState.Pending);

            if (enrollment is null)
            {
                TempData["ErrorMessage"] = "Không thể hủy đăng ký này.";
                return RedirectToAction(nameof(MyEnrollments));
            }

            // Đã thanh toán đủ (hệ thống không cho đóng thiếu -> Paid là trạng thái đã thu tiền)
            if (enrollment.Payment is not null && enrollment.Payment.Status == PaymentState.Paid)
            {
                TempData["ErrorMessage"] =
                    "Đăng ký này đã được thanh toán, vui lòng liên hệ để được hỗ trợ hủy/hoàn tiền.";
                return RedirectToAction(nameof(MyEnrollments));
            }

            enrollment.Status = EnrollmentState.Cancelled; // Chuyển trạng thái sang Đã hủy.
            enrollment.CourseClassId = null; // Gỡ lớp đã chọn khỏi đăng ký bị hủy.

            if (enrollment.Payment is not null)
            {
                enrollment.Payment.Status = PaymentState.Cancelled;
            }

            await _context.SaveChangesAsync(); // Cập nhật dữ liệu xuống database.
            await transaction.CommitAsync(); // Xác nhận giao dịch hủy.

            TempData["SuccessMessage"] = "Đã hủy đăng ký.";
            return RedirectToAction(nameof(MyEnrollments));
        });
    }

    // Hàm phụ đọc claim StudentId; trả null nếu claim không tồn tại hoặc không đổi được thành số.
    private int? CurrentStudentId()
    {
        return int.TryParse(User.FindFirstValue("StudentId"), out var id) ? id : null;
    }
}
