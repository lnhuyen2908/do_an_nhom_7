namespace web_do_an1.Models
{
    public class SavedCourse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.Now;
        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }
}
