using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model Application - Đơn ứng tuyển (QUAN TRỌNG NHẤT)
    /// Đại diện cho MỘT LẦN ứng tuyển của ứng viên vào một tin tuyển dụng
    /// Một Candidate có thể có nhiều Application
    /// </summary>
    public class Application : BaseEntity
    {
        private int _candidateId;
        private int _jobPostingId;
        private DateTime _applyDate;
        private string? _coverLetter;
        private string? _source;
        private string _currentStatus = string.Empty;
        private decimal _score;
        private string? _notes;
        private int? _updatedBy;

        public int CandidateId
        {
            get => _candidateId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn ứng viên");
                _candidateId = value;
            }
        }

        public int JobPostingId
        {
            get => _jobPostingId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn tin tuyển dụng");
                _jobPostingId = value;
            }
        }

        public DateTime ApplyDate
        {
            get => _applyDate;
            set => _applyDate = value;
        }

        /// <summary>
        /// Thư xin việc riêng cho lần ứng tuyển này
        /// </summary>
        public string? CoverLetter
        {
            get => _coverLetter;
            set => _coverLetter = value?.Trim();
        }

        /// <summary>
        /// Nguồn ứng tuyển: Website, LinkedIn, Referral, JobFair, etc.
        /// </summary>
        public string? Source
        {
            get => _source;
            set => _source = value?.Trim();
        }

        /// <summary>
        /// Trạng thái hiện tại: Nộp đơn, Sơ tuyển, Phỏng vấn vòng 1, 2, 3, Đạt, Không đạt
        /// </summary>
        public string CurrentStatus
        {
            get => _currentStatus;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Trạng thái không được trống");
                _currentStatus = value.Trim();
            }
        }

        /// <summary>
        /// Điểm đánh giá tổng thể (0-100)
        /// </summary>
        public decimal Score
        {
            get => _score;
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentException("Điểm phải từ 0 đến 100");
                _score = value;
            }
        }

        /// <summary>
        /// Ghi chú của HR
        /// </summary>
        public string? Notes
        {
            get => _notes;
            set => _notes = value?.Trim();
        }

        /// <summary>
        /// Người cập nhật cuối cùng (FK đến Employee)
        /// </summary>
        public int? UpdatedBy
        {
            get => _updatedBy;
            set => _updatedBy = value;
        }

        // Navigation properties
        public Candidate? Candidate { get; set; }
        public JobPosting? JobPosting { get; set; }
        public Employee? Updater { get; set; }

        // Display properties for DataGridView binding
        public string CandidateName => string.IsNullOrWhiteSpace(Candidate?.FullName) ? "N/A" : Candidate.FullName;
        public string CandidateEmail => string.IsNullOrWhiteSpace(Candidate?.Email) ? "N/A" : Candidate.Email;
        public string JobTitle => string.IsNullOrWhiteSpace(JobPosting?.JobTitle) ? "N/A" : JobPosting.JobTitle;
        public string JobCode => string.IsNullOrWhiteSpace(JobPosting?.JobCode) ? "N/A" : JobPosting.JobCode;

        public Application()
        {
            _applyDate = DateTime.Now;
            _currentStatus = "Nộp đơn";
            _score = 0;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   _candidateId > 0 &&
                   _jobPostingId > 0 &&
                   !string.IsNullOrWhiteSpace(_currentStatus);
        }

        /// <summary>
        /// Kiểm tra đã vượt qua vòng phỏng vấn chưa
        /// </summary>
        public bool IsPassed()
        {
            return _currentStatus.Equals("Đạt", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra đã bị loại chưa
        /// </summary>
        public bool IsFailed()
        {
            return _currentStatus.Equals("Không đạt", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra đang trong quá trình xét duyệt
        /// </summary>
        public bool IsInProgress()
        {
            return !IsPassed() && !IsFailed();
        }

        /// <summary>
        /// Lấy số ngày từ khi nộp đơn
        /// </summary>
        public int GetDaysSinceApply()
        {
            return (DateTime.Now - _applyDate).Days;
        }

        /// <summary>
        /// Kiểm tra có điểm tốt không (>= 70)
        /// </summary>
        public bool HasGoodScore()
        {
            return _score >= 70;
        }
    }
}

