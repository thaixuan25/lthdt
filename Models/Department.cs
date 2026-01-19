using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model Department - Phòng ban
    /// </summary>
    public class Department : BaseEntity
    {
        // Private fields
        private string _departmentCode = string.Empty;
        private string _departmentName = string.Empty;
        private string? _description;
        private int? _managerId;
        private int _currentHeadcount;
        private int _maxHeadcount;

        /// <summary>
        /// Mã phòng ban
        /// </summary>
        public string DepartmentCode
        {
            get => _departmentCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Mã phòng ban không được trống");
                _departmentCode = value.Trim().ToUpper();
            }
        }

        /// <summary>
        /// Tên phòng ban
        /// </summary>
        public string DepartmentName
        {
            get => _departmentName;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
                    throw new ArgumentException("Tên phòng ban phải có ít nhất 2 ký tự");
                _departmentName = value.Trim();
            }
        }

        /// <summary>
        /// Mô tả phòng ban
        /// </summary>
        public string? Description
        {
            get => _description;
            set => _description = value?.Trim();
        }

        /// <summary>
        /// ID của người quản lý (FK đến Employee)
        /// </summary>
        public int? ManagerId
        {
            get => _managerId;
            set => _managerId = value;
        }

        /// <summary>
        /// Tên người quản lý
        /// </summary>
        public string? ManagerName { get; set; }

        /// <summary>
        /// Địa điểm văn phòng
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Số lượng nhân viên hiện tại
        /// </summary>
        public int CurrentHeadcount
        {
            get => _currentHeadcount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Số lượng nhân viên không thể âm");
                _currentHeadcount = value;
            }
        }

        /// <summary>
        /// Số lượng nhân viên tối đa
        /// </summary>
        public int MaxHeadcount
        {
            get => _maxHeadcount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Số lượng tối đa không thể âm");
                _maxHeadcount = value;
            }
        }

        // Navigation properties
        public Employee? Manager { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Department()
        {
            _currentHeadcount = 0;
            _maxHeadcount = 0;
        }

        /// <summary>
        /// Override IsValid
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(_departmentCode) &&
                   !string.IsNullOrWhiteSpace(_departmentName);
        }

        /// <summary>
        /// Hiển thị tên đầy đủ
        /// </summary>
        public string GetDisplayName()
        {
            return $"{DepartmentCode} - {DepartmentName}";
        }

        /// <summary>
        /// Kiểm tra còn chỗ trống
        /// </summary>
        public bool HasVacancy()
        {
            return _currentHeadcount < _maxHeadcount;
        }

        /// <summary>
        /// Lấy số chỗ trống
        /// </summary>
        public int GetVacancyCount()
        {
            return Math.Max(0, _maxHeadcount - _currentHeadcount);
        }
    }
}

