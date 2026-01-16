using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    public class InterviewRepository : BaseRepository<Interview>
    {
        protected override string TableName => "Interview";

        public override Interview GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE InterviewID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<Interview> GetAll()
        {
            var list = new List<Interview>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} ORDER BY InterviewDate DESC", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(MapToEntity(reader));
                }
            }
            return list;
        }

        public override int Add(Interview entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO Interview (ApplicationID, InterviewerID, InterviewDate, InterviewType, 
                        InterviewRound, InterviewNotes, Result, Score, CreatedDate)
                      VALUES (@AppId, @InterviewerId, @Date, @Type, @Round, 
                        @Notes, @Result, @Score, @Created);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@AppId", entity.ApplicationId);
                cmd.Parameters.AddWithValue("@InterviewerId", entity.InterviewerId);
                cmd.Parameters.AddWithValue("@Date", entity.InterviewDate);
                cmd.Parameters.AddWithValue("@Type", entity.InterviewType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Round", entity.InterviewRound ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", entity.InterviewNotes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Result", entity.Result ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Score", entity.Score);
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(Interview entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE Interview SET 
                        InterviewerID = @InterviewerId,
                        InterviewDate = @Date, 
                        InterviewType = @Type,
                        InterviewRound = @Round,
                        InterviewNotes = @Notes, 
                        Result = @Result, 
                        Score = @Score,
                        UpdatedDate = @Updated
                      WHERE InterviewID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@InterviewerId", entity.InterviewerId);
                cmd.Parameters.AddWithValue("@Date", entity.InterviewDate);
                cmd.Parameters.AddWithValue("@Type", entity.InterviewType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Round", entity.InterviewRound ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", entity.InterviewNotes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Result", entity.Result ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Score", entity.Score);
                cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Lấy tất cả interviews của một application
        /// </summary>
        public IEnumerable<Interview> GetByApplicationId(int applicationId)
        {
            return Find(i => i.ApplicationId == applicationId)
                .OrderBy(i => i.InterviewDate);
        }

        /// <summary>
        /// Lấy interviews trong khoảng thời gian
        /// </summary>
        public IEnumerable<Interview> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            return Find(i => i.InterviewDate >= fromDate && i.InterviewDate <= toDate)
                .OrderBy(i => i.InterviewDate);
        }

        /// <summary>
        /// Lấy interviews theo người phỏng vấn (theo ID)
        /// </summary>
        public IEnumerable<Interview> GetByInterviewer(int interviewerId)
        {
            return Find(i => i.InterviewerId == interviewerId)
                .OrderByDescending(i => i.InterviewDate);
        }

        /// <summary>
        /// Lấy interviews theo tên người phỏng vấn (cần JOIN với Employee)
        /// </summary>
        public IEnumerable<Interview> GetByInterviewerName(string interviewerName)
        {
            var list = new List<Interview>();
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT i.* 
                               FROM Interview i
                               INNER JOIN Employee e ON i.InterviewerID = e.EmployeeID
                               WHERE e.FullName LIKE @Name
                               ORDER BY i.InterviewDate DESC";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", $"%{interviewerName}%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                list.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(GetByInterviewerName), ex);
            }
            return list;
        }

        private Interview MapToEntity(MySqlDataReader reader)
        {
            var interview = new Interview
            {
                Id = reader.GetInt32("InterviewID"),
                ApplicationId = reader.GetInt32("ApplicationID"),
                InterviewerId = reader.GetInt32("InterviewerID"),
                InterviewDate = reader.GetDateTime("InterviewDate"),
                InterviewType = GetSafeString(reader, "InterviewType"),
                InterviewRound = GetSafeString(reader, "InterviewRound"),
                InterviewNotes = GetSafeString(reader, "InterviewNotes"),
                Result = GetSafeString(reader, "Result"),
                Score = reader.IsDBNull(reader.GetOrdinal("Score")) ? 0 : reader.GetDecimal("Score"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };

            // Set alias properties
            interview.Notes = interview.InterviewNotes;

            return interview;
        }
    }
}

