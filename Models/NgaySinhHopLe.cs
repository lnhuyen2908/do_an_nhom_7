using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public sealed class NgaySinhHopLeAttribute : ValidationAttribute
    {
        public NgaySinhHopLeAttribute()
        {
            ErrorMessage = "Ngày sinh không hợp lệ. Học viên phải từ 5 đến 100 tuổi.";
        }

        public override bool IsValid(object? value)
        {
            if (value is not DateTime dateOfBirth)
            {
                return false;
            }

            var today = DateTime.Today;
            return dateOfBirth.Date <= today.AddYears(-5)
                && dateOfBirth.Date >= today.AddYears(-100);
        }
    }
}
