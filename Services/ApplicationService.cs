using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.Models;
using LTHDT2.DataAccess.Repositories;
using AppModel = LTHDT2.Models.Application;

namespace LTHDT2.Services
{
    /// <summary>
    /// Application Service
    /// Quản lý đơn ứng tuyển
    /// </summary>
    public class ApplicationService : BaseService
    {
        private readonly ApplicationRepository _applicationRepository;
        private readonly CandidateRepository _candidateRepository;
        private readonly IEmailService? _emailService;

        /// <summary>
        /// Constructor Injection
        /// </summary>
        public ApplicationService(IEmailService? emailService = null)
        {
            _applicationRepository = new ApplicationRepository();
            _candidateRepository = new CandidateRepository();
            _emailService = emailService;
        }

        /// <summary>
        /// Tạo đơn ứng tuyển mới
        /// </summary>
        public ApplicationResult CreateApplication(int candidateId, int jobPostingId, string? coverLetter, string? source)
        {
            try
            {
                // Validate
                if (candidateId <= 0)
                    throw new ArgumentException("Phải chọn ứng viên");
                
                if (jobPostingId <= 0)
                    throw new ArgumentException("Phải chọn tin tuyển dụng");

                // Kiểm tra trùng lặp
                if (_applicationRepository.CheckDuplicateApplication(candidateId, jobPostingId))
                {
                    LogAction("Create Application Failed", $"CandidateID={candidateId} đã ứng tuyển JobPostingID={jobPostingId}");
                    return new ApplicationResult
                    {
                        Success = false,
                        Message = "Ứng viên đã ứng tuyển vị trí này."
                    };
                }

                // Tạo Application
                var application = new AppModel
                {
                    CandidateId = candidateId,
                    JobPostingId = jobPostingId,
                    CoverLetter = coverLetter,
                    Source = source,
                    ApplyDate = DateTime.Now,
                    CurrentStatus = "Nộp đơn",
                    Score = 0,
                    UpdatedBy = GetCurrentEmployeeId()
                };

                // Validate
                ValidateEntity(application);

                // Save
                var id = _applicationRepository.Add(application);
                if (id <= 0)
                {
                    return new ApplicationResult { Success = false, Message = "Lỗi khi lưu đơn ứng tuyển" };
                }

                application.Id = id;

                // Load đầy đủ thông tin để gửi email
                var fullApplication = _applicationRepository.GetById(id);

                // Gửi email xác nhận
                if (_emailService != null && fullApplication != null)
                {
                    try
                    {
                        _emailService.SendApplicationConfirmation(fullApplication);
                    }
                    catch (Exception emailEx)
                    {
                        LogError("SendApplicationConfirmation", emailEx);
                        // Không throw - email fail không ảnh hưởng việc tạo application
                    }
                }

                LogAction("Create Application", $"ApplicationID={id} created for CandidateID={candidateId}, JobPostingID={jobPostingId}");

                return new ApplicationResult
                {
                    Success = true,
                    ApplicationId = id,
                    Application = fullApplication,
                    Message = "Tạo đơn ứng tuyển thành công!"
                };
            }
            catch (Exception ex)
            {
                LogError(nameof(CreateApplication), ex);
                return new ApplicationResult { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn ứng tuyển
        /// </summary>
        public bool UpdateStatus(int applicationId, string newStatus, string? comments, decimal? score = null)
        {
            try
            {
                // Lấy application
                var application = _applicationRepository.GetById(applicationId);
                if (application == null)
                {
                    throw new ArgumentException("Đơn ứng tuyển không tồn tại");
                }

                // Cập nhật
                application.CurrentStatus = newStatus;
                
                // if (score.HasValue)
                //     application.Score = score.Value;
                
                if (!string.IsNullOrWhiteSpace(comments))
                {
                    application.Notes = string.IsNullOrWhiteSpace(application.Notes)
                        ? comments
                        : application.Notes + "\n" + comments;
                }

                application.UpdatedBy = GetCurrentEmployeeId();
                application.UpdatedDate = DateTime.Now;

                var result = _applicationRepository.Update(application);

                if (result)
                {
                    // Gửi email thông báo nếu là trạng thái cuối
                    if ((newStatus.Equals("Đạt", StringComparison.OrdinalIgnoreCase) ||
                         newStatus.Equals("Không đạt", StringComparison.OrdinalIgnoreCase)) &&
                        _emailService != null)
                    {
                        try
                        {
                            var emailResult = newStatus.Equals("Đạt", StringComparison.OrdinalIgnoreCase) ? "Pass" : "Fail";
                            _emailService.SendResultNotification(application, emailResult);
                        }
                        catch (Exception emailEx)
                        {
                            LogError("SendResultNotification", emailEx);
                        }
                    }

                    LogAction("Update Application Status", $"ApplicationID={applicationId} => {newStatus}");
                }

                return result;
            }
            catch (Exception ex)
            {
                LogError(nameof(UpdateStatus), ex);
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách Application theo JobPosting
        /// </summary>
        public IEnumerable<AppModel> GetByJobPosting(int jobPostingId)
        {
            return _applicationRepository.GetByJobPosting(jobPostingId);
        }

        /// <summary>
        /// Lấy danh sách Application theo Candidate
        /// </summary>
        public IEnumerable<AppModel> GetByCandidate(int candidateId)
        {
            return _applicationRepository.GetByCandidate(candidateId);
        }

        /// <summary>
        /// Lấy danh sách Application theo trạng thái
        /// </summary>
        public IEnumerable<AppModel> GetByStatus(string status)
        {
            return _applicationRepository.GetByStatus(status);
        }

        /// <summary>
        /// Lấy Application với đầy đủ thông tin
        /// </summary>
        public AppModel? GetApplicationDetail(int applicationId)
        {
            return _applicationRepository.GetById(applicationId);
        }

        /// <summary>
        /// Tìm kiếm Applications
        /// </summary>
        public IEnumerable<AppModel> SearchApplications(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _applicationRepository.GetAll();

            return _applicationRepository.Find(a =>
                a.Candidate?.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true ||
                a.Candidate?.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true ||
                a.JobPosting?.JobTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true ||
                a.CurrentStatus.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Lấy thống kê Applications
        /// </summary>
        public ApplicationStatistics GetStatistics(int? jobPostingId = null)
        {
            IEnumerable<AppModel> applications;
            
            if (jobPostingId.HasValue)
                applications = _applicationRepository.GetByJobPosting(jobPostingId.Value);
            else
                applications = _applicationRepository.GetAll();

            var stats = new ApplicationStatistics
            {
                Total = applications.Count(),
                Pending = applications.Count(a => a.IsInProgress()),
                Passed = applications.Count(a => a.IsPassed()),
                Failed = applications.Count(a => a.IsFailed()),
                AverageScore = applications.Any() ? applications.Average(a => (double)a.Score) : 0
            };

            return stats;
        }
    }

    /// <summary>
    /// Result class cho CreateApplication
    /// </summary>
    public class ApplicationResult
    {
        public bool Success { get; set; }
        public int ApplicationId { get; set; }
        public AppModel? Application { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Statistics class
    /// </summary>
    public class ApplicationStatistics
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public double AverageScore { get; set; }
        public double PassRate => Total > 0 ? (double)Passed / Total * 100 : 0;
        public double FailRate => Total > 0 ? (double)Failed / Total * 100 : 0;
    }
}

