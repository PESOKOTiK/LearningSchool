using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace LearningSchool.Pages.Student
{
    public class FeedbackModel : PageModel
    {
        private readonly IConfiguration _config;
        public FeedbackModel(IConfiguration config) => _config = config;

        public List<(int ID, string Title)> EnrolledCourses { get; set; } = new();

        [BindProperty, Required]
        public int CourseID { get; set; }

        [BindProperty, Required, Range(1, 5)]
        public int Rating { get; set; }

        [BindProperty]
        public string Comment { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            LoadEnrolled();
        }

        public IActionResult OnPost()
        {
            LoadEnrolled();

            if (!ModelState.IsValid)
            {
                Message = "Please fix the errors and try again.";
                return Page();
            }

            int studentId = HttpContext.Session.GetInt32("UserID") ?? 0;
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            try
            {
                using var cmd = new MySqlCommand(
                    "INSERT INTO Feedback (StudentID, CourseID, Rating, Comment, CreatedAt) " +
                    "VALUES (@S,@C,@R,@Cm,NOW())", conn);
                cmd.Parameters.AddWithValue("@S", studentId);
                cmd.Parameters.AddWithValue("@C", CourseID);
                cmd.Parameters.AddWithValue("@R", Rating);
                cmd.Parameters.AddWithValue("@Cm", Comment ?? "");
                cmd.ExecuteNonQuery();
                Message = "Thank you for your feedback!";
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }

            return Page();
        }

        private void LoadEnrolled()
        {
            EnrolledCourses.Clear();
            int studentId = HttpContext.Session.GetInt32("UserID") ?? 0;
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand(
                "SELECT c.ID, c.Title " +
                "FROM Courses c JOIN Enrollments e ON c.ID=e.CourseID " +
                "WHERE e.StudentID=@S", conn);
            cmd.Parameters.AddWithValue("@S", studentId);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                EnrolledCourses.Add((rdr.GetInt32(0), rdr.GetString(1)));
        }
    }
}
