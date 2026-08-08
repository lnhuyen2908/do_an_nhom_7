using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

[Authorize]
public class CourseClassesController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public CourseClassesController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Schedule(string? keyword, string? mode, DateTime? startDate, int page = 1)
    {
        const int pageSize = 10;
        keyword = keyword?.Trim();
        mode = mode?.Trim();
        page = Math.Max(page, 1);
        var query = _context.CourseClasses.AsNoTracking()
            .Include(x => x.Course).Include(x => x.Teacher)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Code.Contains(keyword)
                || x.Course.Code.Contains(keyword)
                || x.Course.Name.Contains(keyword));
        }
        if (mode == "Online")
        {
            query = query.Where(x => x.Room.Contains("Online"));
        }
        else if (mode == "Offline")
        {
            query = query.Where(x => !x.Room.Contains("Online"));
        }
        if (startDate.HasValue)
        {
            var selectedDate = startDate.Value.Date;
            query = query.Where(x => x.StartDate.Date == selectedDate);
        }

        if (User.IsInRole("Teacher"))
        {
            var teacherId = ClaimId("TeacherId");
            if (!teacherId.HasValue)
            {
                return Forbid();
            }

            query = query.Where(x => x.TeacherId == teacherId.Value);
            ViewBag.IsTeacherSchedule = true;
        }

        ViewBag.Keyword = keyword;
        ViewBag.Mode = mode;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Min(page, totalPages);
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(await query.OrderByDescending(x => x.StartDate).ThenBy(x => x.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync());
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index(string? keyword, DateTime? startDate, int page = 1)
    {
        const int pageSize = 10;
        keyword = keyword?.Trim();
        page = Math.Max(page, 1);
        var query = _context.CourseClasses.AsNoTracking()
            .Include(x => x.Course).Include(x => x.Teacher)
            .Include(x => x.Enrollments)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Code.Contains(keyword)
                || x.Course.Code.Contains(keyword)
                || x.Course.Name.Contains(keyword)
                || x.Teacher.FullName.Contains(keyword));
        }
        if (startDate.HasValue)
        {
            var selectedDate = startDate.Value.Date;
            query = query.Where(x => x.StartDate.Date == selectedDate);
        }

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Min(page, totalPages);
        ViewBag.Keyword = keyword;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(await query.OrderByDescending(x => x.StartDate).ThenBy(x => x.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync());
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Details(int? id)
    {
        var courseClass = id.HasValue
            ? await _context.CourseClasses.AsNoTracking()
                .Include(x => x.Course).Include(x => x.Teacher)
                .Include(x => x.Enrollments).ThenInclude(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == id.Value)
            : null;
        return courseClass is null ? NotFound() : View(courseClass);
    }

    [Authorize(Roles = "Admin,Staff")]
    public IActionResult Create()
    {
        SetSelections();
        return View();
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Code,CourseId,TeacherId,Room,Schedule,StartDate,EndDate,Status,Capacity")]
        CourseClass courseClass)
    {
        courseClass.Code = courseClass.Code.Trim().ToUpperInvariant();
        courseClass.Room = courseClass.Room.Trim();
        courseClass.Schedule = courseClass.Schedule.Trim();
        ModelState.Clear();
        TryValidateModel(courseClass);

        await ValidateCourseClassAsync(courseClass);
        if (!ModelState.IsValid)
        {
            SetSelections(courseClass.CourseId, courseClass.TeacherId);
            return View(courseClass);
        }

        _context.Add(courseClass);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã thêm lớp học.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Edit(int? id)
    {
        var courseClass = id.HasValue ? await _context.CourseClasses.FindAsync(id.Value) : null;
        if (courseClass is null)
        {
            return NotFound();
        }
        SetSelections(courseClass.CourseId, courseClass.TeacherId);
        return View(courseClass);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Code,CourseId,TeacherId,Room,Schedule,StartDate,EndDate,Status,Capacity")]
        CourseClass courseClass)
    {
        if (id != courseClass.Id)
        {
            return NotFound();
        }
        courseClass.Code = courseClass.Code.Trim().ToUpperInvariant();
        courseClass.Room = courseClass.Room.Trim();
        courseClass.Schedule = courseClass.Schedule.Trim();
        ModelState.Clear();
        TryValidateModel(courseClass);

        await ValidateCourseClassAsync(courseClass, id);
        if (!ModelState.IsValid)
        {
            SetSelections(courseClass.CourseId, courseClass.TeacherId);
            return View(courseClass);
        }

        _context.Update(courseClass);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã cập nhật lớp học.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int? id)
    {
        var courseClass = id.HasValue
            ? await _context.CourseClasses.AsNoTracking()
                .Include(x => x.Course).Include(x => x.Teacher)
                .FirstOrDefaultAsync(x => x.Id == id.Value)
            : null;
        return courseClass is null ? NotFound() : View(courseClass);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var courseClass = await _context.CourseClasses.FindAsync(id);
        if (courseClass is not null)
        {
            try
            {
                _context.CourseClasses.Remove(courseClass);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa lớp học.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa lớp học vì đã có đăng ký, điểm hoặc điểm danh liên quan.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> MyClasses()
    {
        var teacherId = ClaimId("TeacherId");
        if (!teacherId.HasValue)
        {
            return Forbid();
        }

        return View(await _context.CourseClasses.AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value)
            .Include(x => x.Course)
            .Include(x => x.Enrollments)
            .OrderBy(x => x.StartDate).ToListAsync());
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ClassRoster(int id)
    {
        var teacherId = ClaimId("TeacherId");
        var courseClass = teacherId.HasValue
            ? await _context.CourseClasses.AsNoTracking()
                .Include(x => x.Course)
                .Include(x => x.Enrollments
                    .Where(e => e.Status == EnrollmentState.Approved))
                    .ThenInclude(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == id && x.TeacherId == teacherId.Value)
            : null;
        return courseClass is null ? NotFound() : View(courseClass);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MySchedule()
    {
        var studentId = ClaimId("StudentId");
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        return View(await _context.Enrollments.AsNoTracking()
            .Where(x => x.StudentId == studentId.Value
                && x.Status == EnrollmentState.Approved
                && x.CourseClassId.HasValue)
            .Include(x => x.Course)
            .Include(x => x.CourseClass).ThenInclude(x => x!.Teacher)
            .OrderBy(x => x.CourseClass!.StartDate)
            .ThenBy(x => x.CourseClass!.Schedule)
            .ToListAsync());
    }

    private void SetSelections(int? courseId = null, int? teacherId = null)
    {
        ViewData["CourseId"] = new SelectList(
            _context.Courses.OrderBy(x => x.Code), "Id", "Name", courseId);
        ViewData["TeacherId"] = new SelectList(
            _context.Teachers.OrderBy(x => x.Code), "Id", "FullName", teacherId);
    }

    private async Task ValidateCourseClassAsync(CourseClass courseClass, int? currentId = null)
    {
        if (await _context.CourseClasses.AnyAsync(x =>
                x.Id != currentId && x.Code == courseClass.Code))
        {
            ModelState.AddModelError(nameof(CourseClass.Code), "Mã lớp đã tồn tại.");
        }

        if (!await _context.Courses.AnyAsync(x => x.Id == courseClass.CourseId))
        {
            ModelState.AddModelError(nameof(CourseClass.CourseId), "Vui lòng chọn khóa học hợp lệ.");
        }

        if (!await _context.Teachers.AnyAsync(x => x.Id == courseClass.TeacherId))
        {
            ModelState.AddModelError(nameof(CourseClass.TeacherId), "Vui lòng chọn giáo viên hợp lệ.");
        }

        if (courseClass.EndDate.HasValue && courseClass.EndDate.Value.Date < courseClass.StartDate.Date)
        {
            ModelState.AddModelError(nameof(CourseClass.EndDate), "Ngày kết thúc phải sau ngày bắt đầu.");
        }

        if (ParseStudyDays(courseClass.Schedule).Count == 0)
        {
            ModelState.AddModelError(
                nameof(CourseClass.Schedule),
                "Lịch học cần ghi rõ thứ học, ví dụ: Thứ 2-4-6, 18:00-19:30.");
        }
    }

    private int? ClaimId(string type)
    {
        return int.TryParse(User.FindFirstValue(type), out var id) ? id : null;
    }

    private static HashSet<DayOfWeek> ParseStudyDays(string schedule)
    {
        var days = new HashSet<DayOfWeek>();
        var firstPart = schedule.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? schedule;
        firstPart = firstPart.Replace("Thứ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("thu", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        foreach (var token in firstPart.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var normalized = token.Trim().ToUpperInvariant();
            if (normalized is "CN" or "CHỦ NHẬT" or "CHU NHAT")
            {
                days.Add(DayOfWeek.Sunday);
            }
            else if (int.TryParse(normalized, out var dayNumber))
            {
                var day = dayNumber switch
                {
                    2 => DayOfWeek.Monday,
                    3 => DayOfWeek.Tuesday,
                    4 => DayOfWeek.Wednesday,
                    5 => DayOfWeek.Thursday,
                    6 => DayOfWeek.Friday,
                    7 => DayOfWeek.Saturday,
                    _ => (DayOfWeek?)null
                };
                if (day.HasValue)
                {
                    days.Add(day.Value);
                }
            }
        }

        return days;
    }
}
