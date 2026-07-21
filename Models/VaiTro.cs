using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class RoleItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên vai trò.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên hiển thị của vai trò.")]
        public string DisplayName { get; set; } = string.Empty;

        public List<UserAccount> Users { get; set; } = new();
    }
}
