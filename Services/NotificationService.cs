using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Services;

public class NotificationService
{
    private readonly EnglishCenterDbContext _context;

    public NotificationService(EnglishCenterDbContext context)
    {
        _context = context;
    }

    public async Task NotifyUserAsync(int userAccountId, string title, string message, string url = "")
    {
        _context.Notifications.Add(new Notification
        {
            UserAccountId = userAccountId,
            Title = title.Trim(),
            Message = message.Trim(),
            Url = url.Trim(),
            CreatedAt = DateTime.Now
        });
        await _context.SaveChangesAsync();
    }

    public async Task NotifyRolesAsync(IEnumerable<string> roleNames, string title, string message, string url = "")
    {
        var roles = roleNames.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        if (roles.Length == 0)
        {
            return;
        }

        var accountIds = await _context.UserAccounts.AsNoTracking()
            .Where(x => x.IsActive && roles.Contains(x.Role.Name))
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var accountId in accountIds)
        {
            _context.Notifications.Add(new Notification
            {
                UserAccountId = accountId,
                Title = title.Trim(),
                Message = message.Trim(),
                Url = url.Trim(),
                CreatedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
    }
}
