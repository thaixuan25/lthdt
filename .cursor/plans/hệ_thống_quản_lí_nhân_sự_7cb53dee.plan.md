---
name: Hệ thống quản lí nhân sự
overview: Xây dựng hệ thống quản lí nhân sự bằng C# Windows Forms với MySQL, tập trung vào module tuyển dụng đầy đủ với quản lí đợt tuyển dụng, kiểm tra định biên, đăng nhập và gửi email tự động.
todos:
  - id: setup-database
    content: Tạo database MySQL với tất cả các bảng mới và setup connection
    status: pending
  - id: create-models
    content: "Tạo các Models: User, RecruitmentCampaign, Headcount, EmailLog"
    status: pending
    dependencies:
      - setup-database
  - id: build-dal
    content: Xây dựng Data Access Layer với Repository cho tất cả entities
    status: pending
    dependencies:
      - create-models
  - id: auth-service
    content: Triển khai AuthenticationService và PasswordHasher
    status: pending
    dependencies:
      - build-dal
  - id: headcount-service
    content: Triển khai HeadcountService với logic kiểm tra định biên
    status: pending
    dependencies:
      - build-dal
  - id: email-service
    content: Triển khai EmailService với SMTP và template
    status: pending
    dependencies:
      - build-dal
  - id: login-form
    content: Tạo LoginForm và SessionManager
    status: pending
    dependencies:
      - auth-service
  - id: main-form
    content: Tạo MainForm với menu và phân quyền
    status: pending
    dependencies:
      - login-form
  - id: headcount-forms
    content: Tạo HeadcountManagementForm và tích hợp kiểm tra vào JobPosting
    status: pending
    dependencies:
      - headcount-service
      - main-form
  - id: campaign-forms
    content: Tạo RecruitmentCampaign forms và tích hợp với JobPosting
    status: pending
    dependencies:
      - main-form
  - id: recruitment-forms
    content: Xây dựng các Forms tuyển dụng cơ bản
    status: pending
    dependencies:
      - campaign-forms
  - id: email-integration
    content: Tích hợp gửi email tự động vào quy trình tuyển dụng
    status: pending
    dependencies:
      - email-service
      - recruitment-forms
  - id: support-forms
    content: Xây dựng Forms cho Phòng ban và Nhân viên
    status: pending
    dependencies:
      - main-form
  - id: reporting
    content: Tích hợp báo cáo theo đợt tuyển dụng và định biên
    status: pending
    dependencies:
      - recruitment-forms
      - headcount-forms
  - id: testing
    content: Testing toàn bộ hệ thống và bảo mật
    status: pending
    dependencies:
      - email-integration
      - support-forms
      - reporting
---

# Kế hoạch Hệ thống Quản lí Nhân sự (Cập nhật)

## Tổng quan Kiến trúc

Hệ thống sử dụng kiến trúc 3 lớp (3-Layer Architecture) với C# Windows Forms và MySQL:

```mermaid
flowchart TB
    PresentationLayer[Presentation Layer<br/>Windows Forms UI]
    BusinessLayer[Business Logic Layer<br/>Services & Validation]
    DataLayer[Data Access Layer<br/>Repository Pattern]
    Database[(MySQL Database)]
    EmailService[Email Service<br/>SMTP]
    
    PresentationLayer -->|Gọi| BusinessLayer
    BusinessLayer -->|Xử lý| DataLayer
    BusinessLayer -->|Gửi email| EmailService
    DataLayer -->|Truy vấn| Database
```



## Cấu trúc Database (Cập nhật)

### Các bảng chính

