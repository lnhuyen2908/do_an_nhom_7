using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public partial class QuanTriController
    {
        public IActionResult KhoaHoc(string? keyword, int page = 1)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            const int pageSize = EnglishCenterStore.DefaultPageSize;
            keyword = keyword?.Trim();
            var courses = Db.Courses.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                courses = courses.Where(x => x.Name.Contains(keyword) || x.Code.Contains(keyword));
            }

            var totalItems = courses.Count();
            var totalPages = EnglishCenterStore.TotalPages(totalItems, pageSize);
            page = EnglishCenterStore.NormalizePage(page, totalItems, pageSize);

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            return View(courses.OrderBy(x => x.Code).Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        [HttpPost]
        public IActionResult LuuKhoaHoc([Bind("Id,Code,Name,Level,Tuition,Duration,Description,ImageUrl")] Course course)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            course.Code = course.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            course.Name = course.Name?.Trim() ?? string.Empty;
            course.Level = course.Level?.Trim() ?? string.Empty;
            course.Duration = course.Duration?.Trim() ?? string.Empty;
            course.Description = course.Description?.Trim() ?? string.Empty;
            course.ImageUrl = course.ImageUrl?.Trim() ?? string.Empty;
            ModelState.Clear();
            TryValidateModel(course);

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(KhoaHoc));
            }

            if (!string.IsNullOrEmpty(course.ImageUrl)
                && !EnglishCenterStore.CourseImageOptions.Contains(course.ImageUrl, StringComparer.Ordinal))
            {
                TempData["Message"] = "Ảnh khóa học được chọn không hợp lệ.";
                return RedirectToAction(nameof(KhoaHoc));
            }

            if (Db.Courses.AsNoTracking().Any(x => x.Id != course.Id && x.Code == course.Code))
            {
                TempData["Message"] = $"Mã khóa học {course.Code} đã tồn tại.";
                return RedirectToAction(nameof(KhoaHoc));
            }

            if (course.Id == 0)
            {
                Db.Courses.Add(course);
            }
            else
            {
                var current = Db.Courses.FirstOrDefault(x => x.Id == course.Id);
                if (current == null) return NotFound();
                current.Code = course.Code;
                current.Name = course.Name;
                current.Level = course.Level;
                current.Tuition = course.Tuition;
                current.Duration = course.Duration;
                current.Description = course.Description;
                current.ImageUrl = course.ImageUrl;
            }

            Db.SaveChanges();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(KhoaHoc));
        }

        [HttpPost]
        public IActionResult XoaKhoaHoc(int id)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;
            var course = Db.Courses.FirstOrDefault(x => x.Id == id);
            var lectureFiles = new List<string>();
            if (course != null)
            {
                var classIds = Db.Classes.Where(x => x.CourseId == id).Select(x => x.Id).ToList();
                var enrollmentIds = Db.Enrollments.Where(x => x.CourseId == id).Select(x => x.Id).ToList();
                var paymentIds = Db.Payments.Where(x => enrollmentIds.Contains(x.EnrollmentId)).Select(x => x.Id).ToList();
                Db.Attendance.RemoveRange(Db.Attendance.Where(x => classIds.Contains(x.ClassId)));
                Db.Scores.RemoveRange(Db.Scores.Where(x => classIds.Contains(x.ClassId)));
                Db.PaymentTransactions.RemoveRange(Db.PaymentTransactions.Where(x => paymentIds.Contains(x.PaymentId)));
                Db.Payments.RemoveRange(Db.Payments.Where(x => enrollmentIds.Contains(x.EnrollmentId)));
                Db.Enrollments.RemoveRange(Db.Enrollments.Where(x => enrollmentIds.Contains(x.Id)));
                Db.Classes.RemoveRange(Db.Classes.Where(x => classIds.Contains(x.Id)));
                var lectures = Db.Lectures.Where(x => x.CourseId == id).ToList();
                lectureFiles.AddRange(lectures.Select(x => x.FileUrl));
                Db.Lectures.RemoveRange(lectures);
                Db.SavedCourses.RemoveRange(Db.SavedCourses.Where(x => x.CourseId == id));
                Db.Courses.Remove(course);
            }
            Db.SaveChanges();
            foreach (var fileReference in lectureFiles)
            {
                LectureFileStorage.DeleteIfExists(_environment.ContentRootPath, fileReference);
            }
            TempData["Message"] = "Đã xóa khóa học và tải lại danh sách mới.";
            return RedirectToAction(nameof(KhoaHoc));
        }
    }
}
