using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public enum EnrollmentState
{
    [Display(Name = "Chờ duyệt")]
    Pending,

    [Display(Name = "Đã duyệt")]
    Approved,

    [Display(Name = "Đã hủy")]
    Cancelled
}