```mermaid
erDiagram
    User ||--o{ Employee : is
    Department ||--o{ Employee : contains
    Department ||--o{ Headcount : has
    Department ||--o{ RecruitmentCampaign : initiates
    RecruitmentCampaign ||--o{ JobPosting : contains
    JobPosting ||--o{ Candidate : attracts
    Candidate ||--o{ Interview : has
    Interview }o--|| Employee : conducted_by
    Candidate ||--o{ CandidateStatus : tracks
    Candidate ||--o{ EmailLog : receives
    
    User {
        int UserID PK
        string Username
        string PasswordHash
        string Salt
        int EmployeeID FK
        string Role
        bool IsActive
        datetime LastLogin
    }
    
    Department {
        int DepartmentID PK
        string DepartmentName
        string Description
        int ManagerID FK
        int CurrentHeadcount
        int MaxHeadcount
        datetime CreatedDate
    }
    
    Headcount {
        int HeadcountID PK
        int DepartmentID FK
        string Position
        int Approved
        int Filled
        int Remaining
        int Year
    }
    
    RecruitmentCampaign {
        int CampaignID PK
        string CampaignName
        string Description
        int DepartmentID FK
        datetime StartDate
        datetime EndDate
        string Status
        int CreatedBy FK
        datetime CreatedDate
    }
    
    Employee {
        int EmployeeID PK
        string FullName
        string Email
        string Phone
        int DepartmentID FK
        string Position
        datetime HireDate
        string Status
    }
    
    JobPosting {
        int JobPostingID PK
        int CampaignID FK
        string JobTitle
        text JobDescription
        int DepartmentID FK
        int PositionCount
        string Requirements
        decimal Salary
        datetime PostDate
        datetime Deadline
        string Status
        bool IsHeadcountApproved
    }
    
    Candidate {
        int CandidateID PK
        string FullName
        string Email
        string Phone
        int JobPostingID FK
        text Resume
        datetime ApplyDate
        string CurrentStatus
    }
    
    Interview {
        int InterviewID PK
        int CandidateID FK
        int InterviewerID FK
        datetime InterviewDate
        string InterviewType
        text Notes
        decimal Score
        string Result
    }
    
    CandidateStatus {
        int StatusID PK
        int CandidateID FK
        string StatusName
        text Comments
        datetime UpdateDate
        int UpdatedBy FK
    }
    
    EmailLog {
        int EmailLogID PK
        int CandidateID FK
        string EmailType
        string Subject
        text Body
        datetime SentDate
        bool IsSent
        string ErrorMessage
    }
```



## Cấu trúc Thư mục Dự án (Cập nhật)

```javascript
LTHDT2/
├── Models/                    
│   ├── User.cs                    # MỚI
│   ├── Department.cs
│   ├── Headcount.cs               # MỚI
│   ├── RecruitmentCampaign.cs     # MỚI
│   ├── Employee.cs
│   ├── JobPosting.cs
│   ├── Candidate.cs
│   ├── Interview.cs
│   ├── CandidateStatus.cs
│   └── EmailLog.cs                # MỚI
├── DataAccess/               
│   ├── DatabaseConnection.cs
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   ├── UserRepository.cs             # MỚI
│   │   ├── DepartmentRepository.cs
│   │   ├── HeadcountRepository.cs        # MỚI
│   │   ├── RecruitmentCampaignRepository.cs  # MỚI
│   │   ├── EmployeeRepository.cs
│   │   ├── JobPostingRepository.cs
│   │   ├── CandidateRepository.cs
│   │   ├── InterviewRepository.cs
│   │   └── EmailLogRepository.cs         # MỚI
├── Services/                 
│   ├── AuthenticationService.cs   # MỚI
│   ├── DepartmentService.cs
│   ├── HeadcountService.cs        # MỚI
│   ├── EmployeeService.cs
│   ├── RecruitmentService.cs
│   ├── EmailService.cs            # MỚI
│   └── ValidationService.cs
├── Forms/                    
│   ├── MainForm.cs          
│   ├── Login/
│   │   ├── LoginForm.cs           # CẬP NHẬT
│   │   └── ChangePasswordForm.cs  # MỚI
│   ├── Department/
│   │   ├── DepartmentListForm.cs
│   │   ├── DepartmentEditForm.cs
│   │   └── HeadcountManagementForm.cs  # MỚI
│   ├── Employee/
│   │   ├── EmployeeListForm.cs
│   │   └── EmployeeEditForm.cs
│   └── Recruitment/
│       ├── RecruitmentCampaignListForm.cs    # MỚI
│       ├── RecruitmentCampaignEditForm.cs    # MỚI
│       ├── JobPostingListForm.cs
│       ├── JobPostingEditForm.cs             # CẬP NHẬT (kiểm tra định biên)
│       ├── CandidateListForm.cs
│       ├── CandidateDetailForm.cs
│       ├── InterviewScheduleForm.cs
│       ├── InterviewEvaluationForm.cs
│       ├── EmailTemplateForm.cs              # MỚI
│       └── RecruitmentReportForm.cs
├── Utils/                    
│   ├── ConfigManager.cs
│   ├── PasswordHasher.cs          # MỚI
│   ├── EmailHelper.cs             # CẬP NHẬT
│   └── ReportGenerator.cs
└── Program.cs
```



