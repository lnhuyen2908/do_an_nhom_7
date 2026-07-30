using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public enum PaymentState
{
    [Display(Name = "Chưa thanh toán")]
    Unpaid,

    [Display(Name = "Đã thanh toán")]
    Paid,

    [Display(Name = "Đã hủy")]
    Cancelled
}
