using LTHDT2.Models;

namespace LTHDT2.Services
{
    /// <summary>
    /// Interface Authentication Service
    /// Áp dụng Abstraction - Định nghĩa contract cho authentication
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Đăng nhập
        /// </summary>
        User? Login(string username, string password);

        /// <summary>
        /// Đăng xuất
        /// </summary>
        bool Logout();

        /// <summary>
        /// Lấy thông tin user hiện tại
        /// </summary>
        User? GetCurrentUser();

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        bool ChangePassword(int userId, string oldPassword, string newPassword);

        /// <summary>
        /// Kiểm tra user có quyền không
        /// </summary>
        bool HasPermission(string role);

        /// <summary>
        /// Tạo user mới
        /// </summary>
        int CreateUser(string username, string password, int employeeId, string role);
    }
}

