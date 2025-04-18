// EnrollStudent.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Teacher
{
    public class EnrollStudentModel : PageModel
    {
        private readonly IConfiguration _config;
        public EnrollStudentModel(IConfiguration config) => _config = config;

        [BindProperty] public int CourseID { get; set; }
        [BindProperty] public int StudentID { get; set; }
        public List<(int ID, string Name)> StudentOptions { get; set; } = new();
        public string Message { get; set; }

        public void OnGet(int courseId)
        {
            CourseID = courseId;
            LoadStudents();
        }

        public IActionResult OnPost()
        {
            LoadStudents();
            try
            {
                using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
                conn.Open();
                var cmd = new MySqlCommand(
                  "INSERT INTO Enrollments (StudentID, CourseID, EnrollmentDate, Status) " +
                  "VALUES (@S,@C, CURDATE(), 'Active')", conn);
                cmd.Parameters.AddWithValue("@S", StudentID);
                cmd.Parameters.AddWithValue("@C", CourseID);
                cmd.ExecuteNonQuery();
                Message = "Student enrolled successfully.";
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }
            return Page();
        }

        private void LoadStudents()
        {
            StudentOptions.Clear();
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand("SELECT ID, CONCAT(FirstName,' ',LastName) FROM Students", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                StudentOptions.Add((rdr.GetInt32(0), rdr.GetString(1)));
        }
    }
}
