using System;
using System.Security.Cryptography;
using System.Text;

namespace LTHDT2.Utils
{
    /// <summary>
    /// Mã hóa mật khẩu với SHA256 + Salt
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Hash password với salt
        /// </summary>
        public static (string hash, string salt) Hash(string password)
        {
            // Tạo salt ngẫu nhiên
            var salt = GenerateSalt();
            
            // Hash password với salt
            var hash = HashWithSalt(password, salt);
            
            return (hash, salt);
        }

        /// <summary>
        /// Verify password
        /// </summary>
        public static bool Verify(string password, string hash, string salt)
        {
            var newHash = HashWithSalt(password, salt);
            return newHash.Equals(hash, StringComparison.Ordinal);
        }

        /// <summary>
        /// Tạo salt ngẫu nhiên
        /// </summary>
        private static string GenerateSalt()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Hash password với salt sử dụng SHA256
        /// </summary>
        private static string HashWithSalt(string password, string salt)
        {
            var combined = password + salt;
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(combined);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}

