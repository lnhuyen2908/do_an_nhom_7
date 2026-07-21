using Microsoft.AspNetCore.Mvc;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    public partial class QuanTriController : CoSoController
    {
        private readonly IWebHostEnvironment _environment;

        public QuanTriController(
            EnglishCenterDbContext db,
            IWebHostEnvironment environment) : base(db)
        {
            _environment = environment;
        }

        public IActionResult TongQuan()
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;

            return View(BuildDashboardModel());
        }
    }
}
