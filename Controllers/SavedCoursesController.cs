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
    public class SavedCoursesController : Controller
    {
        private readonly EnglishCenterDbContext _context;

        public SavedCoursesController(EnglishCenterDbContext context)
        {
            _context = context;
        }

        // GET: SavedCourses
        public async Task<IActionResult> Index()
        {
            var englishCenterDbContext = _context.SavedCourses.Include(s => s.Course).Include(s => s.Student);
            return View(await englishCenterDbContext.ToListAsync());
        }

        // GET: SavedCourses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var savedCourse = await _context.SavedCourses
                .Include(s => s.Course)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (savedCourse == null)
            {
                return NotFound();
            }

            return View(savedCourse);
        }

        // GET: SavedCourses/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Code");
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName");
            return View();
        }

        // POST: SavedCourses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentId,CourseId,SavedAt")] SavedCourse savedCourse)
        {
            await ValidateSavedCourseAsync(savedCourse);
            if (ModelState.IsValid)
            {
                _context.Add(savedCourse);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm khóa học đã lưu.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Code", savedCourse.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", savedCourse.StudentId);
            return View(savedCourse);
        }

        // GET: SavedCourses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var savedCourse = await _context.SavedCourses.FindAsync(id);
            if (savedCourse == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Code", savedCourse.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", savedCourse.StudentId);
            return View(savedCourse);
        }

        // POST: SavedCourses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,CourseId,SavedAt")] SavedCourse savedCourse)
        {
            if (id != savedCourse.Id)
            {
                return NotFound();
            }

            await ValidateSavedCourseAsync(savedCourse, id);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(savedCourse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SavedCourseExists(savedCourse.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Đã cập nhật khóa học đã lưu.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Code", savedCourse.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", savedCourse.StudentId);
            return View(savedCourse);
        }

        // GET: SavedCourses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var savedCourse = await _context.SavedCourses
                .Include(s => s.Course)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (savedCourse == null)
            {
                return NotFound();
            }

            return View(savedCourse);
        }

        // POST: SavedCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var savedCourse = await _context.SavedCourses.FindAsync(id);
            if (savedCourse != null)
            {
                _context.SavedCourses.Remove(savedCourse);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa khóa học đã lưu.";
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> MySavedCourses()
        {
            if (!User.IsInRole("Student")
                || !int.TryParse(User.FindFirstValue("StudentId"), out var studentId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            return View(await _context.SavedCourses.AsNoTracking()
                .Where(x => x.StudentId == studentId)
                .Include(x => x.Course)
                .OrderByDescending(x => x.SavedAt)
                .ToListAsync());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            if (!User.IsInRole("Student")
                || !int.TryParse(User.FindFirstValue("StudentId"), out var studentId))
            {
                return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();
            }

            var savedCourse = await _context.SavedCourses
                .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);
            if (savedCourse is null)
            {
                return NotFound();
            }
            _context.SavedCourses.Remove(savedCourse);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã bỏ lưu khóa học.";
            return RedirectToAction(nameof(MySavedCourses));
        }

        private bool SavedCourseExists(int id)
        {
            return _context.SavedCourses.Any(e => e.Id == id);
        }

        private async Task ValidateSavedCourseAsync(SavedCourse savedCourse, int? currentId = null)
        {
            if (!await _context.Students.AnyAsync(x => x.Id == savedCourse.StudentId))
            {
                ModelState.AddModelError(nameof(SavedCourse.StudentId), "Vui lòng chọn học viên hợp lệ.");
            }

            if (!await _context.Courses.AnyAsync(x => x.Id == savedCourse.CourseId))
            {
                ModelState.AddModelError(nameof(SavedCourse.CourseId), "Vui lòng chọn khóa học hợp lệ.");
            }

            if (await _context.SavedCourses.AnyAsync(x =>
                    x.Id != currentId
                    && x.StudentId == savedCourse.StudentId
                    && x.CourseId == savedCourse.CourseId))
            {
                ModelState.AddModelError(string.Empty, "Học viên đã lưu khóa học này.");
            }
        }
    }
}
