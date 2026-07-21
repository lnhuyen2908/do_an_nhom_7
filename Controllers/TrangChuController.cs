using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    public class TrangChuController : CoSoController
    {
        private readonly ILogger<TrangChuController> _logger;

        public TrangChuController(EnglishCenterDbContext db, ILogger<TrangChuController> logger) : base(db)
        {
            _logger = logger;
        }

        public IActionResult TrangChu()
        {
            return View(BuildDashboardModel(featuredCourseCount: 3));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Loi()
        {
            return View(model: Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        }
    }
}



