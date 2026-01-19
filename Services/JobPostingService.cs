using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Services
{
    /// <summary>
    /// JobPosting Service
    /// </summary>
    public class JobPostingService : BaseService
    {
        private readonly JobPostingRepository _jobPostingRepository;
        private readonly HeadcountRepository _headcountRepository;
        private readonly ApplicationRepository _applicationRepository;
        private readonly IHeadcountService _headcountService;

        public JobPostingService(IHeadcountService headcountService)
        {
            _jobPostingRepository = new JobPostingRepository();
            _headcountRepository = new HeadcountRepository();
            _applicationRepository = new ApplicationRepository();
            _headcountService = headcountService;
        }

        /// <summary>
        /// Lấy tất cả tin tuyển dụng
        /// </summary>
        public IEnumerable<JobPosting> GetAll()
        {
            try
            {
                return _jobPostingRepository.GetAll();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting all job postings: {ex.Message}");
                return new List<JobPosting>();
            }
        }

        /// <summary>
        /// Lấy tin tuyển dụng theo ID
        /// </summary>
        public JobPosting? GetById(int id)
        {
            try
            {
                return _jobPostingRepository.GetById(id);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting job posting by ID {id}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Cập nhật tin tuyển dụng
        /// </summary>
        public bool UpdateJobPosting(JobPosting jobPosting, bool overrideHeadcount = false)
        {
            try
            {
                ValidateEntity(jobPosting);

                // Check headcount if needed
                if (!overrideHeadcount && (jobPosting.IsHeadcountApproved || jobPosting.NumberOfPositions > 0))
                {
                    var headcountCheck = _headcountService.CheckHeadcount(
                        jobPosting.DepartmentId,
                        jobPosting.PositionId,
                        jobPosting.NumberOfPositions);

                    if (!headcountCheck.IsApproved)
                    {
                        jobPosting.IsHeadcountApproved = false;
                    }
                    else
                    {
                        jobPosting.IsHeadcountApproved = true;
                    }
                }

                jobPosting.UpdatedDate = DateTime.Now;
                bool updated = _jobPostingRepository.Update(jobPosting);

                if (updated)
                {
                    LogAction("Update", $"Job posting updated: {jobPosting.JobCode}");
                }

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR updating job posting: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Xóa tin tuyển dụng
        /// </summary>
        public bool DeleteJobPosting(int id)
        {
            try
            {
                // Kiểm tra xem có ứng viên đã nộp đơn chưa
                var applicationCount = _applicationRepository.GetAll()
                    .Count(a => a.JobPostingId == id);

                if (applicationCount > 0)
                {
                    throw new InvalidOperationException(
                        $"Không thể xóa tin tuyển dụng. Đã có {applicationCount} ứng viên nộp đơn cho tin này.");
                }

                bool deleted = _jobPostingRepository.Delete(id);

                if (deleted)
                {
                    LogAction("Delete", $"Job posting deleted: ID={id}");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR deleting job posting: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tạo tin tuyển dụng mới với kiểm tra định biên
        /// </summary>
        public JobPostingResult CreateJobPosting(JobPosting jobPosting, bool overrideHeadcount = false)
        {
            try
            {
                ValidateEntity(jobPosting);

                // Validate deadline
                if (jobPosting.Deadline < DateTime.Now)
                {
                    return new JobPostingResult 
                    { 
                        Success = false, 
                        Message = "Deadline phải sau ngày hôm nay" 
                    };
                }

                // Set CreatedBy
                jobPosting.CreatedBy = GetCurrentEmployeeId();
                if (jobPosting.CreatedBy <= 0)
                {
                    return new JobPostingResult
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Employee của user hiện tại.\nVui lòng liên hệ Admin để gắn User với Employee."
                    };
                }

                // Check headcount
                if (!overrideHeadcount)
                {
                    var headcountCheck = _headcountService.CheckHeadcount(
                        jobPosting.DepartmentId, 
                        jobPosting.PositionId, 
                        jobPosting.NumberOfPositions);

                    if (!headcountCheck.IsApproved)
                    {
                        return new JobPostingResult
                        {
                            Success = false,
                            HeadcountResult = headcountCheck,
                            Message = $"Vượt định biên! {headcountCheck.Message}"
                        };
                    }

                    jobPosting.IsHeadcountApproved = true;
                }
                else
                {
                    jobPosting.IsHeadcountApproved = false;
                    Log.Error($"WARNING: Headcount override for job posting {jobPosting.JobCode}");
                }

                // Create
                var id = _jobPostingRepository.Add(jobPosting);
                jobPosting.Id = id;

                if (id > 0)
                {
                    LogAction("Create", $"Job posting created: {jobPosting.JobCode} (ID={id})");
                    return new JobPostingResult
                    {
                        Success = true,
                        JobPosting = jobPosting,
                        Message = "Tạo tin tuyển dụng thành công"
                    };
                }

                return new JobPostingResult { Success = false, Message = "Không thể tạo tin tuyển dụng" };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR creating job posting: {ex.Message}");
                return new JobPostingResult { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        /// <summary>
        /// Đóng tin tuyển dụng
        /// </summary>
        public bool CloseJobPosting(int jobPostingId, string reason)
        {
            try
            {
                var jobPosting = _jobPostingRepository.GetById(jobPostingId);
                if (jobPosting == null)
                    return false;

                jobPosting.Status = "Đóng";
                jobPosting.UpdatedDate = DateTime.Now;

                bool updated = _jobPostingRepository.Update(jobPosting);

                if (updated)
                {
                    Log.Error($"Job posting closed: ID={jobPostingId}, Reason: {reason}");
                }

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR closing job posting: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gia hạn deadline
        /// </summary>
        public bool ExtendDeadline(int jobPostingId, DateTime newDeadline, string reason)
        {
            try
            {
                var jobPosting = _jobPostingRepository.GetById(jobPostingId);
                if (jobPosting == null)
                    return false;

                if (newDeadline <= jobPosting.Deadline)
                {
                    Log.Error($"ERROR: New deadline must be after current deadline");
                    return false;
                }

                DateTime oldDeadline = jobPosting.Deadline;
                jobPosting.Deadline = newDeadline;
                jobPosting.UpdatedDate = DateTime.Now;

                bool updated = _jobPostingRepository.Update(jobPosting);

                if (updated)
                {
                    Log.Error($"Deadline extended: ID={jobPostingId}, {oldDeadline:dd/MM/yyyy} -> {newDeadline:dd/MM/yyyy}, Reason: {reason}");
                }

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR extending deadline: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy tin tuyển dụng đang mở
        /// </summary>
        public IEnumerable<JobPosting> GetActiveJobPostings()
        {
            try
            {
                var now = DateTime.Now;
                return _jobPostingRepository.GetAll()
                    .Where(j => j.Status == "Đang mở" && j.Deadline >= now)
                    .OrderByDescending(j => j.PostDate);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting active job postings: {ex.Message}");
                return new List<JobPosting>();
            }
        }

        /// <summary>
        /// Tìm kiếm tin tuyển dụng
        /// </summary>
        public IEnumerable<JobPosting> SearchJobPostings(string keyword, int? departmentId = null, 
            int? positionId = null, string? status = null)
        {
            try
            {
                var jobs = _jobPostingRepository.GetAll().AsEnumerable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    jobs = jobs.Where(j =>
                        j.JobCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        j.JobTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                }

                if (departmentId.HasValue)
                    jobs = jobs.Where(j => j.DepartmentId == departmentId.Value);

                if (positionId.HasValue)
                    jobs = jobs.Where(j => j.PositionId == positionId.Value);

                if (!string.IsNullOrWhiteSpace(status))
                    jobs = jobs.Where(j => j.Status == status);

                return jobs.OrderByDescending(j => j.PostDate).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR searching job postings: {ex.Message}");
                return new List<JobPosting>();
            }
        }

        /// <summary>
        /// Thống kê tin tuyển dụng
        /// </summary>
        public JobPostingStatistics GetStatistics(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var jobs = _jobPostingRepository.GetAll().ToList();

                if (fromDate.HasValue)
                    jobs = jobs.Where(j => j.PostDate >= fromDate.Value).ToList();

                if (toDate.HasValue)
                    jobs = jobs.Where(j => j.PostDate <= toDate.Value).ToList();

                var now = DateTime.Now;

                return new JobPostingStatistics
                {
                    TotalJobPostings = jobs.Count,
                    ActiveJobPostings = jobs.Count(j => j.Status == "Đang mở" && j.Deadline >= now),
                    ClosedJobPostings = jobs.Count(j => j.Status == "Đóng"),
                    ExpiredJobPostings = jobs.Count(j => j.Status == "Đang mở" && j.Deadline < now),
                    TotalPositions = jobs.Sum(j => j.NumberOfPositions),
                    HeadcountApproved = jobs.Count(j => j.IsHeadcountApproved),
                    HeadcountNotApproved = jobs.Count(j => !j.IsHeadcountApproved)
                };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting statistics: {ex.Message}");
                return new JobPostingStatistics();
            }
        }

        /// <summary>
        /// Lấy số lượng ứng viên đã apply cho một job posting
        /// </summary>
        public int GetApplicationCount(int jobPostingId)
        {
            try
            {
                return _applicationRepository.GetAll()
                    .Count(a => a.JobPostingId == jobPostingId);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting application count: {ex.Message}");
                return 0;
            }
        }
    }

    /// <summary>
    /// Result class cho việc tạo job posting
    /// </summary>
    public class JobPostingResult
    {
        public bool Success { get; set; }
        public JobPosting? JobPosting { get; set; }
        public HeadcountResult? HeadcountResult { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Thống kê tin tuyển dụng
    /// </summary>
    public class JobPostingStatistics
    {
        public int TotalJobPostings { get; set; }
        public int ActiveJobPostings { get; set; }
        public int ClosedJobPostings { get; set; }
        public int ExpiredJobPostings { get; set; }
        public int TotalPositions { get; set; }
        public int HeadcountApproved { get; set; }
        public int HeadcountNotApproved { get; set; }
    }
}

