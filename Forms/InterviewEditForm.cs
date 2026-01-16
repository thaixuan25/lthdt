using System;
using System.Linq;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Services;
using AppModel = LTHDT2.Models.Application;

namespace LTHDT2.Forms
{
    /// <summary>
    /// InterviewEditForm - Form thêm/sửa lịch phỏng vấn
    /// Kế thừa BaseEditForm<Interview>
    /// Sử dụng InterviewService thay vì gọi trực tiếp Repository
    /// </summary>
    public class InterviewEditForm : BaseEditForm<Interview>
    {
        private readonly InterviewService _interviewService;
        private readonly ApplicationRepository _applicationRepository;
        private readonly EmployeeRepository _employeeRepository;

        private ComboBox cmbApplication = null!;
        private ComboBox cmbInterviewer = null!;
        private DateTimePicker dtpInterviewDate = null!;
        private DateTimePicker dtpInterviewTime = null!;
        private ComboBox cmbType = null!;
        private ComboBox cmbRound = null!;
        private TextBox txtLocation = null!;
        private TextBox txtNotes = null!;
        private NumericUpDown numScore = null!;
        private ComboBox cmbResult = null!;

        public InterviewEditForm() : base()
        {
            // Khởi tạo EmailService và InterviewService
            var emailService = new EmailService();
            _interviewService = new InterviewService(emailService);
            _applicationRepository = new ApplicationRepository();
            _employeeRepository = new EmployeeRepository();
        }

        public InterviewEditForm(Interview interview) : base(interview)
        {
            // Khởi tạo EmailService và InterviewService
            var emailService = new EmailService();
            _interviewService = new InterviewService(emailService);
            _applicationRepository = new ApplicationRepository();
            _employeeRepository = new EmployeeRepository();
        }

        protected override string GetEntityName()
        {
            return "Lịch phỏng vấn";
        }

        protected override void InitializeFormControls()
        {
            int startY = 20;
            int spacing = 45;
            int currentY = startY;

            // Application (Candidate)
            AddLabelAndComboBox("Đơn ứng tuyển:", ref cmbApplication, currentY, 400);
            LoadApplicationList();
            currentY += spacing;

            // Interviewer
            AddLabelAndComboBox("Người phỏng vấn:", ref cmbInterviewer, currentY, 350);
            LoadInterviewerList();
            currentY += spacing;

            // Interview Date
            AddLabelAndDateTimePicker("Ngày phỏng vấn:", ref dtpInterviewDate, currentY);
            dtpInterviewDate.Value = DateTime.Today.AddDays(7); // Default: 7 ngày sau
            currentY += spacing;

            // Interview Time
            var lblTime = new Label
            {
                Text = "Giờ phỏng vấn:",
                Location = new System.Drawing.Point(20, currentY + 3),
                Size = new System.Drawing.Size(150, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F)
            };
            dtpInterviewTime = new DateTimePicker
            {
                Location = new System.Drawing.Point(180, currentY),
                Size = new System.Drawing.Size(200, 25),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Today.AddHours(9) // Default: 9:00 AM
            };
            mainPanel.Controls.Add(lblTime);
            mainPanel.Controls.Add(dtpInterviewTime);
            currentY += spacing;

            // Interview Type
            AddLabelAndComboBox("Loại PV:", ref cmbType, currentY, 200);
            cmbType.Items.AddRange(new[] { "Online", "Offline" });
            cmbType.SelectedIndex = 0;
            currentY += spacing;

            // Interview Round
            AddLabelAndComboBox("Vòng PV:", ref cmbRound, currentY, 200);
            cmbRound.Items.AddRange(new[] { "Vòng 1", "Vòng 2", "Vòng 3", "Vòng cuối" });
            cmbRound.SelectedIndex = 0;
            currentY += spacing;

            // Location
            AddLabelAndTextBox("Địa điểm:", ref txtLocation, currentY);
            txtLocation.MaxLength = 200;
            txtLocation.PlaceholderText = "VD: Phòng họp A, Tầng 5 hoặc Link Google Meet";
            currentY += spacing;

            // Notes
            AddLabelAndTextBox("Ghi chú:", ref txtNotes, currentY, true, 80);
            txtNotes.MaxLength = 500;
            txtNotes.PlaceholderText = "Ghi chú về lịch phỏng vấn, yêu cầu chuẩn bị...";
            currentY += 80 + 10;

            // Score (for edit mode)
            AddLabelAndNumericUpDown("Điểm (0-100):", ref numScore, currentY, 0, 100, 150);
            numScore.DecimalPlaces = 2;
            numScore.Increment = 0.5M;
            numScore.Value = 0;
            currentY += spacing;

            // Result (for edit mode)
            AddLabelAndComboBox("Kết quả:", ref cmbResult, currentY, 200);
            cmbResult.Items.AddRange(new[] { "Pending", "Pass", "Fail" });
            cmbResult.SelectedIndex = 0;
            cmbResult.SelectedIndexChanged += CmbResult_SelectedIndexChanged;
            currentY += spacing;
        }

