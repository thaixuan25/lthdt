using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    /// <summary>
    /// Repository cho lịch sử thay đổi trạng thái đơn ứng tuyển
    /// </summary>
    public class ApplicationStatusRepository : BaseRepository<ApplicationStatus>
    {
        protected override string TableName => "ApplicationStatus";

        public override ApplicationStatus GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE StatusID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<ApplicationStatus> GetAll()
        {
            var list = new List<ApplicationStatus>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} ORDER BY ChangedDate DESC", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(MapToEntity(reader));
                }
            }
            return list;
        }

        public override int Add(ApplicationStatus entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO ApplicationStatus (ApplicationID, OldStatus, NewStatus, 
                        ChangedDate, ChangedBy, Comments, CreatedDate)
                      VALUES (@AppId, @OldStatus, @NewStatus, @Changed, @ChangedBy, @Comments, @Created);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@AppId", entity.ApplicationId);
                cmd.Parameters.AddWithValue("@OldStatus", entity.OldStatus ?? "");
                cmd.Parameters.AddWithValue("@NewStatus", entity.NewStatus);
                cmd.Parameters.AddWithValue("@Changed", entity.ChangedDate);
                cmd.Parameters.AddWithValue("@ChangedBy", entity.ChangedBy);
                cmd.Parameters.AddWithValue("@Comments", entity.Comments ?? "");
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(ApplicationStatus entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE ApplicationStatus SET 
                        Comments = @Comments, UpdatedDate = @Updated
                      WHERE StatusID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Comments", entity.Comments ?? "");
                cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Lấy lịch sử thay đổi trạng thái của một đơn ứng tuyển
        /// </summary>
        public IEnumerable<ApplicationStatus> GetByApplicationId(int applicationId)
        {
            return Find(s => s.ApplicationId == applicationId)
                .OrderBy(s => s.ChangedDate);
        }

        /// <summary>
        /// Lấy trạng thái gần nhất của một đơn
        /// </summary>
        public ApplicationStatus? GetLatestStatus(int applicationId)
        {
            return Find(s => s.ApplicationId == applicationId)
                .OrderByDescending(s => s.ChangedDate)
                .FirstOrDefault();
        }

        /// <summary>
        /// Lấy các thay đổi trạng thái trong khoảng thời gian
        /// </summary>
        public IEnumerable<ApplicationStatus> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            return Find(s => s.ChangedDate >= fromDate && s.ChangedDate <= toDate)
                .OrderByDescending(s => s.ChangedDate);
        }

        /// <summary>
        /// Lấy các thay đổi trạng thái theo user
        /// </summary>
        public IEnumerable<ApplicationStatus> GetByChangedBy(int userId)
        {
            return Find(s => s.ChangedBy == userId)
                .OrderByDescending(s => s.ChangedDate);
        }

        /// <summary>
        /// Đếm số lần thay đổi trạng thái của một đơn
        /// </summary>
        public int CountChanges(int applicationId)
        {
            return Find(s => s.ApplicationId == applicationId).Count();
        }

        /// <summary>
        /// Thống kê thay đổi trạng thái theo loại
        /// </summary>
        public Dictionary<string, int> GetStatusChangeStats()
        {
            var statuses = GetAll().GroupBy(s => s.NewStatus);
            return statuses.ToDictionary(g => g.Key, g => g.Count());
        }

        private ApplicationStatus MapToEntity(MySqlDataReader reader)
        {
            return new ApplicationStatus
            {
                Id = reader.GetInt32("StatusID"),
                ApplicationId = reader.GetInt32("ApplicationID"),
                OldStatus = reader.IsDBNull(reader.GetOrdinal("OldStatus")) ? null : reader.GetString("OldStatus"),
                NewStatus = reader.GetString("NewStatus"),
                ChangedDate = reader.GetDateTime("ChangedDate"),
                ChangedBy = reader.GetInt32("ChangedBy"),
                Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString("Comments"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}


