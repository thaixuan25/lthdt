
-- Tạo database
DROP DATABASE IF EXISTS HRManagementDB;
CREATE DATABASE HRManagementDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE HRManagementDB;

-- ================================================================
-- 1. BẢNG POSITION - Danh mục vị trí công việc
-- ================================================================
CREATE TABLE Position (
    PositionID INT AUTO_INCREMENT PRIMARY KEY,
    PositionCode VARCHAR(20) NOT NULL UNIQUE,
    PositionName VARCHAR(100) NOT NULL,
    Description TEXT,
    Level INT DEFAULT 1 COMMENT 'Cấp bậc: 1=Junior, 2=Middle, 3=Senior, 4=Lead, 5=Manager',
    MinSalary DECIMAL(15,2) DEFAULT 0,
    MaxSalary DECIMAL(15,2) DEFAULT 0,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_position_code (PositionCode),
    INDEX idx_position_active (IsActive)
) ENGINE=InnoDB;

-- ================================================================
-- 2. BẢNG DEPARTMENT - Phòng ban
-- ================================================================
CREATE TABLE Department (
    DepartmentID INT AUTO_INCREMENT PRIMARY KEY,
    DepartmentCode VARCHAR(20) NOT NULL UNIQUE,
    DepartmentName VARCHAR(100) NOT NULL,
    Description TEXT,
    ManagerID INT NULL COMMENT 'FK đến Employee - sẽ thêm constraint sau',
    Location VARCHAR(200) NULL COMMENT 'Địa điểm văn phòng',
    CurrentHeadcount INT DEFAULT 0,
    MaxHeadcount INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_department_code (DepartmentCode)
) ENGINE=InnoDB;

-- ================================================================
-- 3. BẢNG EMPLOYEE - Nhân viên
-- ================================================================
CREATE TABLE Employee (
    EmployeeID INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeCode VARCHAR(20) NOT NULL UNIQUE,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Phone VARCHAR(20),
    DepartmentID INT NOT NULL,
    PositionID INT NOT NULL,
    HireDate DATE NOT NULL,
    Status VARCHAR(20) DEFAULT 'Active' COMMENT 'Active, Resigned, Terminated',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (DepartmentID) REFERENCES Department(DepartmentID),
	FOREIGN KEY (PositionID) REFERENCES `Position`(PositionID),
    INDEX idx_employee_code (EmployeeCode),
    INDEX idx_employee_email (Email),
    INDEX idx_employee_dept (DepartmentID),
    INDEX idx_employee_status (Status)
) ENGINE=InnoDB;

-- Thêm constraint cho Department.ManagerID
ALTER TABLE Department 
ADD CONSTRAINT fk_department_manager 
FOREIGN KEY (ManagerID) REFERENCES Employee(EmployeeID) ON DELETE SET NULL;

-- ================================================================
-- 4. BẢNG USER - Tài khoản đăng nhập
-- ================================================================
CREATE TABLE User (
    UserID INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Salt VARCHAR(255) NOT NULL,
    EmployeeID INT NOT NULL,
    Role VARCHAR(20) DEFAULT 'Staff' COMMENT 'Admin, HRManager, Staff',
    IsActive BOOLEAN DEFAULT TRUE,
    LastLogin DATETIME NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (EmployeeID) REFERENCES Employee(EmployeeID),
    INDEX idx_user_username (Username),
    INDEX idx_user_active (IsActive)
) ENGINE=InnoDB;

-- ================================================================
-- 5. BẢNG HEADCOUNT - Định biên nhân sự
-- ================================================================
CREATE TABLE Headcount (
    HeadcountID INT AUTO_INCREMENT PRIMARY KEY,
    DepartmentID INT NOT NULL,
    PositionID INT NOT NULL,
    ApprovedCount INT NOT NULL DEFAULT 0 COMMENT 'Số lượng được phê duyệt',
    FilledCount INT NOT NULL DEFAULT 0 COMMENT 'Số lượng đã tuyển',
    Year INT NOT NULL,
    ApprovedDate DATE NOT NULL,
    ApprovedBy INT NOT NULL COMMENT 'FK đến Employee',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (DepartmentID) REFERENCES Department(DepartmentID),
    FOREIGN KEY (PositionID) REFERENCES `Position`(PositionID),
    FOREIGN KEY (ApprovedBy) REFERENCES Employee(EmployeeID),
    UNIQUE KEY uk_headcount (DepartmentID, PositionID, Year),
    INDEX idx_headcount_year (Year)
) ENGINE=InnoDB;

