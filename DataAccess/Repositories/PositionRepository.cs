using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    public class PositionRepository : BaseRepository<Position>
    {
        protected override string TableName => "Position";

        public override Position GetById(int id)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $"SELECT * FROM {TableName} WHERE PositionID = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return MapToEntity(reader);
                }
            }
            return null!;
        }

        public override IEnumerable<Position> GetAll()
        {
            var list = new List<Position>();
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

        public override int Add(Position entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO Position (PositionCode, PositionName, Description, Level, MinSalary, MaxSalary, CreatedDate)
                      VALUES (@Code, @Name, @Desc, @Level, @MinSalary, @MaxSalary, @Created);
                      SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@Code", entity.PositionCode);
                cmd.Parameters.AddWithValue("@Name", entity.PositionName);
                cmd.Parameters.AddWithValue("@Desc", entity.Description);
                cmd.Parameters.AddWithValue("@Level", entity.Level);
                cmd.Parameters.AddWithValue("@MinSalary", entity.MinSalary);
                cmd.Parameters.AddWithValue("@MaxSalary", entity.MaxSalary);
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public override bool Update(Position entity)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE Position SET 
                        PositionName = @Name, Description = @Desc,
                        Level = @Level, MinSalary = @MinSalary, MaxSalary = @MaxSalary,
                        UpdatedDate = @Updated
                      WHERE PositionID = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", entity.Id);
                cmd.Parameters.AddWithValue("@Name", entity.PositionName);
                cmd.Parameters.AddWithValue("@Desc", entity.Description);
                cmd.Parameters.AddWithValue("@Level", entity.Level);
                cmd.Parameters.AddWithValue("@MinSalary", entity.MinSalary);
                cmd.Parameters.AddWithValue("@MaxSalary", entity.MaxSalary);
                cmd.Parameters.AddWithValue("@Updated", DateTime.Now);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private Position MapToEntity(MySqlDataReader reader)
        {
            return new Position
            {
                Id = reader.GetInt32("PositionID"),
                PositionCode = reader.GetString("PositionCode"),
                PositionName = reader.GetString("PositionName"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                Level = reader.GetInt32("Level"),
                MinSalary = reader.GetDecimal("MinSalary"),
                MaxSalary = reader.GetDecimal("MaxSalary"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}


