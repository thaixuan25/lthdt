using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model JobPosting - Tin tuyển dụng
    /// Thuộc phòng ban và vị trí cụ thể
    /// </summary>
    public class JobPosting : BaseEntity
    {
        private string _jobCode = string.Empty;
        private string _jobTitle = string.Empty;
        private string? _jobDescription;
        private string? _jobResponsibilities;
        private string? _jobRequirements;
        private int _departmentId;
        private int _positionId;
        private int _vacancyCount;
        private decimal _minSalary;
        private decimal _maxSalary;
        private string? _workLocation;
        private DateTime _postDate;
        private DateTime _deadline;
        private string _status = string.Empty;
        private bool _isHeadcountApproved;
        private int _createdBy;
        private int _campaignId;
        
        // Yêu cầu tuyển dụng có thể thuộc hoặc không thuộc đợt tuyển dụng nào
        public int CampaignId
        {
            get => _campaignId;
            set
            {
                _campaignId = value;
            }
        }
        public string JobCode
        {
            get => _jobCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Mã tin tuyển dụng không được trống");
                _jobCode = value.Trim().ToUpper();
            }
        }

        public string JobTitle
        {
            get => _jobTitle;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tiêu đề tin tuyển dụng không được trống");
                _jobTitle = value.Trim();
            }
        }

        /// <summary>
        /// Mô tả công việc tổng quan
        /// </summary>
        public string? JobDescription
        {
            get => _jobDescription;
            set => _jobDescription = value?.Trim();
        }

        /// <summary>
        /// Trách nhiệm cụ thể - dùng để hiển thị
        /// </summary>
        public string? JobResponsibilities
        {
            get => _jobResponsibilities;
            set => _jobResponsibilities = value?.Trim();
        }

        /// <summary>
        /// Yêu cầu ứng viên - dùng để sàng lọc/JD
        /// </summary>
        public string? JobRequirements
        {
            get => _jobRequirements;
            set => _jobRequirements = value?.Trim();
        }

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

        public int VacancyCount
        {
            get => _vacancyCount;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Số lượng cần tuyển phải lớn hơn 0");
                _vacancyCount = value;
            }
        }

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

        public decimal MaxSalary
        {
            get => _maxSalary;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Lương tối đa không thể âm");
                if (value > 0 && value < _minSalary)
                    throw new ArgumentException("Lương tối đa phải >= lương tối thiểu");
                _maxSalary = value;
            }
        }

        public string? WorkLocation
        {
            get => _workLocation;
            set => _workLocation = value?.Trim();
        }

        public DateTime PostDate
        {
            get => _postDate;
            set => _postDate = value;
        }

        public DateTime Deadline
        {
            get => _deadline;
            set
            {
                if (value < _postDate)
                    throw new ArgumentException("Deadline phải sau ngày đăng");
                _deadline = value;
            }
        }

        /// <summary>
        /// Status: Draft, Active, Closed, Cancelled
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

        /// <summary>
        /// Đã kiểm tra định biên hay chưa
        /// </summary>
        public bool IsHeadcountApproved
        {
            get => _isHeadcountApproved;
            set => _isHeadcountApproved = value;
        }

        public int CreatedBy
        {
            get => _createdBy;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải có người tạo");
                _createdBy = value;
            }
        }

        // ========== ALIAS PROPERTIES (để tương thích với Repository/Service) ==========
        
        /// <summary>
        /// Alias: VacancyCount = NumberOfPositions
        /// </summary>
        public int NumberOfPositions
        {
            get => _vacancyCount;
            set => _vacancyCount = value;
        }

        /// <summary>
        /// Alias: JobResponsibilities = Responsibilities
        /// </summary>
        public string? Responsibilities
        {
            get => _jobResponsibilities;
            set => _jobResponsibilities = value?.Trim();
        }

        /// <summary>
        /// Alias: JobRequirements = Requirements
        /// </summary>
        public string? Requirements
        {
            get => _jobRequirements;
            set => _jobRequirements = value?.Trim();
        }

        /// <summary>
        /// Alias: WorkLocation = Location
        /// </summary>
        public string? Location
        {
            get => _workLocation;
            set => _workLocation = value?.Trim();
        }

        // Navigation properties
        public Department? Department { get; set; }
        public Position? Position { get; set; }
        public Employee? Creator { get; set; }

        public JobPosting()
        {
            _postDate = DateTime.Now;
            _deadline = DateTime.Now.AddMonths(1);
            _status = "Draft";
            _isHeadcountApproved = false;
            _vacancyCount = 1;
        }

        public bool IsActive()
        {
            return _status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                   DateTime.Now <= _deadline;
        }

        public bool IsExpired()
        {
            return DateTime.Now > _deadline;
        }

        public int GetDaysRemaining()
        {
            var remaining = (_deadline - DateTime.Now).Days;
            return Math.Max(0, remaining);
        }

        public string GetSalaryRange()
        {
            if (_minSalary == 0 && _maxSalary == 0)
                return "Thỏa thuận";
            if (_maxSalary == 0)
                return $"Từ {_minSalary:N0} VNĐ";
            return $"{_minSalary:N0} - {_maxSalary:N0} VNĐ";
        }
    }
}

