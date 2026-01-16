using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Services
{
    /// <summary>
    /// Recruitment Campaign Service - Quản lý đợt tuyển dụng
    /// Kế thừa BaseService
    /// </summary>
    public class RecruitmentCampaignService : BaseService
    {
        private readonly RecruitmentCampaignRepository _campaignRepository;
        private readonly CampaignJobPostingRepository _campaignJobRepository;
        private readonly JobPostingRepository _jobPostingRepository;

        public RecruitmentCampaignService()
        {
            _campaignRepository = new RecruitmentCampaignRepository();
            _campaignJobRepository = new CampaignJobPostingRepository();
            _jobPostingRepository = new JobPostingRepository();
        }

        /// <summary>
        /// Tạo đợt tuyển dụng mới
        /// </summary>
        public CampaignResult CreateCampaign(RecruitmentCampaign campaign)
        {
            try
            {
                // Validate
                if (campaign.EndDate <= campaign.StartDate)
                {
                    return new CampaignResult 
                    { 
                        Success = false, 
                        Message = "Ngày kết thúc phải sau ngày bắt đầu" 
                    };
                }

                if (campaign.Budget < 0)
                {
                    return new CampaignResult 
                    { 
                        Success = false, 
                        Message = "Ngân sách không hợp lệ" 
                    };
                }

                // Check duplicate code
                var existing = _campaignRepository.GetAll()
                    .FirstOrDefault(c => c.CampaignCode == campaign.CampaignCode);

                if (existing != null)
                {
                    return new CampaignResult 
                    { 
                        Success = false, 
                        Message = "Mã đợt tuyển dụng đã tồn tại" 
                    };
                }

                var id = _campaignRepository.Add(campaign);
                campaign.Id = id;

                if (id > 0)
                {
                    Log.Error($"Campaign created: {campaign.CampaignCode}");
                    return new CampaignResult
                    {
                        Success = true,
                        Campaign = campaign,
                        Message = "Tạo đợt tuyển dụng thành công"
                    };
                }

                return new CampaignResult { Success = false, Message = "Không thể tạo đợt tuyển dụng" };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR creating campaign: {ex.Message}");
                return new CampaignResult { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        /// <summary>
        /// Thêm job posting vào campaign
        /// </summary>
        public bool AddJobToCampaign(int campaignId, int jobPostingId)
        {
            try
            {
                // Check if already exists
                if (_campaignJobRepository.ExistsInCampaign(campaignId, jobPostingId))
                {
                    Log.Error($"Job {jobPostingId} already in campaign {campaignId}");
                    return false;
                }

                var campaignJob = new CampaignJobPosting
                {
                    CampaignId = campaignId,
                    JobPostingId = jobPostingId,
                    AddedDate = DateTime.Now
                };

                var id = _campaignJobRepository.Add(campaignJob);
                
                if (id > 0)
                {
                    Log.Error($"Added job {jobPostingId} to campaign {campaignId}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR adding job to campaign: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa job posting khỏi campaign
        /// </summary>
        public bool RemoveJobFromCampaign(int campaignId, int jobPostingId)
        {
            try
            {
                bool removed = _campaignJobRepository.RemoveFromCampaign(campaignId, jobPostingId);
                
                if (removed)
                {
                    Log.Error($"Removed job {jobPostingId} from campaign {campaignId}");
                }

                return removed;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR removing job from campaign: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy tất cả job postings trong một campaign
        /// </summary>
        public List<JobPosting> GetCampaignJobs(int campaignId)
        {
            try
            {
                var campaignJobs = _campaignJobRepository.GetByCampaignId(campaignId).ToList();
                var jobIds = campaignJobs.Select(cj => cj.JobPostingId).ToList();

                return _jobPostingRepository.GetAll()
                    .Where(j => jobIds.Contains(j.Id))
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting campaign jobs: {ex.Message}");
                return new List<JobPosting>();
            }
        }

        /// <summary>
        /// Bắt đầu campaign
        /// </summary>
        public bool StartCampaign(int campaignId)
        {
            try
            {
                var campaign = _campaignRepository.GetById(campaignId);
                if (campaign == null)
                    return false;

                campaign.Status = "Đang chạy";
                campaign.UpdatedDate = DateTime.Now;

                bool updated = _campaignRepository.Update(campaign);
                
                if (updated)
                {
                    Log.Error($"Campaign started: {campaign.CampaignCode}");
                }

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR starting campaign: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kết thúc campaign
        /// </summary>
        public bool EndCampaign(int campaignId, string reason)
        {
            try
            {
                var campaign = _campaignRepository.GetById(campaignId);
                if (campaign == null)
                    return false;

                campaign.Status = "Đã kết thúc";
                campaign.UpdatedDate = DateTime.Now;

                bool updated = _campaignRepository.Update(campaign);
                
                if (updated)
                {
                    Log.Error($"Campaign ended: {campaign.CampaignCode}, Reason: {reason}");
                }

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR ending campaign: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Thống kê campaign
        /// </summary>
        public CampaignStatistics GetCampaignStatistics(int campaignId)
        {
            try
            {
                var campaign = _campaignRepository.GetById(campaignId);
                if (campaign == null)
                    return new CampaignStatistics();

                var jobs = GetCampaignJobs(campaignId);
                int totalPositions = jobs.Sum(j => j.NumberOfPositions);
                
                // Đếm số applications cho các jobs trong campaign
                // (cần ApplicationRepository)

                return new CampaignStatistics
                {
                    CampaignId = campaignId,
                    CampaignName = campaign.CampaignName,
                    TotalJobPostings = jobs.Count,
                    TotalPositions = totalPositions,
                    ActiveJobPostings = jobs.Count(j => j.Status == "Đang mở"),
                    Budget = campaign.Budget,
                    DurationDays = (campaign.EndDate - campaign.StartDate).Days
                };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting campaign statistics: {ex.Message}");
                return new CampaignStatistics();
            }
        }

        /// <summary>
        /// Lấy campaigns đang active
        /// </summary>
        public IEnumerable<RecruitmentCampaign> GetActiveCampaigns()
        {
            return _campaignRepository.GetActiveCampaigns();
        }

        /// <summary>
        /// Lấy tất cả campaigns
        /// </summary>
        public IEnumerable<RecruitmentCampaign> GetAllCampaigns()
        {
            try
            {
                return _campaignRepository.GetAll();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting all campaigns: {ex.Message}");
                return new List<RecruitmentCampaign>();
            }
        }

        /// <summary>
        /// Xóa campaign
        /// </summary>
        public bool DeleteCampaign(int campaignId)
        {
            try
            {
                // Kiểm tra xem campaign có job postings không
                var jobs = GetCampaignJobs(campaignId);
                if (jobs.Any())
                {
                    Log.Error($"Cannot delete campaign {campaignId}: Has job postings");
                    return false;
                }

                bool deleted = _campaignRepository.Delete(campaignId);
                
                if (deleted)
                {
                    Log.Info($"Campaign deleted: ID={campaignId}");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR deleting campaign: {ex.Message}");
                return false;
            }
        }
    }

    public class CampaignResult
    {
        public bool Success { get; set; }
        public RecruitmentCampaign? Campaign { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CampaignStatistics
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public int TotalJobPostings { get; set; }
        public int TotalPositions { get; set; }
        public int ActiveJobPostings { get; set; }
        public decimal Budget { get; set; }
        public int DurationDays { get; set; }
    }
}

