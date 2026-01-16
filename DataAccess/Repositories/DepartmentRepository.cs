using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    public class DepartmentRepository : BaseRepository<Department>
    {
        protected override string TableName => "Department";

        public override Department GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT d.*, e.FullName AS ManagerName 
                      FROM Department d
                      LEFT JOIN Employee e ON d.ManagerID = e.EmployeeID
                      WHERE d.DepartmentID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<Department> GetAll()
        {
            var list = new List<Department>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT d.*, e.FullName AS ManagerName 
                      FROM Department d
                      LEFT JOIN Employee e ON d.ManagerID = e.EmployeeID
                      ORDER BY d.DepartmentCode", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(MapToEntity(reader));
                }
            }
            return list;
        }

        public override int Add(Department entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO Department (DepartmentCode, DepartmentName, Description, ManagerID, Location, CreatedDate)
                      VALUES (@Code, @Name, @Description, @ManagerId, @Location, @Created);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@Code", entity.DepartmentCode);
                cmd.Parameters.AddWithValue("@Name", entity.DepartmentName);
                cmd.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ManagerId", entity.ManagerId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Location", entity.Location ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(Department entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE Department SET 
                        DepartmentName = @Name, 
                        Description = @Description,
                        ManagerID = @ManagerId, 
                        Location = @Location, 
                        CurrentHeadcount = @CurrentHeadcount,
                        MaxHeadcount = @MaxHeadcount,
                        UpdatedDate = @Updated
                      WHERE DepartmentID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Name", entity.DepartmentName);
                cmd.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ManagerId", entity.ManagerId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Location", entity.Location ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CurrentHeadcount", entity.CurrentHeadcount);
                cmd.Parameters.AddWithValue("@MaxHeadcount", entity.MaxHeadcount);
                cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private Department MapToEntity(MySqlDataReader reader)
        {
            try
            {
                // Log available columns for debugging
                var availableColumns = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    availableColumns.Add(reader.GetName(i));
                }
                
                return new Department
                {
                    Id = reader.GetInt32("DepartmentID"),
                    DepartmentCode = reader.GetString("DepartmentCode"),
                    DepartmentName = reader.GetString("DepartmentName"),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                    ManagerId = reader.IsDBNull(reader.GetOrdinal("ManagerID")) ? null : reader.GetInt32("ManagerID"),
                    ManagerName = HasColumn(reader, "ManagerName") && !reader.IsDBNull(reader.GetOrdinal("ManagerName")) 
                        ? reader.GetString("ManagerName") 
                        : null,
                    Location = HasColumn(reader, "Location") && !reader.IsDBNull(reader.GetOrdinal("Location")) 
                        ? reader.GetString("Location") 
                        : null,
                    CurrentHeadcount = reader.IsDBNull(reader.GetOrdinal("CurrentHeadcount")) ? 0 : reader.GetInt32("CurrentHeadcount"),
                    MaxHeadcount = reader.IsDBNull(reader.GetOrdinal("MaxHeadcount")) ? 0 : reader.GetInt32("MaxHeadcount"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                };
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                var columns = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }
                
                throw new Exception(
                    $"Lỗi map Department entity:\n" +
                    $"Message: {ex.Message}\n" +
                    $"Columns có sẵn: {string.Join(", ", columns)}\n" +
                    $"Vui lòng chạy script Database/AddLocationColumn.sql để thêm columns: Location, UpdatedDate",
                    ex
                );
            }
        }

        private bool HasColumn(MySqlDataReader reader, string columnName)
        {
            try
            {
                reader.GetOrdinal(columnName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}


