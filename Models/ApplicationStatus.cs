using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model ApplicationStatus - Lịch sử trạng thái đơn ứng tuyển
    /// Theo dõi sự thay đổi trạng thái của Application
    /// </summary>
    public class ApplicationStatus : BaseEntity
    {
        private int _applicationId;
        private string _statusName = string.Empty;
        private string? _comments;
        private DateTime _statusDate;
        private int _updatedBy;
        private string? _oldStatus;
        private string? _newStatus;

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

        public string StatusName
        {
            get => _statusName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tên trạng thái không được trống");
                _statusName = value.Trim();
            }
        }

        public string? Comments
        {
            get => _comments;
            set => _comments = value?.Trim();
        }

        public DateTime StatusDate
        {
            get => _statusDate;
            set => _statusDate = value;
        }

        public int UpdatedBy
        {
            get => _updatedBy;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải có người cập nhật");
                _updatedBy = value;
            }
        }

        /// <summary>
        /// Trạng thái cũ (trước khi thay đổi)
        /// </summary>
        public string? OldStatus
        {
            get => _oldStatus;
            set => _oldStatus = value?.Trim();
        }

        /// <summary>
        /// Trạng thái mới (sau khi thay đổi)
        /// </summary>
        public string? NewStatus
        {
            get => _newStatus ?? _statusName;
            set => _newStatus = value?.Trim();
        }

        /// <summary>
        /// Alias: StatusDate = ChangedDate
        /// </summary>
        public DateTime ChangedDate
        {
            get => _statusDate;
            set => _statusDate = value;
        }

        /// <summary>
        /// Alias: UpdatedBy = ChangedBy
        /// </summary>
        public int ChangedBy
        {
            get => _updatedBy;
            set => _updatedBy = value;
        }

        // Navigation properties
        public Application? Application { get; set; }
        public Employee? Updater { get; set; }

        public ApplicationStatus()
        {
            _statusDate = DateTime.Now;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   _applicationId > 0 &&
                   !string.IsNullOrWhiteSpace(_statusName) &&
                   _updatedBy > 0;
        }
    }
}

