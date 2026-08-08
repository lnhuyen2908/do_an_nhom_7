using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class PaymentTransaction
{
    public int Id { get; set; }

    [Display(Name = "Học phí")]
    public int PaymentId { get; set; }

    [Display(Name = "Học viên")]
    public int StudentId { get; set; }

    [Range(0.01, 1_000_000_000)]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:N0} đ")]
    [Display(Name = "Số tiền")]
    public decimal Amount { get; set; }

    [Display(Name = "Phương thức")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [Display(Name = "Thời gian ghi nhận")]
    public DateTime PaidAt { get; set; } = DateTime.Now;

    [Display(Name = "Trạng thái")]
    public PaymentTransactionState Status { get; set; } = PaymentTransactionState.Pending;

    [Display(Name = "Thời gian duyệt")]
    public DateTime? ApprovedAt { get; set; }

    [StringLength(100)]
    [Display(Name = "Người duyệt")]
    public string ApprovedBy { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Người ghi nhận")]
    public string RecordedBy { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string Note { get; set; } = string.Empty;

    [Display(Name = "Học phí")]
    public Payment Payment { get; set; } = null!;
    [Display(Name = "Học viên")]
    public Student Student { get; set; } = null!;
}
