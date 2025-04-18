using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LearningSchool.Pages.Student
{
    public class ViewAssignmentsModel : PageModel
    {
        private readonly IConfiguration _config;
        public ViewAssignmentsModel(IConfiguration config) => _config = config;

        [BindProperty(SupportsGet = true)]
        public int LessonID { get; set; }

        public string LessonTitle { get; set; }

        public List<(int ID, string Title, string Description, DateTime DueDate)> Assignments { get; set; }
            = new();

        public void OnGet()
        {
            string cs = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(cs);
            conn.Open();

            // Load lesson title
            using (var cmd = new MySqlCommand("SELECT Title FROM Lessons WHERE ID = @ID", conn))
            {
                cmd.Parameters.AddWithValue("@ID", LessonID);
                LessonTitle = cmd.ExecuteScalar()?.ToString() ?? "";
            }

            // Load assignments
            using (var cmd = new MySqlCommand(
                "SELECT ID, Title, Description, DueDate FROM Assignments WHERE LessonID = @L", conn))
            {
                cmd.Parameters.AddWithValue("@L", LessonID);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Assignments.Add((
                        rdr.GetInt32("ID"),
                        rdr.GetString("Title"),
                        rdr.IsDBNull(rdr.GetOrdinal("Description"))
                            ? ""
                            : rdr.GetString("Description"),
                        rdr.GetDateTime("DueDate")
                    ));
                }
            }
        }
    }
}
