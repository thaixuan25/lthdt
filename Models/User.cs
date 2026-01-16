using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model User - Tài khoản đăng nhập
    /// </summary>
    public class User : BaseEntity
    {
        private string _username = string.Empty;
        private string _passwordHash = string.Empty;
        private string _salt = string.Empty;
        private int _employeeId;
        private string _role = string.Empty;
        private bool _isActive;
        private DateTime? _lastLogin;

        public string Username
        {
            get => _username;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
                    throw new ArgumentException("Tên đăng nhập phải có ít nhất 3 ký tự");
                _username = value.Trim().ToLower();
            }
        }

        public string PasswordHash
        {
            get => _passwordHash;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Password hash không được trống");
                _passwordHash = value;
            }
        }

        public string Salt
        {
            get => _salt;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Salt không được trống");
                _salt = value;
            }
        }

        public int EmployeeId
        {
            get => _employeeId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải liên kết với nhân viên");
                _employeeId = value;
            }
        }

        /// <summary>
        /// Role: Admin, HRManager, Staff
        /// </summary>
        public string Role
        {
            get => _role;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Role không được trống");
                _role = value.Trim();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        public DateTime? LastLogin
        {
            get => _lastLogin;
            set => _lastLogin = value;
        }

        // Navigation property
        public Employee? Employee { get; set; }

        public User()
        {
            _role = "Staff";
            _isActive = true;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(_username) &&
                   !string.IsNullOrWhiteSpace(_passwordHash) &&
                   !string.IsNullOrWhiteSpace(_salt) &&
                   _employeeId > 0;
        }

        public bool IsAdmin()
        {
            return _role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsHRManager()
        {
            return _role.Equals("HRManager", StringComparison.OrdinalIgnoreCase) || IsAdmin();
        }
    }
}