        private void LoadApplicationList()
        {
            try
            {
                var applications = _applicationRepository.GetAll()
                    .Where(a => a.CurrentStatus != "Đạt" && a.CurrentStatus != "Không đạt")
                    .OrderByDescending(a => a.ApplyDate)
                    .ToList();

                cmbApplication.Items.Clear();
                cmbApplication.Items.Add(new { Id = 0, Display = "-- Chọn đơn ứng tuyển --" });
                
                foreach (var app in applications)
                {
                    var candidateName = app.Candidate?.FullName ?? "N/A";
                    var jobTitle = app.JobPosting?.JobTitle ?? "N/A";
                    var display = $"[{app.Id}] {candidateName} - {jobTitle}";
                    cmbApplication.Items.Add(new { Id = app.Id, Display = display });
                }

                cmbApplication.DisplayMember = "Display";
                cmbApplication.ValueMember = "Id";
                cmbApplication.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load đơn ứng tuyển: {ex.Message}");
            }
        }

        private void LoadInterviewerList()
        {
            try
            {
                var employees = _employeeRepository.GetAll()
                    .Where(e => e.Status == "Active")
                    .OrderBy(e => e.FullName)
                    .ToList();

                cmbInterviewer.Items.Clear();
                cmbInterviewer.Items.Add(new { Id = 0, Display = "-- Chọn người phỏng vấn --" });
                
                foreach (var emp in employees)
                {
                    cmbInterviewer.Items.Add(new { Id = emp.Id, Display = $"{emp.EmployeeCode} - {emp.FullName}" });
                }

                cmbInterviewer.DisplayMember = "Display";
                cmbInterviewer.ValueMember = "Id";
                cmbInterviewer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load nhân viên: {ex.Message}");
            }
        }

