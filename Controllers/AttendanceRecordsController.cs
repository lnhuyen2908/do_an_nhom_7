using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

[Authorize]
public class AttendanceRecordsController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public AttendanceRecordsController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index()
    {
        return View(await _context.AttendanceRecords.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.CourseClass).ThenInclude(x => x.Course)
            .OrderByDescending(x => x.StudyDate)
            .ThenBy(x => x.CourseClass.Code).ToListAsync());
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Manage(int classId, DateTime? studyDate)
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

        var selectedDate = (studyDate ?? DateTime.Today).Date;
        ViewBag.CourseClass = courseClass;
        ViewBag.StudyDate = selectedDate;
        ViewBag.Attendance = await _context.AttendanceRecords.AsNoTracking()
            .Where(x => x.CourseClassId == classId && x.StudyDate == selectedDate)
            .ToListAsync();
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
        int classId, int studentId, DateTime studyDate,
        bool isPresent, string? note)
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

        note = note?.Trim() ?? string.Empty;
        if (note.Length > 500)
        {
            TempData["ErrorMessage"] = "Ghi chú không được vượt quá 500 ký tự.";
            return RedirectToAction(nameof(Manage), new { classId, studyDate });
        }

        studyDate = studyDate.Date;
        var record = await _context.AttendanceRecords.FirstOrDefaultAsync(x =>
            x.CourseClassId == classId && x.StudentId == studentId
            && x.StudyDate == studyDate);
        if (record is null)
        {
            record = new AttendanceRecord
            {
                CourseClassId = classId,
                StudentId = studentId,
                StudyDate = studyDate
            };
            _context.AttendanceRecords.Add(record);
        }
        record.IsPresent = isPresent;
        record.Note = note;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã lưu điểm danh.";
        return RedirectToAction(nameof(Manage), new { classId, studyDate });
    }

    private int? ClaimId(string type)
    {
        return int.TryParse(User.FindFirstValue(type), out var id) ? id : null;
    }
}
