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
        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            const int pageSize = 10;
            keyword = keyword?.Trim();
            page = Math.Max(page, 1);
            var query = _context.CourseLectures.AsNoTracking()
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Title.Contains(keyword)
                    || x.Course.Code.Contains(keyword)
                    || x.Course.Name.Contains(keyword)
                    || x.Teacher.FullName.Contains(keyword)
                    || x.FileName.Contains(keyword)
                    || x.YouTubeUrl.Contains(keyword));
            }

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            page = Math.Min(page, totalPages);
            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            return View(await query.OrderByDescending(x => x.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync());
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
        public async Task<IActionResult> Create(int courseId, int teacherId, string? title, IFormFile? lectureFile, string? youTubeUrl)
        {
            var courseLecture = new CourseLecture
            {
                CourseId = courseId,
                TeacherId = teacherId,
                Title = title?.Trim() ?? string.Empty,
                YouTubeUrl = youTubeUrl?.Trim() ?? string.Empty,
                UploadedAt = DateTime.Now
            };

            await ValidateLectureSelectionAsync(courseLecture.CourseId, courseLecture.TeacherId);
            ValidateLectureTitle(courseLecture.Title);
            ValidateLectureSource(lectureFile, courseLecture.YouTubeUrl, required: true);

            if (ModelState.IsValid)
            {
                if (lectureFile is { Length: > 0 })
                {
                    courseLecture.FileName = Path.GetFileName(lectureFile.FileName).Trim();
                    courseLecture.FileUrl = await SaveLectureFileAsync(lectureFile, courseLecture.FileName);
                }
                else
                {
                    courseLecture.FileName = "YouTube";
                    courseLecture.FileUrl = string.Empty;
                }
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
            int id, int courseId, int teacherId, string? title, DateTime uploadedAt, IFormFile? lectureFile, string? youTubeUrl)
        {
            var courseLecture = await _context.CourseLectures.FindAsync(id);
            if (courseLecture is null)
            {
                return NotFound();
            }

            courseLecture.CourseId = courseId;
            courseLecture.TeacherId = teacherId;
            courseLecture.Title = title?.Trim() ?? string.Empty;
            courseLecture.YouTubeUrl = youTubeUrl?.Trim() ?? string.Empty;
            courseLecture.UploadedAt = uploadedAt == default ? courseLecture.UploadedAt : uploadedAt;

            await ValidateLectureSelectionAsync(courseLecture.CourseId, courseLecture.TeacherId);
            ValidateLectureTitle(courseLecture.Title);
            ValidateLectureSource(lectureFile, courseLecture.YouTubeUrl, required: string.IsNullOrWhiteSpace(courseLecture.FileUrl));

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
            int courseId, string? title, IFormFile? lectureFile, string? youTubeUrl)
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
            youTubeUrl = youTubeUrl?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title)
                || ((lectureFile is null || lectureFile.Length == 0) && string.IsNullOrWhiteSpace(youTubeUrl)))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tiêu đề và chọn tệp hoặc link YouTube.";
                return RedirectToAction(nameof(MyLectures));
            }

            if (title.Length > 200)
            {
                TempData["ErrorMessage"] = "Tiêu đề quá dài. Vui lòng rút gọn trước khi tải lên.";
                return RedirectToAction(nameof(MyLectures));
            }

            if (!string.IsNullOrWhiteSpace(youTubeUrl)
                && (!Uri.TryCreate(youTubeUrl, UriKind.Absolute, out var youtubeUri)
                    || !IsYouTubeHost(youtubeUri.Host)))
            {
                TempData["ErrorMessage"] = "Link YouTube không hợp lệ.";
                return RedirectToAction(nameof(MyLectures));
            }

            var originalFileName = string.Empty;
            var fileUrl = string.Empty;
            if (lectureFile is { Length: > 0 })
            {
                originalFileName = Path.GetFileName(lectureFile.FileName).Trim();
                var extension = Path.GetExtension(originalFileName);
                if (originalFileName.Length > 255)
                {
                    TempData["ErrorMessage"] = "Tên tệp quá dài. Vui lòng rút gọn trước khi tải lên.";
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

                fileUrl = await SaveLectureFileAsync(lectureFile, originalFileName);
            }

            _context.CourseLectures.Add(new CourseLecture
            {
                CourseId = courseId,
                TeacherId = teacherId,
                Title = title,
                FileName = string.IsNullOrWhiteSpace(originalFileName) ? "YouTube" : originalFileName,
                FileUrl = fileUrl,
                YouTubeUrl = youTubeUrl,
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

        private void ValidateLectureSource(IFormFile? lectureFile, string youTubeUrl, bool required)
        {
            if (lectureFile is null || lectureFile.Length == 0)
            {
                if (required && string.IsNullOrWhiteSpace(youTubeUrl))
                {
                    ModelState.AddModelError("lectureFile", "Vui lòng chọn tệp bài giảng hoặc nhập link YouTube.");
                }
                if (!string.IsNullOrWhiteSpace(youTubeUrl)
                    && (!Uri.TryCreate(youTubeUrl, UriKind.Absolute, out var emptyFileYoutubeUri)
                        || !IsYouTubeHost(emptyFileYoutubeUri.Host)))
                {
                    ModelState.AddModelError(nameof(CourseLecture.YouTubeUrl), "Link YouTube không hợp lệ.");
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

            if (!string.IsNullOrWhiteSpace(youTubeUrl)
                && (!Uri.TryCreate(youTubeUrl, UriKind.Absolute, out var uri)
                    || !IsYouTubeHost(uri.Host)))
            {
                ModelState.AddModelError(nameof(CourseLecture.YouTubeUrl), "Link YouTube không hợp lệ.");
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

        private static bool IsYouTubeHost(string host)
        {
            return host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".youtube.com", StringComparison.OrdinalIgnoreCase)
                || host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase);
        }
    }
}
