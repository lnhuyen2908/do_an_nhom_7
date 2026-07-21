using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public partial class QuanTriController
    {
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
    }
}
