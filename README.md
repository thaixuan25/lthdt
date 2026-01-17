# HỆ THỐNG QUẢN LÝ NHÂN SỰ

Dự án Windows Forms C# với MySQL

## Công nghệ

- **.NET 10.0** Windows Forms
- **MySQL 8.0+**
- **Kiến trúc 3 lớp**: Presentation → Business Logic → Data Access

### Thư viện NuGet

- **MySql.Data** (v9.1.0) - Kết nối MySQL
- **System.Configuration.ConfigurationManager** (v9.0.0) - Quản lý cấu hình
- **MailKit** (v4.9.0) - Gửi email SMTP
- **ClosedXML** (v0.104.2) - Xuất Excel
- **Guna.UI2.WinForms** (v2.0.4.6) - UI Components

## Cài đặt

### 1. Yêu cầu

- Visual Studio 2022 hoặc mới hơn
- .NET 10.0 SDK
- MySQL Server 8.0+

### 2. Setup Database

```bash
# Mở MySQL Workbench hoặc MySQL Command Line
# Chạy script tạo database
mysql -u root -p < Database/CreateDatabase.sql
```

Hoặc:
1. Mở MySQL Workbench
2. Mở file `Database/CreateDatabase.sql`
3. Execute toàn bộ script

### 3. Cấu hình Connection String và Email

Mở file `App.config` và cấu hình:

**Connection String:**
```xml
<connectionStrings>
    <add name="HRManagementDB" 
         connectionString="Server=localhost;Port=3306;Database=HRManagementDB;Uid=root;Pwd=YOUR_PASSWORD;CharSet=utf8mb4;SslMode=None;" 
         providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

**Cấu hình Email (SMTP):**
```xml
<appSettings>
    <add key="SmtpHost" value="smtp.gmail.com" />
    <add key="SmtpPort" value="587" />
    <add key="SmtpUsername" value="your-email@gmail.com" />
    <add key="SmtpPassword" value="your-app-password" />
    <add key="SmtpEnableSSL" value="true" />
    <add key="SmtpFromEmail" value="your-email@gmail.com" />
    <add key="SmtpFromName" value="HR Management System" />
</appSettings>
```

**Lưu ý:** Với Gmail, cần sử dụng [App Password](https://support.google.com/accounts/answer/185833) thay vì mật khẩu thông thường.

### 4. Build và Chạy

```bash
cd LTHDT2
dotnet restore
dotnet build
dotnet run
```

Hoặc:
- Mở `LTHDT2.sln` trong Visual Studio
- Nhấn F5 để chạy

## Đăng nhập

Tài khoản mặc định (password cần được hash trong code lần đầu):
- **Admin**: `admin` / `admin123`
- **HR Manager**: `hrmanager` / `admin123`
- **Staff**: `staff` / `admin123`

## Cấu trúc Dự án

```
LTHDT2/
├── Models/                    # Entities (Encapsulation)
│   ├── BaseEntity.cs         # Abstract base class
│   ├── User.cs, Employee.cs, Department.cs
│   ├── Position.cs, Headcount.cs
│   ├── Candidate.cs, Application.cs (QUAN TRỌNG)
│   └── JobPosting.cs, Interview.cs, EmailLog.cs
│
├── DataAccess/               # Data Access Layer
│   ├── DatabaseConnection.cs
│   └── Repositories/         # Repository Pattern (Inheritance)
│       ├── IRepository.cs    # Interface (Abstraction)
│       ├── BaseRepository.cs # Abstract class
│       └── *Repository.cs    # Concrete implementations
│
├── Services/                 # Business Logic Layer
│   ├── BaseService.cs        # Abstract base service
│   ├── AuthenticationService.cs
│   ├── ApplicationService.cs # Core service
│   ├── CandidateService.cs
│   ├── EmailService.cs       # Polymorphism (overloading)
│   ├── HeadcountService.cs
│   ├── InterviewService.cs
│   ├── JobPostingService.cs
│   ├── RecruitmentCampaignService.cs
│   ├── ReportService.cs
│   └── I*.cs                 # Service interfaces
│
├── Forms/                    # Presentation Layer
│   ├── BaseForm.cs           # Base form (Inheritance)
│   ├── BaseListForm.cs       # Generic list form
│   ├── BaseEditForm.cs       # Generic edit form
│   └── */                    # Specific forms
│
├── Utils/                    # Utilities
│   ├── SessionManager.cs     # User session
│   ├── PasswordHasher.cs     # Password hashing (SHA256 + Salt)
│   ├── HashPasswordConsole.cs # Tool hash password
│   ├── Logger.cs             # Logging utility
│   └── UITheme.cs            # UI theme management
│
└── Database/                 # SQL Scripts
    └── CreateDatabase.sql