## Tính năng Mới Bổ sung

### 1. Quản lí Đợt Tuyển dụng (RecruitmentCampaign)

**Mục đích**: Nhóm nhiều yêu cầu tuyển dụng (JobPosting) vào một đợt tuyển dụng để quản lí tập trung.**Quy trình**:

```mermaid
flowchart LR
    CreateCampaign[Tạo đợt<br/>tuyển dụng]
    AddJobPostings[Thêm nhiều<br/>yêu cầu tuyển dụng]
    CheckHeadcount[Kiểm tra<br/>định biên]
    ManageCandidates[Quản lí<br/>ứng viên]
    SendEmails[Gửi email<br/>tự động]
    Report[Báo cáo<br/>tổng hợp]
    
    CreateCampaign --> AddJobPostings
    AddJobPostings --> CheckHeadcount
    CheckHeadcount --> ManageCandidates
    ManageCandidates --> SendEmails
    ManageCandidates --> Report
```

**Chức năng**:

- Tạo đợt tuyển dụng với tên, mô tả, thời gian bắt đầu/kết thúc
- Gắn đợt tuyển dụng với phòng ban
- Thêm nhiều yêu cầu tuyển dụng vào một đợt
- Xem tổng quan tất cả yêu cầu trong đợt
- Theo dõi tiến độ của cả đợt tuyển dụng
- Trạng thái: Đang chuẩn bị, Đang tuyển, Hoàn thành, Hủy

**Form**: `RecruitmentCampaignListForm.cs`, `RecruitmentCampaignEditForm.cs`

### 2. Kiểm tra Định biên (Headcount Validation)

**Mục đích**: Đảm bảo số lượng tuyển dụng không vượt quá định biên được phê duyệt của phòng ban.**Quy trình kiểm tra**:

```mermaid
flowchart TD
    Start[Tạo yêu cầu tuyển dụng]
    GetDept[Lấy thông tin<br/>phòng ban]
    GetHeadcount[Lấy định biên<br/>hiện tại]
    Calculate[Tính toán:<br/>Còn lại = Định biên - Đã có]
    Check{Số lượng<br/>tuyển <= Còn lại?}
    Approve[Chấp nhận<br/>yêu cầu]
    Reject[Từ chối<br/>Hiển thị cảnh báo]
    UpdateHeadcount[Cập nhật<br/>số lượng dự kiến]
    
    Start --> GetDept
    GetDept --> GetHeadcount
    GetHeadcount --> Calculate
    Calculate --> Check
    Check -->|Có| Approve
    Check -->|Không| Reject
    Approve --> UpdateHeadcount
```

**Công thức**:

```javascript
Remaining = MaxHeadcount - CurrentHeadcount - PendingRecruitments
If (RequestedPositionCount <= Remaining) Then Approve
Else Reject with Warning
```

**Chức năng**:

- Quản lí định biên của từng phòng ban theo năm
- Quản lí định biên theo vị trí (Position)
- Theo dõi: Đã phê duyệt, Đã tuyển, Còn lại
- Cảnh báo khi tạo yêu cầu tuyển dụng vượt định biên
- Cho phép request vượt định biên (cần phê duyệt đặc biệt)
- Lịch sử thay đổi định biên

