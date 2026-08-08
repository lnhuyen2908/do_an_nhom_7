using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Services;

var cultureInfo = new System.Globalization.CultureInfo("vi-VN");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Tạo đối tượng cấu hình và đăng ký các dịch vụ của ứng dụng ASP.NET Core.
var builder = WebApplication.CreateBuilder(args);

// Đăng ký mô hình MVC gồm Controller và View.
builder.Services.AddControllersWithViews(options =>
{
    // Tự động kiểm tra mã chống giả mạo đối với các yêu cầu làm thay đổi dữ liệu.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Ưu tiên chuỗi kết nối đã chạy ổn định trên máy hiện tại; vẫn giữ kết nối tương đối
// làm phương án dự phòng cho các thành viên khác trong nhóm.
var databaseConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(databaseConnection))
{
    databaseConnection = builder.Configuration.GetConnectionString("ketnoituongdoi");
}

if (string.IsNullOrWhiteSpace(databaseConnection))
{
    throw new InvalidOperationException("Chưa cấu hình chuỗi kết nối cơ sở dữ liệu.");
}

builder.Services.AddDbContext<EnglishCenterDbContext>(options =>
    options.UseSqlServer(
        databaseConnection,
        sql => sql.EnableRetryOnFailure()));

// Cấu hình đăng nhập bằng cookie.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Chưa đăng nhập thì chuyển đến trang Login.
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Đã đăng nhập nhưng thiếu quyền thì báo từ chối.
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "EnglishCenter.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(); // Bật kiểm tra [Authorize] và vai trò người dùng.
builder.Services.AddScoped<DatabaseSeeder>(); // Cho phép lấy DatabaseSeeder bằng Dependency Injection.
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddSingleton<SimplePdfService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication(); // Đọc cookie và xác định người đang đăng nhập.
app.UseAuthorization(); // Kiểm tra người đó có quyền gọi Controller/Action hay không.

// URL mặc định có dạng /TênController/TênAction/id.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EnglishCenterDbContext>();
    // Tự áp dụng các migration chưa chạy để cập nhật cấu trúc database.
    await context.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    // Thêm hoặc cập nhật dữ liệu mẫu phục vụ chạy thử hệ thống.
    await seeder.SeedAsync();
}

await app.RunAsync();
