using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    public class QuanTriController : CoSoController
    {
        private readonly IWebHostEnvironment _environment;

        public QuanTriController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult TongQuan()
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;

            return View(BuildDashboardModel());
        }

        public IActionResult KhoaHoc(string? keyword, int page = 1)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            const int pageSize = EnglishCenterStore.DefaultPageSize;
            keyword = keyword?.Trim();
            var courses = Db.Courses.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                courses = courses.Where(x => x.Name.Contains(keyword) || x.Code.Contains(keyword));
            }

            var totalItems = courses.Count();
            var totalPages = EnglishCenterStore.TotalPages(totalItems, pageSize);
            page = EnglishCenterStore.NormalizePage(page, totalItems, pageSize);

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            return View(courses.OrderBy(x => x.Code).Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LuuKhoaHoc([Bind("Id,Code,Name,Level,Tuition,Duration,Description,ImageUrl")] Course course)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            course.Code = course.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            course.Name = course.Name?.Trim() ?? string.Empty;
            course.Level = course.Level?.Trim() ?? string.Empty;
            course.Duration = course.Duration?.Trim() ?? string.Empty;
            course.Description = course.Description?.Trim() ?? string.Empty;
            course.ImageUrl = course.ImageUrl?.Trim() ?? string.Empty;
            ModelState.Clear();
            TryValidateModel(course);

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(KhoaHoc));
            }

            if (!string.IsNullOrEmpty(course.ImageUrl)
                && !EnglishCenterStore.CourseImageOptions.Contains(course.ImageUrl, StringComparer.Ordinal))
            {
                TempData["Message"] = "Ảnh khóa học được chọn không hợp lệ.";
                return RedirectToAction(nameof(KhoaHoc));
            }

            if (Db.Courses.AsNoTracking().Any(x => x.Id != course.Id && x.Code == course.Code))
            {
                TempData["Message"] = $"Mã khóa học {course.Code} đã tồn tại.";
                return RedirectToAction(nameof(KhoaHoc));
            }

            if (course.Id == 0)
            {
                Db.Courses.Add(course);
            }
            else
            {
                var current = Db.Courses.FirstOrDefault(x => x.Id == course.Id);
                if (current == null) return NotFound();
                current.Code = course.Code;
                current.Name = course.Name;
                current.Level = course.Level;
                current.Tuition = course.Tuition;
                current.Duration = course.Duration;
                current.Description = course.Description;
                current.ImageUrl = course.ImageUrl;
            }

            Db.SaveChanges();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(KhoaHoc));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaKhoaHoc(int id)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;
            var course = Db.Courses.FirstOrDefault(x => x.Id == id);
            var lectureFiles = new List<string>();
            if (course != null)
            {
                var classIds = Db.Classes.Where(x => x.CourseId == id).Select(x => x.Id).ToList();
                var enrollmentIds = Db.Enrollments.Where(x => x.CourseId == id).Select(x => x.Id).ToList();
                var paymentIds = Db.Payments.Where(x => enrollmentIds.Contains(x.EnrollmentId)).Select(x => x.Id).ToList();
                Db.Attendance.RemoveRange(Db.Attendance.Where(x => classIds.Contains(x.ClassId)));
                Db.Scores.RemoveRange(Db.Scores.Where(x => classIds.Contains(x.ClassId)));
                Db.PaymentTransactions.RemoveRange(Db.PaymentTransactions.Where(x => paymentIds.Contains(x.PaymentId)));
                Db.Payments.RemoveRange(Db.Payments.Where(x => enrollmentIds.Contains(x.EnrollmentId)));
                Db.Enrollments.RemoveRange(Db.Enrollments.Where(x => enrollmentIds.Contains(x.Id)));
                Db.Classes.RemoveRange(Db.Classes.Where(x => classIds.Contains(x.Id)));
                var lectures = Db.Lectures.Where(x => x.CourseId == id).ToList();
                lectureFiles.AddRange(lectures.Select(x => x.FileUrl));
                Db.Lectures.RemoveRange(lectures);
                Db.SavedCourses.RemoveRange(Db.SavedCourses.Where(x => x.CourseId == id));
                Db.Courses.Remove(course);
            }
            Db.SaveChanges();
            foreach (var fileReference in lectureFiles)
            {
                LectureFileStorage.DeleteIfExists(_environment.ContentRootPath, fileReference);
            }
            TempData["Message"] = "Đã xóa khóa học và tải lại danh sách mới.";
            return RedirectToAction(nameof(KhoaHoc));
        }

        public IActionResult GiaoVien(int page = 1)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;
            const int pageSize = EnglishCenterStore.DefaultPageSize;
            var query = Db.Teachers.AsNoTracking();
            var totalItems = query.Count();
            var totalPages = EnglishCenterStore.TotalPages(totalItems, pageSize);
            page = EnglishCenterStore.NormalizePage(page, totalItems, pageSize);
            var teachers = query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var teacherIds = teachers.Select(x => x.Id).ToList();
            ViewBag.Classes = Db.Classes.AsNoTracking().Where(x => teacherIds.Contains(x.TeacherId)).ToList();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            return View(teachers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LuuGiaoVien([Bind("Id,Code,FullName,Email,Phone,Specialty")] Teacher teacher)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            teacher.Code = teacher.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            teacher.FullName = teacher.FullName?.Trim() ?? string.Empty;
            teacher.Email = teacher.Email?.Trim() ?? string.Empty;
            teacher.Phone = teacher.Phone?.Trim() ?? string.Empty;
            teacher.Specialty = teacher.Specialty?.Trim() ?? string.Empty;
            ModelState.Clear();
            TryValidateModel(teacher);

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(GiaoVien));
            }

            if (Db.Teachers.AsNoTracking().Any(x => x.Id != teacher.Id && x.Code == teacher.Code))
            {
                TempData["Message"] = $"Mã giáo viên {teacher.Code} đã tồn tại.";
                return RedirectToAction(nameof(GiaoVien));
            }

            var teacherUserName = teacher.Code.ToLowerInvariant();
            var linkedTeacherUserId = teacher.Id == 0
                ? 0
                : Db.Users.AsNoTracking()
                    .Where(x => x.Role == EnglishCenterStore.RoleTeacher && x.LinkedId == teacher.Id)
                    .Select(x => x.Id)
                    .FirstOrDefault();
            if (Db.Users.AsNoTracking().Any(x => x.Id != linkedTeacherUserId && x.UserName == teacherUserName))
            {
                TempData["Message"] = $"Tên đăng nhập {teacherUserName} đã tồn tại.";
                return RedirectToAction(nameof(GiaoVien));
            }

            if (teacher.Id == 0)
            {
                using var transaction = Db.Database.BeginTransaction();
                Db.Teachers.Add(teacher);
                Db.SaveChanges();
                var user = new UserAccount
                {
                    FullName = teacher.FullName,
                    UserName = teacherUserName,
                    Role = "Teacher",
                    LinkedId = teacher.Id,
                    Email = teacher.Email,
                    Phone = teacher.Phone
                };
                user.Password = "123456";
                Db.Users.Add(user);
                Db.SaveChanges();
                transaction.Commit();
            }
            else
            {
                var current = Db.Teachers.FirstOrDefault(x => x.Id == teacher.Id);
                if (current == null) return NotFound();
                current.Code = teacher.Code;
                current.FullName = teacher.FullName;
                current.Email = teacher.Email;
                current.Phone = teacher.Phone;
                current.Specialty = teacher.Specialty;

                var user = Db.Users.FirstOrDefault(x => x.Role == "Teacher" && x.LinkedId == current.Id);
                if (user != null)
                {
                    user.FullName = current.FullName;
                    user.UserName = teacherUserName;
                    user.Email = current.Email;
                    user.Phone = current.Phone;
                }
                Db.SaveChanges();
            }

            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(GiaoVien));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaGiaoVien(int id)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;
            var teacher = Db.Teachers.FirstOrDefault(x => x.Id == id);
            if (teacher != null)
            {
                if (Db.Classes.Any(x => x.TeacherId == id) || Db.Lectures.Any(x => x.TeacherId == id))
                {
                    TempData["Message"] = "Không thể xóa giáo viên vì đang được phân công lớp học hoặc đã đăng bài giảng.";
                    return RedirectToAction(nameof(GiaoVien));
                }

                Db.Users.RemoveRange(Db.Users.Where(x => x.Role == "Teacher" && x.LinkedId == id));
                Db.Teachers.Remove(teacher);
            }
            Db.SaveChanges();
            TempData["Message"] = "Đã xóa giáo viên và tải lại danh sách mới.";
            return RedirectToAction(nameof(GiaoVien));
        }

        public IActionResult HocVien(int page = 1)
        {
            var auth = RequireRole("Staff");
            if (auth != null) return auth;
            const int pageSize = EnglishCenterStore.DefaultPageSize;
            var query = Db.Students.AsNoTracking();
            var totalItems = query.Count();
            var totalPages = EnglishCenterStore.TotalPages(totalItems, pageSize);
            page = EnglishCenterStore.NormalizePage(page, totalItems, pageSize);
            var students = query.OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var studentIds = students.Select(x => x.Id).ToList();
            ViewBag.Enrollments = Db.Enrollments.AsNoTracking().Where(x => studentIds.Contains(x.StudentId)).ToList();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            return View(students);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LuuHocVien([Bind("Id,Code,FullName,Email,Phone,DateOfBirth,Address")] Student student)
        {
            var auth = RequireRole("Staff");
            if (auth != null) return auth;

            student.Code = student.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            student.FullName = student.FullName?.Trim() ?? string.Empty;
            student.Email = student.Email?.Trim() ?? string.Empty;
            student.Phone = student.Phone?.Trim() ?? string.Empty;
            student.Address = student.Address?.Trim() ?? string.Empty;

            ModelState.Clear();
            TryValidateModel(student);
            if (string.IsNullOrWhiteSpace(student.Code))
            {
                ModelState.AddModelError(nameof(student.Code), "Vui lòng nhập mã học viên.");
            }

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(HocVien));
            }

            if (Db.Students.AsNoTracking().Any(x => x.Id != student.Id && x.Code == student.Code))
            {
                TempData["Message"] = $"Mã học viên {student.Code} đã tồn tại.";
                return RedirectToAction(nameof(HocVien));
            }

            var studentUserName = student.Code.ToLowerInvariant();
            var linkedStudentUserId = student.Id == 0
                ? 0
                : Db.Users.AsNoTracking()
                    .Where(x => x.Role == EnglishCenterStore.RoleStudent && x.LinkedId == student.Id)
                    .Select(x => x.Id)
                    .FirstOrDefault();
            if (Db.Users.AsNoTracking().Any(x => x.Id != linkedStudentUserId && x.UserName == studentUserName))
            {
                TempData["Message"] = $"Tên đăng nhập {studentUserName} đã tồn tại.";
                return RedirectToAction(nameof(HocVien));
            }

            if (student.Id == 0)
            {
                using var transaction = Db.Database.BeginTransaction();
                Db.Students.Add(student);
                Db.SaveChanges();
                var user = new UserAccount
                {
                    FullName = student.FullName,
                    UserName = studentUserName,
                    Role = "Student",
                    LinkedId = student.Id,
                    Email = student.Email,
                    Phone = student.Phone
                };
                user.Password = "123456";
                Db.Users.Add(user);
                Db.SaveChanges();
                transaction.Commit();
            }
            else
            {
                var current = Db.Students.FirstOrDefault(x => x.Id == student.Id);
                if (current == null) return NotFound();
                current.Code = student.Code;
                current.FullName = student.FullName;
                current.Email = student.Email;
                current.Phone = student.Phone;
                current.DateOfBirth = student.DateOfBirth;
                current.Address = student.Address;

                var user = Db.Users.FirstOrDefault(x => x.Role == "Student" && x.LinkedId == current.Id);
                if (user != null)
                {
                    user.FullName = current.FullName;
                    user.UserName = studentUserName;
                    user.Email = current.Email;
                    user.Phone = current.Phone;
                }
                Db.SaveChanges();
            }

            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(HocVien));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaHocVien(int id)
        {
            var auth = RequireRole("Staff");
            if (auth != null) return auth;
            var student = Db.Students.FirstOrDefault(x => x.Id == id);
            if (student != null)
            {
                var enrollmentIds = Db.Enrollments.Where(x => x.StudentId == id).Select(x => x.Id).ToList();
                var paymentIds = Db.Payments.Where(x => x.StudentId == id || enrollmentIds.Contains(x.EnrollmentId)).Select(x => x.Id).ToList();
                Db.Attendance.RemoveRange(Db.Attendance.Where(x => x.StudentId == id));
                Db.Scores.RemoveRange(Db.Scores.Where(x => x.StudentId == id));
                Db.PaymentTransactions.RemoveRange(Db.PaymentTransactions.Where(x => x.StudentId == id || paymentIds.Contains(x.PaymentId)));
                Db.Payments.RemoveRange(Db.Payments.Where(x => x.StudentId == id || enrollmentIds.Contains(x.EnrollmentId)));
                Db.Enrollments.RemoveRange(Db.Enrollments.Where(x => x.StudentId == id));
                Db.SavedCourses.RemoveRange(Db.SavedCourses.Where(x => x.StudentId == id));
                Db.Users.RemoveRange(Db.Users.Where(x => x.Role == "Student" && x.LinkedId == id));
                Db.Students.Remove(student);
            }
            Db.SaveChanges();
            TempData["Message"] = "Đã xóa học viên và tải lại danh sách mới.";
            return RedirectToAction(nameof(HocVien));
        }

        public IActionResult LichKhaiGiang()
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;
            ViewBag.CourseItems = new SelectList(Db.Courses.AsNoTracking(), "Id", "Name");
            ViewBag.TeacherItems = new SelectList(Db.Teachers.AsNoTracking(), "Id", "FullName");
            ViewBag.Courses = Db.Courses.AsNoTracking().ToList();
            ViewBag.Teachers = Db.Teachers.AsNoTracking().ToList();
            return View(Db.Classes.AsNoTracking().OrderBy(x => x.Code).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LuuLichKhaiGiang([Bind("Id,Code,CourseId,TeacherId,Room,StudyTime,StartDate,Capacity")] CourseClass courseClass)
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;

            courseClass.Code = courseClass.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            courseClass.Room = courseClass.Room?.Trim() ?? string.Empty;
            courseClass.StudyTime = courseClass.StudyTime?.Trim() ?? string.Empty;
            ModelState.Clear();
            TryValidateModel(courseClass);

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(LichKhaiGiang));
            }

            if (!Db.Courses.AsNoTracking().Any(x => x.Id == courseClass.CourseId))
            {
                TempData["Message"] = "Khóa học được chọn không tồn tại.";
                return RedirectToAction(nameof(LichKhaiGiang));
            }

            if (!Db.Teachers.AsNoTracking().Any(x => x.Id == courseClass.TeacherId))
            {
                TempData["Message"] = "Giáo viên được chọn không tồn tại.";
                return RedirectToAction(nameof(LichKhaiGiang));
            }

            if (Db.Classes.AsNoTracking().Any(x => x.Id != courseClass.Id && x.Code == courseClass.Code))
            {
                TempData["Message"] = $"Mã lớp học {courseClass.Code} đã tồn tại.";
                return RedirectToAction(nameof(LichKhaiGiang));
            }

            CourseClass? current = null;
            if (courseClass.Id != 0)
            {
                current = Db.Classes.FirstOrDefault(x => x.Id == courseClass.Id);
                if (current == null) return NotFound();

                var approvedStudentCount = Db.Enrollments.Count(x =>
                    x.ClassId == courseClass.Id
                    && x.Status == EnglishCenterStore.EnrollmentApproved);
                if (courseClass.Capacity < approvedStudentCount)
                {
                    TempData["Message"] = $"Sĩ số không được nhỏ hơn {approvedStudentCount} học viên đã duyệt.";
                    return RedirectToAction(nameof(LichKhaiGiang));
                }

                if (current.CourseId != courseClass.CourseId && approvedStudentCount > 0)
                {
                    TempData["Message"] = "Không thể đổi khóa học của lớp đã có học viên.";
                    return RedirectToAction(nameof(LichKhaiGiang));
                }
            }

            if (courseClass.Id == 0)
            {
                Db.Classes.Add(courseClass);
            }
            else
            {
                current!.Code = courseClass.Code;
                current.CourseId = courseClass.CourseId;
                current.TeacherId = courseClass.TeacherId;
                current.Room = courseClass.Room;
                current.StudyTime = courseClass.StudyTime;
                current.StartDate = courseClass.StartDate;
                current.Capacity = courseClass.Capacity;
            }

            Db.SaveChanges();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(LichKhaiGiang));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaLichKhaiGiang(int id)
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;
            var courseClass = Db.Classes.FirstOrDefault(x => x.Id == id);
            if (courseClass != null)
            {
                Db.Attendance.RemoveRange(Db.Attendance.Where(x => x.ClassId == id));
                Db.Scores.RemoveRange(Db.Scores.Where(x => x.ClassId == id));
                foreach (var enrollment in Db.Enrollments.Where(x => x.ClassId == id))
                {
                    enrollment.ClassId = null;
                    enrollment.Status = EnglishCenterStore.EnrollmentPending;
                }
                Db.Classes.Remove(courseClass);
            }
            Db.SaveChanges();
            TempData["Message"] = "Đã xóa lịch khai giảng và tải lại danh sách mới.";
            return RedirectToAction(nameof(LichKhaiGiang));
        }

        public IActionResult DangKy()
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;
            ViewBag.Students = Db.Students.AsNoTracking().ToList();
            ViewBag.Courses = Db.Courses.AsNoTracking().ToList();
            ViewBag.Classes = Db.Classes.AsNoTracking().ToList();
            var approvedClassIds = Db.Enrollments
                .AsNoTracking()
                .Where(x => x.ClassId.HasValue && x.Status == EnglishCenterStore.EnrollmentApproved)
                .Select(x => x.ClassId)
                .ToList();
            ViewBag.ClassSeats = approvedClassIds
                .Where(x => x.HasValue)
                .GroupBy(x => x!.Value)
                .ToDictionary(x => x.Key, x => x.Count());
            return View(Db.Enrollments.AsNoTracking().OrderByDescending(x => x.RegisteredAt).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CapNhatDangKy(int id, string status, int? classId)
        {
            var auth = RequireRole("Admin", "Staff");
            if (auth != null) return auth;
            status = status?.Trim() ?? string.Empty;
            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var enrollment = Db.Enrollments.FirstOrDefault(x => x.Id == id);
            if (enrollment == null) return NotFound();

            var allowedStatuses = new[]
            {
                EnglishCenterStore.EnrollmentPending,
                EnglishCenterStore.EnrollmentApproved,
                EnglishCenterStore.EnrollmentCanceled
            };
            if (!allowedStatuses.Contains(status))
            {
                TempData["Message"] = "Trạng thái đăng ký không hợp lệ.";
                return RedirectToAction(nameof(DangKy));
            }

            if (status == EnglishCenterStore.EnrollmentApproved)
            {
                if (!classId.HasValue)
                {
                    TempData["Message"] = "Vui lòng chọn lớp trước khi duyệt đăng ký.";
                    return RedirectToAction(nameof(DangKy));
                }

                var courseClass = Db.Classes.AsNoTracking().FirstOrDefault(x => x.Id == classId.Value);
                if (courseClass == null || courseClass.CourseId != enrollment.CourseId)
                {
                    TempData["Message"] = "Lớp được chọn không thuộc khóa học của đăng ký này.";
                    return RedirectToAction(nameof(DangKy));
                }

                var currentCount = Db.Enrollments.Count(x =>
                    x.Id != id
                    && x.ClassId == classId.Value
                    && x.Status == EnglishCenterStore.EnrollmentApproved);
                if (currentCount >= courseClass.Capacity)
                {
                    TempData["Message"] = $"Lớp {courseClass.Code} đã đủ sĩ số.";
                    return RedirectToAction(nameof(DangKy));
                }
            }

            enrollment.Status = status;
            enrollment.ClassId = status == EnglishCenterStore.EnrollmentApproved ? classId : null;
            Db.SaveChanges();
            transaction.Commit();
            TempData["Message"] = "Đã cập nhật trạng thái đăng ký.";
            return RedirectToAction(nameof(DangKy));
        }

        public IActionResult HocPhi()
        {
            var auth = RequireRole("Staff");
            if (auth != null) return auth;
            ViewBag.Students = Db.Students.AsNoTracking().ToList();
            ViewBag.Enrollments = Db.Enrollments.AsNoTracking().ToList();
            ViewBag.Courses = Db.Courses.AsNoTracking().ToList();
            ViewBag.PaymentTransactions = Db.PaymentTransactions.AsNoTracking()
                .OrderByDescending(x => x.PaidAt)
                .ToList();
            return View(Db.Payments.AsNoTracking().OrderBy(x => x.Status).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CapNhatHocPhi(int id, decimal paidAmount, string? paymentMethod)
        {
            var auth = RequireRole("Staff");
            if (auth != null) return auth;
            paymentMethod = paymentMethod?.Trim();
            using var transaction = Db.Database.BeginTransaction(IsolationLevel.Serializable);
            var payment = Db.Payments.FirstOrDefault(x => x.Id == id);
            if (payment == null) return NotFound();

            if (paidAmount < 0 || paidAmount > payment.Amount)
            {
                TempData["Message"] = $"Số tiền đã đóng phải từ 0 đến {payment.Amount:N0} đồng.";
                return RedirectToAction(nameof(HocPhi));
            }

            if (string.IsNullOrWhiteSpace(paymentMethod) || !EnglishCenterStore.IsValidPaymentMethod(paymentMethod))
            {
                TempData["Message"] = "Vui lòng chọn phương thức thanh toán hợp lệ.";
                return RedirectToAction(nameof(HocPhi));
            }

            var previousPaidAmount = payment.PaidAmount;
            payment.PaidAmount = paidAmount;
            payment.PaidDate = paidAmount > 0 ? DateTime.Today : null;
            payment.Status = EnglishCenterStore.PaymentStatus(payment.PaidAmount, payment.Amount);
            payment.PaymentMethod = paymentMethod;

            var difference = payment.PaidAmount - previousPaidAmount;
            if (difference != 0)
            {
                Db.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentId = payment.Id,
                    StudentId = payment.StudentId,
                    Amount = difference,
                    PaymentMethod = paymentMethod,
                    PaidAt = DateTime.Now,
                    RecordedBy = CurrentUser?.FullName ?? "Nhân viên đào tạo",
                    Note = difference > 0
                        ? "Nhân viên ghi nhận thanh toán"
                        : "Nhân viên điều chỉnh giảm số tiền đã đóng"
                });
            }
            Db.SaveChanges();
            transaction.Commit();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(HocPhi));
        }

        public IActionResult TaiKhoan(string? keyword)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            var users = Db.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                users = users.Where(x =>
                    x.FullName.Contains(keyword)
                    || x.UserName.Contains(keyword)
                    || x.Role.Contains(keyword)
                    || x.Email.Contains(keyword)
                    || x.Phone.Contains(keyword));
            }

            ViewBag.Keyword = keyword;
            return View(users.OrderBy(x => x.Role).ThenBy(x => x.UserName).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LuuTaiKhoan([Bind("Id,FullName,UserName,Password,Role,Email,Phone")] UserAccount user)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            var current = user.Id == 0 ? null : Db.Users.FirstOrDefault(x => x.Id == user.Id);
            if (user.Id != 0 && current == null)
            {
                return NotFound();
            }

            var newPassword = user.Password?.Trim() ?? string.Empty;
            user.UserName = user.UserName?.Trim() ?? string.Empty;
            user.FullName = user.FullName?.Trim() ?? string.Empty;
            user.Role = user.Role?.Trim() ?? string.Empty;
            user.Email = user.Email?.Trim() ?? string.Empty;
            user.Phone = user.Phone?.Trim() ?? string.Empty;

            if (current?.LinkedId > 0)
            {
                user.Role = current.Role;
                user.LinkedId = current.LinkedId;
            }

            if (current != null && current.Id == CurrentUser?.Id && user.Role != current.Role)
            {
                TempData["Message"] = "Không thể thay đổi vai trò của tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(TaiKhoan));
            }

            user.Password = string.IsNullOrWhiteSpace(newPassword)
                ? current?.Password ?? string.Empty
                : newPassword;
            ModelState.Clear();
            TryValidateModel(user);

            if (ThongBaoNeuDuLieuKhongHopLe())
            {
                return RedirectToAction(nameof(TaiKhoan));
            }

            var allowedRoles = new[] { EnglishCenterStore.RoleAdmin, EnglishCenterStore.RoleStaff };
            if ((current?.LinkedId ?? 0) == 0 && !allowedRoles.Contains(user.Role))
            {
                TempData["Message"] = "Tài khoản học viên và giáo viên phải được tạo từ trang quản lý hồ sơ tương ứng.";
                return RedirectToAction(nameof(TaiKhoan));
            }

            if (Db.Users.AsNoTracking().Any(x => x.Id != user.Id && x.UserName == user.UserName))
            {
                TempData["Message"] = "Tên đăng nhập đã tồn tại.";
                return RedirectToAction(nameof(TaiKhoan));
            }

            if (user.Id == 0)
            {
                user.LinkedId = 0;
                user.Password = newPassword;
                Db.Users.Add(user);
            }
            else
            {
                current!.FullName = user.FullName;
                current.UserName = user.UserName;
                current.Role = user.Role;
                current.Email = user.Email;
                current.Phone = user.Phone;
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    current.Password = newPassword;
                }
            }

            Db.SaveChanges();
            TempData["Message"] = "Đã lưu thành công.";
            return RedirectToAction(nameof(TaiKhoan));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaTaiKhoan(int id)
        {
            var auth = RequireRole("Admin");
            if (auth != null) return auth;

            if (CurrentUser?.Id == id)
            {
                TempData["Message"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(TaiKhoan));
            }

            var user = Db.Users.FirstOrDefault(x => x.Id == id);
            if (user != null)
            {
                Db.Users.Remove(user);
                Db.SaveChanges();
                TempData["Message"] = "Đã xóa tài khoản.";
            }

            return RedirectToAction(nameof(TaiKhoan));
        }
    }
}



