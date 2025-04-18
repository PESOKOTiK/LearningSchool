using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Teacher
{
    public class ViewSubmissionsModel : PageModel
    {
        private readonly IConfiguration _config;
        public ViewSubmissionsModel(IConfiguration config) => _config = config;

        [BindProperty(SupportsGet = true)] public int CourseID { get; set; }
        [BindProperty(SupportsGet = true)] public int? SelectedAssignmentID { get; set; }

        public List<(int ID, string Title)> AssignmentOptions { get; set; } = new();
        public List<(string StudentName, string FileURL, DateTime SubmittedAt, decimal? Grade)> Submissions
            = new();

        public void OnGet()
        {
            LoadAssignments();

            if (SelectedAssignmentID.HasValue)
            {
                LoadSubmissions();
            }
        }

        private void LoadAssignments()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            var sql = @"SELECT a.ID, a.Title
                        FROM Assignments a
                        JOIN Lessons l ON a.LessonID = l.ID
                        JOIN Modules m ON l.ModuleID = m.ID
                        JOIN Courses c ON m.CourseID = c.ID
                        WHERE c.TeacherID = @T";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@T", HttpContext.Session.GetInt32("UserID"));
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                AssignmentOptions.Add((rdr.GetInt32(0), rdr.GetString(1)));
        }

        private void LoadSubmissions()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            var sql = @"SELECT CONCAT(s.FirstName,' ',s.LastName) AS StudentName,
                               sub.FileURL, sub.SubmittedAt, sub.Grade
                        FROM Submissions sub
                        JOIN Students s ON sub.StudentID = s.ID
                        WHERE sub.AssignmentID = @A";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@A", SelectedAssignmentID.Value);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                Submissions.Add((
                    rdr.GetString(0),
                    rdr.GetString(1),
                    rdr.GetDateTime(2),
                    rdr.IsDBNull(3) ? (decimal?)null : rdr.GetDecimal(3)
                ));
        }
    }
}
