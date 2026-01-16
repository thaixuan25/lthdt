using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    /// <summary>
    /// Employee Repository - Kế thừa BaseRepository
    /// </summary>
    public class EmployeeRepository : BaseRepository<Employee>
    {
        protected override string TableName => "Employee";

        public override Employee? GetById(int id)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT e.*, d.DepartmentName, p.PositionName 
                               FROM Employee e
                               LEFT JOIN Department d ON e.DepartmentID = d.DepartmentID
                               LEFT JOIN Position p ON e.PositionID = p.PositionID
                               WHERE e.EmployeeID = @Id";
                    
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

        public override IEnumerable<Employee> GetAll()
        {
            var employees = new List<Employee>();
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"SELECT e.*, d.DepartmentName, p.PositionName 
                               FROM Employee e
                               LEFT JOIN Department d ON e.DepartmentID = d.DepartmentID
                               LEFT JOIN Position p ON e.PositionID = p.PositionID
                               ORDER BY e.EmployeeCode";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            employees.Add(MapToEntity(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(nameof(GetAll), ex);
            }
            return employees;
        }

        public override int Add(Employee entity)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"INSERT INTO Employee 
                               (EmployeeCode, FullName, Email, Phone, DepartmentID, PositionID, HireDate, Status, CreatedDate)
                               VALUES (@Code, @Name, @Email, @Phone, @DeptId, @PosId, @HireDate, @Status, @Created);
                               SELECT LAST_INSERT_ID();";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Code", entity.EmployeeCode);
                        cmd.Parameters.AddWithValue("@Name", entity.FullName);
                        cmd.Parameters.AddWithValue("@Email", entity.Email);
                        cmd.Parameters.AddWithValue("@Phone", entity.Phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DeptId", entity.DepartmentId);
                        cmd.Parameters.AddWithValue("@PosId", entity.PositionId);
                        cmd.Parameters.AddWithValue("@HireDate", entity.HireDate);
                        cmd.Parameters.AddWithValue("@Status", entity.Status);
                        cmd.Parameters.AddWithValue("@Created", DateTime.Now);
                        
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

        public override bool Update(Employee entity)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var sql = @"UPDATE Employee SET 
                               FullName = @Name, Email = @Email, Phone = @Phone,
                               DepartmentID = @DeptId, PositionID = @PosId, Status = @Status
                               WHERE EmployeeID = @Id";
                    
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", entity.Id);
                        cmd.Parameters.AddWithValue("@Name", entity.FullName);
                        cmd.Parameters.AddWithValue("@Email", entity.Email);
                        cmd.Parameters.AddWithValue("@Phone", entity.Phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DeptId", entity.DepartmentId);
                        cmd.Parameters.AddWithValue("@PosId", entity.PositionId);
                        cmd.Parameters.AddWithValue("@Status", entity.Status);
                        
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

        private Employee MapToEntity(MySqlDataReader reader)
        {
            return new Employee
            {
                Id = reader.GetInt32("EmployeeID"),
                EmployeeCode = reader.GetString("EmployeeCode"),
                FullName = reader.GetString("FullName"),
                Email = reader.GetString("Email"),
                Phone = GetSafeString(reader, "Phone"),
                DepartmentId = reader.GetInt32("DepartmentID"),
                PositionId = reader.GetInt32("PositionID"),
                HireDate = reader.GetDateTime("HireDate"),
                Status = reader.GetString("Status"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}


