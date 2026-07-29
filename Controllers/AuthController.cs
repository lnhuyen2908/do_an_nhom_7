using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers;

public class AuthController : Controller
{
    private readonly EnglishCenterDbContext _context;

    public AuthController(EnglishCenterDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string? fullName,
        string? userName,
        string? email,
        string? phone,
        DateTime? dateOfBirth,
        string? address,
        string? password,
        string? confirmPassword)
    {
        fullName = fullName?.Trim() ?? string.Empty;
        userName = userName?.Trim().ToLowerInvariant() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;
        phone = phone?.Trim() ?? string.Empty;
        address = address?.Trim() ?? string.Empty;
        password ??= string.Empty;
        confirmPassword ??= string.Empty;

        ViewBag.FullName = fullName;
        ViewBag.UserName = userName;
        ViewBag.Email = email;
        ViewBag.Phone = phone;
        ViewBag.DateOfBirth = dateOfBirth.HasValue ? dateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty;
        ViewBag.Address = address;

        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(userName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(phone)
            || string.IsNullOrWhiteSpace(address)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmPassword)
            || !dateOfBirth.HasValue)
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            return View();
        }

        if (userName.Length < 3)
        {
            ViewBag.Error = "Tên đăng nhập phải có ít nhất 3 ký tự.";
            return View();
        }

        if (password.Length < 6)
        {
            ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Mật khẩu xác nhận không khớp.";
            return View();
        }

        if (!new EmailAddressAttribute().IsValid(email))
        {
            ViewBag.Error = "Email không đúng định dạng.";
            return View();
        }

        if (!new PhoneAttribute().IsValid(phone))
        {
            ViewBag.Error = "Số điện thoại không đúng định dạng.";
            return View();
        }

        if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-5)
            || dateOfBirth.Value.Date < DateTime.Today.AddYears(-100))
        {
            ViewBag.Error = "Học viên phải từ 5 đến 100 tuổi.";
            return View();
        }

        if (await _context.UserAccounts.AnyAsync(x => x.UserName == userName))
        {
            ViewBag.Error = "Tên đăng nhập đã tồn tại.";
            return View();
        }

        if (await _context.Students.AnyAsync(x => x.Email == email))
        {
            ViewBag.Error = "Email đã được sử dụng.";
            return View();
        }

        var studentRole = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "Student");
        if (studentRole is null)
        {
            return Problem("Chưa cấu hình vai trò học viên.");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            var lastNumber = await _context.Students
                .Where(x => x.Code.StartsWith("ST"))
                .Select(x => x.Code)
                .ToListAsync();
            var nextNumber = lastNumber
                .Select(x => int.TryParse(x[2..], out var number) ? number : 0)
                .DefaultIfEmpty().Max() + 1;

            var student = new Student
            {
                Code = $"ST{nextNumber:00}",
                FullName = fullName,
                Email = email,
                Phone = phone,
                DateOfBirth = dateOfBirth.Value,
                Address = address
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            _context.UserAccounts.Add(new UserAccount
            {
                FullName = fullName,
                UserName = userName,
                Password = password,
                Email = email,
                Phone = phone,
                RoleId = studentRole.Id,
                StudentId = student.Id,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        string? userName,
        string? password,
        string? returnUrl)
    {
        userName = userName?.Trim().ToLowerInvariant() ?? string.Empty;
        password ??= string.Empty;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
            ViewBag.UserName = userName;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var account = await _context.UserAccounts
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserName == userName && x.IsActive);

        if (account is null || account.Password != password)
        {
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            ViewBag.UserName = userName;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.FullName),
            new(ClaimTypes.Role, account.Role?.Name ?? string.Empty),
            new("UserName", account.UserName)
        };

        if (account.StudentId.HasValue)
        {
            claims.Add(new Claim("StudentId", account.StudentId.Value.ToString()));
        }

        if (account.TeacherId.HasValue)
        {
            claims.Add(new Claim("TeacherId", account.TeacherId.Value.ToString()));
        }

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
