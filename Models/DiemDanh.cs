namespace web_do_an1.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime StudyDate { get; set; } = DateTime.Today;
        public bool IsPresent { get; set; }
        public string Note { get; set; } = string.Empty;
        public Student? Student { get; set; }
        public CourseClass? CourseClass { get; set; }
    }
}
