using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class TeachersController : Controller
    {
        private readonly EnglishCenterDbContext _context;

        public TeachersController(EnglishCenterDbContext context)
        {
            _context = context;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            const int pageSize = 10;
            keyword = keyword?.Trim();
            page = Math.Max(page, 1);
            var query = _context.Teachers.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Code.Contains(keyword)
                    || x.FullName.Contains(keyword)
                    || x.Email.Contains(keyword)
                    || x.Phone.Contains(keyword)
                    || x.Specialty.Contains(keyword)
                    || x.Certifications.Contains(keyword));
            }

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            page = Math.Min(page, totalPages);
            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            return View(await query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync());
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,FullName,Email,Phone,Specialty,Degree,Certifications")] Teacher teacher)
        {
            NormalizeTeacher(teacher);
            ModelState.Clear();
            TryValidateModel(teacher);
            await ValidateTeacherAsync(teacher);

            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm giáo viên.";
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,FullName,Email,Phone,Specialty,Degree,Certifications")] Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            NormalizeTeacher(teacher);
            ModelState.Clear();
            TryValidateModel(teacher);
            await ValidateTeacherAsync(teacher, id);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Đã cập nhật giáo viên.";
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                try
                {
                    _context.Teachers.Remove(teacher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa giáo viên.";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa giáo viên vì đã có lớp học hoặc bài giảng liên quan.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Profile()
        {
            if (!User.IsInRole("Teacher")
                || !int.TryParse(User.FindFirstValue("TeacherId"), out var teacherId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var teacher = await _context.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == teacherId);
            return teacher is null ? NotFound() : View(teacher);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            [Bind("FullName,Email,Phone,Specialty,Degree,Certifications")] Teacher model)
        {
            if (!User.IsInRole("Teacher")
                || !int.TryParse(User.FindFirstValue("TeacherId"), out var teacherId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher is null)
            {
                return NotFound();
            }

            model.Code = teacher.Code;
            ModelState.Clear();
            TryValidateModel(model);
            await ValidateTeacherAsync(model, teacherId);
            if (!ModelState.IsValid)
            {
                model.Id = teacher.Id;
                return View(model);
            }

            teacher.FullName = model.FullName.Trim();
            teacher.Email = model.Email.Trim();
            teacher.Phone = model.Phone.Trim();
            teacher.Specialty = model.Specialty.Trim();
            teacher.Degree = model.Degree.Trim();
            teacher.Certifications = model.Certifications.Trim();
            var account = await _context.UserAccounts
                .FirstOrDefaultAsync(x => x.TeacherId == teacherId);
            if (account is not null)
            {
                account.FullName = teacher.FullName;
                account.Email = teacher.Email;
                account.Phone = teacher.Phone;
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật hồ sơ giáo viên.";
            return RedirectToAction(nameof(Profile));
        }

        private static void NormalizeTeacher(Teacher teacher)
        {
            teacher.Code = teacher.Code.Trim().ToUpperInvariant();
            teacher.FullName = teacher.FullName.Trim();
            teacher.Email = teacher.Email.Trim();
            teacher.Phone = teacher.Phone.Trim();
            teacher.Specialty = teacher.Specialty.Trim();
            teacher.Degree = teacher.Degree.Trim();
            teacher.Certifications = teacher.Certifications.Trim();
        }

        private async Task ValidateTeacherAsync(Teacher teacher, int? currentId = null)
        {
            if (await _context.Teachers.AnyAsync(x =>
                    x.Id != currentId && x.Code == teacher.Code))
            {
                ModelState.AddModelError(nameof(Teacher.Code), "Mã giáo viên đã tồn tại.");
            }

            if (await _context.Teachers.AnyAsync(x =>
                    x.Id != currentId && x.Email == teacher.Email))
            {
                ModelState.AddModelError(nameof(Teacher.Email), "Email đã được sử dụng.");
            }
            else if (await _context.UserAccounts.AnyAsync(x =>
                         x.TeacherId != currentId && x.Email == teacher.Email))
            {
                ModelState.AddModelError(nameof(Teacher.Email), "Email đã được sử dụng bởi tài khoản khác.");
            }

            if (await _context.Teachers.AnyAsync(x =>
                    x.Id != currentId && x.Phone == teacher.Phone))
            {
                ModelState.AddModelError(nameof(Teacher.Phone), "Số điện thoại đã được sử dụng.");
            }
            else if (await _context.UserAccounts.AnyAsync(x =>
                         x.TeacherId != currentId && x.Phone == teacher.Phone))
            {
                ModelState.AddModelError(nameof(Teacher.Phone), "Số điện thoại đã được sử dụng bởi tài khoản khác.");
            }
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
