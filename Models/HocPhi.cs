using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace web_do_an1.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        [ForeignKey(nameof(Enrollment))]
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
        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}
