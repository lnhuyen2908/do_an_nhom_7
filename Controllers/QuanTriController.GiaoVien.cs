using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public partial class QuanTriController
    {
        public IActionResult GiaoVien(int page = 1)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;
            const int pageSize = EnglishCenterStore.DefaultPageSize;
            var query = Db.Teachers.AsNoTracking();
            var totalItems = query.Count();
            var totalPages = EnglishCenterStore.TotalPages(totalItems, pageSize);
            page = EnglishCenterStore.NormalizePage(page, totalItems, pageSize);
            var teachers = query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var teacherIds = teachers.Select(x => x.Id).ToList();
            ViewBag.Classes = Db.Classes.AsNoTracking().Where(x => teacherIds.Contains(x.TeacherId)).ToList();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            return View(teachers);
        }

        [HttpPost]
        public IActionResult LuuGiaoVien([Bind("Id,Code,FullName,Email,Phone,Specialty")] Teacher teacher)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            teacher.Code = teacher.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            teacher.FullName = teacher.FullName?.Trim() ?? string.Empty;
            teacher.Email = teacher.Email?.Trim() ?? string.Empty;
            teacher.Phone = teacher.Phone?.Trim() ?? string.Empty;
            teacher.Specialty = teacher.Specialty?.Trim() ?? string.Empty;
            ModelState.Clear();
            TryValidateModel(teacher);

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(GiaoVien));
            }

            if (Db.Teachers.AsNoTracking().Any(x => x.Id != teacher.Id && x.Code == teacher.Code))
            {
                TempData["Message"] = $"Mã giáo viên {teacher.Code} đã tồn tại.";
                return RedirectToAction(nameof(GiaoVien));
            }

            var teacherUserName = teacher.Code.ToLowerInvariant();
            var linkedTeacherUserId = teacher.Id == 0
                ? 0
                : Db.Users.AsNoTracking()
                    .Where(x => x.Role == EnglishCenterStore.RoleTeacher && x.LinkedId == teacher.Id)
                    .Select(x => x.Id)
                    .FirstOrDefault();
            if (Db.Users.AsNoTracking().Any(x => x.Id != linkedTeacherUserId && x.UserName == teacherUserName))
            {
                TempData["Message"] = $"Tên đăng nhập {teacherUserName} đã tồn tại.";
                return RedirectToAction(nameof(GiaoVien));
            }

            if (teacher.Id == 0)
            {
                using var transaction = Db.Database.BeginTransaction();
                Db.Teachers.Add(teacher);
                Db.SaveChanges();
                var user = new UserAccount
                {
                    FullName = teacher.FullName,
                    UserName = teacherUserName,
                    Role = "Teacher",
                    LinkedId = teacher.Id,
                    Email = teacher.Email,
                    Phone = teacher.Phone
                };
                user.Password = "123456";
                Db.Users.Add(user);
                Db.SaveChanges();
                transaction.Commit();
            }
            else
            {
                var current = Db.Teachers.FirstOrDefault(x => x.Id == teacher.Id);
                if (current == null) return NotFound();
                current.Code = teacher.Code;
                current.FullName = teacher.FullName;
                current.Email = teacher.Email;
                current.Phone = teacher.Phone;
                current.Specialty = teacher.Specialty;

                var user = Db.Users.FirstOrDefault(x => x.Role == "Teacher" && x.LinkedId == current.Id);
                if (user != null)
                {
                    user.FullName = current.FullName;
                    user.UserName = teacherUserName;
                    user.Email = current.Email;
                    user.Phone = current.Phone;
                }
                Db.SaveChanges();
            }

            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(GiaoVien));
        }

        [HttpPost]
        public IActionResult XoaGiaoVien(int id)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;
            var teacher = Db.Teachers.FirstOrDefault(x => x.Id == id);
            if (teacher != null)
            {
                if (Db.Classes.Any(x => x.TeacherId == id) || Db.Lectures.Any(x => x.TeacherId == id))
                {
                    TempData["Message"] = "Không thể xóa giáo viên vì đang được phân công lớp học hoặc đã đăng bài giảng.";
                    return RedirectToAction(nameof(GiaoVien));
                }

                Db.Users.RemoveRange(Db.Users.Where(x => x.Role == "Teacher" && x.LinkedId == id));
                Db.Teachers.Remove(teacher);
            }
            Db.SaveChanges();
            TempData["Message"] = "Đã xóa giáo viên và tải lại danh sách mới.";
            return RedirectToAction(nameof(GiaoVien));
        }
    }
}
