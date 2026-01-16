using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    public class EmailLogRepository : BaseRepository<EmailLog>
    {
        protected override string TableName => "EmailLog";

        public override EmailLog GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE EmailLogID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<EmailLog> GetAll()
        {
            var list = new List<EmailLog>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} ORDER BY SentDate DESC", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(MapToEntity(reader));
                }
            }
            return list;
        }

        public override int Add(EmailLog entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO EmailLog (ApplicationID, EmailType, RecipientEmail, 
                        Subject, Body, SentDate, IsSuccess, ErrorMessage, CreatedDate)
                      VALUES (@AppId, @Type, @Recipient, @Subject, @Body, 
                        @SentDate, @Success, @Error, @Created);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@AppId", entity.ApplicationId);
                cmd.Parameters.AddWithValue("@Type", entity.EmailType);
                cmd.Parameters.AddWithValue("@Recipient", entity.RecipientEmail);
                cmd.Parameters.AddWithValue("@Subject", entity.Subject);
                cmd.Parameters.AddWithValue("@Body", entity.Body ?? "");
                cmd.Parameters.AddWithValue("@SentDate", entity.SentDate);
                cmd.Parameters.AddWithValue("@Success", entity.IsSent);
                cmd.Parameters.AddWithValue("@Error", entity.ErrorMessage ?? "");
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(EmailLog entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE EmailLog SET 
                        IsSuccess = @Success, ErrorMessage = @Error,
                        UpdatedDate = @Updated
                      WHERE EmailLogID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Success", entity.IsSent);
                cmd.Parameters.AddWithValue("@Error", entity.ErrorMessage ?? "");
                cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Lấy email logs của một application
        /// </summary>
        public IEnumerable<EmailLog> GetByApplicationId(int applicationId)
        {
            return Find(e => e.ApplicationId == applicationId)
                .OrderByDescending(e => e.SentDate);
        }

        /// <summary>
        /// Lấy email logs theo loại
        /// </summary>
        public IEnumerable<EmailLog> GetByType(string emailType)
        {
            return Find(e => e.EmailType == emailType)
                .OrderByDescending(e => e.SentDate);
        }

        /// <summary>
        /// Lấy emails thất bại
        /// </summary>
        public IEnumerable<EmailLog> GetFailedEmails()
        {
            return Find(e => !e.IsSuccess())
                .OrderByDescending(e => e.SentDate);
        }

        /// <summary>
        /// Đếm số email đã gửi theo loại
        /// </summary>
        public Dictionary<string, int> GetEmailCountByType()
        {
            var logs = GetAll().GroupBy(e => e.EmailType);
            return logs.ToDictionary(g => g.Key, g => g.Count());
        }

        private EmailLog MapToEntity(MySqlDataReader reader)
        {
            return new EmailLog
            {
                Id = reader.GetInt32("EmailLogID"),
                ApplicationId = reader.GetInt32("ApplicationID"),
                EmailType = reader.GetString("EmailType"),
                RecipientEmail = reader.GetString("RecipientEmail"),
                Subject = reader.GetString("Subject"),
                Body = reader.IsDBNull(reader.GetOrdinal("Body")) ? null : reader.GetString("Body"),
                SentDate = reader.GetDateTime("SentDate"),
                IsSent = reader.GetBoolean("IsSuccess"),
                ErrorMessage = reader.IsDBNull(reader.GetOrdinal("ErrorMessage")) ? null : reader.GetString("ErrorMessage"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}

