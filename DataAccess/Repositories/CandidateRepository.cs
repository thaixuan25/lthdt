using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    public class CandidateRepository : BaseRepository<Candidate>
    {
        protected override string TableName => "Candidate";

        public override Candidate? GetById(int id)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = "SELECT * FROM Candidate WHERE CandidateID = @Id";
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

        public Candidate? GetByEmail(string email)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = "SELECT * FROM Candidate WHERE Email = @Email";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email.ToLower());
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
                LogError(nameof(GetByEmail), ex);
            }
            return null;
        }

        public override IEnumerable<Candidate> GetAll()
        {
            var candidates = new List<Candidate>();
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = "SELECT * FROM Candidate ORDER BY CreatedDate DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            candidates.Add(MapToEntity(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(GetAll), ex);
            }
            return candidates;
        }

        public override int Add(Candidate entity)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"INSERT INTO Candidate 
                               (FullName, Email, Phone, DateOfBirth, Gender, Address, Education, WorkExperience, Skills, ResumeFileName, ResumeFilePath, CreatedDate)
                               VALUES (@FullName, @Email, @Phone, @DateOfBirth, @Gender, @Address, @Education, @WorkExperience, @Skills, @ResumeFileName, @ResumeFilePath, @CreatedDate);
                               SELECT LAST_INSERT_ID();";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", entity.FullName);
                        cmd.Parameters.AddWithValue("@Email", entity.Email);
                        cmd.Parameters.AddWithValue("@Phone", entity.Phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DateOfBirth", entity.DateOfBirth.HasValue ? entity.DateOfBirth.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", entity.Gender ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", entity.Address ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Education", entity.Education ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@WorkExperience", entity.WorkExperience ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Skills", entity.Skills ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ResumeFileName", entity.ResumeFileName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ResumeFilePath", entity.ResumeFilePath ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        
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

        public override bool Update(Candidate entity)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"UPDATE Candidate SET 
                               FullName = @FullName, Phone = @Phone, DateOfBirth = @DateOfBirth, Gender = @Gender,
                               Address = @Address, Education = @Education, WorkExperience = @WorkExperience, Skills = @Skills,
                               ResumeFileName = @ResumeFileName, ResumeFilePath = @ResumeFilePath, UpdatedDate = @UpdatedDate
                               WHERE CandidateID = @Id";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", entity.Id);
                        cmd.Parameters.AddWithValue("@FullName", entity.FullName);
                        cmd.Parameters.AddWithValue("@Phone", entity.Phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DateOfBirth", entity.DateOfBirth.HasValue ? entity.DateOfBirth.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", entity.Gender ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", entity.Address ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Education", entity.Education ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@WorkExperience", entity.WorkExperience ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Skills", entity.Skills ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ResumeFileName", entity.ResumeFileName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ResumeFilePath", entity.ResumeFilePath ?? (object)DBNull.Value);
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

        private Candidate MapToEntity(MySqlDataReader reader)
        {
            return new Candidate
            {
                Id = reader.GetInt32("CandidateID"),
                FullName = reader.GetString("FullName"),
                Email = reader.GetString("Email"),
                Phone = GetSafeString(reader, "Phone"),
                DateOfBirth = GetSafeDateTime(reader, "DateOfBirth"),
                Gender = GetSafeString(reader, "Gender"),
                Address = GetSafeString(reader, "Address"),
                Education = GetSafeString(reader, "Education"),
                WorkExperience = GetSafeString(reader, "WorkExperience"),
                Skills = GetSafeString(reader, "Skills"),
                ResumeFileName = GetSafeString(reader, "ResumeFileName"),
                ResumeFilePath = GetSafeString(reader, "ResumeFilePath"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                UpdatedDate = GetSafeDateTime(reader, "UpdatedDate")
            };
        }
    }
}

