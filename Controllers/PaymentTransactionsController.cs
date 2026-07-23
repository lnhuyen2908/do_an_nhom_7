using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_do_an1.Data;
using web_do_an1.Models;

namespace web_do_an1.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class PaymentTransactionsController : Controller
    {
        private readonly EnglishCenterDbContext _context;

        public PaymentTransactionsController(EnglishCenterDbContext context)
        {
            _context = context;
        }

        // GET: PaymentTransactions
        public async Task<IActionResult> Index()
        {
            var englishCenterDbContext = _context.PaymentTransactions.Include(p => p.Payment).Include(p => p.Student);
            return View(await englishCenterDbContext.ToListAsync());
        }

        // GET: PaymentTransactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTransaction = await _context.PaymentTransactions
                .Include(p => p.Payment)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentTransaction == null)
            {
                return NotFound();
            }

            return View(paymentTransaction);
        }

        // GET: PaymentTransactions/Create
        public IActionResult Create()
        {
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Id", "Id");
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName");
            return View();
        }

        // POST: PaymentTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PaymentId,StudentId,Amount,PaymentMethod,PaidAt,RecordedBy,Note")] PaymentTransaction paymentTransaction)
        {
            NormalizeTransaction(paymentTransaction);
            ModelState.Clear();
            TryValidateModel(paymentTransaction);
            await ValidateTransactionAsync(paymentTransaction);

            if (ModelState.IsValid)
            {
                _context.Add(paymentTransaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm giao dịch thanh toán.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Id", "Id", paymentTransaction.PaymentId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", paymentTransaction.StudentId);
            return View(paymentTransaction);
        }

        // GET: PaymentTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTransaction = await _context.PaymentTransactions.FindAsync(id);
            if (paymentTransaction == null)
            {
                return NotFound();
            }
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Id", "Id", paymentTransaction.PaymentId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", paymentTransaction.StudentId);
            return View(paymentTransaction);
        }

        // POST: PaymentTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PaymentId,StudentId,Amount,PaymentMethod,PaidAt,RecordedBy,Note")] PaymentTransaction paymentTransaction)
        {
            if (id != paymentTransaction.Id)
            {
                return NotFound();
            }

            NormalizeTransaction(paymentTransaction);
            ModelState.Clear();
            TryValidateModel(paymentTransaction);
            await ValidateTransactionAsync(paymentTransaction);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentTransactionExists(paymentTransaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Đã cập nhật giao dịch thanh toán.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Id", "Id", paymentTransaction.PaymentId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", paymentTransaction.StudentId);
            return View(paymentTransaction);
        }

        // GET: PaymentTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTransaction = await _context.PaymentTransactions
                .Include(p => p.Payment)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentTransaction == null)
            {
                return NotFound();
            }

            return View(paymentTransaction);
        }

        // POST: PaymentTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentTransaction = await _context.PaymentTransactions.FindAsync(id);
            if (paymentTransaction != null)
            {
                _context.PaymentTransactions.Remove(paymentTransaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa giao dịch thanh toán.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PaymentTransactionExists(int id)
        {
            return _context.PaymentTransactions.Any(e => e.Id == id);
        }

        private static void NormalizeTransaction(PaymentTransaction transaction)
        {
            transaction.RecordedBy = transaction.RecordedBy.Trim();
            transaction.Note = transaction.Note.Trim();
        }

        private async Task ValidateTransactionAsync(PaymentTransaction transaction)
        {
            var payment = await _context.Payments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == transaction.PaymentId);
            if (payment is null)
            {
                ModelState.AddModelError(nameof(PaymentTransaction.PaymentId), "Vui lòng chọn học phí hợp lệ.");
            }

            if (!await _context.Students.AnyAsync(x => x.Id == transaction.StudentId))
            {
                ModelState.AddModelError(nameof(PaymentTransaction.StudentId), "Vui lòng chọn học viên hợp lệ.");
            }
            else if (payment is not null && payment.StudentId != transaction.StudentId)
            {
                ModelState.AddModelError(nameof(PaymentTransaction.StudentId), "Học viên không khớp với học phí đã chọn.");
            }
        }
    }
}
