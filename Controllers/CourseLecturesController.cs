using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CourseLecturesController : Controller
    {
        private readonly EnglishCenterDbContext _context;

        public CourseLecturesController(EnglishCenterDbContext context)
        {
            _context = context;
        }

        // GET: CourseLectures
        public async Task<IActionResult> Index()
        {
            var englishCenterDbContext = _context.CourseLectures.Include(c => c.Course).Include(c => c.Teacher);
            return View(await englishCenterDbContext.ToListAsync());
        }

        // GET: CourseLectures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseLecture = await _context.CourseLectures
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseLecture == null)
            {
                return NotFound();
            }

            return View(courseLecture);
        }

        // GET: CourseLectures/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name");
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "FullName");
            return View();
        }

        // POST: CourseLectures/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseId,TeacherId,Title,FileName,FileUrl,UploadedAt")] CourseLecture courseLecture)
        {
            NormalizeLecture(courseLecture);
            ModelState.Clear();
            TryValidateModel(courseLecture);
            await ValidateLectureAsync(courseLecture);

            if (ModelState.IsValid)
            {
                _context.Add(courseLecture);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm bài giảng.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", courseLecture.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", courseLecture.TeacherId);
            return View(courseLecture);
        }

        // GET: CourseLectures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseLecture = await _context.CourseLectures.FindAsync(id);
            if (courseLecture == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", courseLecture.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", courseLecture.TeacherId);
            return View(courseLecture);
        }

        // POST: CourseLectures/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,TeacherId,Title,FileName,FileUrl,UploadedAt")] CourseLecture courseLecture)
        {
            if (id != courseLecture.Id)
            {
                return NotFound();
            }

            NormalizeLecture(courseLecture);
            ModelState.Clear();
            TryValidateModel(courseLecture);
            await ValidateLectureAsync(courseLecture);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(courseLecture);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseLectureExists(courseLecture.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Đã cập nhật bài giảng.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", courseLecture.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", courseLecture.TeacherId);
            return View(courseLecture);
        }

        // GET: CourseLectures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseLecture = await _context.CourseLectures
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courseLecture == null)
            {
                return NotFound();
            }

            return View(courseLecture);
        }

        // POST: CourseLectures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseLecture = await _context.CourseLectures.FindAsync(id);
            if (courseLecture != null)
            {
                _context.CourseLectures.Remove(courseLecture);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa bài giảng.";
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> MyLectures()
        {
            if (!User.IsInRole("Teacher")
                || !int.TryParse(User.FindFirstValue("TeacherId"), out var teacherId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var courseIds = await _context.CourseClasses.AsNoTracking()
                .Where(x => x.TeacherId == teacherId)
                .Select(x => x.CourseId).Distinct().ToListAsync();
            ViewBag.Courses = await _context.Courses.AsNoTracking()
                .Where(x => courseIds.Contains(x.Id))
                .OrderBy(x => x.Code).ToListAsync();
            return View(await _context.CourseLectures.AsNoTracking()
                .Where(x => x.TeacherId == teacherId)
                .Include(x => x.Course)
                .OrderByDescending(x => x.UploadedAt).ToListAsync());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLecture(
            int courseId, string? title, string? fileName, string? fileUrl)
        {
            if (!User.IsInRole("Teacher")
                || !int.TryParse(User.FindFirstValue("TeacherId"), out var teacherId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var canManage = await _context.CourseClasses.AsNoTracking()
                .AnyAsync(x => x.TeacherId == teacherId && x.CourseId == courseId);
            if (!canManage)
            {
                return NotFound();
            }

            title = title?.Trim() ?? string.Empty;
            fileName = fileName?.Trim() ?? string.Empty;
            fileUrl = fileUrl?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(fileName)
                || string.IsNullOrWhiteSpace(fileUrl))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin bài giảng.";
                return RedirectToAction(nameof(MyLectures));
            }

            if (title.Length > 200 || fileName.Length > 255 || fileUrl.Length > 500)
            {
                TempData["ErrorMessage"] =
                    "Thông tin bài giảng quá dài. Vui lòng rút gọn tiêu đề, tên tệp hoặc đường dẫn.";
                return RedirectToAction(nameof(MyLectures));
            }

            _context.CourseLectures.Add(new CourseLecture
            {
                CourseId = courseId,
                TeacherId = teacherId,
                Title = title,
                FileName = fileName,
                FileUrl = fileUrl,
                UploadedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm bài giảng.";
            return RedirectToAction(nameof(MyLectures));
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLecture(int id)
        {
            if (!User.IsInRole("Teacher")
                || !int.TryParse(User.FindFirstValue("TeacherId"), out var teacherId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var lecture = await _context.CourseLectures
                .FirstOrDefaultAsync(x => x.Id == id && x.TeacherId == teacherId);
            if (lecture is null)
            {
                return NotFound();
            }
            _context.CourseLectures.Remove(lecture);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa bài giảng.";
            return RedirectToAction(nameof(MyLectures));
        }

        [AllowAnonymous]
        public async Task<IActionResult> LearningMaterials()
        {
            if (!User.IsInRole("Student")
                || !int.TryParse(User.FindFirstValue("StudentId"), out var studentId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var courseIds = await _context.Enrollments.AsNoTracking()
                .Where(x => x.StudentId == studentId
                    && x.Status == EnrollmentState.Approved)
                .Select(x => x.CourseId).Distinct().ToListAsync();
            return View(await _context.CourseLectures.AsNoTracking()
                .Where(x => courseIds.Contains(x.CourseId))
                .Include(x => x.Course).Include(x => x.Teacher)
                .OrderByDescending(x => x.UploadedAt).ToListAsync());
        }

        private bool CourseLectureExists(int id)
        {
            return _context.CourseLectures.Any(e => e.Id == id);
        }

        private static void NormalizeLecture(CourseLecture lecture)
        {
            lecture.Title = lecture.Title.Trim();
            lecture.FileName = lecture.FileName.Trim();
            lecture.FileUrl = lecture.FileUrl.Trim();
        }

        private async Task ValidateLectureAsync(CourseLecture lecture)
        {
            if (!await _context.Courses.AnyAsync(x => x.Id == lecture.CourseId))
            {
                ModelState.AddModelError(nameof(CourseLecture.CourseId), "Vui lòng chọn khóa học hợp lệ.");
            }

            if (!await _context.Teachers.AnyAsync(x => x.Id == lecture.TeacherId))
            {
                ModelState.AddModelError(nameof(CourseLecture.TeacherId), "Vui lòng chọn giáo viên hợp lệ.");
            }
        }
    }
}
