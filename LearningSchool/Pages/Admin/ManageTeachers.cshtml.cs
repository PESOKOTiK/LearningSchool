using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LearningSchool.Pages.Admin
{
    public class ManageTeachersModel : PageModel
    {
        private readonly IConfiguration _config;

        // Bound properties
        [BindProperty] public int? EditID { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public string FullName { get; set; }
        [BindProperty] public string Biography { get; set; }

        public string Message { get; set; }

        public List<(int ID, string Email, string FullName, string Biography)> TeacherList { get; set; }
            = new();

        public ManageTeachersModel(IConfiguration config) => _config = config;

        public void OnGet(int? editId, int? deleteId)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            // Delete handler
            if (deleteId.HasValue)
            {
                using var tx = conn.BeginTransaction();
                using var cmdT = new MySqlCommand("DELETE FROM Teachers WHERE ID=@ID", conn, tx);
                cmdT.Parameters.AddWithValue("@ID", deleteId.Value);
                cmdT.ExecuteNonQuery();

                using var cmdU = new MySqlCommand("DELETE FROM Users WHERE ID=@ID", conn, tx);
                cmdU.Parameters.AddWithValue("@ID", deleteId.Value);
                cmdU.ExecuteNonQuery();

                tx.Commit();
                Response.Redirect("/Admin/ManageTeachers");
                return;
            }

            // Populate list
            using var cmdList = new MySqlCommand(
                "SELECT u.ID, u.Email, t.FullName, t.Biography " +
                "FROM Users u JOIN Teachers t ON u.ID = t.ID", conn);
            using var reader = cmdList.ExecuteReader();
            while (reader.Read())
            {
                TeacherList.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3)
                ));
            }
            reader.Close();

            // Edit handler: prefill form
            if (editId.HasValue)
            {
                EditID = editId;
                var t = TeacherList.Find(x => x.ID == editId.Value);
                Email = t.Email;
                FullName = t.FullName;
                Biography = t.Biography;
                // Password left blank
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Message = "Please correct the errors and try again.";
                OnGet(null, null);
                return Page();
            }

            string connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            try
            {
                if (EditID.HasValue && EditID.Value > 0)
                {
                    // Update existing user
                    var sb = new StringBuilder("UPDATE Users SET Email=@Email");
                    if (!string.IsNullOrWhiteSpace(Password))
                        sb.Append(", PasswordHash=@Hash");
                    sb.Append(" WHERE ID=@ID");

                    using var cmdU = new MySqlCommand(sb.ToString(), conn);
                    cmdU.Parameters.AddWithValue("@ID", EditID.Value);
                    cmdU.Parameters.AddWithValue("@Email", Email);
                    if (!string.IsNullOrWhiteSpace(Password))
                        cmdU.Parameters.AddWithValue("@Hash", HashPassword(Password));
                    cmdU.ExecuteNonQuery();

                    // Update teacher
                    using var cmdT = new MySqlCommand(
                        "UPDATE Teachers SET FullName=@FullName, Biography=@Biography WHERE ID=@ID", conn);
                    cmdT.Parameters.AddWithValue("@ID", EditID.Value);
                    cmdT.Parameters.AddWithValue("@FullName", FullName);
                    cmdT.Parameters.AddWithValue("@Biography", Biography);
                    cmdT.ExecuteNonQuery();

                    Message = "Teacher updated successfully.";
                }
                else
                {
                    // Insert new user + teacher
                    using var tx = conn.BeginTransaction();

                    using var cmdU = new MySqlCommand(
                        "INSERT INTO Users (Email, PasswordHash, Role) " +
                        "VALUES (@Email, @Hash, 'Teacher'); SELECT LAST_INSERT_ID();",
                        conn, tx);
                    cmdU.Parameters.AddWithValue("@Email", Email);
                    cmdU.Parameters.AddWithValue("@Hash", HashPassword(Password));
                    int newId = Convert.ToInt32(cmdU.ExecuteScalar());

                    using var cmdT = new MySqlCommand(
                        "INSERT INTO Teachers (ID, FullName, Biography) VALUES (@ID, @FullName, @Biography)",
                        conn, tx);
                    cmdT.Parameters.AddWithValue("@ID", newId);
                    cmdT.Parameters.AddWithValue("@FullName", FullName);
                    cmdT.Parameters.AddWithValue("@Biography", Biography);
                    cmdT.ExecuteNonQuery();

                    tx.Commit();
                    Message = "Teacher added successfully.";
                }
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }

            // Refresh list & clear edit state
            EditID = null;
            OnGet(null, null);
            return Page();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashed);
        }
    }
}
