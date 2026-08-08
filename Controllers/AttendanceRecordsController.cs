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
    public async Task<IActionResult> Index(string? keyword, DateTime? studyDate, int page = 1)
    {
        const int pageSize = 10;
        keyword = keyword?.Trim();
        page = Math.Max(page, 1);
        var query = _context.AttendanceRecords.AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.CourseClass).ThenInclude(x => x.Course)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Student.Code.Contains(keyword)
                || x.Student.FullName.Contains(keyword)
                || x.CourseClass.Code.Contains(keyword)
                || x.CourseClass.Course.Name.Contains(keyword));
        }
        if (studyDate.HasValue)
        {
            query = query.Where(x => x.StudyDate == studyDate.Value.Date);
        }

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Min(page, totalPages);
        ViewBag.Keyword = keyword;
        ViewBag.StudyDate = studyDate?.ToString("yyyy-MM-dd");
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        return View(await query.OrderByDescending(x => x.StudyDate)
            .ThenBy(x => x.CourseClass.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync());
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyAttendance(int? classId, DateTime? fromDate, DateTime? toDate)
    {
        var studentId = ClaimId("StudentId");
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        var enrollments = await _context.Enrollments.AsNoTracking()
            .Where(x => x.StudentId == studentId.Value
                && x.Status == EnrollmentState.Approved
                && x.CourseClassId.HasValue)
            .Include(x => x.Course)
            .Include(x => x.CourseClass)
            .OrderBy(x => x.CourseClass!.StartDate)
            .ToListAsync();

        ViewBag.Classes = enrollments;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        if (enrollments.Count == 0)
        {
            ViewBag.NotStartedMessage = "Bạn chưa có lớp học nào đã được duyệt.";
            ViewBag.PresentCount = 0;
            ViewBag.AbsentCount = 0;
            return View(new List<AttendanceRecord>());
        }

        var selectedClassId = classId
            ?? enrollments.FirstOrDefault(x => x.CourseClass!.StartDate.Date <= DateTime.Today)?.CourseClassId
            ?? enrollments.First().CourseClassId!.Value;
        var selectedEnrollment = enrollments.FirstOrDefault(x => x.CourseClassId == selectedClassId);
        if (selectedEnrollment is null)
        {
            return NotFound();
        }

        ViewBag.SelectedClassId = selectedClassId;

        var query = _context.AttendanceRecords.AsNoTracking()
            .Where(x => x.StudentId == studentId.Value
                && x.CourseClassId == selectedClassId)
            .Include(x => x.CourseClass).ThenInclude(x => x.Course)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.StudyDate >= fromDate.Value.Date);
        }
        if (toDate.HasValue)
        {
            query = query.Where(x => x.StudyDate <= toDate.Value.Date);
        }

        var records = await query.OrderByDescending(x => x.StudyDate).ToListAsync();
        ViewBag.PresentCount = records.Count(x => x.IsPresent);
        ViewBag.AbsentCount = records.Count(x => !x.IsPresent);
        return View(records);
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
        var dateError = ValidateStudyDate(courseClass, selectedDate);
        ViewBag.CourseClass = courseClass;
        ViewBag.StudyDate = selectedDate;
        ViewBag.DateValidationMessage = dateError;
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
        var courseClass = teacherId.HasValue
            ? await _context.CourseClasses.FirstOrDefaultAsync(x =>
                x.Id == classId && x.TeacherId == teacherId.Value)
            : null;
        var canManage = courseClass is not null
            && await _context.Enrollments.AnyAsync(x =>
                x.CourseClassId == classId && x.StudentId == studentId
                && x.Status == EnrollmentState.Approved);
        if (!canManage)
        {
            return NotFound();
        }

        studyDate = studyDate.Date;
        var dateError = ValidateStudyDate(courseClass!, studyDate);
        if (!string.IsNullOrWhiteSpace(dateError))
        {
            TempData["ErrorMessage"] = dateError;
            return RedirectToAction(nameof(Manage), new { classId, studyDate });
        }

        note = note?.Trim() ?? string.Empty;
        if (note.Length > 500)
        {
            TempData["ErrorMessage"] = "Ghi chú không được vượt quá 500 ký tự.";
            return RedirectToAction(nameof(Manage), new { classId, studyDate });
        }

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

    [Authorize(Roles = "Teacher")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAll(
        int classId, DateTime studyDate, int[] studentIds)
    {
        var teacherId = ClaimId("TeacherId");
        var courseClass = teacherId.HasValue
            ? await _context.CourseClasses.FirstOrDefaultAsync(x =>
                x.Id == classId && x.TeacherId == teacherId.Value)
            : null;
        if (courseClass is null)
        {
            return NotFound();
        }

        studyDate = studyDate.Date;
        var dateError = ValidateStudyDate(courseClass, studyDate);
        if (!string.IsNullOrWhiteSpace(dateError))
        {
            TempData["ErrorMessage"] = dateError;
            return RedirectToAction(nameof(Manage), new { classId, studyDate });
        }

        studentIds = studentIds.Distinct().ToArray();
        var approvedStudentIds = await _context.Enrollments.AsNoTracking()
            .Where(x => x.CourseClassId == classId
                && x.Status == EnrollmentState.Approved)
            .Select(x => x.StudentId)
            .ToListAsync();
        if (studentIds.Length == 0 || studentIds.Except(approvedStudentIds).Any())
        {
            return NotFound();
        }

        var notes = new Dictionary<int, string>();
        foreach (var studentId in studentIds)
        {
            var note = Request.Form[$"note_{studentId}"].ToString().Trim();
            if (note.Length > 500)
            {
                TempData["ErrorMessage"] = "Ghi chú không được vượt quá 500 ký tự.";
                return RedirectToAction(nameof(Manage), new { classId, studyDate });
            }

            notes[studentId] = note;
        }

        var records = await _context.AttendanceRecords
            .Where(x => x.CourseClassId == classId
                && x.StudyDate == studyDate
                && studentIds.Contains(x.StudentId))
            .ToListAsync();

        foreach (var studentId in studentIds)
        {
            var isPresent = bool.TryParse(
                Request.Form[$"isPresent_{studentId}"].ToString(),
                out var present)
                ? present
                : true;
            var record = records.FirstOrDefault(x => x.StudentId == studentId);
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
            record.Note = notes[studentId];
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã lưu điểm danh cho cả lớp.";
        return RedirectToAction(nameof(Manage), new { classId, studyDate });
    }

    private int? ClaimId(string type)
    {
        return int.TryParse(User.FindFirstValue(type), out var id) ? id : null;
    }

    private static string? ValidateStudyDate(CourseClass courseClass, DateTime studyDate)
    {
        studyDate = studyDate.Date;
        if (studyDate < courseClass.StartDate.Date)
        {
            return $"Ngày điểm danh phải từ ngày bắt đầu lớp {courseClass.StartDate:dd/MM/yyyy}.";
        }

        if (courseClass.EndDate.HasValue && studyDate > courseClass.EndDate.Value.Date)
        {
            return $"Ngày điểm danh phải trước hoặc bằng ngày kết thúc lớp {courseClass.EndDate.Value:dd/MM/yyyy}.";
        }

        var allowedDays = ParseStudyDays(courseClass.Schedule);
        if (allowedDays.Count > 0 && !allowedDays.Contains(studyDate.DayOfWeek))
        {
            return $"Ngày {studyDate:dd/MM/yyyy} không thuộc lịch học của lớp ({courseClass.Schedule}).";
        }

        return null;
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
