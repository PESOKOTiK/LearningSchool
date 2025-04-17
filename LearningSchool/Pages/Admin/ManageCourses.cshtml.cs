using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace LearningSchool.Pages.Admin
{
    public class ManageCoursesModel : PageModel
    {
        private readonly IConfiguration _config;
        public List<(int ID, string Title, string Level, string TeacherName)> Courses { get; set; } = new();
        public List<(int ID, string FullName)> Teachers { get; set; } = new();
        public List<(int ID, string Name)> Categories { get; set; } = new();

        public int EditID { get; set; }
        public string FormTitle { get; set; } = "";
        public string FormLevel { get; set; } = "Beginner";
        public string FormLanguage { get; set; } = "English";
        public int FormTeacherID { get; set; }
        public int FormCategoryID { get; set; }

        public ManageCoursesModel(IConfiguration config) => _config = config;

        public void OnGet()
        {
            LoadDropdowns();
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            if (int.TryParse(Request.Query["editId"], out int editId))
            {
                EditID = editId;
                var cmd = new MySqlCommand("SELECT Title, Level, Language, TeacherID, CategoryID FROM Courses WHERE ID=@ID", conn);
                cmd.Parameters.AddWithValue("@ID", editId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    FormTitle = reader.GetString(0);
                    FormLevel = reader.GetString(1);
                    FormLanguage = reader.GetString(2);
                    FormTeacherID = reader.GetInt32(3);
                    FormCategoryID = reader.GetInt32(4);
                }
            }
            else if (int.TryParse(Request.Query["deleteId"], out int deleteId))
            {
                var delCmd = new MySqlCommand("DELETE FROM Courses WHERE ID=@ID", conn);
                delCmd.Parameters.AddWithValue("@ID", deleteId);
                delCmd.ExecuteNonQuery();
                Response.Redirect("/Admin/ManageCourses");
            }

            var listCmd = new MySqlCommand("SELECT c.ID, c.Title, c.Level, t.FullName FROM Courses c JOIN Teachers t ON c.TeacherID = t.ID", conn);
            using var listReader = listCmd.ExecuteReader();
            while (listReader.Read())
                Courses.Add((listReader.GetInt32(0), listReader.GetString(1), listReader.GetString(2), listReader.GetString(3)));
        }

        public void OnPost()
        {
            LoadDropdowns();

            var id = Request.Form["EditID"];
            var title = Request.Form["Title"];
            var level = Request.Form["Level"];
            var language = Request.Form["Language"];
            var teacherId = Request.Form["TeacherID"];
            var categoryId = Request.Form["CategoryID"];

            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            if (!string.IsNullOrEmpty(id) && int.Parse(id) > 0)
            {
                var cmd = new MySqlCommand("UPDATE Courses SET Title=@Title, Level=@Level, Language=@Language, TeacherID=@TeacherID, CategoryID=@CategoryID WHERE ID=@ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Level", level);
                cmd.Parameters.AddWithValue("@Language", language);
                cmd.Parameters.AddWithValue("@TeacherID", teacherId);
                cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                cmd.ExecuteNonQuery();
            }
            else
            {
                var cmd = new MySqlCommand("INSERT INTO Courses (Title, Description, Level, Language, TeacherID, CategoryID) VALUES (@Title, '', @Level, @Language, @TeacherID, @CategoryID);", conn);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Level", level);
                cmd.Parameters.AddWithValue("@Language", language);
                cmd.Parameters.AddWithValue("@TeacherID", teacherId);
                cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                cmd.ExecuteNonQuery();
            }

            Response.Redirect("/Admin/ManageCourses");
        }

        private void LoadDropdowns()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            var cmdT = new MySqlCommand("SELECT ID, FullName FROM Teachers", conn);
            using var readerT = cmdT.ExecuteReader();
            while (readerT.Read()) Teachers.Add((readerT.GetInt32(0), readerT.GetString(1)));
            readerT.Close();

            var cmdC = new MySqlCommand("SELECT ID, Name FROM Categories", conn);
            using var readerC = cmdC.ExecuteReader();
            while (readerC.Read()) Categories.Add((readerC.GetInt32(0), readerC.GetString(1)));
        }
    }

}
