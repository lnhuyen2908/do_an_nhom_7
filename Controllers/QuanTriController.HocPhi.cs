using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using web_do_an1.Models;
using web_do_an1.Services;

namespace web_do_an1.Controllers
{
    public partial class QuanTriController
    {
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
    }
}
