using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Services
{
    /// <summary>
    /// Candidate Service - Business logic cho ứng viên
    /// Kế thừa BaseService
    /// </summary>
    public class CandidateService : BaseService
    {
        private readonly CandidateRepository _candidateRepository;
        private readonly ApplicationRepository _applicationRepository;

        public CandidateService()
        {
            _candidateRepository = new CandidateRepository();
            _applicationRepository = new ApplicationRepository();
        }

        /// <summary>
        /// Lấy tất cả ứng viên
        /// </summary>
        public IEnumerable<Candidate> GetAll()
        {
            try
            {
                return _candidateRepository.GetAll();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting all candidates: {ex.Message}");
                return new List<Candidate>();
            }
        }

        /// <summary>
        /// Lấy ứng viên theo ID
        /// </summary>
        public Candidate? GetById(int id)
        {
            try
            {
                return _candidateRepository.GetById(id);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting candidate by ID {id}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tạo ứng viên mới với validation
        /// </summary>
        public int CreateCandidate(Candidate candidate)
        {
            try
            {
                // Validate entity
                if (!candidate.IsValid())
                {
                    throw new Exception("Dữ liệu ứng viên không hợp lệ");
                }

                // Kiểm tra trùng email
                var duplicate = CheckDuplicate(candidate.Email, candidate.Phone);
                if (duplicate != null)
                {
                    throw new Exception($"Ứng viên với email '{candidate.Email}' đã tồn tại trong hệ thống");
                }

                // Set created date
                candidate.CreatedDate = DateTime.Now;

                // Insert vào database
                var id = _candidateRepository.Add(candidate);
                if (id <= 0)
                {
                    throw new Exception("Không thể tạo ứng viên");
                }

                Log.Info($"Candidate created: ID={id}, Email={candidate.Email}");
                return id;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR creating candidate: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cập nhật ứng viên với validation
        /// </summary>
        public bool UpdateCandidate(Candidate candidate)
        {
            try
            {
                // Validate entity
                if (!candidate.IsValid())
                {
                    throw new Exception("Dữ liệu ứng viên không hợp lệ");
                }

                // Kiểm tra ứng viên có tồn tại không
                var existing = _candidateRepository.GetById(candidate.Id);
                if (existing == null)
                {
                    throw new Exception("Không tìm thấy ứng viên cần cập nhật");
                }

                // Kiểm tra trùng email (nếu email thay đổi)
                if (!existing.Email.Equals(candidate.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var duplicate = CheckDuplicate(candidate.Email, candidate.Phone);
                    if (duplicate != null && duplicate.Id != candidate.Id)
                    {
                        throw new Exception($"Email '{candidate.Email}' đã được sử dụng bởi ứng viên khác");
                    }
                }

                // Set updated date
                candidate.UpdatedDate = DateTime.Now;

                // Update vào database
                bool updated = _candidateRepository.Update(candidate);
                if (updated)
                {
                    Log.Info($"Candidate updated: ID={candidate.Id}, Email={candidate.Email}");
                }

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR updating candidate: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Xóa ứng viên
        /// </summary>
        public bool DeleteCandidate(int id)
        {
            try
            {
                // Kiểm tra ứng viên có đơn ứng tuyển không
                var applications = _applicationRepository.GetAll()
                    .Where(a => a.CandidateId == id)
                    .ToList();

                if (applications.Any())
                {
                    throw new Exception("Không thể xóa ứng viên đã có đơn ứng tuyển. Vui lòng xóa các đơn ứng tuyển trước.");
                }

                bool deleted = _candidateRepository.Delete(id);
                if (deleted)
                {
                    Log.Info($"Candidate deleted: ID={id}");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR deleting candidate: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tìm kiếm ứng viên theo nhiều tiêu chí
        /// </summary>
        public IEnumerable<Candidate> SearchCandidates(string keyword, string? education = null, 
            int? minExperience = null, string? status = null)
        {
            try
            {
                var candidates = _candidateRepository.GetAll().AsEnumerable();

                // Search by keyword (name, email, phone)
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    candidates = candidates.Where(c =>
                        c.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        c.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        (c.Phone != null && c.Phone.Contains(keyword)));
                }

                // Filter by education
                if (!string.IsNullOrWhiteSpace(education))
                {
                    candidates = candidates.Where(c => 
                        c.Education != null && c.Education.Contains(education, StringComparison.OrdinalIgnoreCase));
                }

                // Filter by experience
                if (minExperience.HasValue)
                {
                    candidates = candidates.Where(c => 
                        GetYearsOfExperience(c.WorkExperience) >= minExperience.Value);
                }

                // Filter by status
                if (!string.IsNullOrWhiteSpace(status))
                {
                    candidates = candidates.Where(c => c.CurrentStatus == status);
                }

                return candidates.ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR searching candidates: {ex.Message}");
                return new List<Candidate>();
            }
        }

        /// <summary>
        /// Kiểm tra ứng viên có trùng lặp không (email/phone)
        /// </summary>
        public Candidate? CheckDuplicate(string email, string? phone = null)
        {
            try
            {
                var candidates = _candidateRepository.GetAll();

                var duplicate = candidates.FirstOrDefault(c => 
                    c.Email.Equals(email, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(phone) && c.Phone == phone));

                return duplicate;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR checking duplicate: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy lịch sử ứng tuyển của ứng viên
        /// </summary>
        public CandidateProfile GetCandidateProfile(int candidateId)
        {
            try
            {
                var candidate = _candidateRepository.GetById(candidateId);
                if (candidate == null)
                    return null!;

                var applications = _applicationRepository.GetAll()
                    .Where(a => a.CandidateId == candidateId)
                    .OrderByDescending(a => a.ApplyDate)
                    .ToList();

                return new CandidateProfile
                {
                    Candidate = candidate,
                    Applications = applications,
                    TotalApplications = applications.Count,
                    ActiveApplications = applications.Count(a => a.CurrentStatus != "Từ chối" && a.CurrentStatus != "Không đạt"),
                    SuccessfulApplications = applications.Count(a => a.CurrentStatus == "Đạt"),
                    LastApplicationDate = applications.Any() ? applications.Max(a => a.ApplyDate) : (DateTime?)null
                };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting candidate profile: {ex.Message}");
                return null!;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái ứng viên
        /// </summary>
        public bool UpdateCandidateStatus(int candidateId, string newStatus, string reason)
        {
            try
            {
                var candidate = _candidateRepository.GetById(candidateId);
                if (candidate == null)
                    return false;

                string oldStatus = candidate.CurrentStatus;
                candidate.CurrentStatus = newStatus;
                candidate.UpdatedDate = DateTime.Now;

                bool updated = _candidateRepository.Update(candidate);

                if (updated)
                {
                    Log.Error($"Candidate status updated: ID={candidateId}, {oldStatus} -> {newStatus}, Reason: {reason}");
                }

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR updating candidate status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tính số năm kinh nghiệm từ text
        /// </summary>
        private int GetYearsOfExperience(string? workExperience)
        {
            if (string.IsNullOrWhiteSpace(workExperience))
                return 0;

            // Simple parsing - trong thực tế nên phức tạp hơn
            if (workExperience.Contains("năm", StringComparison.OrdinalIgnoreCase))
            {
                var parts = workExperience.Split(' ');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int years))
                        return years;
                }
            }

            return 0;
        }

        /// <summary>
        /// Lấy ứng viên tiềm năng cho một vị trí
        /// </summary>
        public IEnumerable<Candidate> GetPotentialCandidates(int positionId, int minExperience = 0)
        {
            try
            {
                // Logic tìm ứng viên phù hợp với vị trí
                var candidates = _candidateRepository.GetAll()
                    .Where(c => c.CurrentStatus == "Mới" || c.CurrentStatus == "Đang tìm việc")
                    .Where(c => GetYearsOfExperience(c.WorkExperience) >= minExperience)
                    .OrderByDescending(c => GetYearsOfExperience(c.WorkExperience))
                    .ToList();

                return candidates;
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting potential candidates: {ex.Message}");
                return new List<Candidate>();
            }
        }

        /// <summary>
        /// Thống kê ứng viên
        /// </summary>
        public CandidateStatistics GetStatistics()
        {
            try
            {
                var candidates = _candidateRepository.GetAll().ToList();

                return new CandidateStatistics
                {
                    TotalCandidates = candidates.Count,
                    NewCandidates = candidates.Count(c => c.CurrentStatus == "Mới"),
                    ActiveCandidates = candidates.Count(c => c.CurrentStatus == "Đang ứng tuyển"),
                    HiredCandidates = candidates.Count(c => c.CurrentStatus == "Đã tuyển"),
                    RejectedCandidates = candidates.Count(c => c.CurrentStatus == "Không đạt"),
                    AverageExperience = candidates.Any() 
                        ? candidates.Average(c => GetYearsOfExperience(c.WorkExperience)) 
                        : 0
                };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting statistics: {ex.Message}");
                return new CandidateStatistics();
            }
        }
    }

    /// <summary>
    /// Profile đầy đủ của ứng viên
    /// </summary>
    public class CandidateProfile
    {
        public Candidate Candidate { get; set; } = null!;
        public List<LTHDT2.Models.Application> Applications { get; set; } = new();
        public int TotalApplications { get; set; }
        public int ActiveApplications { get; set; }
        public int SuccessfulApplications { get; set; }
        public DateTime? LastApplicationDate { get; set; }
    }

    /// <summary>
    /// Thống kê ứng viên
    /// </summary>
    public class CandidateStatistics
    {
        public int TotalCandidates { get; set; }
        public int NewCandidates { get; set; }
        public int ActiveCandidates { get; set; }
        public int HiredCandidates { get; set; }
        public int RejectedCandidates { get; set; }
        public double AverageExperience { get; set; }
    }
}

