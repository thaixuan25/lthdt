using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    public class RecruitmentCampaignRepository : BaseRepository<RecruitmentCampaign>
    {
        protected override string TableName => "RecruitmentCampaign";

        public override RecruitmentCampaign GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE CampaignID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<RecruitmentCampaign> GetAll()
        {
            var list = new List<RecruitmentCampaign>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} ORDER BY StartDate DESC", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(MapToEntity(reader));
                }
            }
            return list;
        }

        public override int Add(RecruitmentCampaign entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO RecruitmentCampaign (CampaignCode, CampaignName, Description,
                        StartDate, EndDate, Budget, Status, CreatedBy, CreatedDate)
                      VALUES (@Code, @Name, @Desc, @Start, @End, @Budget, @Status, @CreatedBy, @Created);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@Code", entity.CampaignCode);
                cmd.Parameters.AddWithValue("@Name", entity.CampaignName);
                cmd.Parameters.AddWithValue("@Desc", entity.Description ?? "");
                cmd.Parameters.AddWithValue("@Start", entity.StartDate);
                cmd.Parameters.AddWithValue("@End", entity.EndDate);
                cmd.Parameters.AddWithValue("@Budget", entity.Budget);
                cmd.Parameters.AddWithValue("@Status", entity.Status);
                cmd.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy);
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(RecruitmentCampaign entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE RecruitmentCampaign SET 
                        CampaignName = @Name, Description = @Desc,
                        StartDate = @Start, EndDate = @End,
                        Budget = @Budget, Status = @Status,
                        UpdatedDate = @Updated
                      WHERE CampaignID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Name", entity.CampaignName);
                cmd.Parameters.AddWithValue("@Desc", entity.Description ?? "");
                cmd.Parameters.AddWithValue("@Start", entity.StartDate);
                cmd.Parameters.AddWithValue("@End", entity.EndDate);
                cmd.Parameters.AddWithValue("@Budget", entity.Budget);
                cmd.Parameters.AddWithValue("@Status", entity.Status);
                cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Lấy campaigns đang active
        /// </summary>
        public IEnumerable<RecruitmentCampaign> GetActiveCampaigns()
        {
            var now = DateTime.Now;
            return Find(c => c.Status == "Đang chạy" 
                          && c.StartDate <= now 
                          && c.EndDate >= now)
                .OrderBy(c => c.StartDate);
        }

        /// <summary>
        /// Lấy campaigns theo năm
        /// </summary>
        public IEnumerable<RecruitmentCampaign> GetByYear(int year)
        {
            return Find(c => c.StartDate.Year == year || c.EndDate.Year == year)
                .OrderByDescending(c => c.StartDate);
        }

        /// <summary>
        /// Lấy campaigns theo status
        /// </summary>
        public IEnumerable<RecruitmentCampaign> GetByStatus(string status)
        {
            return Find(c => c.Status == status)
                .OrderByDescending(c => c.StartDate);
        }

        private RecruitmentCampaign MapToEntity(MySqlDataReader reader)
        {
            return new RecruitmentCampaign
            {
                Id = reader.GetInt32("CampaignID"),
                CampaignCode = reader.GetString("CampaignCode"),
                CampaignName = reader.GetString("CampaignName"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                StartDate = reader.GetDateTime("StartDate"),
                EndDate = reader.GetDateTime("EndDate"),
                Budget = reader.GetDecimal("Budget"),
                Status = reader.GetString("Status"),
                CreatedBy = reader.GetInt32("CreatedBy"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}


