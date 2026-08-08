using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public enum PaymentTransactionState
{
    [Display(Name = "Chờ duyệt")]
    Pending,

    [Display(Name = "Thành công")]
    Approved,

    [Display(Name = "Từ chối")]
    Rejected
}