        protected override void LoadEntity()
        {
            try
            {
                // Select application
                for (int i = 0; i < cmbApplication.Items.Count; i++)
                {
                    dynamic item = cmbApplication.Items[i]!;
                    if (item.Id == Entity.ApplicationId)
                    {
                        cmbApplication.SelectedIndex = i;
                        break;
                    }
                }
                cmbApplication.Enabled = false; // Don't allow changing application in edit mode

                // Select interviewer
                for (int i = 0; i < cmbInterviewer.Items.Count; i++)
                {
                    dynamic item = cmbInterviewer.Items[i]!;
                    if (item.Id == Entity.InterviewerId)
                    {
                        cmbInterviewer.SelectedIndex = i;
                        break;
                    }
                }

                dtpInterviewDate.Value = Entity.InterviewDate.Date;
                dtpInterviewTime.Value = Entity.InterviewDate;
                
                if (!string.IsNullOrEmpty(Entity.InterviewType))
                {
                    int typeIndex = cmbType.Items.IndexOf(Entity.InterviewType);
                    if (typeIndex >= 0) cmbType.SelectedIndex = typeIndex;
                }
                // Loại PV không được cập nhật khi đã có dữ liệu (edit mode)
                if (IsEditMode)
                {
                    cmbType.Enabled = false;
                    cmbRound.Enabled = false;
                }

                if (!string.IsNullOrEmpty(Entity.InterviewRound))
                {
                    int roundIndex = cmbRound.Items.IndexOf(Entity.InterviewRound);
                    if (roundIndex >= 0) cmbRound.SelectedIndex = roundIndex;
                }

                txtLocation.Text = Entity.Location ?? "";
                txtNotes.Text = Entity.InterviewNotes ?? "";
                numScore.Value = Entity.Score;

                if (!string.IsNullOrEmpty(Entity.Result))
                {
                    int resultIndex = cmbResult.Items.IndexOf(Entity.Result);
                    if (resultIndex >= 0) cmbResult.SelectedIndex = resultIndex;
                }

                // Áp dụng rules cho readonly fields
                ApplyReadOnlyRules();
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        protected override bool ValidateInput()
        {
            // Validate Application
            if (cmbApplication.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn đơn ứng tuyển!");
                cmbApplication.Focus();
                return false;
            }
            dynamic app = cmbApplication.SelectedItem!;
            if (app.Id <= 0)
            {
                ShowWarning("Vui lòng chọn đơn ứng tuyển!");
                cmbApplication.Focus();
                return false;
            }

            // Validate Interviewer
            if (cmbInterviewer.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn người phỏng vấn!");
                cmbInterviewer.Focus();
                return false;
            }
            dynamic interviewer = cmbInterviewer.SelectedItem!;
            if (interviewer.Id <= 0)
            {
                ShowWarning("Vui lòng chọn người phỏng vấn!");
                cmbInterviewer.Focus();
                return false;
            }

            // Validate Interview Date
            var interviewDateTime = dtpInterviewDate.Value.Date.Add(dtpInterviewTime.Value.TimeOfDay);
            if (interviewDateTime < DateTime.Now)
            {
                ShowWarning("Ngày giờ phỏng vấn phải trong tương lai!");
                dtpInterviewDate.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Áp dụng rules cho các trường readonly
        /// - Loại PV: không được cập nhật khi đã có dữ liệu (edit mode)
        /// - Điểm: không được cập nhật khi Result đã là Pass hoặc Fail (chỉ trong edit mode)
        /// - Kết quả: không được cập nhật khi đã là Pass hoặc Fail (chỉ trong edit mode)
        /// </summary>
        private void ApplyReadOnlyRules()
        {
            // Chỉ áp dụng rules trong edit mode
            if (!IsEditMode)
            {
                return;
            }

            // Kiểm tra Result hiện tại từ database (không phải từ control)
            string? savedResult = Entity.Result;
            bool isResultFinal = savedResult == "Pass" || savedResult == "Fail";

            // Disable tất cả các trường nếu Result đã được lưu là Pass hoặc Fail
            if (isResultFinal)
            {
                // Disable tất cả các controls
                cmbApplication.Enabled = false;
                cmbInterviewer.Enabled = false;
                dtpInterviewDate.Enabled = false;
                dtpInterviewTime.Enabled = false;
                cmbType.Enabled = false;
                cmbRound.Enabled = false;
                txtLocation.ReadOnly = true;
                txtLocation.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
                numScore.Enabled = false;
                numScore.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
                cmbResult.Enabled = false;
            }
            else
            {
                // Enable lại các controls (trừ những cái đã bị disable bởi rules khác)
                // cmbApplication vẫn disabled trong edit mode
                // cmbType vẫn disabled trong edit mode
                cmbInterviewer.Enabled = true;
                dtpInterviewDate.Enabled = true;
                dtpInterviewTime.Enabled = true;
                cmbRound.Enabled = true;
                txtLocation.ReadOnly = false;
                txtLocation.BackColor = System.Drawing.Color.White;
                txtNotes.ReadOnly = false;
                txtNotes.BackColor = System.Drawing.Color.White;
                numScore.Enabled = true;
                numScore.BackColor = System.Drawing.Color.White;
                cmbResult.Enabled = true;
            }
        }

        /// <summary>
        /// Event handler khi Result thay đổi
        /// </summary>
        private void CmbResult_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyReadOnlyRules();
        }

        protected override void SaveEntity()
        {
            try
            {
                // Map data from controls to entity
                dynamic selectedApp = cmbApplication.SelectedItem!;
                Entity.ApplicationId = (int)selectedApp.Id;
                
                dynamic selectedInterviewer = cmbInterviewer.SelectedItem!;
                Entity.InterviewerId = (int)selectedInterviewer.Id;
                
                // Combine date and time
                var interviewDateTime = dtpInterviewDate.Value.Date.Add(dtpInterviewTime.Value.TimeOfDay);
                Entity.InterviewDate = interviewDateTime;
                
                Entity.InterviewType = cmbType.SelectedItem?.ToString();
                Entity.InterviewRound = cmbRound.SelectedItem?.ToString();
                Entity.Location = string.IsNullOrWhiteSpace(txtLocation.Text) ? null : txtLocation.Text.Trim();
                Entity.InterviewNotes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
                Entity.Score = numScore.Value;
                Entity.Result = cmbResult.SelectedItem?.ToString();

                // Save to database thông qua Service layer
                InterviewResult result;
                if (IsEditMode)
                {
                    result = _interviewService.UpdateInterview(Entity);
                }
                else
                {
                    result = _interviewService.CreateInterview(Entity);
                    if (result.Success && result.Interview != null)
                    {
                        Entity.Id = result.Interview.Id;
                    }
                }

                if (!result.Success)
                {
                    throw new Exception(result.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lưu dữ liệu: {ex.Message}", ex);
            }
        }
    }
}

