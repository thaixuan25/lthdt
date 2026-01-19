using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model Position - Vị trí công việc
    /// </summary>
    public class Position : BaseEntity
    {
        private string _positionCode = string.Empty;
        private string _positionName = string.Empty;
        private string? _description;
        private int _level;
        private decimal _minSalary;
        private decimal _maxSalary;
        private bool _isActive;

        /// <summary>
        /// Mã vị trí
        /// </summary>
        public string PositionCode
        {
            get => _positionCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Mã vị trí không được trống");
                _positionCode = value.Trim().ToUpper();
            }
        }

        /// <summary>
        /// Tên vị trí
        /// </summary>
        public string PositionName
        {
            get => _positionName;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
                    throw new ArgumentException("Tên vị trí phải có ít nhất 2 ký tự");
                _positionName = value.Trim();
            }
        }

        /// <summary>
        /// Mô tả vị trí
        /// </summary>
        public string? Description
        {
            get => _description;
            set => _description = value?.Trim();
        }

        /// <summary>
        /// Cấp bậc: 1=Junior, 2=Middle, 3=Senior, 4=Lead, 5=Manager
        /// </summary>
        public int Level
        {
            get => _level;
            set
            {
                if (value < 1 || value > 5)
                    throw new ArgumentException("Cấp bậc phải từ 1 đến 5");
                _level = value;
            }
        }

        /// <summary>
        /// Lương tối thiểu
        /// </summary>
        public decimal MinSalary
        {
            get => _minSalary;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Lương tối thiểu không thể âm");
                _minSalary = value;
            }
        }

        /// <summary>
        /// Lương tối đa
        /// </summary>
        public decimal MaxSalary
        {
            get => _maxSalary;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Lương tối đa không thể âm");
                if (value > 0 && value < _minSalary)
                    throw new ArgumentException("Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu");
                _maxSalary = value;
            }
        }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        /// <summary>
        /// Constructor mặc định
        /// </summary>
        public Position()
        {
            _level = 1;
            _isActive = true;
        }

        /// <summary>
        /// Override IsValid
        /// </summary>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(_positionCode) &&
                   !string.IsNullOrWhiteSpace(_positionName) &&
                   _level >= 1 && _level <= 5;
        }

        /// <summary>
        /// Hiển thị tên đầy đủ
        /// </summary>
        public string GetDisplayName()
        {
            return $"{PositionCode} - {PositionName}";
        }

        /// <summary>
        /// Lấy tên cấp bậc
        /// </summary>
        public string GetLevelName()
        {
            return _level switch
            {
                1 => "Junior",
                2 => "Middle",
                3 => "Senior",
                4 => "Lead",
                5 => "Manager",
                _ => "Unknown"
            };
        }
    }
}

