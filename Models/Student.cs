using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

// Student = Học viên. Mỗi đối tượng Student tương ứng với một dòng trong bảng Students.
// IValidatableObject cho phép class tự viết thêm quy tắc kiểm tra dữ liệu bằng hàm Validate ở cuối file.
public class Student : IValidatableObject
{
    // Khóa chính của học viên; database tự tăng giá trị này khi thêm học viên mới.
    public int Id { get; set; }

    // Required = bắt buộc nhập; StringLength(20) = tối đa 20 ký tự.
    [Required, StringLength(20)]
    // Mã phải bắt đầu bằng ST và có ít nhất hai chữ số, ví dụ ST01.
    [RegularExpression(@"^ST\d{2,}$", ErrorMessage = "Mã học viên phải có dạng ST01.")]
    [Display(Name = "Mã học viên")]
    public string Code { get; set; } = string.Empty;

    // Họ tên bắt buộc nhập và không dài quá 100 ký tự.
    [Required, StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    // Email bắt buộc, phải đúng định dạng email và tối đa 150 ký tự.
    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    // Số điện thoại bắt buộc, phải đúng định dạng số điện thoại và tối đa 20 ký tự.
    [Required, Phone, StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    // DataType.Date yêu cầu giao diện chỉ nhập phần ngày; giá trị mặc định là ngày cách hiện tại 18 năm.
    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

    // Địa chỉ bắt buộc nhập và tối đa 250 ký tự.
    [Required, StringLength(250)]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    // Một học viên có thể liên kết với một tài khoản đăng nhập; dấu ? nghĩa là có thể chưa có tài khoản.
    public UserAccount? UserAccount { get; set; }
    // Một học viên có thể có nhiều phiếu đăng ký khóa học.
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    // Một học viên có thể có nhiều khoản học phí.
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    // Một học viên có thể có nhiều lần giao dịch thanh toán.
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    // Một học viên có thể có nhiều kết quả điểm.
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    // Một học viên có thể có nhiều bản ghi điểm danh.
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    // Một học viên có thể lưu nhiều khóa học yêu thích.
    public ICollection<SavedCourse> SavedCourses { get; set; } = new List<SavedCourse>();

    // Validate = kiểm tra dữ liệu tùy chỉnh ngoài các thuộc tính Required, EmailAddress, Phone ở trên.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Ngày sinh phải nằm trong khoảng khiến học viên có độ tuổi từ 5 đến 100.
        if (DateOfBirth.Date > DateTime.Today.AddYears(-5) ||
            DateOfBirth.Date < DateTime.Today.AddYears(-100))
        {
            // yield return trả lỗi về ModelState và gắn lỗi vào trường DateOfBirth.
            yield return new ValidationResult(
                "Học viên phải từ 5 đến 100 tuổi.",
                new[] { nameof(DateOfBirth) });
        }
    }
}