**Form**: `HeadcountManagementForm.cs`, tích hợp vào `JobPostingEditForm.cs`

### 3. Hệ thống Đăng nhập (Authentication)

**Mục đích**: Bảo mật hệ thống và phân quyền người dùng.**Quy trình đăng nhập**:

```mermaid
flowchart TD
    Login[Nhập Username<br/>& Password]
    Hash[Hash password<br/>với Salt]
    Query[Truy vấn<br/>User table]
    Validate{Password<br/>khớp?}
    CheckActive{Tài khoản<br/>active?}
    LoadUser[Load thông tin<br/>User & Employee]
    SetSession[Tạo Session<br/>toàn cục]
    ShowMain[Hiển thị<br/>MainForm]
    Error[Hiển thị lỗi<br/>đăng nhập]
    
    Login --> Hash
    Hash --> Query
    Query --> Validate
    Validate -->|Không| Error
    Validate -->|Có| CheckActive
    CheckActive -->|Không| Error
    CheckActive -->|Có| LoadUser
    LoadUser --> SetSession
    SetSession --> ShowMain
```

**Chức năng**:

- Đăng nhập bằng Username/Password
- Mã hóa mật khẩu bằng SHA256 + Salt
- Phân quyền: Admin, HR Manager, HR Staff, Department Manager
- Lưu thông tin người dùng hiện tại (CurrentUser session)
- Đổi mật khẩu
- Logout
- Ghi log lần đăng nhập cuối

**Phân quyền**:

- **Admin**: Toàn quyền
- **HR Manager**: Quản lí toàn bộ module tuyển dụng
- **HR Staff**: Thực hiện tuyển dụng, không sửa định biên
- **Department Manager**: Xem yêu cầu tuyển dụng của phòng mình

**Form**: `LoginForm.cs`, `ChangePasswordForm.cs`

### 4. Gửi Email Tự động (Email Automation)

**Mục đích**: Tự động thông báo kết quả cho ứng viên qua email.**Các loại email**:

1. **Email xác nhận nhận hồ sơ**

- Gửi sau khi ứng viên nộp đơn
- Nội dung: Cảm ơn, thông tin đợt tuyển dụng

2. **Email mời phỏng vấn**

- Gửi khi lên lịch phỏng vấn
- Nội dung: Thời gian, địa điểm, người phỏng vấn

3. **Email thông báo kết quả**

- Gửi sau mỗi vòng phỏng vấn
- Nội dung: Đỗ/Trượt, lý do, bước tiếp theo

4. **Email chúc mừng trúng tuyển**

- Gửi khi ứng viên đạt
- Nội dung: Chúc mừng, hướng dẫn onboarding

5. **Email từ chối**

- Gửi khi ứng viên không đạt
- Nội dung: Cảm ơn, khuyến khích apply lần sau

**Quy trình gửi email**:

```mermaid
flowchart TD
    Trigger[Sự kiện kích hoạt<br/>Cập nhật trạng thái]
    CheckEmail{Email<br/>enabled?}
    GetTemplate[Lấy template<br/>tương ứng]
    FillData[Điền thông tin<br/>ứng viên]
    SendEmail[Gửi email<br/>qua SMTP]
    LogEmail[Ghi log<br/>EmailLog table]
    CheckSuccess{Gửi<br/>thành công?}
    UpdateLog[Cập nhật<br/>trạng thái]
    ShowError[Hiển thị<br/>thông báo lỗi]
    
    Trigger --> CheckEmail
    CheckEmail -->|Có| GetTemplate
    CheckEmail -->|Không| LogEmail
    GetTemplate --> FillData
    FillData --> SendEmail
    SendEmail --> CheckSuccess
    CheckSuccess -->|Có| LogEmail
    CheckSuccess -->|Không| ShowError
    LogEmail --> UpdateLog
    ShowError --> LogEmail
```

**Cấu hình SMTP**:

