using System;
using LTHDT2.Models;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Utils;

namespace LTHDT2.Services
{
    /// <summary>
    /// Authentication Service - Implement IAuthenticationService
    /// Kế thừa BaseService (Inheritance)
    /// Implement Interface (Polymorphism)
    /// </summary>
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        private readonly UserRepository _userRepository;

        public AuthenticationService()
        {
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        public User? Login(string username, string password)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    LogAction("Login Failed", "Username hoặc password trống");
                    return null;
                }

                // Lấy user từ database
                var user = _userRepository.GetByUsername(username);
                if (user == null)
                {
                    LogAction("Login Failed", $"User '{username}' không tồn tại");
                    return null;
                }

                // Kiểm tra user có active không
                if (!user.IsActive)
                {
                    LogAction("Login Failed", $"User '{username}' đã bị vô hiệu hóa");
                    return null;
                }

                // Verify password
                if (!PasswordHasher.Verify(password, user.PasswordHash, user.Salt))
                {
                    LogAction("Login Failed", $"Sai mật khẩu cho user '{username}'");
                    return null;
                }

                // Cập nhật LastLogin
                user.LastLogin = DateTime.Now;
                _userRepository.Update(user);

                // Set session
                SessionManager.CurrentUser = user;

                LogAction("Login Success", $"User '{username}' đăng nhập thành công");
                return user;
            }
            catch (Exception ex)
            {
                LogError(nameof(Login), ex);
                return null;
            }
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        public bool Logout()
        {
            var username = _currentUser?.Username ?? "Unknown";
            SessionManager.Clear();
            LogAction("Logout", $"User '{username}' đã đăng xuất");
            return true;
        }

        /// <summary>
        /// Lấy user hiện tại
        /// </summary>
        public User? GetCurrentUser()
        {
            return SessionManager.CurrentUser;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                {
                    throw new ArgumentException("Mật khẩu không được trống");
                }

                if (newPassword.Length < 6)
                {
                    throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự");
                }

                // Lấy user
                var user = _userRepository.GetById(userId);
                if (user == null)
                {
                    throw new ArgumentException("User không tồn tại");
                }

                // Verify old password
                if (!PasswordHasher.Verify(oldPassword, user.PasswordHash, user.Salt))
                {
                    LogAction("Change Password Failed", $"Sai mật khẩu cũ cho UserID={userId}");
                    return false;
                }

                // Hash new password
                var (newHash, newSalt) = PasswordHasher.Hash(newPassword);
                user.PasswordHash = newHash;
                user.Salt = newSalt;

                // Update
                var result = _userRepository.Update(user);
                
                if (result)
                {
                    LogAction("Change Password Success", $"UserID={userId} đã đổi mật khẩu");
                }

                return result;
            }
            catch (Exception ex)
            {
                LogError(nameof(ChangePassword), ex);
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra quyền
        /// </summary>
        public bool HasPermission(string role)
        {
            return base.HasPermission(role);
        }

        /// <summary>
        /// Tạo user mới
        /// </summary>
        public int CreateUser(string username, string password, int employeeId, string role)
        {
            try
            {
                // Validate permission - chỉ Admin mới được tạo user
                ValidatePermission("Admin");

                // Validate input
                if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
                {
                    throw new ArgumentException("Username phải có ít nhất 3 ký tự");
                }

                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    throw new ArgumentException("Password phải có ít nhất 6 ký tự");
                }

                // Kiểm tra username đã tồn tại chưa
                var existingUser = _userRepository.GetByUsername(username);
                if (existingUser != null)
                {
                    throw new ArgumentException($"Username '{username}' đã tồn tại");
                }

                // Hash password
                var (hash, salt) = PasswordHasher.Hash(password);

                // Tạo user mới
                var user = new User
                {
                    Username = username,
                    PasswordHash = hash,
                    Salt = salt,
                    EmployeeId = employeeId,
                    Role = role,
                    IsActive = true
                };

                // Validate entity
                ValidateEntity(user);

                // Save
                var userId = _userRepository.Add(user);

                if (userId > 0)
                {
                    LogAction("Create User", $"Đã tạo user '{username}' với role '{role}'");
                }

                return userId;
            }
            catch (Exception ex)
            {
                LogError(nameof(CreateUser), ex);
                throw;
            }
        }
    }
}

