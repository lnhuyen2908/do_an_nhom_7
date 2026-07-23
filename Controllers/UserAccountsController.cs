using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

[Authorize(Roles = "Admin")]
public class UserAccountsController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public UserAccountsController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var accounts = _context.UserAccounts
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Student)
            .Include(x => x.Teacher)
            .OrderBy(x => x.UserName);

        return View(await accounts.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var account = await _context.UserAccounts
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Student)
            .Include(x => x.Teacher)
            .FirstOrDefaultAsync(x => x.Id == id);

        return account is null ? NotFound() : View(account);
    }

    public async Task<IActionResult> Create()
    {
        await SetSelectListsAsync();
        return View(new UserAccount { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("FullName,UserName,Password,Email,Phone,RoleId,StudentId,TeacherId,IsActive")]
        UserAccount account)
    {
        NormalizeAccount(account);
        ModelState.Clear();
        TryValidateModel(account);

        if (string.IsNullOrWhiteSpace(account.Password))
        {
            ModelState.AddModelError(nameof(UserAccount.Password), "Vui lòng nhập mật khẩu.");
        }

        await ValidateAccountAsync(account);

        if (ModelState.IsValid)
        {
            account.CreatedAt = DateTime.Now;
            _context.UserAccounts.Add(account);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã tạo tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        await SetSelectListsAsync(account);
        return View(account);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var account = await _context.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
        {
            return NotFound();
        }

        account.Password = string.Empty;
        await SetSelectListsAsync(account);
        return View(account);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,FullName,UserName,Password,Email,Phone,RoleId,StudentId,TeacherId,IsActive")]
        UserAccount input)
    {
        if (id != input.Id)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(UserAccount.Password));

        var account = await _context.UserAccounts.FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
        {
            return NotFound();
        }

        NormalizeAccount(input);
        ModelState.Clear();
        ModelState.Remove(nameof(UserAccount.Password));
        TryValidateModel(input);
        ModelState.Remove(nameof(UserAccount.Password));
        await ValidateAccountAsync(input, id);

        if (ModelState.IsValid)
        {
            account.FullName = input.FullName;
            account.UserName = input.UserName;
            account.Email = input.Email;
            account.Phone = input.Phone;
            account.RoleId = input.RoleId;
            account.StudentId = input.StudentId;
            account.TeacherId = input.TeacherId;
            account.IsActive = input.IsActive;

            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                account.Password = input.Password;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        input.CreatedAt = account.CreatedAt;
        await SetSelectListsAsync(input);
        return View(input);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var account = await _context.UserAccounts
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Student)
            .Include(x => x.Teacher)
            .FirstOrDefaultAsync(x => x.Id == id);

        return account is null ? NotFound() : View(account);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var currentAccountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentAccountId == id.ToString())
        {
            TempData["ErrorMessage"] = "Không thể xóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        var account = await _context.UserAccounts.FindAsync(id);
        if (account is not null)
        {
            try
            {
                _context.UserAccounts.Remove(account);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tài khoản.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản vì dữ liệu liên quan chưa hợp lệ.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetSelectListsAsync(UserAccount? account = null)
    {
        ViewData["RoleId"] = new SelectList(
            await _context.Roles.AsNoTracking().OrderBy(x => x.DisplayName).ToListAsync(),
            "Id",
            "DisplayName",
            account?.RoleId);
        ViewData["StudentId"] = new SelectList(
            await _context.Students.AsNoTracking().OrderBy(x => x.FullName).ToListAsync(),
            "Id",
            "FullName",
            account?.StudentId);
        ViewData["TeacherId"] = new SelectList(
            await _context.Teachers.AsNoTracking().OrderBy(x => x.FullName).ToListAsync(),
            "Id",
            "FullName",
            account?.TeacherId);
    }

    private static void NormalizeAccount(UserAccount account)
    {
        account.FullName = account.FullName.Trim();
        account.UserName = account.UserName.Trim().ToLowerInvariant();
        account.Password = account.Password.Trim();
        account.Email = account.Email.Trim();
        account.Phone = account.Phone.Trim();
    }

    private async Task ValidateAccountAsync(UserAccount account, int? currentId = null)
    {
        if (await _context.UserAccounts.AnyAsync(x =>
                x.Id != currentId && x.UserName == account.UserName))
        {
            ModelState.AddModelError(nameof(UserAccount.UserName), "Tên đăng nhập đã tồn tại.");
        }

        var role = await _context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == account.RoleId);
        if (role is null)
        {
            ModelState.AddModelError(nameof(UserAccount.RoleId), "Vui lòng chọn vai trò hợp lệ.");
            return;
        }

        if (role.Name == "Student")
        {
            if (!account.StudentId.HasValue)
            {
                ModelState.AddModelError(nameof(UserAccount.StudentId), "Tài khoản học viên phải liên kết học viên.");
            }

            account.TeacherId = null;
        }
        else if (role.Name == "Teacher")
        {
            if (!account.TeacherId.HasValue)
            {
                ModelState.AddModelError(nameof(UserAccount.TeacherId), "Tài khoản giáo viên phải liên kết giáo viên.");
            }

            account.StudentId = null;
        }
        else
        {
            account.StudentId = null;
            account.TeacherId = null;
        }

        if (account.StudentId.HasValue)
        {
            if (!await _context.Students.AnyAsync(x => x.Id == account.StudentId.Value))
            {
                ModelState.AddModelError(nameof(UserAccount.StudentId), "Học viên liên kết không hợp lệ.");
            }
            else if (await _context.UserAccounts.AnyAsync(x =>
                         x.Id != currentId && x.StudentId == account.StudentId))
            {
                ModelState.AddModelError(nameof(UserAccount.StudentId), "Học viên này đã có tài khoản.");
            }
        }

        if (account.TeacherId.HasValue)
        {
            if (!await _context.Teachers.AnyAsync(x => x.Id == account.TeacherId.Value))
            {
                ModelState.AddModelError(nameof(UserAccount.TeacherId), "Giáo viên liên kết không hợp lệ.");
            }
            else if (await _context.UserAccounts.AnyAsync(x =>
                         x.Id != currentId && x.TeacherId == account.TeacherId))
            {
                ModelState.AddModelError(nameof(UserAccount.TeacherId), "Giáo viên này đã có tài khoản.");
            }
        }
    }
}
