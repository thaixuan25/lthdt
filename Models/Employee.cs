using System;
using System.Text.RegularExpressions;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model Employee - Nhân viên
    /// </summary>
    public class Employee : BaseEntity
    {
        // Private fields
        private string _employeeCode = string.Empty;
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string? _phone;
        private int _departmentId;
        private int _positionId;
        private DateTime _hireDate;
        private string _status = string.Empty;

        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public string EmployeeCode
        {
            get => _employeeCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Mã nhân viên không được trống");
                _employeeCode = value.Trim().ToUpper();
            }
        }

        /// <summary>
        /// Họ tên
        /// </summary>
        public string FullName
        {
            get => _fullName;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
                    throw new ArgumentException("Họ tên phải có ít nhất 2 ký tự");
                _fullName = value.Trim();
            }
        }

        /// <summary>
        /// Email
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                if (!IsValidEmail(value))
                    throw new ArgumentException("Email không hợp lệ");
                _email = value.Trim().ToLower();
            }
        }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? Phone
        {
            get => _phone;
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !IsValidPhone(value))
                    throw new ArgumentException("Số điện thoại không hợp lệ");
                _phone = value?.Trim();
            }
        }

        /// <summary>
        /// ID phòng ban (FK)
        /// </summary>
        public int DepartmentId
        {
            get => _departmentId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn phòng ban");
                _departmentId = value;
            }
        }

        /// <summary>
        /// ID vị trí (FK)
        /// </summary>
        public int PositionId
        {
            get => _positionId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn vị trí");
                _positionId = value;
            }
        }

        /// <summary>
        /// Ngày vào làm
        /// </summary>
        public DateTime HireDate
        {
            get => _hireDate;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("Ngày vào làm không thể là tương lai");
                _hireDate = value;
            }
        }

        /// <summary>
        /// Trạng thái: Active, Resigned, Terminated
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Trạng thái không được trống");
                _status = value.Trim();
            }
        }

        // Navigation properties
        public Department? Department { get; set; }
        public Position? Position { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Employee()
        {
            _hireDate = DateTime.Now;
            _status = "Active";
        }

        /// <summary>
        /// Private method - Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // regex cho email
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Private method - Validate phone format
        /// </summary>
        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var regex = new Regex(@"^0\d{9,10}$");
            return regex.IsMatch(phone.Replace(" ", "").Replace("-", ""));
        }

        /// <summary>
        /// Override IsValid
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(_employeeCode) &&
                   !string.IsNullOrWhiteSpace(_fullName) &&
                   IsValidEmail(_email) &&
                   _departmentId > 0 &&
                   _positionId > 0;
        }

        /// <summary>
        /// Public method - Hiển thị tên đầy đủ
        /// </summary>
        public string GetDisplayName()
        {
            return $"{EmployeeCode} - {FullName}";
        }

        /// <summary>
        /// Public method - Tính số năm làm việc
        /// </summary>
        public int GetYearsOfService()
        {
            return (DateTime.Now - _hireDate).Days / 365;
        }

        /// <summary>
        /// Public method - Kiểm tra còn làm việc
        /// </summary>
        public bool IsActive()
        {
            return _status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }
    }
}

