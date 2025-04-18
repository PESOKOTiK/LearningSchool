using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LearningSchool.Pages.Student
{
    public class CourseContentModel : PageModel
    {
        private readonly IConfiguration _config;
        public CourseContentModel(IConfiguration config) => _config = config;

        [BindProperty(SupportsGet = true)]
        public int CourseID { get; set; }
        public string Title { get; set; }

        public List<ModuleItem> Modules { get; set; } = new();

        public class ModuleItem
        {
            public int ID;
            public string Title;
            public List<LessonItem> Lessons = new();
        }

        public class LessonItem
        {
            public int ID;
            public string Title;
            public string ContentURL;            // <-- added
            public List<AssignmentItem> Assignments = new();
        }

        public class AssignmentItem
        {
            public int ID;
            public string Title;
            public DateTime DueDate;
        }

        public void OnGet()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            // Load course title
            using (var cmd = new MySqlCommand("SELECT Title FROM Courses WHERE ID=@ID", conn))
            {
                cmd.Parameters.AddWithValue("@ID", CourseID);
                Title = cmd.ExecuteScalar()?.ToString() ?? "";
            }

            // Pull modules, lessons (incl. ContentURL), assignments
            var sql = @"
SELECT 
  m.ID   AS ModuleID,   m.Title AS ModuleTitle,
  l.ID   AS LessonID,   l.Title AS LessonTitle,   l.ContentURL,
  a.ID   AS AssignID,   a.Title AS AssignTitle,    a.DueDate
FROM Modules m
LEFT JOIN Lessons l  ON l.ModuleID    = m.ID
LEFT JOIN Assignments a ON a.LessonID  = l.ID
WHERE m.CourseID = @C
ORDER BY m.ID, l.ID, a.ID";

            using var cmd2 = new MySqlCommand(sql, conn);
            cmd2.Parameters.AddWithValue("@C", CourseID);

            ModuleItem currentMod = null;
            LessonItem currentLes = null;

            using var rdr = cmd2.ExecuteReader();
            while (rdr.Read())
            {
                // New module?
                var mId = rdr.GetInt32("ModuleID");
                if (currentMod == null || currentMod.ID != mId)
                {
                    currentMod = new ModuleItem
                    {
                        ID = mId,
                        Title = rdr.GetString("ModuleTitle")
                    };
                    Modules.Add(currentMod);
                    currentLes = null;
                }

                // Lesson row present?
                if (!rdr.IsDBNull(rdr.GetOrdinal("LessonID")))
                {
                    var lId = rdr.GetInt32("LessonID");
                    if (currentLes == null || currentLes.ID != lId)
                    {
                        currentLes = new LessonItem
                        {
                            ID = lId,
                            Title = rdr.GetString("LessonTitle"),
                            ContentURL = rdr.IsDBNull(rdr.GetOrdinal("ContentURL"))
                                          ? "#"
                                          : rdr.GetString("ContentURL")
                        };
                        currentMod.Lessons.Add(currentLes);
                    }

                    // Assignment row present?
                    if (!rdr.IsDBNull(rdr.GetOrdinal("AssignID")))
                    {
                        currentLes.Assignments.Add(new AssignmentItem
                        {
                            ID = rdr.GetInt32("AssignID"),
                            Title = rdr.GetString("AssignTitle"),
                            DueDate = rdr.GetDateTime("DueDate")
                        });
                    }
                }
            }
        }
    }
}
