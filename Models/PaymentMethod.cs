using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public enum PaymentMethod
{
    [Display(Name = "Tiền mặt")]
    Cash,

    [Display(Name = "Chuyển khoản")]
    BankTransfer,

    [Display(Name = "Thẻ")]
    Card
}
