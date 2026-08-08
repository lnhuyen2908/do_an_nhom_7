using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers;

// Courses = Khóa học. Controller này xử lý danh sách, chi tiết, đăng ký và lưu khóa học.
public class CoursesController : Controller
{
    private readonly EnglishCenterDbContext _context;
    private readonly NotificationService _notificationService;

    public CoursesController(EnglishCenterDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [AllowAnonymous] // Khách chưa đăng nhập cũng được xem danh sách khóa học.
    public async Task<IActionResult> Index(string? keyword, string? level, DateTime? startDate, int page = 1)
    {
        const int pageSize = 6; // Mỗi trang hiển thị tối đa 6 khóa học.
        var query = _context.Courses.AsNoTracking()
            .Include(x => x.CourseClasses)
            .AsQueryable(); // Bắt đầu tạo truy vấn đọc bảng Courses.
        var hasKeywordParameter = keyword is not null;
        keyword = keyword?.Trim(); // Xóa khoảng trắng thừa ở từ khóa.
        level = level?.Trim(); // Xóa khoảng trắng thừa ở trình độ.
        page = Math.Max(page, 1); // Không cho số trang nhỏ hơn 1.

        var hasInvalidKeyword = hasKeywordParameter
            && (string.IsNullOrWhiteSpace(keyword) || keyword.All(x => x == '+'));

        if (hasInvalidKeyword)
        {
            ViewBag.SearchMessage = "Vui lòng nhập từ khóa hợp lệ.";
        }
        else if (!string.IsNullOrWhiteSpace(keyword))
        {
            // Where lọc khóa học có mã hoặc tên chứa từ khóa người dùng nhập.
            query = query.Where(x => x.Code.Contains(keyword) || x.Name.Contains(keyword));
        }

        //lọc theo trình độ 
        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(x => x.Level == level); // Chỉ giữ khóa học đúng trình độ đã chọn.
        }

        if (startDate.HasValue)
        {
            var selectedDate = startDate.Value.Date;
            query = query.Where(x => x.CourseClasses.Any(c => c.StartDate.Date == selectedDate));
        }

        ViewBag.Keyword = keyword;
        ViewBag.Level = level;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.Levels = await _context.Courses.AsNoTracking()
            .Select(x => x.Level) // Chỉ lấy cột trình độ.
            .Distinct() // Loại bỏ các trình độ bị trùng.
            .OrderBy(x => x) // Sắp xếp trình độ tăng dần.
            .ToListAsync(); // Thực thi truy vấn và chuyển kết quả thành danh sách.

        var totalCourses = await query.CountAsync(); // Đếm số khóa học sau khi lọc.
        if (totalCourses == 0 && !string.IsNullOrWhiteSpace(keyword))
        {
            ViewBag.SearchMessage = "Không tìm thấy khóa học phù hợp.";
        }

        // Chia tổng khóa học cho 6 và làm tròn lên để tính tổng số trang.
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCourses / (double)pageSize));
        page = Math.Min(page, totalPages); // Không cho trang hiện tại vượt quá trang cuối.

        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCourses = totalCourses;

        var orderedQuery = startDate.HasValue
            ? query.OrderByDescending(x => x.CourseClasses.Max(c => c.StartDate)).ThenBy(x => x.Code)
            : query.OrderBy(x => x.Code);

        return View(await orderedQuery
            .Skip((page - 1) * pageSize) // Bỏ qua dữ liệu thuộc các trang trước.
            .Take(pageSize) // Chỉ lấy tối đa 6 khóa học cho trang hiện tại.
            .ToListAsync()); // Truy vấn database rồi gửi danh sách sang Views/Courses/Index.cshtml.
    }

    [AllowAnonymous] // Khách cũng được xem nội dung chi tiết khóa học.
    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue) // Không có mã khóa học trên URL thì trả lỗi 404.
        {
            return NotFound();
        }

        var course = await _context.Courses.AsNoTracking()
            .Include(x => x.CourseClasses) // Lấy kèm các lớp đang mở của khóa học.
                .ThenInclude(x => x.Teacher) // Trong mỗi lớp, lấy tiếp thông tin giáo viên.
            .FirstOrDefaultAsync(x => x.Id == id.Value); // Tìm khóa học có Id nhận từ URL.
        if (course is null)
        {
            return NotFound();
        }

        var classIds = course.CourseClasses.Select(x => x.Id).ToList(); // Lấy danh sách mã lớp của khóa học.
        ViewBag.ClassSeats = await _context.Enrollments.AsNoTracking()
            // Chỉ đếm đăng ký đã duyệt thuộc các lớp của khóa học đang xem.
            .Where(x => x.CourseClassId.HasValue
                && classIds.Contains(x.CourseClassId.Value)
                && x.Status == EnrollmentState.Approved)
            .GroupBy(x => x.CourseClassId!.Value) // Nhóm đăng ký theo mã lớp.
            .ToDictionaryAsync(x => x.Key, x => x.Count()); // Tạo từ điển: mã lớp -> số học viên.

        var studentId = CurrentClaimId("StudentId"); // Đọc mã học viên từ cookie đăng nhập.
        ViewBag.IsSaved = studentId.HasValue
            && await _context.SavedCourses.AsNoTracking()
                .AnyAsync(x => x.StudentId == studentId && x.CourseId == course.Id); // Kiểm tra khóa học đã lưu chưa.
        ViewBag.HasEnrollment = studentId.HasValue
            && await _context.Enrollments.AsNoTracking()
                .AnyAsync(x => x.StudentId == studentId
                    && x.CourseId == course.Id
                    && x.Status != EnrollmentState.Cancelled); // Kiểm tra học viên đã có đăng ký còn hiệu lực chưa.
        return View(course); // Gửi khóa học sang Views/Courses/Details.cshtml.
    }

    [Authorize(Roles = "Student")] // Chỉ tài khoản có vai trò Học viên được đăng ký khóa học.
    [HttpPost] // Nhận dữ liệu từ nút đăng ký trên trang chi tiết.
    [ValidateAntiForgeryToken] // Xác minh form được gửi từ chính website này.
    public async Task<IActionResult> Register(int courseId, int? courseClassId)
    {
        var studentId = CurrentClaimId("StudentId"); // Xác định học viên đang đăng nhập.
        if (!studentId.HasValue)
        {
            return Forbid(); // Trả lỗi 403 nếu tài khoản không có StudentId hợp lệ.
        }

        var strategy = _context.Database.CreateExecutionStrategy(); // Cho phép thử lại khi lỗi SQL tạm thời.
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            // Khóa giao dịch để việc kiểm tra chỗ trống và thêm đăng ký diễn ra nhất quán.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var course = await _context.Courses.FindAsync(courseId); // Tìm khóa học theo khóa chính.
            if (course is null)
            {
                return NotFound();
            }

            // Kiểm tra học viên đã đăng ký khóa học này và chưa hủy hay chưa.
            var exists = await _context.Enrollments.AnyAsync(x =>
                x.StudentId == studentId && x.CourseId == courseId
                && x.Status != EnrollmentState.Cancelled);
            if (exists)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký khóa học này.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            if (courseClassId.HasValue)
            {
                var selectedClass = await _context.CourseClasses.FindAsync(courseClassId.Value); // Tìm lớp đã chọn.
                if (selectedClass is null || selectedClass.CourseId != courseId)
                {
                    TempData["ErrorMessage"] = "Lớp học được chọn không hợp lệ.";
                    return RedirectToAction(nameof(Details), new { id = courseId });
                }

                if (!selectedClass.CanRegister)
                {
                    TempData["ErrorMessage"] = $"Lớp {selectedClass.Code} hiện không nhận đăng ký.";
                    return RedirectToAction(nameof(Details), new { id = courseId });
                }

                // Đếm số đăng ký đã được duyệt trong lớp để kiểm tra sĩ số.
                var occupied = await _context.Enrollments.CountAsync(x =>
                    x.CourseClassId == courseClassId && x.Status == EnrollmentState.Approved);
                if (occupied >= selectedClass.Capacity)
                {
                    TempData["ErrorMessage"] = $"Lớp {selectedClass.Code} đã đủ sĩ số.";
                    return RedirectToAction(nameof(Details), new { id = courseId });
                }
            }

            // Tạo phiếu đăng ký mới ở trạng thái Chờ duyệt.
            var enrollment = new Enrollment
            {
                StudentId = studentId.Value,
                CourseId = courseId,
                CourseClassId = courseClassId,
                Status = EnrollmentState.Pending,
                RegisteredAt = DateTime.Now
            };
            _context.Enrollments.Add(enrollment); // Đánh dấu cần INSERT vào bảng Enrollments.
            await _context.SaveChangesAsync(); // Thực hiện lệnh INSERT.
            await _notificationService.NotifyRolesAsync(
                new[] { "Admin", "Staff" },
                "Đăng ký khóa học mới",
                $"{User.Identity?.Name} vừa đăng ký khóa {course.Name}.",
                Url.Action("Index", "Enrollments") ?? string.Empty);
            await transaction.CommitAsync(); // Xác nhận giao dịch.

            TempData["SuccessMessage"] = "Đăng ký thành công. Hồ sơ đang chờ nhân viên đào tạo duyệt.";
            return RedirectToAction("MyEnrollments", "Enrollments");
        });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int courseId)
    {
        var studentId = CurrentClaimId("StudentId");
        if (!studentId.HasValue || !await _context.Courses.AnyAsync(x => x.Id == courseId))
        {
            return NotFound();
        }

        if (!await _context.SavedCourses.AnyAsync(x =>
                x.StudentId == studentId && x.CourseId == courseId))
        {
            _context.SavedCourses.Add(new SavedCourse
            {
                StudentId = studentId.Value,
                CourseId = courseId,
                SavedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Đã lưu khóa học.";
        return RedirectToAction(nameof(Details), new { id = courseId });
    }

    [Authorize(Roles = "Admin,Staff")]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Code,Name,Level,Tuition,Duration,Description,ImageUrl")] Course course)
    {
        course.Code = course.Code.Trim().ToUpperInvariant();
        course.Name = course.Name.Trim();
        course.Level = course.Level.Trim();
        course.Duration = course.Duration.Trim();
        course.Description = course.Description.Trim();
        course.ImageUrl = course.ImageUrl.Trim();
        ModelState.Clear();
        TryValidateModel(course);

        if (!ModelState.IsValid)
        {
            return View(course);
        }

        if (await _context.Courses.AnyAsync(x => x.Code == course.Code))
        {
            ModelState.AddModelError(nameof(Course.Code), "Mã khóa học đã tồn tại.");
            return View(course);
        }

        _context.Add(course);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã thêm khóa học.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Edit(int? id)
    {
        var course = id.HasValue ? await _context.Courses.FindAsync(id.Value) : null;
        return course is null ? NotFound() : View(course);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Code,Name,Level,Tuition,Duration,Description,ImageUrl")] Course course)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        course.Code = course.Code.Trim().ToUpperInvariant();
        course.Name = course.Name.Trim();
        course.Level = course.Level.Trim();
        course.Duration = course.Duration.Trim();
        course.Description = course.Description.Trim();
        course.ImageUrl = course.ImageUrl.Trim();
        ModelState.Clear();
        TryValidateModel(course);

        if (!ModelState.IsValid)
        {
            return View(course);
        }

        if (await _context.Courses.AnyAsync(x => x.Id != id && x.Code == course.Code))
        {
            ModelState.AddModelError(nameof(Course.Code), "Mã khóa học đã tồn tại.");
            return View(course);
        }

        _context.Update(course);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã cập nhật khóa học.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int? id)
    {
        var course = id.HasValue
            ? await _context.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id.Value)
            : null;
        return course is null ? NotFound() : View(course);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course is not null)
        {
            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa khóa học.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa khóa học vì đã có lớp học, đăng ký hoặc bài giảng liên quan.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private int? CurrentClaimId(string claimType)
    {
        return int.TryParse(User.FindFirstValue(claimType), out var id) ? id : null;
    }
}
