using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class Notification
{
    public int Id { get; set; }

    [Display(Name = "Tài khoản nhận")]
    public int UserAccountId { get; set; }

    [Required, StringLength(160)]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(800)]
    [Display(Name = "Nội dung")]
    public string Message { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Liên kết")]
    public string Url { get; set; } = string.Empty;

    [Display(Name = "Đã đọc")]
    public bool IsRead { get; set; }

    [Display(Name = "Thời gian")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public UserAccount UserAccount { get; set; } = null!;
}
