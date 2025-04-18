using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Student
{
    public class DashboardModel : PageModel
    {
        private readonly IConfiguration _config;
        public List<(int ID, string Title)> Available { get; set; } = new();
        public List<(int ID, string Title)> Enrolled { get; set; } = new();

        public DashboardModel(IConfiguration config) => _config = config;

        public void OnGet()
        {
            int studentId = HttpContext.Session.GetInt32("UserID") ?? 0;
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            // Available courses (not yet enrolled)
            var sqlAvail = @"
              SELECT ID, Title 
              FROM Courses 
              WHERE ID NOT IN (
                SELECT CourseID FROM Enrollments WHERE StudentID=@S
              )";
            using var cmdA = new MySqlCommand(sqlAvail, conn);
            cmdA.Parameters.AddWithValue("@S", studentId);
            using var rdrA = cmdA.ExecuteReader();
            while (rdrA.Read())
                Available.Add((rdrA.GetInt32(0), rdrA.GetString(1)));
            rdrA.Close();

            // Enrolled courses
            var sqlEnr = @"
              SELECT c.ID, c.Title 
              FROM Courses c
              JOIN Enrollments e ON c.ID=e.CourseID
              WHERE e.StudentID=@S";
            using var cmdE = new MySqlCommand(sqlEnr, conn);
            cmdE.Parameters.AddWithValue("@S", studentId);
            using var rdrE = cmdE.ExecuteReader();
            while (rdrE.Read())
                Enrolled.Add((rdrE.GetInt32(0), rdrE.GetString(1)));
        }
    }
}
