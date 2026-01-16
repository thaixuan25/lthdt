using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    /// <summary>
    /// Repository cho bảng trung gian Campaign-JobPosting (Many-to-Many)
    /// </summary>
    public class CampaignJobPostingRepository : BaseRepository<CampaignJobPosting>
    {
        protected override string TableName => "CampaignJobPosting";

        public override CampaignJobPosting GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE CampaignJobPostingID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<CampaignJobPosting> GetAll()
        {
            var list = new List<CampaignJobPosting>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand($"SELECT * FROM {TableName}", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(MapToEntity(reader));
                }
            }
            return list;
        }

        public override int Add(CampaignJobPosting entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO CampaignJobPosting (CampaignID, JobPostingID, AddedDate, CreatedDate)
                      VALUES (@CampaignId, @JobId, @Added, @Created);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@CampaignId", entity.CampaignId);
                cmd.Parameters.AddWithValue("@JobId", entity.JobPostingId);
                cmd.Parameters.AddWithValue("@Added", entity.AddedDate);
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(CampaignJobPosting entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE CampaignJobPosting SET 
                        AddedDate = @Added, UpdatedDate = @Updated
                      WHERE CampaignJobPostingID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Added", entity.AddedDate);
                cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Lấy tất cả JobPostings của một Campaign
        /// </summary>
        public IEnumerable<CampaignJobPosting> GetByCampaignId(int campaignId)
        {
            return Find(c => c.CampaignId == campaignId)
                .OrderBy(c => c.AddedDate);
        }

        /// <summary>
        /// Lấy tất cả Campaigns của một JobPosting
        /// </summary>
        public IEnumerable<CampaignJobPosting> GetByJobPostingId(int jobPostingId)
        {
            return Find(c => c.JobPostingId == jobPostingId)
                .OrderBy(c => c.AddedDate);
        }

        /// <summary>
        /// Kiểm tra JobPosting đã có trong Campaign chưa
        /// </summary>
        public bool ExistsInCampaign(int campaignId, int jobPostingId)
        {
            return Find(c => c.CampaignId == campaignId && c.JobPostingId == jobPostingId).Any();
        }

        /// <summary>
        /// Xóa JobPosting khỏi Campaign
        /// </summary>
        public bool RemoveFromCampaign(int campaignId, int jobPostingId)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"DELETE FROM {TableName} WHERE CampaignID = @CampaignId AND JobPostingID = @JobId", conn);
                cmd.Parameters.AddWithValue("@CampaignId", campaignId);
                cmd.Parameters.AddWithValue("@JobId", jobPostingId);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Đếm số JobPostings trong một Campaign
        /// </summary>
        public int CountJobPostingsInCampaign(int campaignId)
        {
            return Find(c => c.CampaignId == campaignId).Count();
        }

        private CampaignJobPosting MapToEntity(MySqlDataReader reader)
        {
            return new CampaignJobPosting
            {
                Id = reader.GetInt32("CampaignJobPostingID"),
                CampaignId = reader.GetInt32("CampaignID"),
                JobPostingId = reader.GetInt32("JobPostingID"),
                AddedDate = reader.GetDateTime("AddedDate"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}


