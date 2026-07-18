using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly decimal _defaultCustomerBalance;

        public AuthService(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _defaultCustomerBalance = configuration.GetValue("Shopping:DefaultCustomerBalance", 100_000m);
        }

        public User? Register(string fullName, string email, string password, string? phone)
        {
            if (_db.Users.Any(u => u.Email == email))
                return null; // Email already exists

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Phone = phone,
                Role = "customer",
                Balance = _defaultCustomerBalance
            };
            _db.Users.Add(user);
            _db.SaveChanges();
            return user;
        }

        public (User? user, string? error) Login(string email, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return (null, "Invalid email or password.");

            if (user.IsLocked)
                return (null, "Your account is locked due to too many failed attempts. Contact support.");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                user.FailedAttempts++;
                if (user.FailedAttempts >= 5)
                    user.IsLocked = true;
                _db.SaveChanges();
                int remaining = Math.Max(0, 5 - user.FailedAttempts);
                return (null, user.IsLocked
                    ? "Account locked after 5 failed attempts."
                    : $"Invalid password. {remaining} attempt(s) remaining.");
            }

            // Success - reset attempts
            user.FailedAttempts = 0;
            _db.SaveChanges();
            return (user, null);
        }

        public User? GetById(int id) =>
            _db.Users.FirstOrDefault(u => u.Id == id);

        public bool UpdateUser(int id, string fullName, string email, string? phone, string? newPassword)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return false;

            user.FullName = fullName;
            user.Email = email;
            user.Phone = phone;
            if (!string.IsNullOrWhiteSpace(newPassword))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _db.SaveChanges();
            return true;
        }

        public (bool ok, string? error) ResetPasswordByEmail(string email, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required.");
            if (string.IsNullOrWhiteSpace(newPassword))
                return (false, "New password is required.");
            if (newPassword.Length < 6)
                return (false, "Password must be at least 6 characters.");

            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return (false, "No account found with this email.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.FailedAttempts = 0;
            user.IsLocked = false;
            _db.SaveChanges();

            return (true, null);
        }
    }
}
