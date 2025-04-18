using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Student
{
    public class ViewAssignmentsModel : PageModel
    {
        private readonly IConfiguration _config;
        public ViewAssignmentsModel(IConfiguration config) => _config = config;

        [BindProperty(SupportsGet = true)] public int LessonID { get; set; }
        public string LessonTitle { get; set; }

        public List<(string Title, string Description, DateTime DueDate)> Assignments { get; set; }
            = new();

        public void OnGet()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            // Lesson title
            using (var cmd = new MySqlCommand("SELECT Title FROM Lessons WHERE ID=@ID", conn))
            {
                cmd.Parameters.AddWithValue("@ID", LessonID);
                LessonTitle = cmd.ExecuteScalar()?.ToString() ?? "";
            }

            // Assignments
            var sql = "SELECT Title, Description, DueDate FROM Assignments WHERE LessonID=@L";
            using var cmd2 = new MySqlCommand(sql, conn);
            cmd2.Parameters.AddWithValue("@L", LessonID);
            using var rdr = cmd2.ExecuteReader();

            while (rdr.Read())
                Assignments.Add((
                    rdr.GetString(0),
                    rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                    rdr.GetDateTime(2)
                ));
        }
    }
}
