using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LearningSchool.Pages
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using MySql.Data.MySqlClient;
    using System.Security.Cryptography;
    using System.Text;
    using static Org.BouncyCastle.Math.EC.ECCurve;

    public class RegisterModel : PageModel
    {
        private readonly IConfiguration _config;
        [BindProperty] public string FirstName { get; set; } = null!;
        [BindProperty] public string LastName { get; set; } = null!;
        [BindProperty] public string Email { get; set; } = null!;
        [BindProperty] public string Password { get; set; } = null!;
        public string Message { get; set; } = null!;

        public RegisterModel(IConfiguration config)
        {
            _config = config;
        }

        public void OnPost()
        {
            var hash = HashPassword(Password);

            string connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            var cmd = new MySqlCommand("INSERT INTO Users (Email, PasswordHash, Role) VALUES (@Email, @PasswordHash, 'Student'); SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@Email", Email);
            cmd.Parameters.AddWithValue("@PasswordHash", hash);

            var userId = Convert.ToInt32(cmd.ExecuteScalar());

            var cmd2 = new MySqlCommand("INSERT INTO Students (ID, FirstName, LastName) VALUES (@ID, @FirstName, @LastName);", conn);
            cmd2.Parameters.AddWithValue("@ID", userId);
            cmd2.Parameters.AddWithValue("@FirstName", FirstName);
            cmd2.Parameters.AddWithValue("@LastName", LastName);
            cmd2.ExecuteNonQuery();

            Message = "Registration successful!";
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

}
