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

    public async Task<IActionResult> Index(string? keyword, int page = 1)
    {
        const int pageSize = 10;
        keyword = keyword?.Trim();
        page = Math.Max(page, 1);
        var accounts = _context.UserAccounts
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Student)
            .Include(x => x.Teacher)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            accounts = accounts.Where(x => x.UserName.Contains(keyword)
                || x.FullName.Contains(keyword)
                || x.Email.Contains(keyword)
                || x.Phone.Contains(keyword)
                || x.Role.DisplayName.Contains(keyword));
        }

        var totalItems = await accounts.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Min(page, totalPages);
        ViewBag.Keyword = keyword;
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(await accounts.OrderBy(x => x.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync());
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
        account ??= new UserAccount();
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
            try
            {
                _context.UserAccounts.Add(account);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã tạo tài khoản.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // Vẫn có thể xảy ra xung đột nếu một tài khoản được tạo đồng thời ở phiên khác.
                ModelState.AddModelError(string.Empty,
                    "Không thể tạo tài khoản. Tên đăng nhập hoặc hồ sơ liên kết có thể vừa được sử dụng.");
            }
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
        var roles = await _context.Roles.AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync();
        ViewBag.Roles = roles;
        ViewData["RoleId"] = new SelectList(
            roles,
            "Id",
            "DisplayName",
            account?.RoleId);
        var students = await _context.Students.AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync();
        ViewBag.StudentIdItems = students;
        ViewData["StudentId"] = new SelectList(
            students,
            "Id",
            "FullName",
            account?.StudentId);
        var teachers = await _context.Teachers.AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync();
        ViewBag.TeacherIdItems = teachers;
        ViewData["TeacherId"] = new SelectList(
            teachers,
            "Id",
            "FullName",
            account?.TeacherId);
    }

    private static void NormalizeAccount(UserAccount account)
    {
        account.FullName = account.FullName?.Trim() ?? string.Empty;
        account.UserName = account.UserName?.Trim().ToLowerInvariant() ?? string.Empty;
        account.Password = account.Password?.Trim() ?? string.Empty;
        account.Email = account.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        account.Phone = account.Phone?.Trim() ?? string.Empty;
    }

    private async Task ValidateAccountAsync(UserAccount account, int? currentId = null)
    {
        if (await _context.UserAccounts.AnyAsync(x =>
                x.Id != currentId && x.UserName == account.UserName))
        {
            ModelState.AddModelError(nameof(UserAccount.UserName), "Tên đăng nhập đã tồn tại.");
        }

        if (!string.IsNullOrWhiteSpace(account.Email)
            && await _context.UserAccounts.AnyAsync(x =>
                x.Id != currentId && x.Email == account.Email))
        {
            ModelState.AddModelError(nameof(UserAccount.Email), "Email đã được sử dụng.");
        }

        if (!string.IsNullOrWhiteSpace(account.Phone)
            && await _context.UserAccounts.AnyAsync(x =>
                x.Id != currentId && x.Phone == account.Phone))
        {
            ModelState.AddModelError(nameof(UserAccount.Phone), "Số điện thoại đã được sử dụng.");
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
