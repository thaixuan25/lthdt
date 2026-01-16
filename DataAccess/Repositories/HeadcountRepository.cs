using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    public class HeadcountRepository : BaseRepository<Headcount>
    {
        protected override string TableName => "Headcount";

        public override Headcount GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE HeadcountID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<Headcount> GetAll()
        {
            var list = new List<Headcount>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand($"SELECT * FROM {TableName} ORDER BY Year DESC, DepartmentID", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(MapToEntity(reader));
                }
            }
            return list;
        }

        public override int Add(Headcount entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO Headcount (DepartmentID, PositionID, Year, ApprovedCount, FilledCount, ApprovedDate, ApprovedBy, CreatedDate)
                      VALUES (@DeptId, @PosId, @Year, @Approved, @Filled, @ApprovedDate, @ApprovedBy, @Created);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@DeptId", entity.DepartmentId);
                cmd.Parameters.AddWithValue("@PosId", entity.PositionId);
                cmd.Parameters.AddWithValue("@Year", entity.Year);
                cmd.Parameters.AddWithValue("@Approved", entity.ApprovedCount);
                cmd.Parameters.AddWithValue("@Filled", entity.FilledCount);
                cmd.Parameters.AddWithValue("@ApprovedDate", entity.ApprovedDate);
                cmd.Parameters.AddWithValue("@ApprovedBy", entity.ApprovedBy);
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(Headcount entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE Headcount SET 
                        ApprovedCount = @Approved, 
                        FilledCount = @Filled,
                        ApprovedDate = @ApprovedDate,
                        ApprovedBy = @ApprovedBy
                      WHERE HeadcountID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Approved", entity.ApprovedCount);
                cmd.Parameters.AddWithValue("@Filled", entity.FilledCount);
                cmd.Parameters.AddWithValue("@ApprovedDate", entity.ApprovedDate);
                cmd.Parameters.AddWithValue("@ApprovedBy", entity.ApprovedBy);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public Headcount? GetByDepartmentPositionYear(int departmentId, int positionId, int year)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE DepartmentID = @DeptId AND PositionID = @PosId AND Year = @Year", conn);
                cmd.Parameters.AddWithValue("@DeptId", departmentId);
                cmd.Parameters.AddWithValue("@PosId", positionId);
                cmd.Parameters.AddWithValue("@Year", year);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null;
        }

        private Headcount MapToEntity(MySqlDataReader reader)
        {
            return new Headcount
            {
                Id = reader.GetInt32("HeadcountID"),
                DepartmentId = reader.GetInt32("DepartmentID"),
                PositionId = reader.GetInt32("PositionID"),
                Year = reader.GetInt32("Year"),
                ApprovedCount = reader.GetInt32("ApprovedCount"),
                FilledCount = reader.GetInt32("FilledCount"),
                ApprovedDate = reader.GetDateTime("ApprovedDate"),
                ApprovedBy = reader.GetInt32("ApprovedBy"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}


