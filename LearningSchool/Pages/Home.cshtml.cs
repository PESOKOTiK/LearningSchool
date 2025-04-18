using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace LearningSchool.Pages
{

    public class HomeModel : PageModel
    {
        public IConfiguration _config;
        public string Role { get; set; }

        public List<string> AvailableCourses { get; set; } = new();
        public List<string> EnrolledCourses { get; set; } = new();
        public List<string> StudentAssignments { get; set; } = new();

        public List<string> TeacherCourses { get; set; } = new();
        public List<string> EnrolledStudents { get; set; } = new();
        public List<string> AssignmentSubmissions { get; set; } = new();

        public List<string> AllCourses { get; set; } = new();
        public List<string> AllStudents { get; set; } = new();

        [BindProperty]
        public string NewCourseTitle { get; set; }

        public HomeModel(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            Role = HttpContext.Session.GetString("UserRole");

            string connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            if (Role == "Student")
            {
                using var cmd1 = new MySqlCommand("SELECT Title FROM Courses", conn);
                using var reader1 = cmd1.ExecuteReader();
                while (reader1.Read()) AvailableCourses.Add(reader1.GetString(0));
                reader1.Close();

                using var cmd2 = new MySqlCommand(@"
                SELECT c.Title FROM Enrollments e
                JOIN Courses c ON e.CourseID = c.ID
                WHERE e.StudentID = @StudentID", conn);
                cmd2.Parameters.AddWithValue("@StudentID", userId);
                using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read()) EnrolledCourses.Add(reader2.GetString(0));
                reader2.Close();

                using var cmd3 = new MySqlCommand(@"
                SELECT a.Title FROM Submissions s
                JOIN Assignments a ON s.AssignmentID = a.ID
                WHERE s.StudentID = @StudentID", conn);
                cmd3.Parameters.AddWithValue("@StudentID", userId);
                using var reader3 = cmd3.ExecuteReader();
                while (reader3.Read()) StudentAssignments.Add(reader3.GetString(0));
            }
            else if (Role == "Teacher")
            {
                using var cmd1 = new MySqlCommand("SELECT Title FROM Courses WHERE TeacherID = @TeacherID", conn);
                cmd1.Parameters.AddWithValue("@TeacherID", userId);
                using var reader1 = cmd1.ExecuteReader();
                while (reader1.Read()) TeacherCourses.Add(reader1.GetString(0));
                reader1.Close();

                using var cmd2 = new MySqlCommand(@"
                SELECT s.FirstName FROM Enrollments e
                JOIN Students s ON e.StudentID = s.ID
                JOIN Courses c ON e.CourseID = c.ID
                WHERE c.TeacherID = @TeacherID", conn);
                cmd2.Parameters.AddWithValue("@TeacherID", userId);
                using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read()) EnrolledStudents.Add(reader2.GetString(0));
                reader2.Close();

                using var cmd3 = new MySqlCommand(@"
                SELECT FileURL FROM Submissions s
                JOIN Assignments a ON s.AssignmentID = a.ID
                JOIN Lessons l ON a.LessonID = l.ID
                JOIN Modules m ON l.ModuleID = m.ID
                JOIN Courses c ON m.CourseID = c.ID
                WHERE c.TeacherID = @TeacherID", conn);
                cmd3.Parameters.AddWithValue("@TeacherID", userId);
                using var reader3 = cmd3.ExecuteReader();
                while (reader3.Read()) AssignmentSubmissions.Add(reader3.GetString(0));
            }
            else if (Role == "Admin")
            {
                using var cmd1 = new MySqlCommand("SELECT Title FROM Courses", conn);
                using var reader1 = cmd1.ExecuteReader();
                while (reader1.Read()) AllCourses.Add(reader1.GetString(0));
                reader1.Close();

                using var cmd2 = new MySqlCommand("SELECT FirstName FROM Students", conn);
                using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read()) AllStudents.Add(reader2.GetString(0));
            }
        }

        public IActionResult OnPost()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin") return Unauthorized();

            string connStr = "your_connection_string_here";
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            var cmd = new MySqlCommand("INSERT INTO Courses (Title, Description, Level, Language, TeacherID, CategoryID) VALUES (@Title, '', 'Beginner', 'English', 1, 1);", conn);
            cmd.Parameters.AddWithValue("@Title", NewCourseTitle);
            cmd.ExecuteNonQuery();

            return RedirectToPage("/Dashboard");
        }
    }
}
