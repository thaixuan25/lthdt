using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    /// <summary>
    /// User Repository - Kế thừa BaseRepository
    /// </summary>
    public class UserRepository : BaseRepository<User>
    {
        // Override abstract property
        protected override string TableName => "User";

        /// <summary>
        /// Override abstract method - Lấy User theo ID
        /// </summary>
        public override User? GetById(int id)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT u.*, e.FullName as EmployeeName, e.Email as EmployeeEmail
                               FROM User u
                               INNER JOIN Employee e ON u.EmployeeID = e.EmployeeID
                               WHERE u.UserID = @Id";
                    
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

        /// <summary>
        /// Lấy User theo Username - Method riêng
        /// </summary>
        public User? GetByUsername(string username)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT u.*, e.FullName as EmployeeName, e.Email as EmployeeEmail
                               FROM User u
                               INNER JOIN Employee e ON u.EmployeeID = e.EmployeeID
                               WHERE u.Username = @Username";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username.ToLower());
                        
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
                LogError(nameof(GetByUsername), ex);
            }
            return null;
        }

        /// <summary>
        /// Override GetAll
        /// </summary>
        public override IEnumerable<User> GetAll()
        {
            var users = new List<User>();
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT u.*, e.FullName as EmployeeName, e.Email as EmployeeEmail
                               FROM User u
                               INNER JOIN Employee e ON u.EmployeeID = e.EmployeeID
                               ORDER BY u.Username";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(GetAll), ex);
            }
            return users;
        }

        /// <summary>
        /// Override Add
        /// </summary>
        public override int Add(User entity)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"INSERT INTO User (Username, PasswordHash, Salt, EmployeeID, Role, IsActive, CreatedDate)
                               VALUES (@Username, @PasswordHash, @Salt, @EmployeeID, @Role, @IsActive, @CreatedDate);
                               SELECT LAST_INSERT_ID();";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", entity.Username);
                        cmd.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                        cmd.Parameters.AddWithValue("@Salt", entity.Salt);
                        cmd.Parameters.AddWithValue("@EmployeeID", entity.EmployeeId);
                        cmd.Parameters.AddWithValue("@Role", entity.Role);
                        cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
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

        /// <summary>
        /// Override Update
        /// </summary>
        public override bool Update(User entity)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"UPDATE User SET 
                               PasswordHash = @PasswordHash,
                               Salt = @Salt,
                               Role = @Role,
                               IsActive = @IsActive,
                               LastLogin = @LastLogin
                               WHERE UserID = @Id";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", entity.Id);
                        cmd.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                        cmd.Parameters.AddWithValue("@Salt", entity.Salt);
                        cmd.Parameters.AddWithValue("@Role", entity.Role);
                        cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
                        cmd.Parameters.AddWithValue("@LastLogin", entity.LastLogin.HasValue ? entity.LastLogin.Value : (object)DBNull.Value);
                        
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
        /// Map DataReader sang User entity
        /// </summary>
        private User MapToEntity(MySqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("UserID"),
                Username = reader.GetString("Username"),
                PasswordHash = reader.GetString("PasswordHash"),
                Salt = reader.GetString("Salt"),
                EmployeeId = reader.GetInt32("EmployeeID"),
                Role = reader.GetString("Role"),
                IsActive = reader.GetBoolean("IsActive"),
                LastLogin = GetSafeDateTime(reader, "LastLogin"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                Employee = new Employee { FullName = GetSafeString(reader, "EmployeeName") }
            };
        }
    }
}

