using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net.Mail;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    public class TaiKhoanController : CoSoController
    {
        public TaiKhoanController(EnglishCenterDbContext db) : base(db)
        {
        }

        public IActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        [EnableRateLimiting("dang-nhap")]
        public IActionResult DangNhap(string userName, string password)
        {
            userName = userName?.Trim() ?? string.Empty;
            password = password?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                ViewBag.UserName = userName;
                return View();
            }

            var user = Db.Users.FirstOrDefault(x => x.UserName == userName);

            if (user == null || user.Password != password)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                ViewBag.UserName = userName;
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            TempData["Message"] = $"Đăng nhập thành công. Xin chào {user.FullName}.";

            return user.Role switch
            {
                "Admin" => RedirectToAction("TongQuan", "QuanTri"),
                "Staff" => RedirectToAction("TongQuan", "QuanTri"),
                "Teacher" => RedirectToAction("LopHoc", "GiaoVien"),
                "Student" => RedirectToAction("TongQuan", "HocVien"),
                _ => RedirectToAction("TrangChu", "TrangChu")
            };
        }

        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangKy(
            [Bind("FullName,Email,Phone,DateOfBirth,Address")] Student student,
            string userName,
            string password)
        {
            userName = userName?.Trim() ?? string.Empty;
            password = password?.Trim() ?? string.Empty;
            student.FullName = student.FullName?.Trim() ?? string.Empty;
            student.Email = student.Email?.Trim() ?? string.Empty;
            student.Phone = student.Phone?.Trim() ?? string.Empty;
            student.Address = student.Address?.Trim() ?? string.Empty;
            ModelState.Clear();
            TryValidateModel(student);

            var errors = ValidateRegistration(student, userName, password);
            if (errors.Any())
            {
                ViewBag.Errors = errors;
                ViewBag.UserName = userName;
                return View(student);
            }

            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var nextStudentNumber = Db.Students
                .AsNoTracking()
                .Select(x => x.Code)
                .AsEnumerable()
                .Select(code => code.StartsWith("HV", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(code[2..], out var number) ? number : 0)
                .DefaultIfEmpty()
                .Max() + 1;
            student.Code = $"HV{nextStudentNumber:00}";
            Db.Students.Add(student);
            Db.SaveChanges();

            var user = new UserAccount
            {
                FullName = student.FullName,
                UserName = userName,
                Role = "Student",
                LinkedId = student.Id,
                Email = student.Email,
                Phone = student.Phone
            };
            user.Password = password;
            Db.Users.Add(user);

            Db.SaveChanges();
            transaction.Commit();
            TempData["Message"] = "Đăng ký tài khoản thành công. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(DangNhap));
        }

        [HttpPost]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "Đăng xuất thành công.";
            return RedirectToAction("TrangChu", "TrangChu");
        }

        public IActionResult TuChoi()
        {
            return View();
        }

        private List<string> ValidateRegistration(Student student, string userName, string password)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(student.FullName)) errors.Add("Vui lòng nhập họ tên.");
            if (string.IsNullOrWhiteSpace(userName)) errors.Add("Vui lòng nhập tên đăng nhập.");
            if (string.IsNullOrWhiteSpace(password)) errors.Add("Vui lòng nhập mật khẩu.");
            if (student.DateOfBirth.Date > DateTime.Today.AddYears(-5)
                || student.DateOfBirth.Date < DateTime.Today.AddYears(-100))
            {
                errors.Add("Ngày sinh không hợp lệ. Học viên phải từ 5 đến 100 tuổi.");
            }
            if (string.IsNullOrWhiteSpace(student.Email)) errors.Add("Vui lòng nhập email.");
            if (string.IsNullOrWhiteSpace(student.Phone)) errors.Add("Vui lòng nhập số điện thoại.");
            if (string.IsNullOrWhiteSpace(student.Address)) errors.Add("Vui lòng nhập địa chỉ.");

            if (!string.IsNullOrWhiteSpace(student.Email) && !IsValidEmail(student.Email))
            {
                errors.Add("Email chưa đúng định dạng.");
            }

            if (!string.IsNullOrWhiteSpace(userName) && userName.Length < 3)
            {
                errors.Add("Tên đăng nhập phải có ít nhất 3 ký tự.");
            }

            if (!string.IsNullOrWhiteSpace(password) && password.Length < 6)
            {
                errors.Add("Mật khẩu phải có ít nhất 6 ký tự.");
            }

            if (!string.IsNullOrWhiteSpace(student.Phone)
                && (student.Phone.Length < 9 || student.Phone.Length > 11 || student.Phone.Any(x => !char.IsDigit(x))))
            {
                errors.Add("Số điện thoại phải gồm 9 đến 11 chữ số.");
            }

            if (!string.IsNullOrWhiteSpace(userName) && Db.Users.AsNoTracking().Any(x => x.UserName == userName))
            {
                errors.Add("Tên đăng nhập đã tồn tại.");
            }

            errors.AddRange(ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage)
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            return errors.Distinct().ToList();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                return new MailAddress(email).Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
