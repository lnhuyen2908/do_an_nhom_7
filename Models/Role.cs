using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class Role
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Tên vai trò")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Tên hiển thị")]
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
}
