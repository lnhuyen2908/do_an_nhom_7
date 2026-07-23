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
    public class StudentsController : Controller
    {
        private readonly EnglishCenterDbContext _context;

        public StudentsController(EnglishCenterDbContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,FullName,Email,Phone,DateOfBirth,Address")] Student student)
        {
            NormalizeStudent(student);
            ModelState.Clear();
            TryValidateModel(student);
            await ValidateStudentAsync(student);

            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm học viên.";
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,FullName,Email,Phone,DateOfBirth,Address")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            NormalizeStudent(student);
            ModelState.Clear();
            TryValidateModel(student);
            await ValidateStudentAsync(student, id);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Đã cập nhật học viên.";
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                try
                {
                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa học viên.";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa học viên vì đã có đăng ký, học phí, điểm hoặc điểm danh liên quan.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Profile()
        {
            if (!User.IsInRole("Student")
                || !int.TryParse(User.FindFirstValue("StudentId"), out var studentId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == studentId);
            return student is null ? NotFound() : View(student);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            [Bind("FullName,Email,Phone,DateOfBirth,Address")] Student model)
        {
            if (!User.IsInRole("Student")
                || !int.TryParse(User.FindFirstValue("StudentId"), out var studentId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var student = await _context.Students.FindAsync(studentId);
            if (student is null)
            {
                return NotFound();
            }

            model.Code = student.Code;
            ModelState.Clear();
            TryValidateModel(model);
            if (!ModelState.IsValid)
            {
                model.Id = student.Id;
                return View(model);
            }

            student.FullName = model.FullName.Trim();
            student.Email = model.Email.Trim();
            student.Phone = model.Phone.Trim();
            student.DateOfBirth = model.DateOfBirth;
            student.Address = model.Address.Trim();
            var account = await _context.UserAccounts
                .FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (account is not null)
            {
                account.FullName = student.FullName;
                account.Email = student.Email;
                account.Phone = student.Phone;
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật hồ sơ học viên.";
            return RedirectToAction(nameof(Profile));
        }

        private static void NormalizeStudent(Student student)
        {
            student.Code = student.Code.Trim().ToUpperInvariant();
            student.FullName = student.FullName.Trim();
            student.Email = student.Email.Trim();
            student.Phone = student.Phone.Trim();
            student.Address = student.Address.Trim();
        }

        private async Task ValidateStudentAsync(Student student, int? currentId = null)
        {
            if (await _context.Students.AnyAsync(x =>
                    x.Id != currentId && x.Code == student.Code))
            {
                ModelState.AddModelError(nameof(Student.Code), "Mã học viên đã tồn tại.");
            }

            if (await _context.Students.AnyAsync(x =>
                    x.Id != currentId && x.Email == student.Email))
            {
                ModelState.AddModelError(nameof(Student.Email), "Email đã được sử dụng.");
            }
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
