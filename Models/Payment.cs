using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class Payment
{
    public int Id { get; set; }

    [Display(Name = "Học viên")]
    public int StudentId { get; set; }

    [Display(Name = "Đăng ký")]
    public int EnrollmentId { get; set; }

    [Range(0, 1_000_000_000)]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:N0} đ")]
    [Display(Name = "Số tiền phải đóng")]
    public decimal Amount { get; set; }

    [Range(0, 1_000_000_000)]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:N0} đ")]
    [Display(Name = "Số tiền đã đóng")]
    public decimal PaidAmount { get; set; }

    [Display(Name = "Trạng thái")]
    public PaymentState Status { get; set; } = PaymentState.Unpaid;

    [Display(Name = "Phương thức")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [DataType(DataType.Date)]
    [Display(Name = "Ngày thanh toán")]
    public DateTime? PaidDate { get; set; }

    public Student Student { get; set; } = null!;
    public Enrollment Enrollment { get; set; } = null!;
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
