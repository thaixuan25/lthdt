using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    /// <summary>
    /// Base Repository Abstract Class
    /// </summary>
    public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        // Protected - class con có thể truy cập
        protected readonly string _connectionString;

        /// <summary>
        /// Abstract property - class con PHẢI override
        /// Tên bảng trong database
        /// </summary>
        protected abstract string TableName { get; }

        /// <summary>
        /// Protected constructor - chỉ class con có thể gọi
        /// </summary>
        protected BaseRepository()
        {
            _connectionString = DatabaseConnection.ConnectionString;
        }

        /// <summary>
        /// Lấy entity theo ID - Abstract method
        /// Class con phải implement cách map từ DataReader sang Entity
        /// </summary>
        public abstract T? GetById(int id);

        /// <summary>
        /// Lấy tất cả entities - Abstract method
        /// </summary>
        public abstract IEnumerable<T> GetAll();

        /// <summary>
        /// Thêm mới entity - Abstract method
        /// </summary>
        public abstract int Add(T entity);

        /// <summary>
        /// Cập nhật entity - Abstract method
        /// </summary>
        public abstract bool Update(T entity);

        /// <summary>
        /// Xóa entity - Virtual method (có implementation mặc định)
        /// Class con có thể override nếu cần logic khác
        /// </summary>
        public virtual bool Delete(int id)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand($"DELETE FROM {TableName} WHERE {GetPrimaryKeyName()} = @Id", conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                LogError("Delete", ex);
                return false;
            }
        }

        /// <summary>
        /// Tìm kiếm theo predicate - Virtual method
        /// Implementation đơn giản: lấy tất cả rồi filter
        /// Class con có thể override để query hiệu quả hơn
        /// </summary>
        public virtual IEnumerable<T> Find(Func<T, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        /// <summary>
        /// Đếm số lượng - Virtual method
        /// </summary>
        public virtual int Count()
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {TableName}", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                LogError("Count", ex);
                return 0;
            }
        }

        /// <summary>
        /// Kiểm tra tồn tại - Virtual method
        /// </summary>
        public virtual bool Exists(int id)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {TableName} WHERE {GetPrimaryKeyName()} = @Id", conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                LogError("Exists", ex);
                return false;
            }
        }

        /// <summary>
        /// Tạo MySqlConnection - Protected method
        /// Class con có thể dùng để tạo connection
        /// </summary>
        protected MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Lấy tên primary key - Virtual, có thể override
        /// Mặc định: {TableName}ID (VD: EmployeeID, DepartmentID)
        /// </summary>
        protected virtual string GetPrimaryKeyName()
        {
            return $"{TableName}ID";
        }

        /// <summary>
        /// Log lỗi - Protected method
        /// Class con có thể gọi để log lỗi
        /// </summary>
        protected void LogError(string methodName, Exception ex)
        {
            // TODO: Implement proper logging (file, database, etc.)
            Console.WriteLine($"[ERROR] {typeof(T).Name}Repository.{methodName}: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        /// <summary>
        /// Execute Non Query - Protected helper
        /// Thực thi câu lệnh INSERT/UPDATE/DELETE
        /// </summary>
        protected bool ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        if (parameters != null && parameters.Length > 0)
                            cmd.Parameters.AddRange(parameters);
                        
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ExecuteNonQuery", ex);
                return false;
            }
        }

        /// <summary>
        /// Execute Scalar - Protected helper
        /// Trả về giá trị đơn (COUNT, MAX, MIN, etc.)
        /// </summary>
        protected object? ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        if (parameters != null && parameters.Length > 0)
                            cmd.Parameters.AddRange(parameters);
                        
                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ExecuteScalar", ex);
                return null;
            }
        }

        /// <summary>
        /// Safe Get String - Xử lý DBNull
        /// </summary>
        protected string GetSafeString(MySqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        /// <summary>
        /// Safe Get Int - Xử lý DBNull
        /// </summary>
        protected int? GetSafeInt(MySqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
        }

        /// <summary>
        /// Safe Get DateTime - Xử lý DBNull
        /// </summary>
        protected DateTime? GetSafeDateTime(MySqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }

        /// <summary>
        /// Safe Get Decimal - Xử lý DBNull
        /// </summary>
        protected decimal? GetSafeDecimal(MySqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
        }

        /// <summary>
        /// Safe Get Bool - Xử lý DBNull
        /// </summary>
        protected bool GetSafeBool(MySqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }
    }
}

