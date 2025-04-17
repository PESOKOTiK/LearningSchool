using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace LearningSchool.Pages.Admin
{
    public class ManageModulesModel : PageModel
    {
        private readonly IConfiguration _config;
        public ManageModulesModel(IConfiguration config) => _config = config;

        // Dropdown options & list
        public List<(int ID, int CourseID, string CourseTitle, string Title, string Description)> ModuleList { get; set; }
            = new();
        public List<(int ID, string Title)> CourseOptions { get; set; } = new();

        // Bound properties for form
        [BindProperty] public int? EditID { get; set; }
        [BindProperty, Required] public int CourseID { get; set; }
        [BindProperty, Required] public string Title { get; set; }
        [BindProperty] public string Description { get; set; }

        public string Message { get; set; }

        public void OnGet(int? editId, int? deleteId)
        {
            LoadData();

            if (deleteId.HasValue)
            {
                DeleteModule(deleteId.Value);
                Response.Redirect("/Admin/ManageModules");
                return;
            }

            if (editId.HasValue)
            {
                EditID = editId;
                var mod = ModuleList.First(m => m.ID == editId.Value);
                CourseID = mod.CourseID;
                Title = mod.Title;
                Description = mod.Description;
            }
        }

        public IActionResult OnPost()
        {
            LoadData();

            if (!ModelState.IsValid)
            {
                Message = "Please fix errors before submitting.";
                return Page();
            }

            try
            {
                if (EditID.HasValue)
                    UpdateModule(EditID.Value);
                else
                    InsertModule();

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

            // Load courses for dropdown
            using var cmdC = new MySqlCommand("SELECT ID, Title FROM Courses", conn);
            using var rdrC = cmdC.ExecuteReader();
            while (rdrC.Read())
                CourseOptions.Add((rdrC.GetInt32(0), rdrC.GetString(1)));
            rdrC.Close();

            // Load existing modules
            using var cmdM = new MySqlCommand(@"
                SELECT m.ID, m.CourseID, c.Title AS CourseTitle, m.Title, m.Description
                FROM Modules m
                JOIN Courses c ON m.CourseID = c.ID", conn);
            using var rdrM = cmdM.ExecuteReader();
            while (rdrM.Read())
                ModuleList.Add((
                    rdrM.GetInt32(0),
                    rdrM.GetInt32(1),
                    rdrM.GetString(2),
                    rdrM.GetString(3),
                    rdrM.IsDBNull(4) ? "" : rdrM.GetString(4)
                ));
        }

        private void InsertModule()
        {
            string cs = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(cs);
            conn.Open();
            using var cmd = new MySqlCommand(
                "INSERT INTO Modules (CourseID, Title, Description) VALUES (@C, @T, @D)", conn);
            cmd.Parameters.AddWithValue("@C", CourseID);
            cmd.Parameters.AddWithValue("@T", Title);
            cmd.Parameters.AddWithValue("@D", Description ?? "");
            cmd.ExecuteNonQuery();
        }

        private void UpdateModule(int id)
        {
            string cs = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(cs);
            conn.Open();
            using var cmd = new MySqlCommand(
                "UPDATE Modules SET CourseID=@C, Title=@T, Description=@D WHERE ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@C", CourseID);
            cmd.Parameters.AddWithValue("@T", Title);
            cmd.Parameters.AddWithValue("@D", Description ?? "");
            cmd.ExecuteNonQuery();
        }

        private void DeleteModule(int id)
        {
            string cs = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(cs);
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM Modules WHERE ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.ExecuteNonQuery();
        }
    }
}
