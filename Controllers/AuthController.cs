using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers;

// Auth = Authentication = xác thực. Controller này xử lý đăng ký, đăng nhập và đăng xuất.
public class AuthController : Controller
{
    // _context là đối tượng dùng để đọc và ghi dữ liệu trong database.
    private readonly EnglishCenterDbContext _context;
    private readonly EmailSender _emailSender;
    private readonly IWebHostEnvironment _environment;
    private const string PendingRegistrationSessionKey = "PendingRegistration";

    // Dependency Injection tự truyền EnglishCenterDbContext vào khi tạo AuthController.
    public AuthController(
        EnglishCenterDbContext context,
        EmailSender emailSender,
        IWebHostEnvironment environment)
    {
        _context = context;
        _emailSender = emailSender;
        _environment = environment;
    }

    [AllowAnonymous] // Cho phép người chưa đăng nhập mở trang đăng nhập.
    public IActionResult Login(string? returnUrl = null)
    {
        // Nếu đã đăng nhập thì không cần xem lại form; tài khoản vận hành về Dashboard riêng.
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterSignIn(User.FindFirstValue(ClaimTypes.Role));
        }

        ViewBag.ReturnUrl = returnUrl; // Lưu địa chỉ cũ để đăng nhập xong có thể quay lại.
        return View(); // Trả về Views/Auth/Login.cshtml.
    }

    [AllowAnonymous] // Cho phép khách mở trang tạo tài khoản.
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterSignIn(User.FindFirstValue(ClaimTypes.Role));
        }
        return View();
    }

    [HttpPost] // Hàm nhận dữ liệu được gửi từ form bằng phương thức POST.
    [AllowAnonymous] // Người chưa có tài khoản vẫn được gọi hàm này.
    [ValidateAntiForgeryToken] // Chặn yêu cầu giả mạo gửi từ một website khác.
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
        // Trim xóa khoảng trắng thừa; ?? string.Empty đổi giá trị null thành chuỗi rỗng.
        fullName = fullName?.Trim() ?? string.Empty;
        // ToLowerInvariant đổi tên đăng nhập thành chữ thường để tránh Dat và dat thành hai tài khoản.
        userName = userName?.Trim().ToLowerInvariant() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;
        phone = phone?.Trim() ?? string.Empty;
        address = address?.Trim() ?? string.Empty;
        // Toán tử ??= chỉ gán chuỗi rỗng khi mật khẩu đang là null.
        password ??= string.Empty;
        confirmPassword ??= string.Empty;

        // ViewBag gửi lại dữ liệu cũ sang View để người dùng không phải nhập lại khi có lỗi.
        ViewBag.FullName = fullName;
        ViewBag.UserName = userName;
        ViewBag.Email = email;
        ViewBag.Phone = phone;
        ViewBag.DateOfBirth = dateOfBirth.HasValue ? dateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty;
        ViewBag.Address = address;

        // IsNullOrWhiteSpace trả true nếu giá trị null, rỗng hoặc chỉ có khoảng trắng.
        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(userName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(phone)
            || string.IsNullOrWhiteSpace(address)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmPassword)
            || !dateOfBirth.HasValue)
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin."; // Gửi thông báo lỗi sang giao diện.
            return View(); // Dừng xử lý và hiển thị lại form đăng ký.
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

        // EmailAddressAttribute kiểm tra chuỗi có đúng cấu trúc email hay không.
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

        // AnyAsync kiểm tra trong bảng UserAccounts có ít nhất một tên đăng nhập giống giá trị vừa nhập.
        if (await _context.UserAccounts.AnyAsync(x => x.UserName == userName))
        {
            ViewBag.Error = "Tên đăng nhập đã tồn tại.";
            return View();
        }

        // Kiểm tra email đã thuộc về học viên nào trong database hay chưa.
        if (await _context.Students.AnyAsync(x => x.Email == email))
        {
            ViewBag.Error = "Email đã được sử dụng.";
            return View();
        }

        if (await _context.UserAccounts.AnyAsync(x => x.Email == email))
        {
            ViewBag.Error = "Email đã được sử dụng.";
            return View();
        }

        if (await _context.Students.AnyAsync(x => x.Phone == phone)
            || await _context.UserAccounts.AnyAsync(x => x.Phone == phone))
        {
            ViewBag.Error = "Số điện thoại đã tồn tại trong hệ thống.";
            return View();
        }

        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var pending = new PendingRegistration(
            fullName,
            userName,
            email,
            phone,
            dateOfBirth.Value,
            address,
            password,
            otp,
            DateTime.Now.AddMinutes(10));
        HttpContext.Session.SetString(PendingRegistrationSessionKey, JsonSerializer.Serialize(pending));

        var sent = await _emailSender.SendOtpAsync(email, otp);
        TempData["SuccessMessage"] = sent
            ? "Mã OTP đã được gửi đến Gmail của bạn."
            : _environment.IsDevelopment()
                ? $"Chưa cấu hình Gmail SMTP. Mã OTP chạy thử là {otp}."
                : "Chưa gửi được OTP qua Gmail. Vui lòng liên hệ trung tâm.";

        return RedirectToAction(nameof(VerifyOtp));
    }

    [AllowAnonymous]
    public IActionResult VerifyOtp()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterSignIn(User.FindFirstValue(ClaimTypes.Role));
        }

        var pending = ReadPendingRegistration();
        if (pending is null)
        {
            TempData["ErrorMessage"] = "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.";
            return RedirectToAction(nameof(Register));
        }

        ViewBag.Email = pending.Email;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(string? otp)
    {
        var pending = ReadPendingRegistration();
        if (pending is null)
        {
            TempData["ErrorMessage"] = "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.";
            return RedirectToAction(nameof(Register));
        }

        if (pending.ExpiresAt < DateTime.Now)
        {
            HttpContext.Session.Remove(PendingRegistrationSessionKey);
            TempData["ErrorMessage"] = "Mã OTP đã hết hạn. Vui lòng đăng ký lại.";
            return RedirectToAction(nameof(Register));
        }

        if (otp?.Trim() != pending.Otp)
        {
            ViewBag.Email = pending.Email;
            ViewBag.Error = "Mã OTP không đúng.";
            return View();
        }

        if (await _context.UserAccounts.AnyAsync(x => x.UserName == pending.UserName)
            || await _context.Students.AnyAsync(x => x.Email == pending.Email)
            || await _context.UserAccounts.AnyAsync(x => x.Email == pending.Email)
            || await _context.Students.AnyAsync(x => x.Phone == pending.Phone)
            || await _context.UserAccounts.AnyAsync(x => x.Phone == pending.Phone))
        {
            HttpContext.Session.Remove(PendingRegistrationSessionKey);
            TempData["ErrorMessage"] = "Thông tin đăng ký đã tồn tại. Vui lòng kiểm tra lại.";
            return RedirectToAction(nameof(Register));
        }

        await CreateStudentAccountAsync(pending);
        HttpContext.Session.Remove(PendingRegistrationSessionKey);
        TempData["SuccessMessage"] = "Xác minh OTP thành công. Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp()
    {
        var pending = ReadPendingRegistration();
        if (pending is null)
        {
            TempData["ErrorMessage"] = "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.";
            return RedirectToAction(nameof(Register));
        }

        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        pending = pending with
        {
            Otp = otp,
            ExpiresAt = DateTime.Now.AddMinutes(10)
        };
        HttpContext.Session.SetString(PendingRegistrationSessionKey, JsonSerializer.Serialize(pending));

        var sent = await _emailSender.SendOtpAsync(pending.Email, otp);
        TempData["SuccessMessage"] = sent
            ? "Mã OTP mới đã được gửi đến Gmail của bạn."
            : _environment.IsDevelopment()
                ? $"Chưa cấu hình Gmail SMTP. Mã OTP chạy thử là {otp}."
                : "Chưa gửi được OTP qua Gmail. Vui lòng kiểm tra cấu hình SMTP.";

        return RedirectToAction(nameof(VerifyOtp));
    }

    [HttpPost] // Nhận tên đăng nhập và mật khẩu từ form đăng nhập.
    [AllowAnonymous] // Không yêu cầu đăng nhập trước khi gọi hàm Login.
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
            .AsNoTracking() // Chỉ đọc dữ liệu, không theo dõi thay đổi nên truy vấn nhẹ hơn.
            .Include(x => x.Role) // Lấy kèm vai trò để tạo quyền đăng nhập.
            .FirstOrDefaultAsync(x => x.UserName == userName && x.IsActive); // Tìm tài khoản đang hoạt động.

        // Không tìm thấy tài khoản hoặc mật khẩu không khớp thì trả lại form cùng thông báo lỗi.
        if (account is null || account.Password != password)
        {
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            ViewBag.UserName = userName;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Claim là các thông tin nhận dạng được lưu vào phiên đăng nhập bằng cookie.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()), // Mã tài khoản.
            new(ClaimTypes.Name, account.FullName), // Họ tên hiển thị.
            new(ClaimTypes.Role, account.Role?.Name ?? string.Empty), // Vai trò dùng để phân quyền.
            new("UserName", account.UserName) // Tên đăng nhập do hệ thống tự đặt tên claim.
        };

        // Nếu đây là tài khoản học viên, lưu StudentId để các chức năng chỉ lấy dữ liệu của người đó.
        if (account.StudentId.HasValue)
        {
            claims.Add(new Claim("StudentId", account.StudentId.Value.ToString()));
        }

        if (account.TeacherId.HasValue)
        {
            claims.Add(new Claim("TeacherId", account.TeacherId.Value.ToString()));
        }

        // ClaimsIdentity gom các claim thành một danh tính; ClaimsPrincipal đại diện người đang đăng nhập.
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        // Tạo cookie đăng nhập trên trình duyệt từ principal vừa tạo.
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false, // Đóng phiên trình duyệt thì cookie đăng nhập không được giữ lâu dài.
                AllowRefresh = true // Cho phép hệ thống làm mới thời hạn cookie.
            });

        // Admin/NVĐT luôn bắt đầu tại khu vận hành riêng.
        if (account.Role?.Name is "Admin" or "Staff")
        {
            return RedirectToAction("Dashboard", "Home");
        }

        // Học viên, giáo viên và tài khoản chưa có vai trò dùng trang chủ công khai.
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectAfterSignIn(string? roleName)
    {
        return roleName is "Admin" or "Staff"
            ? RedirectToAction("Dashboard", "Home")
            : RedirectToAction("Index", "Home");
    }

    [HttpPost] // Đăng xuất làm thay đổi trạng thái đăng nhập nên dùng POST.
    [Authorize] // Chỉ người đang đăng nhập mới được đăng xuất.
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Xóa cookie đăng nhập.
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private PendingRegistration? ReadPendingRegistration()
    {
        var json = HttpContext.Session.GetString(PendingRegistrationSessionKey);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<PendingRegistration>(json);
    }

    private async Task CreateStudentAccountAsync(PendingRegistration pending)
    {
        var studentRole = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "Student");
        if (studentRole is null)
        {
            throw new InvalidOperationException("Chưa cấu hình vai trò học viên.");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
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
                FullName = pending.FullName,
                Email = pending.Email,
                Phone = pending.Phone,
                DateOfBirth = pending.DateOfBirth,
                Address = pending.Address
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            _context.UserAccounts.Add(new UserAccount
            {
                FullName = pending.FullName,
                UserName = pending.UserName,
                Password = pending.Password,
                Email = pending.Email,
                Phone = pending.Phone,
                RoleId = studentRole.Id,
                StudentId = student.Id,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        });
    }

    private sealed record PendingRegistration(
        string FullName,
        string UserName,
        string Email,
        string Phone,
        DateTime DateOfBirth,
        string Address,
        string Password,
        string Otp,
        DateTime ExpiresAt);
}
