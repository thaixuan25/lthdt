using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model CampaignJobPosting - Bảng trung gian Many-to-Many
    /// Liên kết giữa Campaign và JobPosting
    /// </summary>
    public class CampaignJobPosting : BaseEntity
    {
        private int _campaignId;
        private int _jobPostingId;
        private DateTime _addedDate;

        public int CampaignId
        {
            get => _campaignId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn đợt tuyển dụng");
                _campaignId = value;
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

        public DateTime AddedDate
        {
            get => _addedDate;
            set => _addedDate = value;
        }

        // Navigation properties
        public RecruitmentCampaign? Campaign { get; set; }
        public JobPosting? JobPosting { get; set; }

        public CampaignJobPosting()
        {
            _addedDate = DateTime.Now;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   _campaignId > 0 &&
                   _jobPostingId > 0;
        }
    }
}

