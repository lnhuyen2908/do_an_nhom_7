using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public int StudentId { get; set; }

        [Range(-1000000000, 1000000000, ErrorMessage = "Số tiền giao dịch không hợp lệ.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; } = "Cash";

        public DateTime PaidAt { get; set; } = DateTime.Now;
        public string RecordedBy { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public Payment? Payment { get; set; }
        public Student? Student { get; set; }
    }
}