-- ================================================================
-- 6. BẢNG RECRUITMENTCAMPAIGN - Đợt tuyển dụng
-- ================================================================
CREATE TABLE RecruitmentCampaign (
    CampaignID INT AUTO_INCREMENT PRIMARY KEY,
    CampaignCode VARCHAR(20) NOT NULL UNIQUE,
    CampaignName VARCHAR(200) NOT NULL,
    Description TEXT,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Budget DECIMAL(15,2) DEFAULT 0,
    Status VARCHAR(20) DEFAULT 'Planning' COMMENT 'Planning, Active, Completed, Cancelled',
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (CreatedBy) REFERENCES Employee(EmployeeID),
    INDEX idx_campaign_code (CampaignCode),
    INDEX idx_campaign_status (Status),
    INDEX idx_campaign_dates (StartDate, EndDate)
) ENGINE=InnoDB;

-- ================================================================
-- 7. BẢNG JOBPOSTING - Tin tuyển dụng
-- ================================================================
CREATE TABLE JobPosting (
    JobPostingID INT AUTO_INCREMENT PRIMARY KEY,
    JobCode VARCHAR(20) NOT NULL UNIQUE,
    JobTitle VARCHAR(200) NOT NULL,
    JobDescription TEXT COMMENT 'Mô tả tổng quan',
    JobResponsibilities TEXT COMMENT 'Trách nhiệm cụ thể',
    JobRequirements TEXT COMMENT 'Yêu cầu ứng viên',
    DepartmentID INT NOT NULL,
    PositionID INT NOT NULL,
    VacancyCount INT NOT NULL DEFAULT 1 COMMENT 'Số lượng cần tuyển',
    MinSalary DECIMAL(15,2) DEFAULT 0,
    MaxSalary DECIMAL(15,2) DEFAULT 0,
    WorkLocation VARCHAR(200),
    PostDate DATE NOT NULL,
    Deadline DATE NOT NULL,
    Status VARCHAR(20) DEFAULT 'Draft' COMMENT 'Draft, Active, Closed, Cancelled',
    IsHeadcountApproved BOOLEAN DEFAULT FALSE COMMENT 'Đã kiểm tra định biên',
    CampaignID INT,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (DepartmentID) REFERENCES Department(DepartmentID),
    FOREIGN KEY (PositionID) REFERENCES `Position`(PositionID),
    FOREIGN KEY (CreatedBy) REFERENCES Employee(EmployeeID),
    FOREIGN KEY (CampaignID) REFERENCES RecruitmentCampaign(CampaignID),
    INDEX idx_job_code (JobCode),
    INDEX idx_job_status (Status),
    INDEX idx_job_dept (DepartmentID),
    INDEX idx_job_position (PositionID),
    INDEX idx_job_deadline (Deadline)
) ENGINE=InnoDB;

-- ================================================================
-- 8. BẢNG CANDIDATE - Hồ sơ ứng viên
-- ================================================================
CREATE TABLE Candidate (
    CandidateID INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Phone VARCHAR(20),
    DateOfBirth DATE,
    Gender VARCHAR(10) COMMENT 'Male, Female, Other',
    Address TEXT,
    Education VARCHAR(200) COMMENT 'Trình độ học vấn',
    WorkExperience TEXT COMMENT 'Kinh nghiệm làm việc',
    Skills TEXT COMMENT 'Kỹ năng',
    ResumeFileName VARCHAR(255),
    ResumeFilePath VARCHAR(500),
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_candidate_email (Email),
    INDEX idx_candidate_name (FullName),
    INDEX idx_candidate_created (CreatedDate)
) ENGINE=InnoDB;

-- ================================================================
-- 9. BẢNG APPLICATION - Đơn ứng tuyển (QUAN TRỌNG)
-- ================================================================
CREATE TABLE Application (
    ApplicationID INT AUTO_INCREMENT PRIMARY KEY,
    CandidateID INT NOT NULL,
    JobPostingID INT NOT NULL,
    ApplyDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    CoverLetter TEXT COMMENT 'Thư xin việc',
    Source VARCHAR(50) COMMENT 'Website, LinkedIn, Referral, JobFair, etc.',
    CurrentStatus VARCHAR(50) DEFAULT 'Nộp đơn' COMMENT 'Nộp đơn, Sơ tuyển, Phỏng vấn vòng 1, 2, 3, Đạt, Không đạt',
    Score DECIMAL(5,2) DEFAULT 0 COMMENT 'Điểm đánh giá 0-100',
    Notes TEXT COMMENT 'Ghi chú của HR',
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UpdatedBy INT NULL COMMENT 'FK đến Employee',
    FOREIGN KEY (CandidateID) REFERENCES Candidate(CandidateID),
    FOREIGN KEY (JobPostingID) REFERENCES JobPosting(JobPostingID),
    FOREIGN KEY (UpdatedBy) REFERENCES Employee(EmployeeID),
    INDEX idx_application_candidate (CandidateID),
    INDEX idx_application_job (JobPostingID),
    INDEX idx_application_status (CurrentStatus),
    INDEX idx_application_date (ApplyDate)
) ENGINE=InnoDB;


