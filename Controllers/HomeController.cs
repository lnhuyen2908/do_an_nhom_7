using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

public class HomeController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public HomeController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.StudentCount = await _context.Students.CountAsync();
        ViewBag.TeacherCount = await _context.Teachers.CountAsync();
        ViewBag.CourseCount = await _context.Courses.CountAsync();
        ViewBag.ClassCount = await _context.CourseClasses.CountAsync();

        var featuredCourses = await _context.Courses .AsNoTracking()    .OrderBy(x => x.Code).Take(3).ToListAsync();

        return View(featuredCourses);
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.StudentCount = await _context.Students.CountAsync();
        ViewBag.TeacherCount = await _context.Teachers.CountAsync();
        ViewBag.CourseCount = await _context.Courses.CountAsync();
        ViewBag.ClassCount = await _context.CourseClasses.CountAsync();
        ViewBag.PendingEnrollmentCount = await _context.Enrollments.CountAsync(x => x.Status == EnrollmentState.Pending);
        ViewBag.ApprovedEnrollmentCount = await _context.Enrollments.CountAsync(x => x.Status == EnrollmentState.Approved);
        ViewBag.ExpectedTuition = await _context.Payments.Where(x => x.Enrollment.Status != EnrollmentState.Cancelled) .SumAsync(x => (decimal?)x.Amount) ?? 0;
        ViewBag.CollectedTuition = await _context.Payments .Where(x => x.Enrollment.Status != EnrollmentState.Cancelled).SumAsync(x => (decimal?)x.PaidAmount) ?? 0;

        ViewBag.RecentEnrollments = await _context.Enrollments.AsNoTracking()
            .Where(x => x.Status == EnrollmentState.Approved && x.Payment != null && (x.Payment.Status == PaymentState.Paid || x.Payment.PaidAmount >= x.Payment.Amount))
            .Include(x => x.Student).Include(x => x.Course).Include(x => x.Payment)
            .OrderByDescending(x => x.RegisteredAt).Take(8).ToListAsync();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View(model: requestId);
    }
}
