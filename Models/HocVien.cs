using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class Student
    {
        public int Id { get; set; }

        [RegularExpression(@"^$|^HV\d{2,}$", ErrorMessage = "Mã học viên phải có dạng HV01.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên học viên.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email học viên.")]
        [EmailAddress(ErrorMessage = "Email học viên chưa đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại học viên.")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại phải gồm 9 đến 11 chữ số.")]
        public string Phone { get; set; } = string.Empty;

        [NgaySinhHopLe]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ học viên.")]
        public string Address { get; set; } = string.Empty;

        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<PaymentTransaction> PaymentTransactions { get; set; } = new();
        public List<Score> Scores { get; set; } = new();
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
        public List<SavedCourse> SavedCourses { get; set; } = new();
    }
}
