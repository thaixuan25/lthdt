using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model Interview - Phỏng vấn
    /// </summary>
    public class Interview : BaseEntity
    {
        private int _applicationId;
        private int _interviewerId;
        private DateTime _interviewDate;
        private string? _interviewType;
        private string? _interviewRound;
        private string? _interviewNotes;
        private decimal _score;
        private string? _result;

        public int ApplicationId
        {
            get => _applicationId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn đơn ứng tuyển");
                _applicationId = value;
            }
        }

        public int InterviewerId
        {
            get => _interviewerId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn người phỏng vấn");
                _interviewerId = value;
            }
        }

        public DateTime InterviewDate
        {
            get => _interviewDate;
            set => _interviewDate = value;
        }

        /// <summary>
        /// Loại phỏng vấn: Online, Offline
        /// </summary>
        public string? InterviewType
        {
            get => _interviewType;
            set => _interviewType = value?.Trim();
        }

        /// <summary>
        /// Vòng phỏng vấn: Vòng 1, Vòng 2, Vòng 3
        /// </summary>
        public string? InterviewRound
        {
            get => _interviewRound;
            set => _interviewRound = value?.Trim();
        }

        /// <summary>
        /// Ghi chú phỏng vấn
        /// </summary>
        public string? InterviewNotes
        {
            get => _interviewNotes;
            set => _interviewNotes = value?.Trim();
        }

        /// <summary>
        /// Điểm phỏng vấn (0-100)
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
        /// Kết quả: Pass, Fail, Pending
        /// </summary>
        public string? Result
        {
            get => _result;
            set => _result = value?.Trim();
        }

        /// <summary>
        /// InterviewerName
        /// </summary>
        public string InterviewerName { get; set; } = string.Empty;

        /// <summary>
        /// Notes
        /// </summary>
        public string? Notes
        {
            get => _interviewNotes;
            set => _interviewNotes = value?.Trim();
        }

        /// <summary>
        /// Địa điểm phỏng vấn
        /// </summary>
        public string? Location { get; set; }

        // Navigation properties
        public Application? Application { get; set; }
        public Employee? Interviewer { get; set; }

        public Interview()
        {
            _interviewDate = DateTime.Now.AddDays(3);
            _score = 0;
            _result = "Pending";
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   _applicationId > 0 &&
                   _interviewerId > 0;
        }

        public bool IsPassed()
        {
            return _result?.Equals("Pass", StringComparison.OrdinalIgnoreCase) == true;
        }

        public bool IsFailed()
        {
            return _result?.Equals("Fail", StringComparison.OrdinalIgnoreCase) == true;
        }

        public bool IsPending()
        {
            return _result?.Equals("Pending", StringComparison.OrdinalIgnoreCase) == true ||
                   string.IsNullOrWhiteSpace(_result);
        }

        public bool IsUpcoming()
        {
            return _interviewDate > DateTime.Now;
        }

        public bool IsCompleted()
        {
            return !IsPending() && _interviewDate <= DateTime.Now;
        }

        public int GetDaysUntilInterview()
        {
            return (_interviewDate - DateTime.Now).Days;
        }
    }
}

