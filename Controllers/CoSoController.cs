using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    public abstract class CoSoController : Controller
    {
        private UserAccount? _currentUser;
        private Student? _currentStudent;
        private Teacher? _currentTeacher;
        private bool _userLoaded;
        private bool _studentLoaded;
        private bool _teacherLoaded;

        protected EnglishCenterDbContext Db => HttpContext.RequestServices.GetRequiredService<EnglishCenterDbContext>();

        protected UserAccount? CurrentUser
        {
            get
            {
                if (_userLoaded)
                {
                    return _currentUser;
                }

                _userLoaded = true;
                var userId = HttpContext.Session.GetInt32("UserId");
                _currentUser = userId.HasValue
                    ? Db.Users.FirstOrDefault(x => x.Id == userId.Value)
                    : null;
                return _currentUser;
            }
        }

        protected Student? CurrentStudent
        {
            get
            {
                if (_studentLoaded)
                {
                    return _currentStudent;
                }

                _studentLoaded = true;
                var user = CurrentUser;
                _currentStudent = user?.Role == EnglishCenterStore.RoleStudent
                    ? Db.Students.FirstOrDefault(x => x.Id == user.LinkedId)
                    : null;
                return _currentStudent;
            }
        }

        protected Teacher? CurrentTeacher
        {
            get
            {
                if (_teacherLoaded)
                {
                    return _currentTeacher;
                }

                _teacherLoaded = true;
                var user = CurrentUser;
                _currentTeacher = user?.Role == EnglishCenterStore.RoleTeacher
                    ? Db.Teachers.FirstOrDefault(x => x.Id == user.LinkedId)
                    : null;
                return _currentTeacher;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.CurrentUser = CurrentUser;
            base.OnActionExecuting(context);
        }

        protected IActionResult? RequireRole(params string[] roles)
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            return roles.Contains(CurrentUser.Role)
                ? null
                : RedirectToAction("TuChoi", "TaiKhoan");
        }

        protected bool ThongBaoNeuDuLieuKhongHopLe(string? thongBaoMacDinh = null)
        {
            if (ModelState.IsValid)
            {
                return false;
            }

            var errors = ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage)
                    ? "Dữ liệu nhập chưa đúng định dạng."
                    : x.ErrorMessage)
                .Distinct()
                .ToList();

            TempData["Message"] = errors.Count > 0
                ? string.Join(" ", errors)
                : thongBaoMacDinh ?? "Vui lòng kiểm tra lại thông tin đã nhập.";
            return true;
        }

        protected DashboardViewModel BuildDashboardModel(int featuredCourseCount = 0)
        {
            var courses = Db.Courses.AsNoTracking().OrderBy(x => x.Code).ToList();
            var classes = Db.Classes.AsNoTracking().OrderBy(x => x.Code).ToList();
            var courseNames = courses.ToDictionary(x => x.Id, x => x.Name);
            var approvedClassIds = Db.Enrollments
                .AsNoTracking()
                .Where(x => x.ClassId.HasValue && x.Status == EnglishCenterStore.EnrollmentApproved)
                .Select(x => x.ClassId)
                .ToList();
            var approvedByClass = approvedClassIds
                .Where(x => x.HasValue)
                .GroupBy(x => x!.Value)
                .ToDictionary(x => x.Key, x => x.Count());

            return new DashboardViewModel
            {
                CourseCount = courses.Count,
                StudentCount = Db.Students.Count(),
                TeacherCount = Db.Teachers.Count(),
                ClassCount = classes.Count,
                PendingEnrollmentCount = Db.Enrollments.Count(x => x.Status == EnglishCenterStore.EnrollmentPending),
                ApprovedEnrollmentCount = Db.Enrollments.Count(x => x.Status == EnglishCenterStore.EnrollmentApproved),
                CanceledEnrollmentCount = Db.Enrollments.Count(x => x.Status == EnglishCenterStore.EnrollmentCanceled),
                Revenue = Db.Payments.Sum(x => x.PaidAmount),
                TotalTuition = Db.Payments.Sum(x => x.Amount),
                OutstandingTuition = Db.Payments.Sum(x => x.Amount - x.PaidAmount),
                PaymentTransactionCount = Db.PaymentTransactions.Count(),
                FeaturedCourses = courses.Take(featuredCourseCount).ToList(),
                ClassStatistics = classes.Select(courseClass => new ClassStatistic
                {
                    ClassCode = courseClass.Code,
                    CourseName = courseNames.GetValueOrDefault(courseClass.CourseId, string.Empty),
                    StudentCount = approvedByClass.GetValueOrDefault(courseClass.Id)
                }).ToList()
            };
        }
    }
}
