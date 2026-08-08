using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public enum CourseClassStatus
{
    [Display(Name = "Sắp khai giảng")]
    Upcoming,

    [Display(Name = "Đang mở")]
    Open,

    [Display(Name = "Đã khóa lớp")]
    Locked,

    [Display(Name = "Đã đóng")]
    Closed
}
