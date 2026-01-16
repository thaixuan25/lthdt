using LTHDT2.Models;

namespace LTHDT2.Utils
{
    /// <summary>
    /// Session Manager - Quản lý user hiện tại
    /// Singleton pattern
    /// </summary>
    public static class SessionManager
    {
        private static User? _currentUser;

        /// <summary>
        /// User hiện tại đang đăng nhập
        /// </summary>
        public static User? CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        /// <summary>
        /// Kiểm tra đã đăng nhập chưa
        /// </summary>
        public static bool IsLoggedIn => _currentUser != null;

        /// <summary>
        /// Kiểm tra có quyền Admin không
        /// </summary>
        public static bool IsAdmin => _currentUser?.IsAdmin() == true;

        /// <summary>
        /// Kiểm tra có quyền HR Manager không
        /// </summary>
        public static bool IsHRManager => _currentUser?.IsHRManager() == true;

        /// <summary>
        /// Lấy Employee ID của user hiện tại
        /// </summary>
        public static int? CurrentEmployeeId => _currentUser?.EmployeeId;

        /// <summary>
        /// Xóa session (logout)
        /// </summary>
        public static void Clear()
        {
            _currentUser = null;
        }
    }
}

