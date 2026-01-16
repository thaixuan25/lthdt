using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model RecruitmentCampaign - Đợt tuyển dụng
    /// Không thuộc phòng ban cụ thể, chỉ là "tag" để nhóm các tin tuyển dụng
    /// </summary>
    public class RecruitmentCampaign : BaseEntity
    {
        private string _campaignCode = string.Empty;
        private string _campaignName = string.Empty;
        private string? _description;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _status = string.Empty;
        private int _createdBy;
        private decimal _budget = 0;

        public string CampaignCode
        {
            get => _campaignCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Mã đợt tuyển dụng không được trống");
                _campaignCode = value.Trim().ToUpper();
            }
        }

        public string CampaignName
        {
            get => _campaignName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tên đợt tuyển dụng không được trống");
                _campaignName = value.Trim();
            }
        }

        public string? Description
        {
            get => _description;
            set => _description = value?.Trim();
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => _startDate = value;
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (value < _startDate)
                    throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu");
                _endDate = value;
            }
        }

        /// <summary>
        /// Status: Planning, Active, Completed, Cancelled
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

        /// <summary>
        /// Ngân sách cho đợt tuyển dụng
        /// </summary>
        public decimal Budget
        {
            get => _budget;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Ngân sách không thể âm");
                _budget = value;
            }
        }

        // Navigation property
        public Employee? Creator { get; set; }

        public RecruitmentCampaign()
        {
            _status = "Planning";
            _startDate = DateTime.Now;
            _endDate = DateTime.Now.AddMonths(3);
        }

        public bool IsActive()
        {
            return _status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                   DateTime.Now >= _startDate &&
                   DateTime.Now <= _endDate;
        }

        public int GetDurationDays()
        {
            return (_endDate - _startDate).Days;
        }
    }
}

