using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Admin
{
    public class AdminDashboardModel : PageModel
    {
        private readonly IConfiguration _config;
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public List<string> CourseTitles { get; set; } = new();
        public List<int> CourseEnrollments { get; set; } = new();

        public AdminDashboardModel(IConfiguration config) => _config = config;

        public void OnGet()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            TotalCourses = new MySqlCommand("SELECT COUNT(*) FROM Courses", conn).ExecuteScalarAsInt();
            TotalStudents = new MySqlCommand("SELECT COUNT(*) FROM Students", conn).ExecuteScalarAsInt();
            TotalTeachers = new MySqlCommand("SELECT COUNT(*) FROM Teachers", conn).ExecuteScalarAsInt();

            var cmd = new MySqlCommand("SELECT c.Title, COUNT(e.ID) FROM Courses c LEFT JOIN Enrollments e ON c.ID = e.CourseID GROUP BY c.ID", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CourseTitles.Add(reader.GetString(0));
                CourseEnrollments.Add(reader.GetInt32(1));
            }
        }
    }
    public static class MySqlExtensions
    {
        public static int ExecuteScalarAsInt(this MySqlCommand cmd)
        {
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
