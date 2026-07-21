using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using web_do_an1.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddControllersWithViews(options =>
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
            builder.Services.AddDbContext<EnglishCenterDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ketnoituongdoi")));
            builder.Services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = 16 * 1024;
                options.MultipartBodyLengthLimit = LectureFileStorage.MaxFileSize + 1024 * 1024;
            });
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("dang-nhap", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }));
            });
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.Name = "EnglishCenter.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EnglishCenterDbContext>();
                db.Database.Migrate();
                PasswordUtility.UsePlainTextDemoPasswords(db);
            }

            try
            {
                LectureFileStorage.MigratePublicFiles(app.Environment.ContentRootPath);
            }
            catch (IOException exception)
            {
                app.Logger.LogWarning(exception, "Không thể chuyển toàn bộ bài giảng sang thư mục riêng tư.");
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/TrangChu/Loi");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                context.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "base-uri 'self'; object-src 'none'; frame-ancestors 'none'; form-action 'self'; " +
                    "img-src 'self' data:; font-src 'self' data:; " +
                    "style-src 'self' 'unsafe-inline'; script-src 'self'; " +
                    "connect-src 'self' https://api.mymemory.translated.net";
                await next();
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseRateLimiter();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=TrangChu}/{action=TrangChu}/{id?}");

            app.Run();
        }
    }
}
