using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    public class JobPostingRepository : BaseRepository<JobPosting>
    {
        protected override string TableName => "JobPosting";

        public override JobPosting GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE JobPostingID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<JobPosting> GetAll()
        {
            var list = new List<JobPosting>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand($"SELECT * FROM {TableName} ORDER BY PostDate DESC", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(MapToEntity(reader));
                }
            }
            return list;
        }

        public override int Add(JobPosting entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO JobPosting (JobCode, JobTitle, DepartmentID, PositionID, VacancyCount,
                        JobDescription, JobResponsibilities, JobRequirements, MinSalary, MaxSalary, WorkLocation,
                        PostDate, Deadline, Status, IsHeadcountApproved, CreatedBy, CreatedDate, CampaignID)
                      VALUES (@Code, @Title, @DeptId, @PosId, @VacancyCount, @Desc, @Resp, @Req, 
                        @MinSalary, @MaxSalary, @Location, @PostDate, @Deadline, @Status, @Approved, @CreatedBy, @Created, @CampaignId);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@Code", entity.JobCode);
                cmd.Parameters.AddWithValue("@Title", entity.JobTitle);
                cmd.Parameters.AddWithValue("@DeptId", entity.DepartmentId);
                cmd.Parameters.AddWithValue("@PosId", entity.PositionId);
                cmd.Parameters.AddWithValue("@VacancyCount", entity.VacancyCount);
                cmd.Parameters.AddWithValue("@Desc", entity.JobDescription ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Resp", entity.JobResponsibilities ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Req", entity.JobRequirements ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MinSalary", entity.MinSalary);
                cmd.Parameters.AddWithValue("@MaxSalary", entity.MaxSalary);
                cmd.Parameters.AddWithValue("@Location", entity.WorkLocation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PostDate", entity.PostDate);
                cmd.Parameters.AddWithValue("@Deadline", entity.Deadline);
                cmd.Parameters.AddWithValue("@Status", entity.Status);
                cmd.Parameters.AddWithValue("@Approved", entity.IsHeadcountApproved);
                cmd.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy);
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);
                cmd.Parameters.AddWithValue("@CampaignId", entity.CampaignId > 0 ? (object)entity.CampaignId : DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(JobPosting entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE JobPosting SET 
                        JobTitle = @Title, VacancyCount = @VacancyCount,
                        JobDescription = @Desc, JobResponsibilities = @Resp, JobRequirements = @Req,
                        MinSalary = @MinSalary, MaxSalary = @MaxSalary, WorkLocation = @Location,
                        Deadline = @Deadline, Status = @Status, IsHeadcountApproved = @Approved, CampaignID = @CampaignId
                      WHERE JobPostingID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Title", entity.JobTitle);
                cmd.Parameters.AddWithValue("@VacancyCount", entity.VacancyCount);
                cmd.Parameters.AddWithValue("@Desc", entity.JobDescription ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Resp", entity.JobResponsibilities ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Req", entity.JobRequirements ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MinSalary", entity.MinSalary);
                cmd.Parameters.AddWithValue("@MaxSalary", entity.MaxSalary);
                cmd.Parameters.AddWithValue("@Location", entity.WorkLocation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Deadline", entity.Deadline);
                cmd.Parameters.AddWithValue("@Status", entity.Status);
                cmd.Parameters.AddWithValue("@Approved", entity.IsHeadcountApproved);
                cmd.Parameters.AddWithValue("@CampaignId", entity.CampaignId > 0 ? (object)entity.CampaignId : DBNull.Value);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private JobPosting MapToEntity(MySqlDataReader reader)
        {
            return new JobPosting
            {
                Id = reader.GetInt32("JobPostingID"),
                JobCode = reader.GetString("JobCode"),
                JobTitle = reader.GetString("JobTitle"),
                DepartmentId = reader.GetInt32("DepartmentID"),
                PositionId = reader.GetInt32("PositionID"),
                VacancyCount = reader.GetInt32("VacancyCount"),
                JobDescription = reader.IsDBNull(reader.GetOrdinal("JobDescription")) ? null : reader.GetString("JobDescription"),
                JobResponsibilities = reader.IsDBNull(reader.GetOrdinal("JobResponsibilities")) ? null : reader.GetString("JobResponsibilities"),
                JobRequirements = reader.IsDBNull(reader.GetOrdinal("JobRequirements")) ? null : reader.GetString("JobRequirements"),
                MinSalary = reader.GetDecimal("MinSalary"),
                MaxSalary = reader.GetDecimal("MaxSalary"),
                WorkLocation = reader.IsDBNull(reader.GetOrdinal("WorkLocation")) ? null : reader.GetString("WorkLocation"),
                PostDate = reader.GetDateTime("PostDate"),
                Deadline = reader.GetDateTime("Deadline"),
                Status = reader.GetString("Status"),
                IsHeadcountApproved = reader.GetBoolean("IsHeadcountApproved"),
                CreatedBy = reader.GetInt32("CreatedBy"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                CampaignId = reader.IsDBNull(reader.GetOrdinal("CampaignID")) ? 0 : reader.GetInt32("CampaignID")
            };
        }
    }
}


