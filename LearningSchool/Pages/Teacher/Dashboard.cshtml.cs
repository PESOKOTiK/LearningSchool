using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Teacher
{
    public class DashboardModel : PageModel
    {
        private readonly IConfiguration _config;
        public List<(int ID, string Title)> Courses { get; set; } = new();
        public DashboardModel(IConfiguration config) => _config = config;

        public void OnGet()
        {
            int teacherId = HttpContext.Session.GetInt32("UserID") ?? 0;
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand("SELECT ID, Title FROM Courses WHERE TeacherID=@T", conn);
            cmd.Parameters.AddWithValue("@T", teacherId);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                Courses.Add((rdr.GetInt32(0), rdr.GetString(1)));
        }
    }
}
