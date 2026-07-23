using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public enum PaymentState
{
    [Display(Name = "Chưa thanh toán")]
    Unpaid,

    [Display(Name = "Thanh toán một phần")]
    PartiallyPaid,

    [Display(Name = "Đã thanh toán")]
    Paid
}
