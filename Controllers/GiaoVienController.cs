using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IO.Compression;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    public class GiaoVienController : CoSoController
    {
        private readonly IWebHostEnvironment _environment;

        public GiaoVienController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult LopHoc()
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacher = CurrentTeacher;
            var teacherId = teacher?.Id ?? 0;
            var classes = Db.Classes.AsNoTracking().Where(x => x.TeacherId == teacherId).OrderBy(x => x.StartDate).ToList();
            var courseIds = classes.Select(x => x.CourseId).Distinct().ToList();
            var classIds = classes.Select(x => x.Id).ToList();
            ViewBag.Courses = Db.Courses.AsNoTracking().Where(x => courseIds.Contains(x.Id)).ToList();
            ViewBag.Enrollments = Db.Enrollments.AsNoTracking()
                .Where(x => x.ClassId.HasValue && classIds.Contains(x.ClassId.Value))
                .ToList();
            return View(classes);
        }

        public IActionResult ChiTietLop(int id)
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacher = CurrentTeacher;
            var teacherId = teacher?.Id ?? 0;
            var courseClass = Db.Classes.AsNoTracking().FirstOrDefault(x => x.Id == id && x.TeacherId == teacherId);
            if (courseClass == null) return NotFound();

            var enrollments = Db.Enrollments.AsNoTracking()
                .Where(x => x.ClassId == id && x.Status == EnglishCenterStore.EnrollmentApproved)
                .ToList();
            var studentIds = enrollments.Select(x => x.StudentId).ToList();
            ViewBag.Class = courseClass;
            ViewBag.Course = Db.Courses.AsNoTracking().FirstOrDefault(x => x.Id == courseClass.CourseId);
            ViewBag.Enrollments = enrollments;
            ViewBag.Students = Db.Students.AsNoTracking().Where(x => studentIds.Contains(x.Id)).ToList();
            ViewBag.Scores = Db.Scores.AsNoTracking().Where(x => x.ClassId == id).ToList();
            ViewBag.Attendance = Db.Attendance.AsNoTracking().Where(x => x.ClassId == id).ToList();
            return View();
        }

        public IActionResult BaiGiang()
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacherId = CurrentTeacher?.Id ?? 0;
            var courseIds = Db.Classes.AsNoTracking()
                .Where(x => x.TeacherId == teacherId)
                .Select(x => x.CourseId)
                .Distinct()
                .ToList();

            ViewBag.Courses = Db.Courses.AsNoTracking()
                .Where(x => courseIds.Contains(x.Id))
                .OrderBy(x => x.Code)
                .ToList();

            return View(Db.Lectures.AsNoTracking()
                .Where(x => x.TeacherId == teacherId)
                .OrderByDescending(x => x.UploadedAt)
                .ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(LectureFileStorage.MaxFileSize + 1024 * 1024)]
        public IActionResult TaiBaiGiang(int courseId, string title, IFormFile? file)
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacherId = CurrentTeacher?.Id ?? 0;
            var canManage = Db.Classes.AsNoTracking().Any(x => x.TeacherId == teacherId && x.CourseId == courseId);
            if (!canManage) return NotFound();

            title = title?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title) || file == null || file.Length == 0)
            {
                TempData["Message"] = "Vui lòng nhập tiêu đề và chọn file bài giảng.";
                return RedirectToAction(nameof(BaiGiang));
            }

            if (title.Length > 150)
            {
                TempData["Message"] = "Tiêu đề bài giảng không được vượt quá 150 ký tự.";
                return RedirectToAction(nameof(BaiGiang));
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!LectureFileStorage.IsAllowedExtension(ext))
            {
                TempData["Message"] = "Chỉ hỗ trợ file PDF, DOCX hoặc PPTX.";
                return RedirectToAction(nameof(BaiGiang));
            }

            if (file.Length > LectureFileStorage.MaxFileSize)
            {
                TempData["Message"] = "File bài giảng không được vượt quá 10 MB.";
                return RedirectToAction(nameof(BaiGiang));
            }

            if (!HasValidFileSignature(file, ext))
            {
                TempData["Message"] = "Nội dung file không đúng với định dạng đã chọn.";
                return RedirectToAction(nameof(BaiGiang));
            }

            var storedName = $"{Guid.NewGuid():N}{ext}";
            var storedPath = LectureFileStorage.CreatePrivatePath(_environment.ContentRootPath, storedName);
            var originalName = Path.GetFileName(file.FileName).Trim();
            if (originalName.Length > 255)
            {
                originalName = originalName[..255];
            }
            using (var stream = System.IO.File.Create(storedPath))
            {
                file.CopyTo(stream);
            }

            try
            {
                Db.Lectures.Add(new CourseLecture
                {
                    CourseId = courseId,
                    TeacherId = teacherId,
                    Title = title,
                    FileName = originalName,
                    FileUrl = storedName,
                    UploadedAt = DateTime.Now
                });
                Db.SaveChanges();
            }
            catch
            {
                System.IO.File.Delete(storedPath);
                throw;
            }

            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(BaiGiang));
        }

        public IActionResult TaiXuongBaiGiang(int id)
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var lecture = Db.Lectures.AsNoTracking().FirstOrDefault(x => x.Id == id);
            if (lecture == null)
            {
                return NotFound();
            }

            var canDownload = CurrentUser.Role switch
            {
                EnglishCenterStore.RoleTeacher => lecture.TeacherId == CurrentTeacher?.Id,
                EnglishCenterStore.RoleStudent => CurrentStudent != null && Db.Enrollments.AsNoTracking().Any(x =>
                    x.StudentId == CurrentStudent.Id
                    && x.CourseId == lecture.CourseId
                    && x.Status == EnglishCenterStore.EnrollmentApproved),
                EnglishCenterStore.RoleAdmin or EnglishCenterStore.RoleStaff => true,
                _ => false
            };

            if (!canDownload)
            {
                return NotFound();
            }

            var filePath = LectureFileStorage.ResolveExistingPath(_environment.ContentRootPath, lecture.FileUrl);
            if (filePath == null)
            {
                return NotFound();
            }

            Response.Headers["Cache-Control"] = "private, no-store";
            return PhysicalFile(
                filePath,
                LectureFileStorage.GetContentType(filePath),
                Path.GetFileName(lecture.FileName),
                enableRangeProcessing: true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaBaiGiang(int id)
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacherId = CurrentTeacher?.Id ?? 0;
            var lecture = Db.Lectures.FirstOrDefault(x => x.Id == id && x.TeacherId == teacherId);
            if (lecture == null) return NotFound();

            var fileReference = lecture.FileUrl;
            Db.Lectures.Remove(lecture);
            Db.SaveChanges();
            LectureFileStorage.DeleteIfExists(_environment.ContentRootPath, fileReference);
            TempData["Message"] = "Đã xóa bài giảng.";
            return RedirectToAction(nameof(BaiGiang));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LuuDiem(int classId, int studentId, double midterm, double final, string comment)
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacherId = CurrentTeacher?.Id ?? 0;
            var canManage = Db.Classes.AsNoTracking().Any(x => x.Id == classId && x.TeacherId == teacherId)
                && Db.Enrollments.AsNoTracking().Any(x =>
                    x.ClassId == classId
                    && x.StudentId == studentId
                    && x.Status == EnglishCenterStore.EnrollmentApproved);
            if (!canManage)
            {
                return NotFound();
            }

            if (midterm < 0 || midterm > 10 || final < 0 || final > 10)
            {
                TempData["Message"] = "Điểm giữa kỳ và cuối kỳ phải nằm trong khoảng 0 - 10.";
                return RedirectToAction(nameof(ChiTietLop), new { id = classId });
            }

            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var score = Db.Scores.FirstOrDefault(x => x.ClassId == classId && x.StudentId == studentId);
            if (score == null)
            {
                score = new Score { ClassId = classId, StudentId = studentId };
                Db.Scores.Add(score);
            }

            score.Midterm = midterm;
            score.Final = final;
            score.Comment = (comment ?? string.Empty).Trim();
            if (score.Comment.Length > 500)
            {
                TempData["Message"] = "Nhận xét không được vượt quá 500 ký tự.";
                return RedirectToAction(nameof(ChiTietLop), new { id = classId });
            }
            Db.SaveChanges();
            transaction.Commit();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(ChiTietLop), new { id = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LuuDiemDanh(int classId, int studentId, bool isPresent, string note)
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacherId = CurrentTeacher?.Id ?? 0;
            var canManage = Db.Classes.AsNoTracking().Any(x => x.Id == classId && x.TeacherId == teacherId)
                && Db.Enrollments.AsNoTracking().Any(x =>
                    x.ClassId == classId
                    && x.StudentId == studentId
                    && x.Status == EnglishCenterStore.EnrollmentApproved);

            if (!canManage)
            {
                return NotFound();
            }

            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var today = DateTime.Today;
            var attendance = Db.Attendance.FirstOrDefault(x =>
                x.ClassId == classId
                && x.StudentId == studentId
                && x.StudyDate == today);
            if (attendance == null)
            {
                attendance = new AttendanceRecord { ClassId = classId, StudentId = studentId, StudyDate = today };
                Db.Attendance.Add(attendance);
            }

            attendance.IsPresent = isPresent;
            attendance.Note = (note ?? string.Empty).Trim();
            if (attendance.Note.Length > 500)
            {
                TempData["Message"] = "Ghi chú điểm danh không được vượt quá 500 ký tự.";
                return RedirectToAction(nameof(ChiTietLop), new { id = classId });
            }
            Db.SaveChanges();
            transaction.Commit();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(ChiTietLop), new { id = classId });
        }

        public IActionResult HoSo()
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacher = CurrentTeacher;
            if (teacher == null) return NotFound();

            ViewBag.User = CurrentUser;
            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HoSo([Bind("FullName,Email,Phone,Specialty")] Teacher model)
        {
            var auth = RequireRole("Teacher");
            if (auth != null) return auth;

            var teacher = CurrentTeacher;
            if (teacher == null) return NotFound();

            model.Code = teacher.Code;
            model.FullName = model.FullName?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim() ?? string.Empty;
            model.Phone = model.Phone?.Trim() ?? string.Empty;
            model.Specialty = model.Specialty?.Trim() ?? string.Empty;
            ModelState.Clear();
            TryValidateModel(model);
            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(HoSo));
            }

            teacher.FullName = model.FullName;
            teacher.Email = model.Email;
            teacher.Phone = model.Phone;
            teacher.Specialty = model.Specialty;

            var user = CurrentUser;
            if (user != null)
            {
                user.FullName = teacher.FullName;
                user.Email = teacher.Email;
                user.Phone = teacher.Phone;
            }

            Db.SaveChanges();
            TempData["Message"] = "Đã cập nhật thông tin giảng viên.";
            return RedirectToAction(nameof(HoSo));
        }

        private static bool HasValidFileSignature(IFormFile file, string extension)
        {
            Span<byte> header = stackalloc byte[8];
            using var stream = file.OpenReadStream();
            var bytesRead = stream.Read(header);
            if (bytesRead < 4)
            {
                return false;
            }

            return extension switch
            {
                ".pdf" => bytesRead >= 5 && header[..5].SequenceEqual(
                    new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }),
                ".docx" => header[..4].SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 })
                    && HasExpectedOfficeStructure(file, "word/"),
                ".pptx" => header[..4].SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 })
                    && HasExpectedOfficeStructure(file, "ppt/"),
                _ => false
            };
        }

        private static bool HasExpectedOfficeStructure(IFormFile file, string contentFolder)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                return archive.GetEntry("[Content_Types].xml") != null
                    && archive.Entries.Any(entry =>
                        entry.FullName.StartsWith(contentFolder, StringComparison.OrdinalIgnoreCase));
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }
    }
}



