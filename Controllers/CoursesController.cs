using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

public class CoursesController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public CoursesController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? keyword, string? level, int page = 1)
    {
        const int pageSize = 6;
        var query = _context.Courses.AsNoTracking();
        keyword = keyword?.Trim();
        level = level?.Trim();
        page = Math.Max(page, 1);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Code.Contains(keyword) || x.Name.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(x => x.Level == level);
        }

        ViewBag.Keyword = keyword;
        ViewBag.Level = level;
        ViewBag.Levels = await _context.Courses.AsNoTracking()
            .Select(x => x.Level).Distinct().OrderBy(x => x).ToListAsync();

        var totalCourses = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCourses / (double)pageSize));
        page = Math.Min(page, totalPages);

        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCourses = totalCourses;

        return View(await query
            .OrderBy(x => x.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var course = await _context.Courses.AsNoTracking()
            .Include(x => x.CourseClasses)
                .ThenInclude(x => x.Teacher)
            .FirstOrDefaultAsync(x => x.Id == id.Value);
        if (course is null)
        {
            return NotFound();
        }

        var classIds = course.CourseClasses.Select(x => x.Id).ToList();
        ViewBag.ClassSeats = await _context.Enrollments.AsNoTracking()
            .Where(x => x.CourseClassId.HasValue
                && classIds.Contains(x.CourseClassId.Value)
                && x.Status == EnrollmentState.Approved)
            .GroupBy(x => x.CourseClassId!.Value)
            .ToDictionaryAsync(x => x.Key, x => x.Count());

        var studentId = CurrentClaimId("StudentId");
        ViewBag.IsSaved = studentId.HasValue
            && await _context.SavedCourses.AsNoTracking()
                .AnyAsync(x => x.StudentId == studentId && x.CourseId == course.Id);
        ViewBag.HasEnrollment = studentId.HasValue
            && await _context.Enrollments.AsNoTracking()
                .AnyAsync(x => x.StudentId == studentId
                    && x.CourseId == course.Id
                    && x.Status != EnrollmentState.Cancelled);
        return View(course);
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int courseId, int? courseClassId)
    {
        var studentId = CurrentClaimId("StudentId");
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var course = await _context.Courses.FindAsync(courseId);
            if (course is null)
            {
                return NotFound();
            }

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
                var selectedClass = await _context.CourseClasses.FindAsync(courseClassId.Value);
                if (selectedClass is null || selectedClass.CourseId != courseId)
                {
                    TempData["ErrorMessage"] = "Lớp học được chọn không hợp lệ.";
                    return RedirectToAction(nameof(Details), new { id = courseId });
                }

                var occupied = await _context.Enrollments.CountAsync(x =>
                    x.CourseClassId == courseClassId && x.Status == EnrollmentState.Approved);
                if (occupied >= selectedClass.Capacity)
                {
                    TempData["ErrorMessage"] = $"Lớp {selectedClass.Code} đã đủ sĩ số.";
                    return RedirectToAction(nameof(Details), new { id = courseId });
                }
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId.Value,
                CourseId = courseId,
                CourseClassId = courseClassId,
                Status = EnrollmentState.Pending,
                RegisteredAt = DateTime.Now
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

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
