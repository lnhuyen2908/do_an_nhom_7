using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using web_do_an1.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public class HocVienController : CoSoController
    {
        public HocVienController(EnglishCenterDbContext db) : base(db)
        {
        }

        public IActionResult TongQuan()
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var student = CurrentStudent;
            var studentId = student?.Id ?? 0;
            var enrollments = Db.Enrollments.AsNoTracking()
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.RegisteredAt)
                .ToList();
            var courseIds = enrollments.Select(x => x.CourseId).Distinct().ToList();
            var classIds = enrollments.Where(x => x.ClassId.HasValue).Select(x => x.ClassId!.Value).ToList();
            ViewBag.Courses = Db.Courses.AsNoTracking().Where(x => courseIds.Contains(x.Id)).ToList();
            ViewBag.Classes = Db.Classes.AsNoTracking().Where(x => classIds.Contains(x.Id)).ToList();
            ViewBag.Payments = Db.Payments.AsNoTracking().Where(x => x.StudentId == studentId).ToList();
            return View(enrollments);
        }

        public IActionResult LichHoc()
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var student = CurrentStudent;
            var studentId = student?.Id ?? 0;
            var classIds = Db.Enrollments
                .AsNoTracking()
                .Where(x => x.StudentId == studentId && x.Status == EnglishCenterStore.EnrollmentApproved && x.ClassId.HasValue)
                .Select(x => x.ClassId!.Value)
                .ToHashSet();
            var classes = Db.Classes.AsNoTracking().Where(x => classIds.Contains(x.Id)).ToList();
            var courseIds = classes.Select(x => x.CourseId).Distinct().ToList();
            var teacherIds = classes.Select(x => x.TeacherId).Distinct().ToList();
            ViewBag.Courses = Db.Courses.AsNoTracking().Where(x => courseIds.Contains(x.Id)).ToList();
            ViewBag.Teachers = Db.Teachers.AsNoTracking().Where(x => teacherIds.Contains(x.Id)).ToList();
            return View(classes);
        }

        public IActionResult DiemSo()
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var student = CurrentStudent;
            var studentId = student?.Id ?? 0;
            var scores = Db.Scores.AsNoTracking().Where(x => x.StudentId == studentId).ToList();
            var classIds = scores.Select(x => x.ClassId).Distinct().ToList();
            var classes = Db.Classes.AsNoTracking().Where(x => classIds.Contains(x.Id)).ToList();
            var courseIds = classes.Select(x => x.CourseId).Distinct().ToList();
            ViewBag.Classes = classes;
            ViewBag.Courses = Db.Courses.AsNoTracking().Where(x => courseIds.Contains(x.Id)).ToList();
            return View(scores);
        }

        public IActionResult HocPhi()
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var student = CurrentStudent;
            var studentId = student?.Id ?? 0;
            var enrollments = Db.Enrollments.AsNoTracking().Where(x => x.StudentId == studentId).ToList();
            var courseIds = enrollments.Select(x => x.CourseId).Distinct().ToList();
            ViewBag.Enrollments = enrollments;
            ViewBag.Courses = Db.Courses.AsNoTracking().Where(x => courseIds.Contains(x.Id)).ToList();
            ViewBag.PaymentTransactions = Db.PaymentTransactions.AsNoTracking()
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.PaidAt)
                .ToList();
            return View(Db.Payments.AsNoTracking().Where(x => x.StudentId == studentId).ToList());
        }

        public IActionResult BaiGiang()
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var studentId = CurrentStudent?.Id ?? 0;
            var courseIds = Db.Enrollments.AsNoTracking()
                .Where(x => x.StudentId == studentId && x.Status == EnglishCenterStore.EnrollmentApproved)
                .Select(x => x.CourseId)
                .Distinct()
                .ToList();

            ViewBag.Courses = Db.Courses.AsNoTracking().Where(x => courseIds.Contains(x.Id)).ToList();
            ViewBag.Teachers = Db.Teachers.AsNoTracking().ToList();
            return View(Db.Lectures.AsNoTracking()
                .Where(x => courseIds.Contains(x.CourseId))
                .OrderByDescending(x => x.UploadedAt)
                .ToList());
        }

        public IActionResult KhoaHocDaLuu()
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var studentId = CurrentStudent?.Id ?? 0;
            var savedCourses = Db.SavedCourses.AsNoTracking()
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.SavedAt)
                .ToList();
            var courseIds = savedCourses.Select(x => x.CourseId).ToList();
            ViewBag.Courses = Db.Courses.AsNoTracking().Where(x => courseIds.Contains(x.Id)).ToList();
            return View(savedCourses);
        }

        [HttpPost]
        public IActionResult BoLuuKhoaHoc(int id)
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var studentId = CurrentStudent?.Id ?? 0;
            var saved = Db.SavedCourses.FirstOrDefault(x => x.Id == id && x.StudentId == studentId);
            if (saved == null) return NotFound();

            Db.SavedCourses.Remove(saved);
            Db.SaveChanges();
            TempData["Message"] = "Đã bỏ lưu khóa học.";
            return RedirectToAction(nameof(KhoaHocDaLuu));
        }

        public IActionResult HoSo()
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var student = CurrentStudent;
            if (student == null) return NotFound();

            ViewBag.User = CurrentUser;
            return View(student);
        }

        [HttpPost]
        public IActionResult HoSo([Bind("FullName,Email,Phone,DateOfBirth,Address")] Student model)
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var student = CurrentStudent;
            if (student == null) return NotFound();

            model.Code = student.Code;
            model.FullName = model.FullName?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim() ?? string.Empty;
            model.Phone = model.Phone?.Trim() ?? string.Empty;
            model.Address = model.Address?.Trim() ?? string.Empty;
            ModelState.Clear();
            TryValidateModel(model);
            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(HoSo));
            }

            student.FullName = model.FullName;
            student.Email = model.Email;
            student.Phone = model.Phone;
            student.DateOfBirth = model.DateOfBirth;
            student.Address = model.Address;

            var user = CurrentUser;
            if (user != null)
            {
                user.FullName = student.FullName;
                user.Email = student.Email;
                user.Phone = student.Phone;
            }

            Db.SaveChanges();
            TempData["Message"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(HoSo));
        }

        public IActionResult ThanhToan(int id)
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            var student = CurrentStudent;
            var studentId = student?.Id ?? 0;
            var payment = Db.Payments.AsNoTracking().FirstOrDefault(x => x.Id == id && x.StudentId == studentId);
            if (payment == null) return NotFound();

            var enrollment = Db.Enrollments.AsNoTracking().FirstOrDefault(x => x.Id == payment.EnrollmentId);
            ViewBag.Enrollment = enrollment;
            ViewBag.Course = enrollment == null ? null : Db.Courses.AsNoTracking().FirstOrDefault(x => x.Id == enrollment.CourseId);
            ViewBag.PaymentTransactions = Db.PaymentTransactions.AsNoTracking()
                .Where(x => x.PaymentId == payment.Id)
                .OrderByDescending(x => x.PaidAt)
                .ToList();
            return View(payment);
        }

        [HttpPost]
        public IActionResult XacNhanThanhToan(int id, decimal paidAmount, string paymentMethod)
        {
            var auth = RequireRole("Student");
            if (auth != null) return auth;

            paymentMethod = paymentMethod?.Trim() ?? string.Empty;
            var student = CurrentStudent;
            var studentId = student?.Id ?? 0;
            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var payment = Db.Payments.FirstOrDefault(x => x.Id == id && x.StudentId == studentId);
            if (payment == null) return NotFound();

            var remaining = payment.Amount - payment.PaidAmount;
            if (remaining <= 0)
            {
                TempData["Message"] = "Hóa đơn này đã được thanh toán đủ.";
                return RedirectToAction(nameof(HocPhi));
            }

            if (paidAmount <= 0)
            {
                TempData["Message"] = "Số tiền thanh toán phải lớn hơn 0.";
                return RedirectToAction(nameof(ThanhToan), new { id });
            }

            if (paidAmount > remaining)
            {
                TempData["Message"] = $"Số tiền thanh toán không được vượt quá {remaining:N0} đồng.";
                return RedirectToAction(nameof(ThanhToan), new { id });
            }

            if (!EnglishCenterStore.IsValidPaymentMethod(paymentMethod))
            {
                TempData["Message"] = "Vui lòng chọn phương thức thanh toán hợp lệ.";
                return RedirectToAction(nameof(ThanhToan), new { id });
            }

            payment.PaidAmount += paidAmount;
            payment.PaidDate = payment.PaidAmount > 0 ? DateTime.Today : null;
            payment.Status = EnglishCenterStore.PaymentStatus(payment.PaidAmount, payment.Amount);
            payment.PaymentMethod = paymentMethod;
            Db.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.Id,
                StudentId = studentId,
                Amount = paidAmount,
                PaymentMethod = paymentMethod,
                PaidAt = DateTime.Now,
                RecordedBy = CurrentUser?.FullName ?? student?.FullName ?? "Học viên",
                Note = "Học viên thanh toán học phí"
            });

            Db.SaveChanges();
            transaction.Commit();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(HocPhi));
        }
    }
}



