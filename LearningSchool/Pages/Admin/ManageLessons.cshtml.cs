using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearningSchool.Pages.Admin
{
    public class ManageLessonsModel : PageModel
    {
        private readonly IConfiguration _config;

        public ManageLessonsModel(IConfiguration config) => _config = config;

        // Dropdown options & list
        public List<(int ID, string Title)> ModuleOptions { get; set; } = new();
        public List<(int ID, int ModuleID, string ModuleTitle, string Title, string ContentURL)> LessonList { get; set; }
            = new();

        // Bound properties for form
        [BindProperty] public int? EditID { get; set; }

        [BindProperty, Required]
        public int ModuleID { get; set; }

        [BindProperty, Required]
        public string Title { get; set; }

        [BindProperty, Required, Url]
        public string ContentURL { get; set; }

        public string Message { get; set; }

        public void OnGet(int? editId, int? deleteId)
        {
            LoadData();

            if (deleteId.HasValue)
            {
                DeleteLesson(deleteId.Value);
                Response.Redirect("/Admin/ManageLessons");
                return;
            }

            if (editId.HasValue)
            {
                EditID = editId;
                var lesson = LessonList.Find(x => x.ID == editId.Value);
                ModuleID = lesson.ModuleID;
                Title = lesson.Title;
                ContentURL = lesson.ContentURL;
            }
        }

        public IActionResult OnPost()
        {
            LoadData();

            if (!ModelState.IsValid)
            {
                Message = "Please fix validation errors.";
                return Page();
            }

            try
            {
                if (EditID.HasValue)
                    UpdateLesson(EditID.Value);
                else
                    InsertLesson();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
                return Page();
            }
        }

        private void LoadData()
        {
            string cs = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(cs);
            conn.Open();

            // Load modules for dropdown
            using (var cmd = new MySqlCommand("SELECT ID, Title FROM Modules", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    ModuleOptions.Add((rdr.GetInt32(0), rdr.GetString(1)));
                rdr.Close();
            }

            // Load existing lessons
            const string sql = @"
                SELECT l.ID, l.ModuleID, m.Title AS ModuleTitle, l.Title, l.ContentURL
                FROM Lessons l
                JOIN Modules m ON l.ModuleID = m.ID";
            using (var cmd = new MySqlCommand(sql, conn))
            using (var rdr = cmd.ExecuteReader())
                while (rdr.Read())
                    LessonList.Add((
                        rdr.GetInt32(0),
                        rdr.GetInt32(1),
                        rdr.GetString(2),
                        rdr.GetString(3),
                        rdr.GetString(4)
                    ));
        }

        private void InsertLesson()
        {
            string cs = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(cs);
            conn.Open();
            using var cmd = new MySqlCommand(
                "INSERT INTO Lessons (ModuleID, Title, ContentURL) VALUES (@M, @T, @U)", conn);
            cmd.Parameters.AddWithValue("@M", ModuleID);
            cmd.Parameters.AddWithValue("@T", Title);
            cmd.Parameters.AddWithValue("@U", ContentURL);
            cmd.ExecuteNonQuery();
        }

        private void UpdateLesson(int id)
        {
            string cs = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(cs);
            conn.Open();
            using var cmd = new MySqlCommand(
                "UPDATE Lessons SET ModuleID=@M, Title=@T, ContentURL=@U WHERE ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@M", ModuleID);
            cmd.Parameters.AddWithValue("@T", Title);
            cmd.Parameters.AddWithValue("@U", ContentURL);
            cmd.ExecuteNonQuery();
        }

        private void DeleteLesson(int id)
        {
            string cs = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(cs);
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM Lessons WHERE ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.ExecuteNonQuery();
        }
    }
}
