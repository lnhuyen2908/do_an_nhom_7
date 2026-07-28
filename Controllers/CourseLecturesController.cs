using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private const long MaxLectureFileSize = 20 * 1024 * 1024;
        private const string LectureUploadFolder = "uploads/lectures";
        private static readonly HashSet<string> AllowedLectureExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".zip"
        };

        private readonly EnglishCenterDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CourseLecturesController(EnglishCenterDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
        public async Task<IActionResult> Create(int courseId, int teacherId, string? title, IFormFile? lectureFile)
        {
            var courseLecture = new CourseLecture
            {
                CourseId = courseId,
                TeacherId = teacherId,
                Title = title?.Trim() ?? string.Empty,
                UploadedAt = DateTime.Now
            };

            await ValidateLectureSelectionAsync(courseLecture.CourseId, courseLecture.TeacherId);
            ValidateLectureTitle(courseLecture.Title);
            ValidateLectureFile(lectureFile, required: true);

            if (ModelState.IsValid)
            {
                courseLecture.FileName = Path.GetFileName(lectureFile!.FileName).Trim();
                courseLecture.FileUrl = await SaveLectureFileAsync(lectureFile, courseLecture.FileName);
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
        public async Task<IActionResult> Edit(
            int id, int courseId, int teacherId, string? title, DateTime uploadedAt, IFormFile? lectureFile)
        {
            var courseLecture = await _context.CourseLectures.FindAsync(id);
            if (courseLecture is null)
            {
                return NotFound();
            }

            courseLecture.CourseId = courseId;
            courseLecture.TeacherId = teacherId;
            courseLecture.Title = title?.Trim() ?? string.Empty;
            courseLecture.UploadedAt = uploadedAt == default ? courseLecture.UploadedAt : uploadedAt;

            await ValidateLectureSelectionAsync(courseLecture.CourseId, courseLecture.TeacherId);
            ValidateLectureTitle(courseLecture.Title);
            ValidateLectureFile(lectureFile, required: false);

            if (ModelState.IsValid)
            {
                try
                {
                    if (lectureFile is { Length: > 0 })
                    {
                        DeleteLectureFile(courseLecture.FileUrl);
                        courseLecture.FileName = Path.GetFileName(lectureFile.FileName).Trim();
                        courseLecture.FileUrl = await SaveLectureFileAsync(lectureFile, courseLecture.FileName);
                    }

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
                DeleteLectureFile(courseLecture.FileUrl);
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
            int courseId, string? title, IFormFile? lectureFile)
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
            if (string.IsNullOrWhiteSpace(title) || lectureFile is null || lectureFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tiêu đề và chọn tệp bài giảng.";
                return RedirectToAction(nameof(MyLectures));
            }

            var originalFileName = Path.GetFileName(lectureFile.FileName).Trim();
            var extension = Path.GetExtension(originalFileName);
            if (title.Length > 200 || originalFileName.Length > 255)
            {
                TempData["ErrorMessage"] = "Tiêu đề hoặc tên tệp quá dài. Vui lòng rút gọn trước khi tải lên.";
                return RedirectToAction(nameof(MyLectures));
            }

            if (!AllowedLectureExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Chỉ hỗ trợ các tệp PDF, Word, PowerPoint, Excel, TXT hoặc ZIP.";
                return RedirectToAction(nameof(MyLectures));
            }

            if (lectureFile.Length > MaxLectureFileSize)
            {
                TempData["ErrorMessage"] = "Tệp bài giảng không được vượt quá 20 MB.";
                return RedirectToAction(nameof(MyLectures));
            }

            var fileUrl = await SaveLectureFileAsync(lectureFile, originalFileName);
            _context.CourseLectures.Add(new CourseLecture
            {
                CourseId = courseId,
                TeacherId = teacherId,
                Title = title,
                FileName = originalFileName,
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
            DeleteLectureFile(lecture.FileUrl);
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

        private void ValidateLectureTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError(nameof(CourseLecture.Title), "Vui lòng nhập tiêu đề bài giảng.");
            }
            else if (title.Length > 200)
            {
                ModelState.AddModelError(nameof(CourseLecture.Title), "Tiêu đề bài giảng không được vượt quá 200 ký tự.");
            }
        }

        private void ValidateLectureFile(IFormFile? lectureFile, bool required)
        {
            if (lectureFile is null || lectureFile.Length == 0)
            {
                if (required)
                {
                    ModelState.AddModelError("lectureFile", "Vui lòng chọn tệp bài giảng.");
                }
                return;
            }

            var originalFileName = Path.GetFileName(lectureFile.FileName).Trim();
            var extension = Path.GetExtension(originalFileName);
            if (originalFileName.Length > 255)
            {
                ModelState.AddModelError("lectureFile", "Tên tệp không được vượt quá 255 ký tự.");
            }

            if (!AllowedLectureExtensions.Contains(extension))
            {
                ModelState.AddModelError("lectureFile", "Chỉ hỗ trợ các tệp PDF, Word, PowerPoint, Excel, TXT hoặc ZIP.");
            }

            if (lectureFile.Length > MaxLectureFileSize)
            {
                ModelState.AddModelError("lectureFile", "Tệp bài giảng không được vượt quá 20 MB.");
            }
        }

        private async Task ValidateLectureSelectionAsync(int courseId, int teacherId)
        {
            if (!await _context.Courses.AnyAsync(x => x.Id == courseId))
            {
                ModelState.AddModelError(nameof(CourseLecture.CourseId), "Vui lòng chọn khóa học hợp lệ.");
            }

            if (!await _context.Teachers.AnyAsync(x => x.Id == teacherId))
            {
                ModelState.AddModelError(nameof(CourseLecture.TeacherId), "Vui lòng chọn giáo viên hợp lệ.");
            }
        }

        private async Task<string> SaveLectureFileAsync(IFormFile file, string originalFileName)
        {
            var uploadRoot = Path.Combine(_environment.WebRootPath, LectureUploadFolder.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(uploadRoot);

            var extension = Path.GetExtension(originalFileName);
            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadRoot, safeFileName);
            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            return $"/{LectureUploadFolder}/{safeFileName}";
        }

        private void DeleteLectureFile(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)
                || !fileUrl.StartsWith($"/{LectureUploadFolder}/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fileName = Path.GetFileName(fileUrl);
            var uploadRoot = Path.Combine(_environment.WebRootPath, LectureUploadFolder.Replace('/', Path.DirectorySeparatorChar));
            var filePath = Path.GetFullPath(Path.Combine(uploadRoot, fileName));
            var safeRoot = Path.GetFullPath(uploadRoot);
            if (!filePath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase)
                || !System.IO.File.Exists(filePath))
            {
                return;
            }

            System.IO.File.Delete(filePath);
        }
    }
}
