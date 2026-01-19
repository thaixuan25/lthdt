using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using LTHDT2.Models;
using AppModel = LTHDT2.Models.Application;

namespace LTHDT2.DataAccess.Repositories
{
    /// <summary>
    /// Application Repository
    /// Quản lý đơn ứng tuyển
    /// </summary>
    public class ApplicationRepository : BaseRepository<AppModel>
    {
        protected override string TableName => "Application";

        public override AppModel? GetById(int id)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT a.*, 
                               c.FullName as CandidateName, c.Email as CandidateEmail, c.Phone as CandidatePhone,
                               j.JobTitle, j.JobCode, j.DepartmentID, j.PositionID,
                               d.DepartmentName, p.PositionName
                               FROM Application a
                               INNER JOIN Candidate c ON a.CandidateID = c.CandidateID
                               INNER JOIN JobPosting j ON a.JobPostingID = j.JobPostingID
                               INNER JOIN Department d ON j.DepartmentID = d.DepartmentID
                               INNER JOIN Position p ON j.PositionID = p.PositionID
                               WHERE a.ApplicationID = @Id";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return MapToEntity(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(GetById), ex);
            }
            return null;
        }

        public override IEnumerable<AppModel> GetAll()
        {
            var applications = new List<AppModel>();
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT a.*, 
                               c.FullName as CandidateName, c.Email as CandidateEmail,
                               j.JobTitle, j.JobCode
                               FROM Application a
                               INNER JOIN Candidate c ON a.CandidateID = c.CandidateID
                               INNER JOIN JobPosting j ON a.JobPostingID = j.JobPostingID
                               ORDER BY a.ApplyDate DESC";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                applications.Add(MapToEntity(reader));
                            }
                            catch (Exception mapEx)
                            {
                                LogError($"Error mapping application row: {mapEx.Message}", mapEx);
                                throw; // Re-throw để form thấy lỗi
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(GetAll), ex);
                throw; // Re-throw để form thấy lỗi
            }
            return applications;
        }

        public override int Add(AppModel entity)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"INSERT INTO Application 
                               (CandidateID, JobPostingID, ApplyDate, CoverLetter, Source, CurrentStatus, Score, Notes, UpdatedBy)
                               VALUES (@CandidateID, @JobPostingID, @ApplyDate, @CoverLetter, @Source, @CurrentStatus, @Score, @Notes, @UpdatedBy);
                               SELECT LAST_INSERT_ID();";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CandidateID", entity.CandidateId);
                        cmd.Parameters.AddWithValue("@JobPostingID", entity.JobPostingId);
                        cmd.Parameters.AddWithValue("@ApplyDate", entity.ApplyDate);
                        cmd.Parameters.AddWithValue("@CoverLetter", entity.CoverLetter ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Source", entity.Source ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CurrentStatus", entity.CurrentStatus);
                        cmd.Parameters.AddWithValue("@Score", entity.Score);
                        cmd.Parameters.AddWithValue("@Notes", entity.Notes ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy.HasValue ? entity.UpdatedBy.Value : (object)DBNull.Value);
                        
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(Add), ex);
                return 0;
            }
        }

        public override bool Update(AppModel entity)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"UPDATE Application SET 
                               CurrentStatus = @CurrentStatus,
                               Score = @Score,
                               Notes = @Notes,
                               UpdatedBy = @UpdatedBy,
                               UpdatedDate = @UpdatedDate
                               WHERE ApplicationID = @Id";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", entity.Id);
                        cmd.Parameters.AddWithValue("@CurrentStatus", entity.CurrentStatus);
                        cmd.Parameters.AddWithValue("@Score", entity.Score);
                        cmd.Parameters.AddWithValue("@Notes", entity.Notes ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy.HasValue ? entity.UpdatedBy.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                        
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(Update), ex);
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra trùng lặp - ứng viên đã ứng tuyển vị trí này chưa
        /// Method riêng quan trọng
        /// </summary>
        public bool CheckDuplicateApplication(int candidateId, int jobPostingId)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = "SELECT COUNT(*) FROM Application WHERE CandidateID = @CandidateID AND JobPostingID = @JobPostingID";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CandidateID", candidateId);
                        cmd.Parameters.AddWithValue("@JobPostingID", jobPostingId);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(CheckDuplicateApplication), ex);
                return false;
            }
        }

        /// <summary>
        /// Lấy Applications theo JobPosting
        /// </summary>
        public IEnumerable<AppModel> GetByJobPosting(int jobPostingId)
        {
            var applications = new List<AppModel>();
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT a.*, 
                               c.FullName as CandidateName, c.Email as CandidateEmail,
                               j.JobTitle, j.JobCode
                               FROM Application a
                               INNER JOIN Candidate c ON a.CandidateID = c.CandidateID
                               INNER JOIN JobPosting j ON a.JobPostingID = j.JobPostingID
                               WHERE a.JobPostingID = @JobPostingID
                               ORDER BY a.ApplyDate DESC";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@JobPostingID", jobPostingId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                applications.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(GetByJobPosting), ex);
            }
            return applications;
        }

        /// <summary>
        /// Lấy Applications theo Candidate
        /// </summary>
        public IEnumerable<AppModel> GetByCandidate(int candidateId)
        {
            var applications = new List<AppModel>();
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT a.*, 
                               c.FullName as CandidateName, c.Email as CandidateEmail,
                               j.JobTitle, j.JobCode
                               FROM Application a
                               INNER JOIN Candidate c ON a.CandidateID = c.CandidateID
                               INNER JOIN JobPosting j ON a.JobPostingID = j.JobPostingID
                               WHERE a.CandidateID = @CandidateID
                               ORDER BY a.ApplyDate DESC";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CandidateID", candidateId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                applications.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(GetByCandidate), ex);
            }
            return applications;
        }

        /// <summary>
        /// Lấy Applications theo trạng thái
        /// </summary>
        public IEnumerable<AppModel> GetByStatus(string status)
        {
            return Find(a => a.CurrentStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        private AppModel MapToEntity(MySqlDataReader reader)
        {
            var application = new AppModel
            {
                Id = reader.GetInt32("ApplicationID"),
                CandidateId = reader.GetInt32("CandidateID"),
                JobPostingId = reader.GetInt32("JobPostingID"),
                ApplyDate = reader.GetDateTime("ApplyDate"),
                CoverLetter = GetSafeString(reader, "CoverLetter"),
                Source = GetSafeString(reader, "Source"),
                CurrentStatus = reader.GetString("CurrentStatus"),
                Score = reader.GetDecimal("Score"),
                Notes = GetSafeString(reader, "Notes"),
                UpdatedBy = GetSafeInt(reader, "UpdatedBy"),
                UpdatedDate = GetSafeDateTime(reader, "UpdatedDate")
                // Application table doesn't have CreatedDate, only ApplyDate
            };

            // Set navigation properties nếu có dữ liệu từ JOIN
            // Kiểm tra bằng cách thử đọc các cột từ JOIN
            if (HasColumn(reader, "CandidateName"))
            {
                // Có JOIN với Candidate, set navigation property
                application.Candidate = new Candidate
                {
                    Id = application.CandidateId,
                    FullName = GetSafeString(reader, "CandidateName"),
                    Email = GetSafeString(reader, "CandidateEmail")
                };
            }

            if (HasColumn(reader, "JobTitle"))
            {
                // Có JOIN với JobPosting, set navigation property
                application.JobPosting = new JobPosting
                {
                    Id = application.JobPostingId,
                    JobTitle = GetSafeString(reader, "JobTitle"),
                    JobCode = GetSafeString(reader, "JobCode")
                };
            }

            return application;
        }

        /// <summary>
        /// Helper method để kiểm tra xem reader có chứa cột với tên cho trước không
        /// </summary>
        private bool HasColumn(MySqlDataReader reader, string columnName)
        {
            try
            {
                reader.GetOrdinal(columnName);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}

