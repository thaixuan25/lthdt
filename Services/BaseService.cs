using System;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Services
{
    /// <summary>
    /// Base Service Abstract Class
    /// </summary>
    public abstract class BaseService
    {
        // Protected
        protected readonly User? _currentUser;

        /// <summary>
        /// Protected constructor
        /// </summary>
        protected BaseService()
        {
            _currentUser = SessionManager.CurrentUser;
        }

        /// <summary>
        /// Log actions method
        /// </summary>
        protected void LogAction(string action, string details)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var username = _currentUser?.Username ?? "Anonymous";
            var message = $"[{timestamp}] User: {username} - {action}: {details}";
        
            Console.WriteLine(message);
            
        }

        /// <summary>
        /// Kiểm tra quyền - Protected method
        /// Throw exception nếu không có quyền
        /// </summary>
        protected void ValidatePermission(string requiredRole)
        {
            if (_currentUser == null)
                throw new UnauthorizedAccessException("Chưa đăng nhập. Vui lòng đăng nhập để tiếp tục.");

            if (!HasPermission(requiredRole))
                throw new UnauthorizedAccessException($"Bạn không có quyền {requiredRole} để thực hiện thao tác này.");
        }

        /// <summary>
        /// Kiểm tra có quyền không
        /// </summary>
        protected bool HasPermission(string requiredRole)
        {
            if (_currentUser == null)
                return false;

            // Admin có tất cả quyền
            if (_currentUser.IsAdmin())
                return true;

            // Kiểm tra role cụ thể
            return _currentUser.Role.Equals(requiredRole, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validate entity trước khi lưu
        /// </summary>
        protected void ValidateEntity<T>(T entity) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity không được null");

            if (!entity.IsValid())
                throw new ArgumentException($"{typeof(T).Name} không hợp lệ");
        }

        /// <summary>
        /// Log error method
        /// </summary>
        protected void LogError(string methodName, Exception ex)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var username = _currentUser?.Username ?? "Anonymous";
            var message = $"[{timestamp}] ERROR - User: {username} - {methodName}: {ex.Message}\nStackTrace: {ex.StackTrace}";
            
            Console.WriteLine(message);
        }

        /// <summary>
        /// Lấy current user ID
        /// </summary>
        protected int GetCurrentUserId()
        {
            if (_currentUser == null)
                throw new UnauthorizedAccessException("Chưa đăng nhập");
            return _currentUser.Id;
        }

        /// <summary>
        /// Lấy current employee ID
        /// </summary>
        protected int GetCurrentEmployeeId()
        {
            if (_currentUser == null)
                throw new UnauthorizedAccessException("Chưa đăng nhập");
            return _currentUser.EmployeeId;
        }
    }
}

