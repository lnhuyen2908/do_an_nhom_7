using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public partial class QuanTriController
    {
        public IActionResult HocVien(int page = 1)
        {
            var auth = RequireRole("Staff");
            if (auth != null) return auth;
            const int pageSize = EnglishCenterStore.DefaultPageSize;
            var query = Db.Students.AsNoTracking();
            var totalItems = query.Count();
            var totalPages = EnglishCenterStore.TotalPages(totalItems, pageSize);
            page = EnglishCenterStore.NormalizePage(page, totalItems, pageSize);
            var students = query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var studentIds = students.Select(x => x.Id).ToList();
            ViewBag.Enrollments = Db.Enrollments.AsNoTracking().Where(x => studentIds.Contains(x.StudentId)).ToList();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            return View(students);
        }

        [HttpPost]
        public IActionResult LuuHocVien([Bind("Id,Code,FullName,Email,Phone,DateOfBirth,Address")] Student student)
        {
            var auth = RequireRole("Staff");
            if (auth != null) return auth;

            student.Code = student.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            student.FullName = student.FullName?.Trim() ?? string.Empty;
            student.Email = student.Email?.Trim() ?? string.Empty;
            student.Phone = student.Phone?.Trim() ?? string.Empty;
            student.Address = student.Address?.Trim() ?? string.Empty;

            ModelState.Clear();
            TryValidateModel(student);
            if (string.IsNullOrWhiteSpace(student.Code))
            {
                ModelState.AddModelError(nameof(student.Code), "Vui lòng nhập mã học viên.");
            }

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(HocVien));
            }

            if (Db.Students.AsNoTracking().Any(x => x.Id != student.Id && x.Code == student.Code))
            {
                TempData["Message"] = $"Mã học viên {student.Code} đã tồn tại.";
                return RedirectToAction(nameof(HocVien));
            }

            var studentUserName = student.Code.ToLowerInvariant();
            var linkedStudentUserId = student.Id == 0
                ? 0
                : Db.Users.AsNoTracking()
                    .Where(x => x.Role == EnglishCenterStore.RoleStudent && x.LinkedId == student.Id)
                    .Select(x => x.Id)
                    .FirstOrDefault();
            if (Db.Users.AsNoTracking().Any(x => x.Id != linkedStudentUserId && x.UserName == studentUserName))
            {
                TempData["Message"] = $"Tên đăng nhập {studentUserName} đã tồn tại.";
                return RedirectToAction(nameof(HocVien));
            }

            if (student.Id == 0)
            {
                using var transaction = Db.Database.BeginTransaction();
                Db.Students.Add(student);
                Db.SaveChanges();
                var user = new UserAccount
                {
                    FullName = student.FullName,
                    UserName = studentUserName,
                    Role = "Student",
                    LinkedId = student.Id,
                    Email = student.Email,
                    Phone = student.Phone
                };
                user.Password = "123456";
                Db.Users.Add(user);
                Db.SaveChanges();
                transaction.Commit();
            }
            else
            {
                var current = Db.Students.FirstOrDefault(x => x.Id == student.Id);
                if (current == null) return NotFound();
                current.Code = student.Code;
                current.FullName = student.FullName;
                current.Email = student.Email;
                current.Phone = student.Phone;
                current.DateOfBirth = student.DateOfBirth;
                current.Address = student.Address;

                var user = Db.Users.FirstOrDefault(x => x.Role == "Student" && x.LinkedId == current.Id);
                if (user != null)
                {
                    user.FullName = current.FullName;
                    user.UserName = studentUserName;
                    user.Email = current.Email;
                    user.Phone = current.Phone;
                }
                Db.SaveChanges();
            }

            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(HocVien));
        }

        [HttpPost]
        public IActionResult XoaHocVien(int id)
        {
            var auth = RequireRole("Staff");
            if (auth != null) return auth;
            var student = Db.Students.FirstOrDefault(x => x.Id == id);
            if (student != null)
            {
                var enrollmentIds = Db.Enrollments.Where(x => x.StudentId == id).Select(x => x.Id).ToList();
                var paymentIds = Db.Payments.Where(x => x.StudentId == id || enrollmentIds.Contains(x.EnrollmentId)).Select(x => x.Id).ToList();
                Db.Attendance.RemoveRange(Db.Attendance.Where(x => x.StudentId == id));
                Db.Scores.RemoveRange(Db.Scores.Where(x => x.StudentId == id));
                Db.PaymentTransactions.RemoveRange(Db.PaymentTransactions.Where(x => x.StudentId == id || paymentIds.Contains(x.PaymentId)));
                Db.Payments.RemoveRange(Db.Payments.Where(x => x.StudentId == id || enrollmentIds.Contains(x.EnrollmentId)));
                Db.Enrollments.RemoveRange(Db.Enrollments.Where(x => x.StudentId == id));
                Db.SavedCourses.RemoveRange(Db.SavedCourses.Where(x => x.StudentId == id));
                Db.Users.RemoveRange(Db.Users.Where(x => x.Role == "Student" && x.LinkedId == id));
                Db.Students.Remove(student);
            }
            Db.SaveChanges();
            TempData["Message"] = "Đã xóa học viên và tải lại danh sách mới.";
            return RedirectToAction(nameof(HocVien));
        }
    }
}