-- ================================================================
-- 10. BẢNG INTERVIEW - Phỏng vấn
-- ================================================================
CREATE TABLE Interview (
    InterviewID INT AUTO_INCREMENT PRIMARY KEY,
    ApplicationID INT NOT NULL,
    InterviewerID INT NOT NULL COMMENT 'FK đến Employee',
    InterviewDate DATETIME NOT NULL,
    InterviewType VARCHAR(50) COMMENT 'Online, Offline',
    InterviewRound VARCHAR(20) COMMENT 'Vòng 1, Vòng 2, Vòng 3',
    Location VARCHAR(200) COMMENT 'Địa điểm phỏng vấn',
    InterviewNotes TEXT,
    Score DECIMAL(5,2) DEFAULT 0 COMMENT 'Điểm phỏng vấn',
    Result VARCHAR(20) COMMENT 'Pass, Fail, Pending',
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (ApplicationID) REFERENCES Application(ApplicationID) ON DELETE CASCADE,
    FOREIGN KEY (InterviewerID) REFERENCES Employee(EmployeeID),
    INDEX idx_interview_application (ApplicationID),
    INDEX idx_interview_date (InterviewDate),
    INDEX idx_interview_interviewer (InterviewerID)
) ENGINE=InnoDB;

-- ================================================================
-- 11. BẢNG EMAILLOG
-- ================================================================
CREATE TABLE EmailLog (
    EmailLogID INT AUTO_INCREMENT PRIMARY KEY,
    ApplicationID INT NOT NULL,
    EmailType VARCHAR(50) COMMENT 'Confirmation, InterviewInvitation, PassNotification, FailNotification',
    RecipientEmail VARCHAR(100) NOT NULL,
    Subject VARCHAR(255),
    Body TEXT,
    SentDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsSent BOOLEAN DEFAULT FALSE,
    ErrorMessage TEXT,
    FOREIGN KEY (ApplicationID) REFERENCES Application(ApplicationID) ON DELETE CASCADE,
    INDEX idx_email_application (ApplicationID),
    INDEX idx_email_sent (IsSent),
    INDEX idx_email_date (SentDate)
) ENGINE=InnoDB;

-- ================================================================
-- DỮ LIỆU MẪU
-- ================================================================

-- Insert Positions
INSERT INTO Position (PositionCode, PositionName, Description, Level, MinSalary, MaxSalary, IsActive) VALUES
('DEV-JR', 'Junior Developer', 'Lập trình viên mới vào nghề', 1, 8000000, 15000000, TRUE),
('DEV-MID', 'Middle Developer', 'Lập trình viên có kinh nghiệm', 2, 15000000, 25000000, TRUE),
('DEV-SR', 'Senior Developer', 'Lập trình viên cao cấp', 3, 25000000, 40000000, TRUE),
('DEV-LEAD', 'Tech Lead', 'Trưởng nhóm kỹ thuật', 4, 35000000, 50000000, TRUE),
('BA', 'Business Analyst', 'Phân tích nghiệp vụ', 2, 12000000, 20000000, TRUE),
('QA', 'QA Engineer', 'Kiểm thử phần mềm', 2, 10000000, 18000000, TRUE),
('PM', 'Project Manager', 'Quản lý dự án', 4, 30000000, 50000000, TRUE),
('HR', 'HR Specialist', 'Chuyên viên nhân sự', 2, 10000000, 18000000, TRUE);

-- Insert Departments
INSERT INTO Department (DepartmentCode, DepartmentName, Description, CurrentHeadcount, MaxHeadcount) VALUES
('IT', 'Phòng Công nghệ thông tin', 'Phát triển và bảo trì phần mềm', 0, 50),
('HR', 'Phòng Nhân sự', 'Quản lý nhân sự và tuyển dụng', 0, 10),
('SALES', 'Phòng Kinh doanh', 'Phát triển thị trường và bán hàng', 0, 30),
('MKT', 'Phòng Marketing', 'Tiếp thị và truyền thông', 0, 15);

