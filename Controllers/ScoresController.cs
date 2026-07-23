using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

[Authorize]
public class ScoresController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public ScoresController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Scores.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.CourseClass).ThenInclude(x => x.Course)
            .OrderBy(x => x.CourseClass.Code).ThenBy(x => x.Student.Code)
            .ToListAsync());
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Manage(int classId)
    {
        var teacherId = ClaimId("TeacherId");
        var courseClass = teacherId.HasValue
            ? await _context.CourseClasses.AsNoTracking()
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.Id == classId && x.TeacherId == teacherId.Value)
            : null;
        if (courseClass is null)
        {
            return NotFound();
        }

        ViewBag.CourseClass = courseClass;
        ViewBag.Scores = await _context.Scores.AsNoTracking()
            .Where(x => x.CourseClassId == classId).ToListAsync();
        return View(await _context.Enrollments.AsNoTracking()
            .Where(x => x.CourseClassId == classId
                && x.Status == EnrollmentState.Approved)
            .Include(x => x.Student)
            .OrderBy(x => x.Student.Code).ToListAsync());
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        int classId, int studentId, double midtermScore,
        double finalScore, string? comment)
    {
        var teacherId = ClaimId("TeacherId");
        var canManage = teacherId.HasValue
            && await _context.CourseClasses.AnyAsync(x =>
                x.Id == classId && x.TeacherId == teacherId.Value)
            && await _context.Enrollments.AnyAsync(x =>
                x.CourseClassId == classId && x.StudentId == studentId
                && x.Status == EnrollmentState.Approved);
        if (!canManage)
        {
            return NotFound();
        }

        if (midtermScore is < 0 or > 10 || finalScore is < 0 or > 10)
        {
            TempData["ErrorMessage"] = "Điểm phải nằm trong khoảng từ 0 đến 10.";
            return RedirectToAction(nameof(Manage), new { classId });
        }

        comment = comment?.Trim() ?? string.Empty;
        if (comment.Length > 500)
        {
            TempData["ErrorMessage"] = "Nhận xét không được vượt quá 500 ký tự.";
            return RedirectToAction(nameof(Manage), new { classId });
        }

        var score = await _context.Scores.FirstOrDefaultAsync(x =>
            x.CourseClassId == classId && x.StudentId == studentId);
        if (score is null)
        {
            score = new Score { CourseClassId = classId, StudentId = studentId };
            _context.Scores.Add(score);
        }
        score.MidtermScore = midtermScore;
        score.FinalScore = finalScore;
        score.Comment = comment;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã lưu điểm và nhận xét.";
        return RedirectToAction(nameof(Manage), new { classId });
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyScores()
    {
        var studentId = ClaimId("StudentId");
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        return View(await _context.Scores.AsNoTracking()
            .Where(x => x.StudentId == studentId.Value)
            .Include(x => x.CourseClass).ThenInclude(x => x.Course)
            .OrderBy(x => x.CourseClass.Code).ToListAsync());
    }

    private int? ClaimId(string type)
    {
        return int.TryParse(User.FindFirstValue(type), out var id) ? id : null;
    }
}
