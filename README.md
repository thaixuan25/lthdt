# HỆ THỐNG QUẢN LÝ NHÂN SỰ

Dự án Windows Forms C# với MySQL - Áp dụng đầy đủ 4 tính chất OOP

## Công nghệ

- **.NET 10.0** Windows Forms
- **MySQL 8.0+**
- **Kiến trúc 3 lớp**: Presentation → Business Logic → Data Access

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

### 3. Cấu hình Connection String

Mở file `App.config` và sửa connection string:

```xml
<connectionStrings>
    <add name="HRManagementDB" 
         connectionString="Server=localhost;Database=HRManagementDB;Uid=root;Pwd=YOUR_PASSWORD;CharSet=utf8mb4;SslMode=None;" 
         providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

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
│   ├── EmailService.cs       # Polymorphism (overloading)
│   └── HeadcountService.cs
│
├── Forms/                    # Presentation Layer
│   ├── BaseForm.cs           # Base form (Inheritance)
│   ├── BaseListForm.cs       # Generic list form
│   ├── BaseEditForm.cs       # Generic edit form
│   └── */                    # Specific forms
│
├── Utils/                    # Utilities
│   ├── SessionManager.cs     # User session
│   ├── PasswordHasher.cs     # Password hashing
│   └── ConfigManager.cs
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

- Password được hash bằng SHA256 + Salt
- File CV được lưu vào thư mục `Resumes/`
- Email templates ở thư mục `EmailTemplates/`
- Một Candidate có thể có nhiều Application (ứng tuyển nhiều lần)

## License

Dự án học tập - LTHDT2

