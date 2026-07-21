using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace web_do_an1.Models
{
    public class Score
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        [ForeignKey(nameof(CourseClass))]
        public int ClassId { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm giữa kỳ phải từ 0 đến 10.")]
        public double Midterm { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm cuối kỳ phải từ 0 đến 10.")]
        public double Final { get; set; }

        public string Comment { get; set; } = string.Empty;
        public string Result => ((Midterm + Final) / 2) >= 5 ? "Đạt" : "Không đạt";
        public Student? Student { get; set; }
        public CourseClass? CourseClass { get; set; }
    }
}
