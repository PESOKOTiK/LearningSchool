using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LearningSchool.Pages.Admin
{
    public class ViewFeedbackModel : PageModel
    {
        private readonly IConfiguration _config;
        public ViewFeedbackModel(IConfiguration config) => _config = config;

        public List<(string CourseTitle, string StudentName, int Rating, string Comment, DateTime CreatedAt)> Feedbacks
            = new();

        public void OnGet()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            var sql = @"
SELECT c.Title AS CourseTitle,
       CONCAT(s.FirstName, ' ', s.LastName) AS StudentName,
       f.Rating, f.Comment, f.CreatedAt
FROM Feedback f
JOIN Courses c ON f.CourseID = c.ID
JOIN Students s ON f.StudentID = s.ID
ORDER BY f.CreatedAt DESC";

            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Feedbacks.Add((
                  rdr.GetString("CourseTitle"),
                  rdr.GetString("StudentName"),
                  rdr.GetInt32("Rating"),
                  rdr.GetString("Comment"),
                  rdr.GetDateTime("CreatedAt")
                ));
            }
        }
    }
}
