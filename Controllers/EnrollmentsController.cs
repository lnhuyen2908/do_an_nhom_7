using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

[Authorize]
public class EnrollmentsController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public EnrollmentsController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index(EnrollmentState? status)
    {
        var query = _context.Enrollments.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.Course)
            .Include(x => x.CourseClass)
            .AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        ViewBag.Status = status;
        ViewBag.Classes = await _context.CourseClasses.AsNoTracking()
            .Include(x => x.Course).OrderBy(x => x.Code).ToListAsync();
        ViewBag.ClassSeats = await _context.Enrollments.AsNoTracking()
            .Where(x => x.CourseClassId.HasValue && x.Status == EnrollmentState.Approved)
            .GroupBy(x => x.CourseClassId!.Value)
            .ToDictionaryAsync(x => x.Key, x => x.Count());
        return View(await query.OrderByDescending(x => x.RegisteredAt).ToListAsync());
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

                var occupied = await _context.Enrollments.CountAsync(x =>
                    x.Id != id && x.CourseClassId == courseClassId
                    && x.Status == EnrollmentState.Approved);
                if (occupied >= courseClass.Capacity)
                {
                    TempData["ErrorMessage"] = $"Lớp {courseClass.Code} đã đủ sĩ số.";
                    return RedirectToAction(nameof(Index));
                }
            }

            enrollment.Status = status;
            enrollment.CourseClassId =
                status == EnrollmentState.Approved ? courseClassId : null;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái đăng ký.";
            return RedirectToAction(nameof(Index));
        });
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyEnrollments()
    {
        var studentId = CurrentStudentId();
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        return View(await _context.Enrollments.AsNoTracking()
            .Where(x => x.StudentId == studentId.Value)
            .Include(x => x.Course)
            .Include(x => x.CourseClass)
                .ThenInclude(x => x!.Teacher)
            .Include(x => x.Payment)
            .OrderByDescending(x => x.RegisteredAt)
            .ToListAsync());
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var studentId = CurrentStudentId();
        var enrollment = studentId.HasValue
            ? await _context.Enrollments.FirstOrDefaultAsync(x =>
                x.Id == id && x.StudentId == studentId.Value
                && x.Status == EnrollmentState.Pending)
            : null;
        if (enrollment is null)
        {
            return NotFound();
        }

        enrollment.Status = EnrollmentState.Cancelled;
        enrollment.CourseClassId = null;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã hủy đăng ký.";
        return RedirectToAction(nameof(MyEnrollments));
    }

    private int? CurrentStudentId()
    {
        return int.TryParse(User.FindFirstValue("StudentId"), out var id) ? id : null;
    }
}
