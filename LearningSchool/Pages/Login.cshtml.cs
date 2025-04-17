using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LearningSchool.Pages
{
    public class LoginModel : PageModel
    {
        IConfiguration _config;
        [BindProperty] public string Email { get; set; } = null!;
        [BindProperty] public string Password { get; set; } = null!;
        public string Message { get; set; } = null!;

        public LoginModel(IConfiguration configuration)
        {
            _config = configuration;
        }
        public IActionResult OnPost()
        {
            var hash = HashPassword(Password);

            string connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            var cmd = new MySqlCommand("SELECT ID, Role FROM Users WHERE Email = @Email AND PasswordHash = @Hash;", conn);
            cmd.Parameters.AddWithValue("@Email", Email);
            cmd.Parameters.AddWithValue("@Hash", hash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int userId = reader.GetInt32(0);
                string role = reader.GetString(1);

                HttpContext.Session.SetInt32("UserID", userId);
                HttpContext.Session.SetString("UserRole", role);

                return RedirectToPage("/Dashboard");
            }

            Message = "Invalid email or password.";
            return Page();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

}
