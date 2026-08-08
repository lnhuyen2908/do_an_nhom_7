using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;

namespace web_do_an1.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public NotificationsController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var accountId = CurrentAccountId();
        if (!accountId.HasValue)
        {
            return Forbid();
        }

        return View(await _context.Notifications.AsNoTracking()
            .Where(x => x.UserAccountId == accountId.Value)
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var accountId = CurrentAccountId();
        if (!accountId.HasValue)
        {
            return Forbid();
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserAccountId == accountId.Value);
        if (notification is null)
        {
            return NotFound();
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        // Các thông báo duyệt đăng ký cũ có thể vẫn lưu URL Khóa học của tôi;
        // luôn đưa học viên tới màn hình học phí để thanh toán ngay sau khi được duyệt.
        var targetUrl = notification.Title.Contains("đã được duyệt", StringComparison.OrdinalIgnoreCase)
            ? Url.Action("MyPayments", "Payments")
            : notification.Url;

        if (!string.IsNullOrWhiteSpace(targetUrl) && Url.IsLocalUrl(targetUrl))
        {
            return LocalRedirect(targetUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var accountId = CurrentAccountId();
        if (!accountId.HasValue)
        {
            return Forbid();
        }

        await _context.Notifications
            .Where(x => x.UserAccountId == accountId.Value && !x.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRead, true));

        TempData["SuccessMessage"] = "Đã đánh dấu tất cả thông báo là đã đọc.";
        return RedirectToAction(nameof(Index));
    }

    private int? CurrentAccountId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    }
}
