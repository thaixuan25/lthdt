using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace LTHDT2.DataAccess
{
    /// <summary>
    /// Quản lý connection string và tạo connection
    /// </summary>
    public static class DatabaseConnection
    {
        private static string? _connectionString;

        /// <summary>
        /// Lấy connection string từ App.config
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = ConfigurationManager.ConnectionStrings["HRManagementDB"]?.ConnectionString;
                    
                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        throw new InvalidOperationException("Connection string 'HRManagementDB' không tìm thấy trong App.config");
                    }
                }
                return _connectionString;
            }
        }

        /// <summary>
        /// Tạo MySqlConnection mới
        /// </summary>
        public static MySqlConnection CreateConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        /// <summary>
        /// Test kết nối database
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}

