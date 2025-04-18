using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System.IO;
using System;

namespace LearningSchool.Pages.Student
{
    public class SubmitAssignmentModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public SubmitAssignmentModel(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        [BindProperty(SupportsGet = true)]
        public int AssignmentID { get; set; }

        public string AssignmentTitle { get; set; }

        [BindProperty]
        public IFormFile SubmissionFile { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand("SELECT Title FROM Assignments WHERE ID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", AssignmentID);
            AssignmentTitle = cmd.ExecuteScalar()?.ToString() ?? "";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (SubmissionFile == null || SubmissionFile.Length == 0)
            {
                Message = "Please select a file to upload.";
                OnGet();
                return Page();
            }

            // Save file
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(SubmissionFile.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = System.IO.File.Create(filePath))
                await SubmissionFile.CopyToAsync(stream);

            var fileUrl = $"/uploads/{fileName}";
            var studentId = HttpContext.Session.GetInt32("UserID") ?? 0;

            try
            {
                using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
                conn.Open();
                using var cmd = new MySqlCommand(@"
                    INSERT INTO Submissions (AssignmentID, StudentID, FileURL, SubmittedAt)
                    VALUES (@A, @S, @F, @Now)", conn);
                cmd.Parameters.AddWithValue("@A", AssignmentID);
                cmd.Parameters.AddWithValue("@S", studentId);
                cmd.Parameters.AddWithValue("@F", fileUrl);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.ExecuteNonQuery();

                Message = "Submission uploaded successfully.";
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }

            OnGet();
            return Page();
        }
    }
}
