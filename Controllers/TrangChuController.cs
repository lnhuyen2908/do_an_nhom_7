using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    public class TrangChuController : CoSoController
    {
        private readonly ILogger<TrangChuController> _logger;

        public TrangChuController(ILogger<TrangChuController> logger)
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}



