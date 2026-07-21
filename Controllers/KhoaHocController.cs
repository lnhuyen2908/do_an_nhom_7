using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using web_do_an1.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public class KhoaHocController : CoSoController
    {
        public KhoaHocController(EnglishCenterDbContext db) : base(db)
        {
        }

        public IActionResult DanhSach(string? keyword, string? level, int page = 1)
        {
            const int pageSize = EnglishCenterStore.DefaultPageSize;
            keyword = keyword?.Trim();
            level = level?.Trim();
            var query = Db.Courses.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword) || x.Code.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(x => x.Level == level);
            }

            var totalItems = query.Count();
            page = EnglishCenterStore.NormalizePage(page, totalItems, pageSize);
            var courses = query
                .OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.Level = level;
            ViewBag.Levels = Db.Courses.AsNoTracking().Select(x => x.Level).Distinct().OrderBy(x => x).ToList();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = EnglishCenterStore.TotalPages(totalItems, pageSize);
            return View(courses);
        }

        public IActionResult ChiTiet(int id)
        {
            var course = Db.Courses.AsNoTracking().FirstOrDefault(x => x.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            var classes = Db.Classes.AsNoTracking().Where(x => x.CourseId == id).OrderBy(x => x.StartDate).ToList();
            var classIds = classes.Select(x => x.Id).ToList();
            ViewBag.Classes = classes;
            var approvedClassIds = Db.Enrollments
                .AsNoTracking()
                .Where(x => x.ClassId.HasValue && classIds.Contains(x.ClassId.Value) && x.Status == EnglishCenterStore.EnrollmentApproved)
                .Select(x => x.ClassId)
                .ToList();
            ViewBag.ClassSeats = approvedClassIds
                .Where(x => x.HasValue)
                .GroupBy(x => x!.Value)
                .ToDictionary(x => x.Key, x => x.Count());
            var teacherIds = classes.Select(x => x.TeacherId).Distinct().ToList();
            ViewBag.Teachers = Db.Teachers.AsNoTracking().Where(x => teacherIds.Contains(x.Id)).ToList();
            var studentId = CurrentStudent?.Id;
            ViewBag.IsSaved = studentId.HasValue
                && Db.SavedCourses.AsNoTracking().Any(x => x.StudentId == studentId.Value && x.CourseId == id);
            return View(course);
        }

        public IActionResult LichKhaiGiang(string? keyword, string? level, string? mode, int page = 1)
        {
            const int pageSize = EnglishCenterStore.DefaultPageSize;
            keyword = keyword?.Trim();
            level = level?.Trim();
            mode = mode?.Trim();
            var classes = Db.Classes.AsNoTracking();
            var teacherId = CurrentUser?.Role == "Teacher" ? CurrentTeacher?.Id : null;

            if (teacherId.HasValue)
            {
                classes = classes.Where(x => x.TeacherId == teacherId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword) || !string.IsNullOrWhiteSpace(level))
            {
                var courseIds = Db.Courses.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    courseIds = courseIds.Where(x => x.Name.Contains(keyword) || x.Code.Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(level))
                {
                    courseIds = courseIds.Where(x => x.Level == level);
                }

                classes = classes.Where(x => courseIds.Select(course => course.Id).Contains(x.CourseId));
            }

            if (!string.IsNullOrWhiteSpace(mode))
            {
                classes = mode == "Online"
                    ? classes.Where(x => x.Room.Contains("Online"))
                    : classes.Where(x => !x.Room.Contains("Online"));
            }

            var totalItems = classes.Count();
            page = EnglishCenterStore.NormalizePage(page, totalItems, pageSize);
            var pageClasses = classes
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.Level = level;
            ViewBag.Mode = mode;
            ViewBag.IsTeacherSchedule = teacherId.HasValue;
            var pageCourseIds = pageClasses.Select(x => x.CourseId).Distinct().ToList();
            var pageTeacherIds = pageClasses.Select(x => x.TeacherId).Distinct().ToList();
            ViewBag.Courses = Db.Courses.AsNoTracking().Where(x => pageCourseIds.Contains(x.Id)).ToList();
            ViewBag.Teachers = Db.Teachers.AsNoTracking().Where(x => pageTeacherIds.Contains(x.Id)).ToList();
            ViewBag.Levels = Db.Courses.AsNoTracking().Select(x => x.Level).Distinct().OrderBy(x => x).ToList();
            ViewBag.Page = page;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = EnglishCenterStore.TotalPages(totalItems, pageSize);
            return View(pageClasses);
        }

        [HttpPost]
        public IActionResult DangKyKhoaHoc(int courseId)
        {
            var auth = RequireRole("Student");
            if (auth != null)
            {
                return auth;
            }

            var student = CurrentStudent;
            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var course = Db.Courses.FirstOrDefault(x => x.Id == courseId);
            if (student == null || course == null)
            {
                return NotFound();
            }

            var existing = Db.Enrollments.FirstOrDefault(x =>
                x.StudentId == student.Id && x.CourseId == courseId && x.Status != EnglishCenterStore.EnrollmentCanceled);
            if (existing != null)
            {
                TempData["Message"] = "Bạn đã đăng ký khóa học này.";
                return RedirectToAction(nameof(ChiTiet), new { id = courseId });
            }

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = courseId,
                Status = EnglishCenterStore.EnrollmentPending,
                RegisteredAt = DateTime.Now
            };

            Db.Enrollments.Add(enrollment);
            Db.SaveChanges();

            var payment = new Payment
            {
                StudentId = student.Id,
                EnrollmentId = enrollment.Id,
                Amount = course.Tuition,
                PaidAmount = 0,
                Status = EnglishCenterStore.PaymentUnpaid,
                PaymentMethod = EnglishCenterStore.PaymentMethodCash
            };
            Db.Payments.Add(payment);
            Db.SaveChanges();
            transaction.Commit();

            TempData["Message"] = "Đăng ký thành công. Bạn có thể thanh toán học phí ngay bây giờ.";
            return RedirectToAction("ThanhToan", "HocVien", new { id = payment.Id });
        }

        [HttpPost]
        public IActionResult LuuKhoaHoc(int courseId)
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var student = CurrentStudent;
            if (student == null || !Db.Courses.AsNoTracking().Any(x => x.Id == courseId))
            {
                return NotFound();
            }

            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var exists = Db.SavedCourses.Any(x => x.StudentId == student.Id && x.CourseId == courseId);
            if (!exists)
            {
                Db.SavedCourses.Add(new SavedCourse { StudentId = student.Id, CourseId = courseId, SavedAt = DateTime.Now });
                Db.SaveChanges();
            }

            transaction.Commit();

            TempData["Message"] = exists ? "Khóa học này đã có trong danh sách đã lưu." : "Đã lưu thành công.";
            return RedirectToAction(nameof(ChiTiet), new { id = courseId });
        }
    }
}
