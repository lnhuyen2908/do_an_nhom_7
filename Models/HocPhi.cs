using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int EnrollmentId { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Số tiền phải đóng không hợp lệ.")]
        public decimal Amount { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Số tiền đã đóng không hợp lệ.")]
        public decimal PaidAmount { get; set; }

        public string Status { get; set; } = "ChuaDong";
        public string PaymentMethod { get; set; } = "Cash";
        public DateTime? PaidDate { get; set; }
        public Student? Student { get; set; }
        public Enrollment? Enrollment { get; set; }
        public List<PaymentTransaction> Transactions { get; set; } = new();
    }
}
