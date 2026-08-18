using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;

        public AuthService(AppDbContext db)
        {
            _db = db;
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
                Role = "customer"
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

        public User? GetByEmail(string email) =>
            _db.Users.FirstOrDefault(u => u.Email == email);

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

        // ─── OTP Verification Flow ────────────────────────────────

        /// <summary>
        /// Creates a 6-digit OTP for the given email, stored hashed in DB,
        /// valid for 10 minutes. Old unused tokens are invalidated.
        /// Returns the plain-text OTP (to be emailed) or an error.
        /// </summary>
        public (string? otp, string? error) CreateResetOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (null, "Email is required.");

            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return (null, "No account found with this email.");

            // Invalidate previous unused tokens for this user
            var oldTokens = _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed)
                .ToList();
            foreach (var t in oldTokens)
                t.IsUsed = true;

            var otp = new Random().Next(100000, 999999).ToString();

            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                Token = BCrypt.Net.BCrypt.HashPassword(otp),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            });

            _db.SaveChanges();
            return (otp, null);
        }

        /// <summary>
        /// Validates the OTP. On success marks it used and returns the user.
        /// </summary>
        public (User? user, string? error) VerifyResetOtp(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(otp))
                return (null, "Please enter the verification code.");

            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return (null, "No account found with this email.");

            var token = _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault();

            if (token == null)
                return (null, "No verification code found. Please request a new one.");

            if (token.ExpiresAt < DateTime.UtcNow)
                return (null, "Verification code has expired. Please request a new one.");

            if (!BCrypt.Net.BCrypt.Verify(otp, token.Token))
                return (null, "Invalid verification code. Please try again.");

            token.IsUsed = true;
            _db.SaveChanges();
            return (user, null);
        }

        public (bool ok, string? error) ResetPasswordForUser(int userId, string newPassword)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return (false, "User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.FailedAttempts = 0;
            user.IsLocked = false;
            _db.SaveChanges();

            return (true, null);
        }
    }
}
