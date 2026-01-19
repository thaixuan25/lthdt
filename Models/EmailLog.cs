using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model EmailLog - Log email gửi đi
    /// </summary>
    public class EmailLog : BaseEntity
    {
        private int _applicationId;
        private string? _emailType;
        private string _recipientEmail = string.Empty;
        private string? _subject;
        private string? _body;
        private DateTime _sentDate;
        private bool _isSent;
        private string? _errorMessage;

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

        /// <summary>
        /// Loại email: Confirmation, InterviewInvitation, PassNotification, FailNotification
        /// </summary>
        public string? EmailType
        {
            get => _emailType;
            set => _emailType = value?.Trim();
        }

        public string RecipientEmail
        {
            get => _recipientEmail;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email người nhận không được trống");
                _recipientEmail = value.Trim().ToLower();
            }
        }

        public string? Subject
        {
            get => _subject;
            set => _subject = value?.Trim();
        }

        public string? Body
        {
            get => _body;
            set => _body = value?.Trim();
        }

        public DateTime SentDate
        {
            get => _sentDate;
            set => _sentDate = value;
        }

        public bool IsSent
        {
            get => _isSent;
            set => _isSent = value;
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => _errorMessage = value?.Trim();
        }

        // Navigation property
        public Application? Application { get; set; }

        public EmailLog()
        {
            _sentDate = DateTime.Now;
            _isSent = false;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   _applicationId > 0 &&
                   !string.IsNullOrWhiteSpace(_recipientEmail);
        }

        public bool IsSuccess()
        {
            return _isSent && string.IsNullOrWhiteSpace(_errorMessage);
        }

        public bool HasError()
        {
            return !_isSent || !string.IsNullOrWhiteSpace(_errorMessage);
        }
    }
}