- Sử dụng Gmail SMTP hoặc SMTP server riêng
- Lưu cấu hình trong app.config
- Host, Port, Username, Password, EnableSSL

**Chức năng**:

- Quản lí template email (HTML format)
- Tự động gửi email khi thay đổi trạng thái ứng viên
- Gửi email thủ công
- Xem lịch sử email đã gửi
- Retry khi gửi thất bại
- Preview email trước khi gửi

**Service**: `EmailService.cs`, **Form**: `EmailTemplateForm.cs`

## Các Bước Triển khai (Cập nhật)

### Bước 1: Setup Database và Connection

- Tạo database MySQL và các bảng (bao gồm bảng mới)
- Viết class `DatabaseConnection.cs`
- Thêm package MySql.Data hoặc MySqlConnector

### Bước 2: Tạo Models

- Tạo tất cả các class entity
- Đặc biệt: User, RecruitmentCampaign, Headcount, EmailLog

### Bước 3: Xây dựng Data Access Layer

- Implement Repository cho tất cả entities
- Đặc biệt: UserRepository với method authentication

### Bước 4: Xây dựng Business Logic Layer

- **AuthenticationService**: Login, hash password, session management
- **HeadcountService**: Kiểm tra định biên, tính toán còn lại
- **EmailService**: Gửi email, quản lí template
- **RecruitmentService**: Logic tuyển dụng tích hợp các service trên

### Bước 5: Triển khai Hệ thống Đăng nhập

- Tạo `LoginForm.cs` với validation
- Implement `PasswordHasher.cs` (SHA256 + Salt)
- Tạo static class `SessionManager.cs` để lưu CurrentUser
- Sửa `Program.cs` để mở LoginForm trước

### Bước 6: Triển khai Module Định biên

- Tạo `HeadcountManagementForm.cs` để quản lí định biên
- Thêm logic kiểm tra định biên vào `JobPostingEditForm.cs`
- Hiển thị cảnh báo khi vượt định biên

### Bước 7: Triển khai Module Đợt Tuyển dụng

- Tạo `RecruitmentCampaignListForm.cs` và `RecruitmentCampaignEditForm.cs`
- Cập nhật `JobPostingEditForm.cs` để chọn Campaign
- Thêm tab Campaign vào MainForm

### Bước 8: Triển khai Email Service

- Cấu hình SMTP trong app.config
- Implement `EmailService.cs` với các method gửi email
- Tạo `EmailTemplateForm.cs` để quản lí template
- Tích hợp tự động gửi email khi cập nhật trạng thái ứng viên

### Bước 9: Xây dựng Forms Module Tuyển dụng

- Cập nhật các form hiện có để tích hợp Campaign và Email
- Thêm nút gửi email thủ công trong CandidateDetailForm

### Bước 10: Xây dựng Forms Module Phòng ban và Nhân viên

- Thêm quản lí định biên vào Department
- CRUD cơ bản cho Employee

### Bước 11: Tích hợp Báo cáo

- Báo cáo theo đợt tuyển dụng
- Báo cáo định biên theo phòng ban
- Báo cáo email đã gửi

### Bước 12: Testing và Hoàn thiện

- Test từng module
- Test tích hợp giữa các module
- Xử lý exception và validation
- Tối ưu performance

## Công nghệ và Thư viện

- **Framework**: .NET 10.0 Windows Forms
- **Database**: MySQL 8.0+
- **Package NuGet**:
- MySql.Data hoặc MySqlConnector
- MailKit hoặc System.Net.Mail (cho SMTP)
- ClosedXML (xuất Excel)
- System.Configuration.ConfigurationManager

## File Quan trọng

- [`Program.cs`](Program.cs): Entry point, khởi động LoginForm
- `DatabaseConnection.cs`: Kết nối MySQL
- `AuthenticationService.cs`: Xử lý đăng nhập
- `HeadcountService.cs`: Kiểm tra định biên
- `EmailService.cs`: Gửi email tự động
- `RecruitmentService.cs`: Logic tuyển dụng tổng hợp