using System;
using System.Text.RegularExpressions;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model Candidate - Hồ sơ ứng viên
    /// </summary>
    public class Candidate : BaseEntity
    {
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string? _phone;
        private DateTime? _dateOfBirth;
        private string? _gender;
        private string? _address;
        private string? _education;
        private string? _workExperience;
        private string? _skills;
        private string? _resumeFileName;
        private string? _resumeFilePath;
        private string _currentStatus = "Mới";
        private int? _yearsOfExperience;
        private string? _currentPosition;

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

        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (value.HasValue)
                {
                    if (value.Value > DateTime.Now)
                        throw new ArgumentException("Ngày sinh không thể là tương lai");
                    if (value.Value.Year < 1950)
                        throw new ArgumentException("Ngày sinh không hợp lệ");
                }
                _dateOfBirth = value;
            }
        }

        /// <summary>
        /// Gender: Male, Female, Other
        /// </summary>
        public string? Gender
        {
            get => _gender;
            set => _gender = value?.Trim();
        }

        public string? Address
        {
            get => _address;
            set => _address = value?.Trim();
        }

        public string? Education
        {
            get => _education;
            set => _education = value?.Trim();
        }

        public string? WorkExperience
        {
            get => _workExperience;
            set => _workExperience = value?.Trim();
        }

        public string? Skills
        {
            get => _skills;
            set => _skills = value?.Trim();
        }

        public string? ResumeFileName
        {
            get => _resumeFileName;
            set => _resumeFileName = value?.Trim();
        }

        public string? ResumeFilePath
        {
            get => _resumeFilePath;
            set => _resumeFilePath = value?.Trim();
        }

        /// <summary>
        /// Trạng thái hiện tại: Mới, Đang ứng tuyển, Đã tuyển, Không đạt
        /// </summary>
        public string CurrentStatus
        {
            get => _currentStatus;
            set => _currentStatus = value?.Trim() ?? "Mới";
        }

        /// <summary>
        /// Số năm kinh nghiệm
        /// </summary>
        public int? YearsOfExperience
        {
            get => _yearsOfExperience;
            set
            {
                if (value.HasValue && (value < 0 || value > 50))
                    throw new ArgumentException("Số năm kinh nghiệm phải từ 0 đến 50");
                _yearsOfExperience = value;
            }
        }

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        public string? CurrentPosition
        {
            get => _currentPosition;
            set => _currentPosition = value?.Trim();
        }

        /// <summary>
        /// Status
        /// </summary>
        public string Status
        {
            get => _currentStatus;
            set => _currentStatus = value?.Trim() ?? "Mới";
        }

        // CVFilePath
        public string? CVFilePath
        {
            get => _resumeFilePath;
            set => _resumeFilePath = value?.Trim();
        }

        public Candidate()
        {
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            var regex = new Regex(@"^0\d{9,10}$");
            return regex.IsMatch(phone.Replace(" ", "").Replace("-", ""));
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(_fullName) &&
                   IsValidEmail(_email);
        }

        public int? GetAge()
        {
            if (!_dateOfBirth.HasValue)
                return null;
            return (DateTime.Now - _dateOfBirth.Value).Days / 365;
        }

        public bool HasResume()
        {
            return !string.IsNullOrWhiteSpace(_resumeFilePath);
        }
    }
}