```

## 4 Tính chất OOP được áp dụng

### 1. Encapsulation (Đóng gói)
- Private fields + Public properties với validation
- Ví dụ: `Models/Employee.cs`, `Models/Application.cs`

### 2. Abstraction (Trừu tượng hóa)
- Interfaces: `IRepository<T>`, `IEmailService`
- Abstract classes: `BaseEntity`, `BaseRepository<T>`, `BaseService`

### 3. Inheritance (Kế thừa)
- Models kế thừa `BaseEntity`
- Repositories kế thừa `BaseRepository<T>`
- Forms kế thừa `BaseForm`, `BaseListForm<T>`

### 4. Polymorphism (Đa hình)
- Method Overriding: Override abstract methods trong repositories
- Method Overloading: `EmailService.SendEmail(...)`
- Interface implementations: Multiple implementations của `IRepository<T>`

## Tính năng Chính

1. ✅ **Quản lý User & Authentication**
2. ✅ **Quản lý Vị trí (Position)**
3. ✅ **Quản lý Phòng ban & Định biên**
4. ✅ **Quản lý Nhân viên**
5. ✅ **Quản lý Đợt tuyển dụng (Campaign)**
6. ✅ **Quản lý Tin tuyển dụng (JobPosting)** - Kiểm tra định biên tự động
7. ✅ **Quản lý Hồ sơ ứng viên (Candidate)**
8. ✅ **Quản lý Đơn ứng tuyển (Application)** - CORE FEATURE
9. ✅ **Quản lý Phỏng vấn (Interview)**
10. ✅ **Gửi Email tự động** - Xác nhận, Mời PV, Thông báo KQ
11. ✅ **Báo cáo thống kê**

## Luồng Nghiệp vụ Chính

### Quy trình Tuyển dụng

1. **Tạo Tin tuyển dụng**
   - Kiểm tra định biên tự động
   - Cảnh báo nếu vượt định biên
   - HR Manager có thể override

2. **Ứng viên Nộp đơn**
   - Kiểm tra email trùng → Lấy Candidate có sẵn hoặc tạo mới
   - Kiểm tra đã ứng tuyển vị trí này chưa
   - Tạo Application mới
   - Gửi email xác nhận tự động

3. **Sàng lọc & Phỏng vấn**
   - HR xem danh sách Application
   - Lên lịch phỏng vấn
   - Gửi email mời phỏng vấn tự động
   - Người phỏng vấn nhập kết quả & điểm

4. **Thông báo Kết quả**
   - Cập nhật trạng thái Application
   - Gửi email thông báo tự động (Đạt/Không đạt)

## Lưu ý

- **Password**: Được hash bằng SHA256 + Salt. Sử dụng `HashPasswordConsole` để tạo hash cho tài khoản mới.
- **File CV**: Được lưu vào thư mục `Resumes/` (tự động tạo nếu chưa có)
- **Email Templates**: Đặt ở thư mục `EmailTemplates/` (tự động tạo nếu chưa có)
- **Một Candidate có thể có nhiều Application**: Ứng viên có thể ứng tuyển nhiều vị trí khác nhau
- **Định biên**: Hệ thống tự động kiểm tra định biên khi tạo JobPosting, cảnh báo nếu vượt quá
- **Session Timeout**: Mặc định 60 phút (có thể cấu hình trong `App.config`)

## Troubleshooting

### Lỗi kết nối Database
- Kiểm tra MySQL Server đã chạy chưa
- Kiểm tra connection string trong `App.config`
- Kiểm tra mật khẩu database
- Đảm bảo database `HRManagementDB` đã được 

### Lỗi gửi Email
- Kiểm tra cấu hình SMTP trong `App.config`
- Với Gmail: Sử dụng App Password, không dùng mật khẩu thông thường
- Kiểm tra firewall/antivirus có chặn port 587 không

### Lỗi Build
- Chạy `dotnet restore` để tải lại packages
- Kiểm tra .NET 10.0 SDK đã cài đặt đúng chưa
- Xóa thư mục `bin/` và `obj/` rồi build lại

## Phân quyền

Hệ thống hỗ trợ 3 vai trò:

- **Admin**: Toàn quyền quản lý hệ thống
- **HR Manager**: Quản lý tuyển dụng, phỏng vấn, định biên
- **Staff**: Xem và quản lý ứng viên, đơn ứng tuyển

## Báo cáo & Thống kê

- Thống kê số lượng ứng viên theo vị trí
- Thống kê tỷ lệ đậu/trượt phỏng vấn
- Báo cáo định biên theo phòng ban
- Xuất Excel danh sách ứng viên, đơn ứng tuyển

## License

Dự án môn học lập trình hướng đối tượng