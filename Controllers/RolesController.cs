using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly EnglishCenterDbContext _context;

        public RolesController(EnglishCenterDbContext context)
        {
            _context = context;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.ToListAsync());
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,DisplayName")] Role role)
        {
            NormalizeRole(role);
            ModelState.Clear();
            TryValidateModel(role);
            await ValidateRoleAsync(role);

            if (ModelState.IsValid)
            {
                _context.Add(role);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm vai trò.";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DisplayName")] Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            NormalizeRole(role);
            ModelState.Clear();
            TryValidateModel(role);
            await ValidateRoleAsync(role, id);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Đã cập nhật vai trò.";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                try
                {
                    _context.Roles.Remove(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa vai trò.";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa vai trò vì đang có tài khoản sử dụng.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private static void NormalizeRole(Role role)
        {
            role.Name = role.Name.Trim();
            role.DisplayName = role.DisplayName.Trim();
        }

        private async Task ValidateRoleAsync(Role role, int? currentId = null)
        {
            if (await _context.Roles.AnyAsync(x =>
                    x.Id != currentId && x.Name == role.Name))
            {
                ModelState.AddModelError(nameof(Role.Name), "Tên vai trò đã tồn tại.");
            }
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}
