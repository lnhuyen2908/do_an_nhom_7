using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public partial class QuanTriController
    {
        public IActionResult TaiKhoan(string? keyword)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            var users = Db.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                users = users.Where(x =>
                    x.FullName.Contains(keyword)
                    || x.UserName.Contains(keyword)
                    || x.Role.Contains(keyword)
                    || x.Email.Contains(keyword)
                    || x.Phone.Contains(keyword));
            }

            ViewBag.Keyword = keyword;
            return View(users.OrderBy(x => x.Role).ThenBy(x => x.UserName).ToList());
        }

        [HttpPost]
        public IActionResult LuuTaiKhoan([Bind("Id,FullName,UserName,Password,Role,Email,Phone")] UserAccount user)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            var current = user.Id == 0 ? null : Db.Users.FirstOrDefault(x => x.Id == user.Id);
            if (user.Id != 0 && current == null)
            {
                return NotFound();
            }

            var newPassword = user.Password?.Trim() ?? string.Empty;
            user.UserName = user.UserName?.Trim() ?? string.Empty;
            user.FullName = user.FullName?.Trim() ?? string.Empty;
            user.Role = user.Role?.Trim() ?? string.Empty;
            user.Email = user.Email?.Trim() ?? string.Empty;
            user.Phone = user.Phone?.Trim() ?? string.Empty;

            if (current?.LinkedId > 0)
            {
                user.Role = current.Role;
                user.LinkedId = current.LinkedId;
            }

            if (current != null && current.Id == CurrentUser?.Id && user.Role != current.Role)
            {
                TempData["Message"] = "Không thể thay đổi vai trò của tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(TaiKhoan));
            }

            user.Password = string.IsNullOrWhiteSpace(newPassword)
                ? current?.Password ?? string.Empty
                : newPassword;
            ModelState.Clear();
            TryValidateModel(user);

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(TaiKhoan));
            }

            var allowedRoles = new[] { EnglishCenterStore.RoleAdmin, EnglishCenterStore.RoleStaff };
            if ((current?.LinkedId ?? 0) == 0 && !allowedRoles.Contains(user.Role))
            {
                TempData["Message"] = "Tài khoản học viên và giáo viên phải được tạo từ trang quản lý hồ sơ tương ứng.";
                return RedirectToAction(nameof(TaiKhoan));
            }

            if (Db.Users.AsNoTracking().Any(x => x.Id != user.Id && x.UserName == user.UserName))
            {
                TempData["Message"] = "Tên đăng nhập đã tồn tại.";
                return RedirectToAction(nameof(TaiKhoan));
            }

            if (user.Id == 0)
            {
                user.LinkedId = 0;
                user.Password = newPassword;
                Db.Users.Add(user);
            }
            else
            {
                current!.FullName = user.FullName;
                current.UserName = user.UserName;
                current.Role = user.Role;
                current.Email = user.Email;
                current.Phone = user.Phone;
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    current.Password = newPassword;
                }
            }

            Db.SaveChanges();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(TaiKhoan));
        }

        [HttpPost]
        public IActionResult XoaTaiKhoan(int id)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            if (CurrentUser?.Id == id)
            {
                TempData["Message"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(TaiKhoan));
            }

            var user = Db.Users.FirstOrDefault(x => x.Id == id);
            if (user != null)
            {
                Db.Users.Remove(user);
                Db.SaveChanges();
                TempData["Message"] = "Đã xóa tài khoản.";
            }

            return RedirectToAction(nameof(TaiKhoan));
        }
    }
}
