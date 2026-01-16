using System;
using LTHDT2.Utils;
using LTHDT2.Services;
using LTHDT2.DataAccess.Repositories;

namespace LTHDT2.Utils
{
    /// <summary>
    /// Console Utility để hash password và tạo user admin đầu tiên
    /// Uncomment trong Program.cs để sử dụng
    /// </summary>
    public static class HashPasswordConsole
    {
        /// <summary>
        /// Hash một password và hiển thị kết quả
        /// </summary>
        public static void HashPassword(string password)
        {
            Console.WriteLine("=== PASSWORD HASHER ===");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine();

            var (hash, salt) = PasswordHasher.Hash(password);
            
            Console.WriteLine($"Hash: {hash}");
            Console.WriteLine($"Salt: {salt}");
            Console.WriteLine();
            Console.WriteLine("Copy 2 giá trị trên và update vào database!");
        }

        /// <summary>
        /// Tạo admin user đầu tiên với password đã hash
        /// </summary>
        public static void CreateDefaultAdminUser()
        {
            try
            {
                Console.WriteLine("=== TẠO ADMIN USER ===");
                
                var authService = new AuthenticationService();
                var userRepo = new UserRepository();
                
                // Kiểm tra admin đã tồn tại chưa
                var existingAdmin = userRepo.GetByUsername("admin");
                if (existingAdmin != null)
                {
                    Console.WriteLine("User 'admin' đã tồn tại!");
                    Console.WriteLine("Bạn có muốn reset password về 'admin123'? (y/n)");
                    var confirm = Console.ReadLine();
                    
                    if (confirm?.ToLower() == "y")
                    {
                        var (hash, salt) = PasswordHasher.Hash("admin123");
                        existingAdmin.PasswordHash = hash;
                        existingAdmin.Salt = salt;
                        
                        if (userRepo.Update(existingAdmin))
                        {
                            Console.WriteLine("✓ Đã reset password admin thành công!");
                        }
                    }
                    return;
                }

                // Tạo user admin mới
                Console.WriteLine("Tạo user admin với thông tin:");
                Console.WriteLine("- Username: admin");
                Console.WriteLine("- Password: admin123");
                Console.WriteLine("- Role: Admin");
                Console.WriteLine("- EmployeeID: 1");
                Console.WriteLine();

                var userId = authService.CreateUser("admin", "admin123", 1, "Admin");
                
                if (userId > 0)
                {
                    Console.WriteLine($"✓ Tạo admin user thành công! UserID: {userId}");
                    Console.WriteLine();
                    Console.WriteLine("Bạn có thể đăng nhập với:");
                    Console.WriteLine("Username: admin");
                    Console.WriteLine("Password: admin123");
                }
                else
                {
                    Console.WriteLine("✗ Tạo admin user thất bại!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo tất cả user mặc định
        /// </summary>
        public static void CreateAllDefaultUsers()
        {
            try
            {
                Console.WriteLine("=== TẠO TẤT CẢ USER MẶC ĐỊNH ===");
                Console.WriteLine();

                var authService = new AuthenticationService();
                
                // Tạo admin
                Console.Write("Tạo user 'admin'... ");
                try
                {
                    var adminId = authService.CreateUser("admin", "admin123", 1, "Admin");
                    Console.WriteLine($"✓ UserID: {adminId}");
                }
                catch
                {
                    Console.WriteLine("✗ (có thể đã tồn tại)");
                }

                // Tạo hrmanager
                Console.Write("Tạo user 'hrmanager'... ");
                try
                {
                    var hrId = authService.CreateUser("hrmanager", "admin123", 2, "HRManager");
                    Console.WriteLine($"✓ UserID: {hrId}");
                }
                catch
                {
                    Console.WriteLine("✗ (có thể đã tồn tại)");
                }

                // Tạo staff
                Console.Write("Tạo user 'staff'... ");
                try
                {
                    var staffId = authService.CreateUser("staff", "admin123", 3, "Staff");
                    Console.WriteLine($"✓ UserID: {staffId}");
                }
                catch
                {
                    Console.WriteLine("✗ (có thể đã tồn tại)");
                }

                Console.WriteLine();
                Console.WriteLine("Hoàn thành! Password mặc định cho tất cả: admin123");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Menu console tương tác
        /// </summary>
        public static void RunInteractiveMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║   HỆ THỐNG QUẢN LÝ NHÂN SỰ - UTILS   ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine("1. Hash một password");
                Console.WriteLine("2. Tạo admin user đầu tiên");
                Console.WriteLine("3. Tạo tất cả user mặc định");
                Console.WriteLine("4. Kiểm tra kết nối database");
                Console.WriteLine("0. Thoát và chạy ứng dụng");
                Console.WriteLine();
                Console.Write("Chọn (0-4): ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Nhập password: ");
                        var pwd = Console.ReadLine();
                        if (!string.IsNullOrEmpty(pwd))
                            HashPassword(pwd);
                        break;

                    case "2":
                        CreateDefaultAdminUser();
                        break;

                    case "3":
                        CreateAllDefaultUsers();
                        break;

                    case "4":
                        TestDatabaseConnection();
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Nhấn phím bất kỳ để tiếp tục...");
                Console.ReadKey();
            }
        }

        private static void TestDatabaseConnection()
        {
            Console.WriteLine("=== KIỂM TRA KẾT NỐI DATABASE ===");
            Console.WriteLine();

            if (DataAccess.DatabaseConnection.TestConnection(out string error))
            {
                Console.WriteLine("✓ Kết nối database thành công!");
                
                // Đếm số bảng
                try
                {
                    var userRepo = new UserRepository();
                    var count = userRepo.Count();
                    Console.WriteLine($"✓ Số user hiện có: {count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Lỗi truy vấn: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"✗ Kết nối thất bại: {error}");
            }
        }
    }
}

