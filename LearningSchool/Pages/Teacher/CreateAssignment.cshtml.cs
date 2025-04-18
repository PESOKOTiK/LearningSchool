using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Teacher
{
    public class CreateAssignmentModel : PageModel
    {
        private readonly IConfiguration _config;
        public CreateAssignmentModel(IConfiguration config) => _config = config;

        [BindProperty] public int CourseID { get; set; }
        [BindProperty] public int LessonID { get; set; }
        [BindProperty] public string Title { get; set; }
        [BindProperty] public string Description { get; set; }
        [BindProperty] public DateTime DueDate { get; set; }

        public List<(int ID, string Title)> LessonOptions { get; set; } = new();
        public string Message { get; set; }

        public void OnGet(int courseId)
        {
            CourseID = courseId;
            LoadLessons();
        }

        public IActionResult OnPost()
        {
            LoadLessons();
            try
            {
                using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
                conn.Open();
                var cmd = new MySqlCommand(
                  "INSERT INTO Assignments (LessonID, Title, Description, DueDate) " +
                  "VALUES (@L,@T,@D,@DD)", conn);
                cmd.Parameters.AddWithValue("@L", LessonID);
                cmd.Parameters.AddWithValue("@T", Title);
                cmd.Parameters.AddWithValue("@D", Description);
                cmd.Parameters.AddWithValue("@DD", DueDate);
                cmd.ExecuteNonQuery();
                Message = "Assignment created.";
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }
            return Page();
        }

        private void LoadLessons()
        {
            LessonOptions.Clear();
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            var sql = @"SELECT l.ID, l.Title 
                        FROM Lessons l
                        JOIN Modules m ON l.ModuleID = m.ID
                        JOIN Courses c ON m.CourseID = c.ID
                        WHERE c.TeacherID = @T";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@T", HttpContext.Session.GetInt32("UserID"));
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                LessonOptions.Add((rdr.GetInt32(0), rdr.GetString(1)));
        }
    }
}
