# HƯỚNG DẪN CÀI ĐẶT VÀ CHẠY ỨNG DỤNG

## Bước 1: Yêu cầu Hệ thống

- **Windows** 10/11
- **.NET 10.0 SDK** (tải tại: https://dotnet.microsoft.com/download)
- **MySQL Server 8.0+** (tải tại: https://dev.mysql.com/downloads/mysql/)
- **Visual Studio 2022** (khuyến nghị) hoặc VS Code

## Bước 2: Cài đặt MySQL

1. Cài đặt MySQL Server
2. Trong quá trình cài đặt, nhớ **mật khẩu root**
3. Khởi động MySQL Service

## Bước 3: Tạo Database

### Cách 1: Dùng MySQL Workbench (Khuyến nghị)

1. Mở **MySQL Workbench**
2. Kết nối đến MySQL Server (localhost:3306)
3. Mở file `Database/CreateDatabase.sql`
4. Click **Execute** (⚡) để chạy toàn bộ script
5. Kiểm tra: Database `HRManagementDB` đã được tạo với 13 bảng

### Cách 2: Dùng Command Line

```bash
mysql -u root -p < Database/CreateDatabase.sql
```

## Bước 4: Cấu hình Connection String

Mở file `App.config` và sửa password của MySQL:

```xml
<connectionStrings>
    <add name="HRManagementDB" 
         connectionString="Server=localhost;Database=HRManagementDB;Uid=root;Pwd=YOUR_MYSQL_PASSWORD;CharSet=utf8mb4;SslMode=None;" 
         providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

**Thay `YOUR_MYSQL_PASSWORD` bằng mật khẩu root MySQL của bạn!**

## Bước 5: Tạo User Mặc định với Password Đã Hash

Database script đã tạo 3 user mẫu nhưng password chưa được hash đúng. Bạn cần chạy một lần để tạo user admin với password đúng.

### Cách 1: Chạy Script Update Password

Chạy script SQL sau trong MySQL Workbench hoặc Command Line:

```sql
USE HRManagementDB;

-- Xóa users cũ
DELETE FROM User;

-- Tạo user admin mới (password sẽ được hash trong code lần đầu chạy)
-- Password mặc định: admin123
INSERT INTO User (Username, PasswordHash, Salt, EmployeeID, Role, IsActive, CreatedDate) 
VALUES 
('admin', 'TEMP_HASH', 'TEMP_SALT', 1, 'Admin', 1, NOW());
```

### Cách 2: Chạy ứng dụng và tạo user đầu tiên

- Bạn sẽ gặp lỗi khi login lần đầu
- Cần có một công cụ nhỏ để hash password (xem Bước 7)

## Bước 6: Build và Chạy Ứng dụng

### Dùng Command Line

```bash
cd LTHDT2
dotnet restore
dotnet build
dotnet run
```

### Dùng Visual Studio

1. Mở file `LTHDT2.sln`
2. Nhấn **F5** để build và chạy
3. Hoặc nhấn **Ctrl+F5** để chạy không debug

## Bước 7: Tạo User Admin Đầu tiên ⭐

Database đã tạo 3 employee nhưng **chưa có user nào để đăng nhập**. Bạn cần tạo user admin đầu tiên.

### Cách 1: Sử dụng Setup Utility (KHUYẾN NGHỊ) ⭐

Chạy ứng dụng với tham số `--setup`:

```bash
cd LTHDT2
dotnet run --setup
```

Menu sẽ hiện ra:

```
╔════════════════════════════════════════╗
║   HỆ THỐNG QUẢN LÝ NHÂN SỰ - UTILS   ║
╚════════════════════════════════════════╝

1. Hash một password
2. Tạo admin user đầu tiên
3. Tạo tất cả user mặc định
4. Kiểm tra kết nối database
0. Thoát và chạy ứng dụng

Chọn (0-4):
```

**Chọn 3** để tạo tất cả user mặc định (admin, hrmanager, staff) với password: `admin123`

### Cách 2: Sử dụng RegisterForm (Đăng ký qua UI)

1. Chạy ứng dụng: `dotnet run`
2. Ở màn hình Login, nhấn link "**Chưa có tài khoản? Đăng ký ngay**" (nếu đã bật)
3. Điền form đăng ký:
   - **Username**: admin
   - **Password**: admin123
   - **Xác nhận password**: admin123
   - **Mã nhân viên**: EMP001
   - **Họ tên**: Nguyễn Văn Admin
   - **Email**: admin@company.com
   - **Số điện thoại**: (tùy chọn)

4. Nhấn "**Đăng ký**"

> **Lưu ý**: Mặc định link đăng ký bị ẩn (để bảo mật). Để bật, mở file `Forms/LoginForm.cs` và uncomment dòng:
> ```csharp
> lnkRegister.Visible = true;
> ```

### Cách 3: Tạo User qua Code

Trong `Program.cs`, uncomment các dòng sau:

```csharp
Console.WriteLine("Tạo user mặc định...");
HashPasswordConsole.CreateAllDefaultUsers();
Console.WriteLine("Nhấn phím bất kỳ để tiếp tục...");
Console.ReadKey();
```

Sau đó chạy: `dotnet run`

## Bước 8: Đăng nhập

Khởi động ứng dụng, form Login sẽ hiện ra:

- **Username**: `admin`
- **Password**: `admin123` (sau khi đã hash đúng ở Bước 7)

## Cấu trúc Dữ liệu Mẫu

Database đã có sẵn dữ liệu mẫu:

### Positions (Vị trí)
- Junior Developer, Middle Developer, Senior Developer, Tech Lead
- Business Analyst, QA Engineer, Project Manager, HR Specialist

### Departments (Phòng ban)
- IT (Công nghệ thông tin)
- HR (Nhân sự)
- SALES (Kinh doanh)
- MKT (Marketing)

### Employees (Nhân viên)
- EMP001: Nguyễn Văn Admin (Admin)
- EMP002: Trần Thị HR Manager (HR Manager)
- EMP003: Lê Văn Tech Lead (Staff)

### Headcount (Định biên 2026)
- IT: 10 Junior Dev, 15 Middle Dev, 8 Senior Dev, 3 Tech Lead
- HR: 5 HR Specialist

### Sample Job Posting
- Tuyển Middle C# Developer (3 người)

### Sample Candidates
- Nguyễn Văn A (Java Developer, 2 năm kinh nghiệm)
- Trần Thị B (C# Developer, 3 năm kinh nghiệm)

## Troubleshooting (Xử lý Lỗi)

### Lỗi: "Không thể kết nối database"

- Kiểm tra MySQL Service đã chạy chưa
- Kiểm tra password trong `App.config`
- Kiểm tra port MySQL (mặc định 3306)

### Lỗi: "Sai tên đăng nhập hoặc mật khẩu"

- Password chưa được hash đúng
- Thực hiện lại Bước 7

### Lỗi: "Table không tồn tại"

- Database chưa được tạo
- Chạy lại script `Database/CreateDatabase.sql`

## Các Tính năng Đã Implement

### Core Features ✅
- ✅ **Đăng nhập/Đăng xuất** với password hash SHA256 + Salt
- ✅ **Đăng ký tài khoản** (RegisterForm) - Nhân viên tự tạo user
- ✅ **Quản lý User** (Admin only) - Tạo, bật/tắt user

### Quản lý Nhân sự ✅
- ✅ Quản lý Vị trí (Position)
- ✅ Quản lý Phòng ban (Department)  
- ✅ Quản lý Nhân viên (Employee)
- ✅ Quản lý Định biên (Headcount)

### Tuyển dụng ✅
- ✅ Quản lý Đợt tuyển dụng (Campaign)
- ✅ Quản lý Tin tuyển dụng (JobPosting) - Tự động kiểm tra định biên
- ✅ Quản lý Hồ sơ ứng viên (Candidate)
- ✅ Quản lý Đơn ứng tuyển (Application) - Core feature
- ✅ Kiểm tra trùng lặp khi ứng tuyển
- ✅ Lịch sử ứng tuyển của ứng viên

### Tiện ích ✅
- ✅ Email Service (cần cấu hình SMTP)
- ✅ Setup Utility Console (--setup mode)
- ✅ Password Hasher
- ✅ Session Management

## Các Tính năng Chưa Hoàn thiện (TODO)

⏳ Gửi Email tự động (cần cấu hình SMTP trong App.config)
⏳ Quản lý Phỏng vấn (Interview)
⏳ Báo cáo thống kê chi tiết
⏳ Export Excel/PDF

## Cấu hình Email (Tùy chọn)

Để kích hoạt gửi email tự động, sửa trong `App.config`:

```xml
<appSettings>
    <add key="SmtpHost" value="smtp.gmail.com" />
    <add key="SmtpPort" value="587" />
    <add key="SmtpUsername" value="your_email@gmail.com" />
    <add key="SmtpPassword" value="your_app_password" />
    <add key="SmtpEnableSSL" value="true" />
    <add key="SmtpFromEmail" value="noreply@company.com" />
</appSettings>
```

**Lưu ý Gmail**: Cần tạo App Password thay vì dùng mật khẩu thường.

## Demo OOP

Dự án này áp dụng đầy đủ 4 tính chất OOP:

1. **Encapsulation**: Private fields + Public properties trong Models
2. **Abstraction**: Interfaces (IRepository, IEmailService) và Abstract classes (BaseEntity, BaseRepository, BaseService)
3. **Inheritance**: Models kế thừa BaseEntity, Repositories kế thừa BaseRepository, Forms kế thừa BaseForm
4. **Polymorphism**: Method Overriding, Method Overloading (EmailService.SendEmail)

## Liên hệ

- Dự án học tập: LTHDT2
- Framework: .NET 10.0 Windows Forms
- Database: MySQL 8.0

