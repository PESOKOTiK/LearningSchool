using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Student
{
    public class BuyCourseModel : PageModel
    {
        private readonly IConfiguration _config;
        public BuyCourseModel(IConfiguration config) => _config = config;

        [BindProperty(SupportsGet = true)] public int CourseID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public void OnGet()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand("SELECT Title FROM Courses WHERE ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", CourseID);
            Title = cmd.ExecuteScalar()?.ToString() ?? "";
        }

        public IActionResult OnPost()
        {
            int studentId = HttpContext.Session.GetInt32("UserID") ?? 0;
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            try
            {
                // Simulate payment
                using var pay = new MySqlCommand(
                  "INSERT INTO Payments (StudentID, CourseID, Amount, PaidAt, Status) " +
                  "VALUES (@S,@C, 0.00, NOW(), 'Paid')", conn);
                pay.Parameters.AddWithValue("@S", studentId);
                pay.Parameters.AddWithValue("@C", CourseID);
                pay.ExecuteNonQuery();

                // Enroll
                using var enr = new MySqlCommand(
                  "INSERT INTO Enrollments (StudentID, CourseID, EnrollmentDate, Status) " +
                  "VALUES (@S,@C, CURDATE(), 'Active')", conn);
                enr.Parameters.AddWithValue("@S", studentId);
                enr.Parameters.AddWithValue("@C", CourseID);
                enr.ExecuteNonQuery();

                Message = "Payment successful and enrolled!";
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }

            return Page();
        }
    }
}