-- Insert Employees (tạo một vài nhân viên mẫu)
INSERT INTO Employee (EmployeeCode, FullName, Email, Phone, DepartmentID, PositionID, HireDate, Status) VALUES
('EMP001', 'Nguyễn Văn Admin', 'admin@company.com', '0901234567', 2, 8, '2020-01-01', 'Active'),
('EMP002', 'Trần Thị HR Manager', 'hrmanager@company.com', '0901234568', 2, 8, '2020-03-15', 'Active'),
('EMP003', 'Lê Văn Tech Lead', 'techlead@company.com', '0901234569', 1, 4, '2019-06-01', 'Active');

-- Update Department Managers
UPDATE Department SET ManagerID = 2 WHERE DepartmentID = 2; -- HR Manager
UPDATE Department SET ManagerID = 3 WHERE DepartmentID = 1; -- Tech Lead

-- Insert Users (password mặc định: "admin123" - sẽ hash trong code)
INSERT INTO User (Username, PasswordHash, Salt, EmployeeID, Role, IsActive) VALUES
('admin', 'c3yCBVgRmh9HWgoBS+KUo/Yz6uFe/9TavSumME6tcCQ=', '022WpcCvooRjdON0t1Vy6O1Zu+DQhNIRcG4qC6FVZok=', 1, 'Admin', TRUE),
('hrmanager', 'c3yCBVgRmh9HWgoBS+KUo/Yz6uFe/9TavSumME6tcCQ=', '022WpcCvooRjdON0t1Vy6O1Zu+DQhNIRcG4qC6FVZok=', 2, 'HRManager', TRUE),
('staff', 'c3yCBVgRmh9HWgoBS+KUo/Yz6uFe/9TavSumME6tcCQ=', '022WpcCvooRjdON0t1Vy6O1Zu+DQhNIRcG4qC6FVZok=', 3, 'Staff', TRUE);

-- Insert Headcount (định biên năm 2026)
INSERT INTO Headcount (DepartmentID, PositionID, ApprovedCount, FilledCount, Year, ApprovedDate, ApprovedBy) VALUES
(1, 1, 10, 0, 2026, '2026-01-01', 1), -- IT - Junior Dev
(1, 2, 15, 0, 2026, '2026-01-01', 1), -- IT - Middle Dev
(1, 3, 8, 0, 2026, '2026-01-01', 1),  -- IT - Senior Dev
(1, 4, 3, 0, 2026, '2026-01-01', 1),  -- IT - Tech Lead
(2, 8, 5, 0, 2026, '2026-01-01', 1);  -- HR - HR Specialist

-- Insert Sample Campaign
INSERT INTO RecruitmentCampaign (CampaignCode, CampaignName, Description, StartDate, EndDate, Status, CreatedBy) VALUES
('CAMP-2026-Q1', 'Tuyển dụng Q1/2026', 'Đợt tuyển dụng quý 1 năm 2026', '2026-01-01', '2026-03-31', 'Active', 1);

-- Insert Sample JobPosting
INSERT INTO JobPosting (JobCode, JobTitle, JobDescription, JobResponsibilities, JobRequirements, 
                        DepartmentID, PositionID, VacancyCount, MinSalary, MaxSalary, 
                        WorkLocation, PostDate, Deadline, Status, IsHeadcountApproved, CreatedBy) VALUES
('JOB-2026-001', 'Tuyển Middle C# Developer', 
 'Phát triển ứng dụng Windows Forms và Web API',
 '- Phát triển tính năng mới\n- Sửa lỗi và bảo trì code\n- Code review\n- Làm việc với team',
 '- 2-3 năm kinh nghiệm C#\n- Biết SQL Server/MySQL\n- Có kinh nghiệm Windows Forms\n- Tiếng Anh đọc hiểu tốt',
 1, 2, 3, 15000000, 25000000,
 'Hà Nội', '2026-01-10', '2026-02-28', 'Active', TRUE, 2);

-- Insert Sample Candidate
INSERT INTO Candidate (FullName, Email, Phone, DateOfBirth, Gender, Address, Education, WorkExperience, Skills) VALUES
('Nguyễn Văn A', 'nguyenvana@email.com', '0987654321', '1995-05-15', 'Male', 
 'Hà Nội', 'Đại học CNTT', '2', 
 'C#, SQL Server, Windows Forms, ASP.NET Core'),
('Trần Thị B', 'tranthib@email.com', '0987654322', '1996-08-20', 'Female',
 'Hà Nội', 'Đại học Bách Khoa', '3',
 'C#, MySQL, Entity Framework, React');


