using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Services
{
    /// <summary>
    /// Interview Service - Quản lý lịch phỏng vấn
    /// </summary>
    public class InterviewService : BaseService
    {
        private readonly InterviewRepository _interviewRepository;
        private readonly ApplicationRepository _applicationRepository;
        private readonly EmployeeRepository _employeeRepository;
        private readonly IEmailService _emailService;

        public InterviewService(IEmailService emailService)
        {
            _interviewRepository = new InterviewRepository();
            _applicationRepository = new ApplicationRepository();
            _employeeRepository = new EmployeeRepository();
            _emailService = emailService;
        }

        /// <summary>
        /// Tạo lịch phỏng vấn mới từ Interview object đầy đủ
        /// </summary>
        public InterviewResult CreateInterview(Interview interview)
        {
            try
            {
                Log.Info($"Creating interview for Application ID={interview.ApplicationId}");

                // Validate application exists
                var application = _applicationRepository.GetById(interview.ApplicationId);
                if (application == null)
                {
                    return new InterviewResult { Success = false, Message = "Đơn ứng tuyển không tồn tại" };
                }

                // Validate interviewer exists
                var interviewer = _employeeRepository.GetById(interview.InterviewerId);
                if (interviewer == null)
                {
                    return new InterviewResult { Success = false, Message = "Người phỏng vấn không tồn tại" };
                }

                // Set interviewer name
                interview.InterviewerName = interviewer.FullName;

                // Validate interview date
                if (interview.InterviewDate < DateTime.Now)
                {
                    return new InterviewResult { Success = false, Message = "Ngày phỏng vấn phải là tương lai" };
                }

                // Check duplicate round - không cho phép tạo trùng lặp round cho cùng Application
                if (!string.IsNullOrWhiteSpace(interview.InterviewRound))
                {
                    var duplicateRoundInterviews = _interviewRepository.GetByApplicationId(interview.ApplicationId)
                        .Where(i => i.InterviewRound == interview.InterviewRound && 
                                   i.Id != interview.Id)
                        .ToList();

                    if (duplicateRoundInterviews.Any())
                    {
                        return new InterviewResult 
                        { 
                            Success = false, 
                            Message = $"Đã có lịch phỏng vấn {interview.InterviewRound} cho đơn ứng tuyển này. Vui lòng chọn vòng khác." 
                        };
                    }
                }

                // Check duplicate date
                var existingInterviews = _interviewRepository.GetByApplicationId(interview.ApplicationId)
                    .Where(i => i.InterviewDate.Date == interview.InterviewDate.Date && 
                               string.IsNullOrEmpty(i.Result) && 
                               i.Id != interview.Id)
                    .ToList();

                if (existingInterviews.Any())
                {
                    return new InterviewResult 
                    { 
                        Success = false, 
                        Message = "Đã có lịch phỏng vấn vào ngày này" 
                    };
                }
                interview.Result = "Pending";
                
                // Set default values if not provided
                if (string.IsNullOrEmpty(interview.Result))
                {
                    interview.Result = "Pending";
                }
                if (interview.Score < 0)
                {
                    interview.Score = 0;
                }

                var id = _interviewRepository.Add(interview);
                interview.Id = id;

                if (id > 0)
                {
                    // Send email invitation
                    try
                    {
                        _emailService.SendInterviewInvitation(interview);
                    }
                    catch (Exception emailEx)
                    {
                        Log.Error($"Failed to send email: {emailEx.Message}");
                        // Không throw exception, chỉ log lỗi
                    }

                    Log.Info($"Interview created successfully. ID={id}");
                    return new InterviewResult 
                    { 
                        Success = true, 
                        Interview = interview,
                        Message = "Đã tạo lịch phỏng vấn thành công" 
                    };
                }

                return new InterviewResult { Success = false, Message = "Không thể tạo lịch phỏng vấn" };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR creating interview: {ex.Message}");
                return new InterviewResult { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        /// <summary>
        /// Cập nhật lịch phỏng vấn
        /// </summary>
        public InterviewResult UpdateInterview(Interview interview)
        {
            try
            {
                // Validate interview exists
                var existingInterview = _interviewRepository.GetById(interview.Id);
                if (existingInterview == null)
                {
                    return new InterviewResult { Success = false, Message = "Lịch phỏng vấn không tồn tại" };
                }
                
                // Kiểm tra: nếu kết quả đã được thay đổi từ Pending sang Pass/Fail (hoặc Đạt/Không đạt) 
                // thì không cho phép cập nhật nữa
                string existingResult = existingInterview.Result ?? string.Empty;
                bool hasFinalResult = !string.IsNullOrEmpty(existingResult) && 
                                     existingResult != "Pending" &&
                                     (existingResult.Equals("Pass", StringComparison.OrdinalIgnoreCase) ||
                                      existingResult.Equals("Đạt", StringComparison.OrdinalIgnoreCase) ||
                                      existingResult.Equals("Fail", StringComparison.OrdinalIgnoreCase) ||
                                      existingResult.Equals("Không đạt", StringComparison.OrdinalIgnoreCase));
                
                if (hasFinalResult)
                {
                    return new InterviewResult 
                    { 
                        Success = false, 
                        Message = "Không thể cập nhật. Kết quả phỏng vấn đã được xác định (Pass/Fail hoặc Đạt/Không đạt)" 
                    };
                }

                // Validate application exists
                var application = _applicationRepository.GetById(interview.ApplicationId);
                if (application == null)
                {
                    return new InterviewResult { Success = false, Message = "Đơn ứng tuyển không tồn tại" };
                }

                // Validate interviewer exists
                var interviewer = _employeeRepository.GetById(interview.InterviewerId);
                if (interviewer == null)
                {
                    return new InterviewResult { Success = false, Message = "Người phỏng vấn không tồn tại" };
                }

                // Set interviewer name
                interview.InterviewerName = interviewer.FullName;

                // Validate interview date (chỉ validate nếu chưa có kết quả)
                if (string.IsNullOrEmpty(interview.Result) || interview.Result == "Pending")
                {
                    if (interview.InterviewDate < DateTime.Now)
                    {
                        return new InterviewResult { Success = false, Message = "Ngày phỏng vấn phải là tương lai" };
                    }
                }

                // Check duplicate round - không cho phép cập nhật thành round đã tồn tại
                if (!string.IsNullOrWhiteSpace(interview.InterviewRound))
                {
                    var duplicateRoundInterviews = _interviewRepository.GetByApplicationId(interview.ApplicationId)
                        .Where(i => i.InterviewRound == interview.InterviewRound && 
                                   i.Id != interview.Id)
                        .ToList();

                    if (duplicateRoundInterviews.Any())
                    {
                        return new InterviewResult 
                        { 
                            Success = false, 
                            Message = $"Đã có lịch phỏng vấn {interview.InterviewRound} cho đơn ứng tuyển này. Vui lòng chọn vòng khác." 
                        };
                    }
                }

                // Check duplicate date (trừ chính nó)
                var existingInterviews = _interviewRepository.GetByApplicationId(interview.ApplicationId)
                    .Where(i => i.InterviewDate.Date == interview.InterviewDate.Date && 
                               string.IsNullOrEmpty(i.Result) && 
                               i.Id != interview.Id)
                    .ToList();

                if (existingInterviews.Any())
                {
                    return new InterviewResult 
                    { 
                        Success = false, 
                        Message = "Đã có lịch phỏng vấn vào ngày này" 
                    };
                }

                // Kiểm tra xem result có thay đổi không
                string newResult = interview.Result ?? string.Empty;
                bool resultChanged = !existingResult.Equals(newResult, StringComparison.OrdinalIgnoreCase);
                bool scoreChanged = existingInterview.Score != interview.Score;
                
                // Preserve created date
                interview.CreatedDate = existingInterview.CreatedDate;
                interview.UpdatedDate = DateTime.Now;

                bool updated = _interviewRepository.Update(interview);

                if (updated)
                {
                    // Cập nhật điểm và trạng thái Application
                    bool isPass = interview.Result == "Pass" || interview.Result == "Đạt";
                    bool isFail = interview.Result == "Fail" || interview.Result == "Không đạt";
                    
                    // Lấy tất cả các interview của application này
                    var allInterviews = _interviewRepository.GetByApplicationId(interview.ApplicationId)
                        .OrderByDescending(i => i.CreatedDate)
                        .ThenByDescending(i => i.Id)
                        .ToList();
                    
                    var latestInterview = allInterviews.FirstOrDefault();
                    bool isLatestInterview = latestInterview != null && latestInterview.Id == interview.Id;
                    bool shouldUpdateStatus = false;
                    bool shouldUpdateScore = false;

                    // Cập nhật điểm: tính trung bình điểm của TẤT CẢ các vòng đã pass
                    if (isPass || isFail || resultChanged || scoreChanged)
                    {
                        var passedInterviews = allInterviews
                            .Where(i => (i.Result == "Pass" || i.Result == "Đạt") && i.Score > 0)
                            .ToList();

                        if (passedInterviews.Any())
                        {
                            // Tính điểm trung bình từ tất cả các vòng đã pass
                            decimal averageScore = passedInterviews.Average(i => i.Score);
                            application.Score = averageScore;
                            shouldUpdateScore = true;
                            Log.Info($"Application score updated (average of {passedInterviews.Count} passed rounds): ApplicationID={application.Id}, NewScore={application.Score}");
                        }
                        else if (isFail && allInterviews.Any(i => i.Result == "Fail" || i.Result == "Không đạt"))
                        {
                            // Nếu không có vòng nào pass, set điểm về 0
                            application.Score = 0;
                            shouldUpdateScore = true;
                        }
                    }

                    // Cập nhật trạng thái Application: chỉ khi TẤT CẢ các vòng đều đã có kết quả
                    // Kiểm tra xem còn vòng nào chưa có kết quả không
                    var pendingInterviews = allInterviews
                        .Where(i => string.IsNullOrEmpty(i.Result) || 
                                   i.Result == "Pending" ||
                                   (i.Result != "Pass" && i.Result != "Đạt" && 
                                    i.Result != "Fail" && i.Result != "Không đạt"))
                        .ToList();

                    // Chỉ cập nhật status khi tất cả các vòng đều đã có kết quả cuối cùng
                    if (pendingInterviews.Count == 0 && (isPass || isFail))
                    {
                        shouldUpdateStatus = true;
                        
                        // Kiểm tra xem có vòng nào fail không
                        var failedInterviews = allInterviews
                            .Where(i => i.Result == "Fail" || i.Result == "Không đạt")
                            .ToList();

                        if (failedInterviews.Any())
                        {
                            // Nếu có bất kỳ vòng nào fail, status = "Không đạt"
                            application.CurrentStatus = "Không đạt";
                            Log.Info($"Application status updated to 'Không đạt' (has failed interviews): ApplicationID={application.Id}");
                        }
                        else if (isPass)
                        {
                            // Nếu tất cả các vòng đều pass, status = "Đạt"
                            application.CurrentStatus = "Đạt";
                            Log.Info($"Application status updated to 'Đạt' (all interviews passed): ApplicationID={application.Id}");
                        }
                    }
                    else if (pendingInterviews.Count > 0)
                    {
                        // Nếu còn vòng chưa có kết quả, giữ status ở trạng thái trung gian
                        // Không cập nhật status thành "Đạt" hoặc "Không đạt"
                        Log.Info($"Application status not updated: {pendingInterviews.Count} interviews still pending. ApplicationID={application.Id}");
                    }

                    // Chỉ cập nhật database nếu có thay đổi
                    if (shouldUpdateScore || shouldUpdateStatus)
                    {
                        application.UpdatedDate = DateTime.Now;
                        application.UpdatedBy = GetCurrentEmployeeId();
                        _applicationRepository.Update(application);

                        // Gửi email thông báo kết quả chỉ khi trạng thái thay đổi
                        if (shouldUpdateStatus)
                        {
                            try
                            {
                                var emailResult = isPass ? "Pass" : "Fail";
                                _emailService.SendResultNotification(application, emailResult);
                            }
                            catch (Exception emailEx)
                            {
                                Log.Error($"Failed to send result notification email: {emailEx.Message}");
                            }
                        }
                    }

                    Log.Info($"Interview updated successfully. ID={interview.Id}");
                    return new InterviewResult 
                    { 
                        Success = true, 
                        Interview = interview,
                        Message = "Đã cập nhật lịch phỏng vấn thành công" 
                    };
                }

                return new InterviewResult { Success = false, Message = "Không thể cập nhật lịch phỏng vấn" };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR updating interview: {ex.Message}");
                return new InterviewResult { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        /// <summary>
        /// Hủy lịch phỏng vấn
        /// </summary>
        public bool CancelInterview(int interviewId, string reason)
        {
            try
            {
                var interview = _interviewRepository.GetById(interviewId);
                if (interview == null)
                {
                    return false;
                }

                interview.Result = "Đã hủy";
                interview.Notes = $"Hủy lịch: {reason}";
                interview.UpdatedDate = DateTime.Now;

                bool updated = _interviewRepository.Update(interview);
                
                if (updated)
                {
                    Log.Error($"Interview cancelled: ID={interviewId}");
                }

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR cancelling interview: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy lịch phỏng vấn của một ứng viên
        /// </summary>
        public IEnumerable<Interview> GetCandidateInterviews(int applicationId)
        {
            return _interviewRepository.GetByApplicationId(applicationId);
        }

        /// <summary>
        /// Lấy lịch phỏng vấn trong ngày
        /// </summary>
        public IEnumerable<Interview> GetTodayInterviews()
        {
            var today = DateTime.Now.Date;
            return _interviewRepository.GetByDateRange(today, today.AddDays(1));
        }

        /// <summary>
        /// Lấy lịch phỏng vấn sắp tới
        /// </summary>
        public IEnumerable<Interview> GetUpcomingInterviews(int days = 7)
        {
            var today = DateTime.Now;
            var endDate = today.AddDays(days);
            return _interviewRepository.GetByDateRange(today, endDate);
        }

        /// <summary>
        /// Thống kê kết quả phỏng vấn
        /// </summary>
        public InterviewStatistics GetStatistics(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var interviews = _interviewRepository.GetAll().ToList();

            if (fromDate.HasValue)
                interviews = interviews.Where(i => i.InterviewDate >= fromDate.Value).ToList();

            if (toDate.HasValue)
                interviews = interviews.Where(i => i.InterviewDate <= toDate.Value).ToList();

            return new InterviewStatistics
            {
                TotalInterviews = interviews.Count,
                CompletedInterviews = interviews.Count(i => !string.IsNullOrEmpty(i.Result)),
                PassedInterviews = interviews.Count(i => i.Result == "Đạt"),
                FailedInterviews = interviews.Count(i => i.Result == "Không đạt"),
                CancelledInterviews = interviews.Count(i => i.Result == "Đã hủy"),
                PendingInterviews = interviews.Count(i => string.IsNullOrEmpty(i.Result)),
                AverageScore = interviews.Where(i => i.Score > 0).Any() 
                    ? interviews.Where(i => i.Score > 0).Average(i => i.Score) 
                    : 0
            };
        }

        /// <summary>
        /// Lấy tất cả lịch phỏng vấn
        /// </summary>
        public IEnumerable<Interview> GetAllInterviews()
        {
            try
            {
                return _interviewRepository.GetAll();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting all interviews: {ex.Message}");
                return new List<Interview>();
            }
        }

        /// <summary>
        /// Xóa lịch phỏng vấn
        /// </summary>
        public bool DeleteInterview(int interviewId)
        {
            try
            {
                var interview = _interviewRepository.GetById(interviewId);
                if (interview == null)
                {
                    Log.Error($"Interview not found: ID={interviewId}");
                    return false;
                }

                // Chỉ cho phép xóa nếu chưa diễn ra hoặc không có kết quả
                if (interview.InterviewDate < DateTime.Now && !string.IsNullOrEmpty(interview.Result))
                {
                    Log.Error($"Cannot delete completed interview: ID={interviewId}");
                    return false;
                }

                bool deleted = _interviewRepository.Delete(interviewId);
                
                if (deleted)
                {
                    Log.Info($"Interview deleted: ID={interviewId}");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR deleting interview: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Result class cho việc tạo lịch phỏng vấn
    /// </summary>
    public class InterviewResult
    {
        public bool Success { get; set; }
        public Interview? Interview { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Thống kê phỏng vấn
    /// </summary>
    public class InterviewStatistics
    {
        public int TotalInterviews { get; set; }
        public int CompletedInterviews { get; set; }
        public int PassedInterviews { get; set; }
        public int FailedInterviews { get; set; }
        public int CancelledInterviews { get; set; }
        public int PendingInterviews { get; set; }
        public decimal AverageScore { get; set; }
        public double PassRate => TotalInterviews > 0 ? (double)PassedInterviews / CompletedInterviews * 100 : 0;
    }
}

