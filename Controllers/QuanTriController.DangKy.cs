using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public partial class QuanTriController
    {
        public IActionResult DangKy()
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;
            ViewBag.Students = Db.Students.AsNoTracking().ToList();
            ViewBag.Courses = Db.Courses.AsNoTracking().ToList();
            ViewBag.Classes = Db.Classes.AsNoTracking().ToList();
            var approvedClassIds = Db.Enrollments
                .AsNoTracking()
                .Where(x => x.ClassId.HasValue && x.Status == EnglishCenterStore.EnrollmentApproved)
                .Select(x => x.ClassId)
                .ToList();
            ViewBag.ClassSeats = approvedClassIds
                .Where(x => x.HasValue)
                .GroupBy(x => x!.Value)
                .ToDictionary(x => x.Key, x => x.Count());
            return View(Db.Enrollments.AsNoTracking().OrderByDescending(x => x.RegisteredAt).ToList());
        }

        [HttpPost]
        public IActionResult CapNhatDangKy(int id, string status, int? classId)
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;
            status = status?.Trim() ?? string.Empty;
            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var enrollment = Db.Enrollments.FirstOrDefault(x => x.Id == id);
            if (enrollment == null) return NotFound();

            var allowedStatuses = new[]
            {
                EnglishCenterStore.EnrollmentPending,
                EnglishCenterStore.EnrollmentApproved,
                EnglishCenterStore.EnrollmentCanceled
            };
            if (!allowedStatuses.Contains(status))
            {
                TempData["Message"] = "Trạng thái đăng ký không hợp lệ.";
                return RedirectToAction(nameof(DangKy));
            }

            if (status == EnglishCenterStore.EnrollmentApproved)
            {
                if (!classId.HasValue)
                {
                    TempData["Message"] = "Vui lòng chọn lớp trước khi duyệt đăng ký.";
                    return RedirectToAction(nameof(DangKy));
                }

                var courseClass = Db.Classes.AsNoTracking().FirstOrDefault(x => x.Id == classId.Value);
                if (courseClass == null || courseClass.CourseId != enrollment.CourseId)
                {
                    TempData["Message"] = "Lớp được chọn không thuộc khóa học của đăng ký này.";
                    return RedirectToAction(nameof(DangKy));
                }

                var currentCount = Db.Enrollments.Count(x =>
                    x.Id != id
                    && x.ClassId == classId.Value
                    && x.Status == EnglishCenterStore.EnrollmentApproved);
                if (currentCount >= courseClass.Capacity)
                {
                    TempData["Message"] = $"Lớp {courseClass.Code} đã đủ sĩ số.";
                    return RedirectToAction(nameof(DangKy));
                }
            }

            enrollment.Status = status;
            enrollment.ClassId = status == EnglishCenterStore.EnrollmentApproved ? classId : null;
            Db.SaveChanges();
            transaction.Commit();
            TempData["Message"] = "Đã cập nhật trạng thái đăng ký.";
            return RedirectToAction(nameof(DangKy));
        }
    }
}
