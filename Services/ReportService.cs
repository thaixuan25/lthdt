using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Services
{
    /// <summary>
    /// Report Service
    /// </summary>
    public class ReportService : BaseService
    {
        private readonly JobPostingRepository _jobPostingRepository;
        private readonly ApplicationRepository _applicationRepository;
        private readonly CandidateRepository _candidateRepository;
        private readonly InterviewRepository _interviewRepository;
        private readonly HeadcountRepository _headcountRepository;
        private readonly DepartmentRepository _departmentRepository;
        private readonly PositionRepository _positionRepository;

        public ReportService()
        {
            _jobPostingRepository = new JobPostingRepository();
            _applicationRepository = new ApplicationRepository();
            _candidateRepository = new CandidateRepository();
            _interviewRepository = new InterviewRepository();
            _headcountRepository = new HeadcountRepository();
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
        }

        /// <summary>
        /// Báo cáo tổng quan tuyển dụng
        /// </summary>
        public RecruitmentOverviewReport GetOverviewReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                Log.Info($"Generating overview report: {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}");

                var jobPostings = _jobPostingRepository.GetAll()
                    .Where(j => j.PostDate >= fromDate && j.PostDate <= toDate)
                    .ToList();

                var applications = _applicationRepository.GetAll()
                    .Where(a => a.ApplyDate >= fromDate && a.ApplyDate <= toDate)
                    .ToList();

                var interviews = _interviewRepository.GetAll()
                    .Where(i => i.InterviewDate >= fromDate && i.InterviewDate <= toDate)
                    .ToList();

                return new RecruitmentOverviewReport
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalJobPostings = jobPostings.Count,
                    ActiveJobPostings = jobPostings.Count(j => j.Status == "Đang mở"),
                    TotalPositionsRequired = jobPostings.Sum(j => j.NumberOfPositions),
                    TotalApplications = applications.Count,
                    UniqueApplicants = applications.Select(a => a.CandidateId).Distinct().Count(),
                    TotalInterviews = interviews.Count,
                    CompletedInterviews = interviews.Count(i => !string.IsNullOrEmpty(i.Result)),
                    PassedApplications = applications.Count(a => a.CurrentStatus == "Đạt"),
                    RejectedApplications = applications.Count(a => a.CurrentStatus == "Không đạt"),
                    AverageApplicationsPerJob = jobPostings.Any() ? (double)applications.Count / jobPostings.Count : 0
                };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR generating overview report: {ex.Message}");
                return new RecruitmentOverviewReport();
            }
        }

        /// <summary>
        /// Báo cáo theo phòng ban
        /// </summary>
        public List<DepartmentRecruitmentReport> GetDepartmentReport(int year)
        {
            try
            {
                Log.Error($"Generating department report for year {year}");

                var departments = _departmentRepository.GetAll().ToList();
                var reports = new List<DepartmentRecruitmentReport>();

                foreach (var dept in departments)
                {
                    var headcounts = _headcountRepository.GetAll()
                        .Where(h => h.DepartmentId == dept.Id && h.Year == year)
                        .ToList();

                    var jobPostings = _jobPostingRepository.GetAll()
                        .Where(j => j.DepartmentId == dept.Id && j.PostDate.Year == year)
                        .ToList();

                    reports.Add(new DepartmentRecruitmentReport
                    {
                        DepartmentId = dept.Id,
                        DepartmentName = dept.DepartmentName,
                        Year = year,
                        ApprovedHeadcount = headcounts.Sum(h => h.ApprovedCount),
                        FilledHeadcount = headcounts.Sum(h => h.FilledCount),
                        RemainingHeadcount = headcounts.Sum(h => h.ApprovedCount - h.FilledCount),
                        TotalJobPostings = jobPostings.Count,
                        TotalPositionsPosted = jobPostings.Sum(j => j.NumberOfPositions)
                    });
                }

                return reports.OrderByDescending(r => r.ApprovedHeadcount).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR generating department report: {ex.Message}");
                return new List<DepartmentRecruitmentReport>();
            }
        }

        /// <summary>
        /// Báo cáo hiệu quả tuyển dụng
        /// </summary>
        public RecruitmentEfficiencyReport GetEfficiencyReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                Log.Error($"Generating efficiency report");

                var applications = _applicationRepository.GetAll()
                    .Where(a => a.ApplyDate >= fromDate && a.ApplyDate <= toDate)
                    .ToList();

                var interviews = _interviewRepository.GetAll()
                    .Where(i => i.InterviewDate >= fromDate && i.InterviewDate <= toDate)
                    .ToList();

                int totalDays = (toDate - fromDate).Days;
                int completedApplications = applications.Count(a => 
                    a.CurrentStatus == "Đạt" || a.CurrentStatus == "Không đạt");

                return new RecruitmentEfficiencyReport
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalApplications = applications.Count,
                    ApplicationsPerDay = totalDays > 0 ? (double)applications.Count / totalDays : 0,
                    InterviewsPerDay = totalDays > 0 ? (double)interviews.Count / totalDays : 0,
                    ApplicationToInterviewRate = applications.Any() 
                        ? (double)interviews.Count / applications.Count * 100 
                        : 0,
                    InterviewToOfferRate = interviews.Count(i => i.Result == "Đạt") > 0 && interviews.Any()
                        ? (double)interviews.Count(i => i.Result == "Đạt") / interviews.Count * 100 
                        : 0,
                    OverallConversionRate = applications.Any() 
                        ? (double)applications.Count(a => a.CurrentStatus == "Đạt") / applications.Count * 100 
                        : 0,
                    AverageTimeToHire = CalculateAverageTimeToHire(applications),
                    AverageInterviewScore = interviews.Where(i => i.Score > 0).Any()
                        ? (double)interviews.Where(i => i.Score > 0).Average(i => i.Score)
                        : 0
                };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR generating efficiency report: {ex.Message}");
                return new RecruitmentEfficiencyReport();
            }
        }

        /// <summary>
        /// Báo cáo theo nguồn ứng viên
        /// </summary>
        public List<SourceReport> GetSourceReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var applications = _applicationRepository.GetAll()
                    .Where(a => a.ApplyDate >= fromDate && a.ApplyDate <= toDate)
                    .ToList();

                var sourceGroups = applications.GroupBy(a => a.Source);

                return sourceGroups.Select(g => new SourceReport
                {
                    Source = g.Key,
                    TotalApplications = g.Count(),
                    SuccessfulApplications = g.Count(a => a.CurrentStatus == "Đạt"),
                    SuccessRate = g.Any() ? (double)g.Count(a => a.CurrentStatus == "Đạt") / g.Count() * 100 : 0
                }).OrderByDescending(s => s.TotalApplications).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR generating source report: {ex.Message}");
                return new List<SourceReport>();
            }
        }

        /// <summary>
        /// Báo cáo định biên chi tiết
        /// </summary>
        public List<HeadcountDetailReport> GetHeadcountDetailReport(int year)
        {
            try
            {
                var headcounts = _headcountRepository.GetAll()
                    .Where(h => h.Year == year)
                    .ToList();

                return headcounts.Select(h => new HeadcountDetailReport
                {
                    DepartmentName = _departmentRepository.GetById(h.DepartmentId)?.DepartmentName ?? "N/A",
                    PositionName = _positionRepository.GetById(h.PositionId)?.PositionName ?? "N/A",
                    Year = h.Year,
                    ApprovedCount = h.ApprovedCount,
                    FilledCount = h.FilledCount,
                    RemainingCount = h.ApprovedCount - h.FilledCount,
                    FillRate = h.ApprovedCount > 0 ? (double)h.FilledCount / h.ApprovedCount * 100 : 0
                }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR generating headcount detail report: {ex.Message}");
                return new List<HeadcountDetailReport>();
            }
        }

        /// <summary>
        /// Tính thời gian trung bình để tuyển được người (từ apply đến Đạt)
        /// </summary>
        private double CalculateAverageTimeToHire(List<LTHDT2.Models.Application> applications)
        {
            var hiredApps = applications.Where(a => a.CurrentStatus == "Đạt").ToList();
            
            if (!hiredApps.Any())
                return 0;

            var times = hiredApps.Where(a => a.UpdatedDate.HasValue)
                .Select(a => (a.UpdatedDate!.Value - a.ApplyDate).TotalDays);

            return times.Any() ? times.Average() : 0;
        }
    }

    #region Report Models

    public class RecruitmentOverviewReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalJobPostings { get; set; }
        public int ActiveJobPostings { get; set; }
        public int TotalPositionsRequired { get; set; }
        public int TotalApplications { get; set; }
        public int UniqueApplicants { get; set; }
        public int TotalInterviews { get; set; }
        public int CompletedInterviews { get; set; }
        public int PassedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public double AverageApplicationsPerJob { get; set; }
    }

    public class DepartmentRecruitmentReport
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int ApprovedHeadcount { get; set; }
        public int FilledHeadcount { get; set; }
        public int RemainingHeadcount { get; set; }
        public int TotalJobPostings { get; set; }
        public int TotalPositionsPosted { get; set; }
        public double FillRate => ApprovedHeadcount > 0 ? (double)FilledHeadcount / ApprovedHeadcount * 100 : 0;
    }

    public class RecruitmentEfficiencyReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalApplications { get; set; }
        public double ApplicationsPerDay { get; set; }
        public double InterviewsPerDay { get; set; }
        public double ApplicationToInterviewRate { get; set; }
        public double InterviewToOfferRate { get; set; }
        public double OverallConversionRate { get; set; }
        public double AverageTimeToHire { get; set; }
        public double AverageInterviewScore { get; set; }
    }

    public class SourceReport
    {
        public string Source { get; set; } = string.Empty;
        public int TotalApplications { get; set; }
        public int SuccessfulApplications { get; set; }
        public double SuccessRate { get; set; }
    }

    public class HeadcountDetailReport
    {
        public string DepartmentName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int ApprovedCount { get; set; }
        public int FilledCount { get; set; }
        public int RemainingCount { get; set; }
        public double FillRate { get; set; }
    }

    #endregion
}

