using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearningSchool.Pages.Admin
{
    public class ManageCategoriesModel : PageModel
    {
        private readonly IConfiguration _config;
        public ManageCategoriesModel(IConfiguration config) => _config = config;

        public List<(int ID, string Name)> Categories { get; set; } = new();

        [BindProperty]
        public int? EditID { get; set; }

        [BindProperty, Required]
        public string Name { get; set; }

        public string Message { get; set; }

        public void OnGet(int? editId, int? deleteId)
        {
            LoadCategories();

            if (deleteId.HasValue)
            {
                DeleteCategory(deleteId.Value);
                Response.Redirect("/Admin/ManageCategories");
                return;
            }

            if (editId.HasValue)
            {
                EditID = editId;
                var cat = Categories.Find(x => x.ID == editId.Value);
                Name = cat.Name;
            }
        }

        public IActionResult OnPost()
        {
            LoadCategories();

            if (!ModelState.IsValid)
            {
                Message = "Please enter a valid name.";
                return Page();
            }

            try
            {
                if (EditID.HasValue)
                    UpdateCategory(EditID.Value);
                else
                    InsertCategory();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
                return Page();
            }
        }

        private void LoadCategories()
        {
            Categories.Clear();
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand("SELECT ID, Name FROM Categories", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                Categories.Add((rdr.GetInt32(0), rdr.GetString(1)));
        }

        private void InsertCategory()
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand("INSERT INTO Categories (Name) VALUES (@Name)", conn);
            cmd.Parameters.AddWithValue("@Name", Name);
            cmd.ExecuteNonQuery();
        }

        private void UpdateCategory(int id)
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand(
                "UPDATE Categories SET Name=@Name WHERE ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@Name", Name);
            cmd.ExecuteNonQuery();
        }

        private void DeleteCategory(int id)
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM Categories WHERE ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.ExecuteNonQuery();
        }
    }
}
