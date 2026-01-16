using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.Services;
using LTHDT2.Utils;

namespace LTHDT2.Forms
{
    /// <summary>
    /// Main Form - Form chính với Sidebar menu
    /// Kế thừa BaseForm
    /// Áp dụng design theo PROMPT_UI_DESIGN_GUIDE.md
    /// </summary>
    public partial class MainForm : BaseForm
    {
        private Guna2Panel sidebarPanel = null!;
        private Guna2Panel headerPanel = null!;
        private Guna2Panel contentPanel = null!;
        private Guna2Panel welcomePanel = null!;
        private Label lblPageTitle = null!;
        private Label lblUserInfo = null!;
        private Label lblDateTime = null!;
        private System.Windows.Forms.Timer? statusTimer;
        
        private Form? activeChildForm = null;

        public MainForm()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo MainForm: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Hệ thống Quản lý Nhân sự";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.FormClosing += MainForm_FormClosing;
            this.BackColor = UITheme.BackgroundMain;

            // ====================
            // SIDEBAR (Left - 347px)
            // ====================
            sidebarPanel = UITheme.CreatePanel(withBorder: false);
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = UITheme.SidebarWidth;
            sidebarPanel.BorderRadius = 0;

            // Logo/Title Panel
            var logoPanel = UITheme.CreatePanel(withBorder: false);
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 120;
            logoPanel.BackColor = UITheme.SecondaryColor;

            var lblLogo = new Label
            {
                Text = "HR SYSTEM",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = UITheme.TextWhite,
                AutoSize = false,
                Size = new Size(UITheme.SidebarWidth - 40, 40),
                Location = new Point(20, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblLogoSub = new Label
            {
                Text = "Quản lý Nhân sự",
                Font = UITheme.BodyRegular,
                ForeColor = Color.FromArgb(220, 240, 250),
                AutoSize = false,
                Size = new Size(UITheme.SidebarWidth - 40, 25),
                Location = new Point(20, 75),
                TextAlign = ContentAlignment.MiddleCenter
            };

            logoPanel.Controls.Add(lblLogo);
            logoPanel.Controls.Add(lblLogoSub);

            // Menu Container (scrollable)
            var menuContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.BackgroundPanel,
                AutoScroll = true
            };

            // Menu Groups
            CreateMenuGroups(menuContainer);

            // Bottom User Info Panel
            var bottomPanel = UITheme.CreatePanel(withBorder: false);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 100;
            bottomPanel.BackColor = UITheme.BackgroundInput;
            bottomPanel.BorderRadius = 0;

            var lblUser = UITheme.CreateLabel($"👤 {CurrentUser?.Username}", UITheme.BodyBold);
            lblUser.Location = new Point(20, 15);
            lblUser.Size = new Size(UITheme.SidebarWidth - 40, 25);

            var lblRole = UITheme.CreateLabel($"Role: {CurrentUser?.Role}", UITheme.BodySmall);
            lblRole.ForeColor = UITheme.TextLight;
            lblRole.Location = new Point(20, 40);
            lblRole.Size = new Size(UITheme.SidebarWidth - 40, 20);

            var btnLogout = UITheme.CreateSecondaryButton("🚪 Đăng xuất", UITheme.SidebarWidth - 140, 35);
            btnLogout.Location = new Point(70, 60);
            btnLogout.Click += MenuLogout_Click;

            bottomPanel.Controls.Add(lblUser);
            bottomPanel.Controls.Add(lblRole);
            bottomPanel.Controls.Add(btnLogout);

            sidebarPanel.Controls.Add(menuContainer);
            sidebarPanel.Controls.Add(bottomPanel);
            sidebarPanel.Controls.Add(logoPanel);

            // ====================
            // HEADER (Top - 108px)
            // ====================
            headerPanel = UITheme.CreatePanel(withBorder: false);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = UITheme.HeaderHeight;
            headerPanel.BorderRadius = 0;
            headerPanel.Padding = new Padding(25, 20, 25, 20);

            lblPageTitle = UITheme.CreateTitleLabel("Trang chủ");
            lblPageTitle.Location = new Point(25, 25);
            lblPageTitle.Size = new Size(400, 60);

            lblUserInfo = UITheme.CreateLabel($"Xin chào, {CurrentUser?.Username}", UITheme.BodyRegular);
            lblUserInfo.Location = new Point(headerPanel.Width - 400, 25);
            lblUserInfo.Size = new Size(350, 25);
            lblUserInfo.TextAlign = ContentAlignment.MiddleRight;
            lblUserInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            lblDateTime = UITheme.CreateLabel(DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy"), UITheme.BodySmall);
            lblDateTime.ForeColor = UITheme.TextLight;
            lblDateTime.Location = new Point(headerPanel.Width - 400, 55);
            lblDateTime.Size = new Size(350, 20);
            lblDateTime.TextAlign = ContentAlignment.MiddleRight;
            lblDateTime.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            headerPanel.Controls.Add(lblPageTitle);
            headerPanel.Controls.Add(lblUserInfo);
            headerPanel.Controls.Add(lblDateTime);
            
            // Timer để cập nhật thời gian
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 1000; // 1 giây
            statusTimer.Tick += (s, e) => lblDateTime.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            statusTimer.Start();

            // ====================
            // CONTENT AREA (Fill)
            // ====================
            contentPanel = UITheme.CreatePanel(withBorder: false);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = UITheme.BackgroundMain;
            contentPanel.BorderRadius = 0;
            contentPanel.Padding = new Padding(15);

            // Welcome Panel
            CreateWelcomePanel();

            // Add controls to form in correct order
            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(sidebarPanel);

            // Make sure controls are visible
            if (sidebarPanel != null) sidebarPanel.Visible = true;
            if (headerPanel != null) headerPanel.Visible = true;
            if (contentPanel != null) contentPanel.Visible = true;
            if (welcomePanel != null) welcomePanel.Visible = true;

            // Setup permissions
            SetupPermissions();
        }

        private void CreateMenuGroups(Guna2Panel menuContainer)
        {
            int yPos = 10;

            // === HỆ THỐNG ===
            var lblSystem = CreateMenuGroupLabel("HỆ THỐNG");
            lblSystem.Location = new Point(0, yPos);
            menuContainer.Controls.Add(lblSystem);
            yPos += 30;

            var btnUserManagement = CreateMenuButton("👥 Quản lý User");
            btnUserManagement.Location = new Point(0, yPos);
            btnUserManagement.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnUserManagement.Click += (s, e) => OpenChildForm(new UserManagementForm(), "Quản lý User");
            menuContainer.Controls.Add(btnUserManagement);
            yPos += UITheme.ButtonHeightMenu;

            var btnChangePassword = CreateMenuButton("🔑 Đổi mật khẩu");
            btnChangePassword.Location = new Point(0, yPos);
            btnChangePassword.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnChangePassword.Click += MenuChangePassword_Click;
            menuContainer.Controls.Add(btnChangePassword);
            yPos += UITheme.ButtonHeightMenu + 10;

            // === NHÂN VIÊN ===
            var lblEmployee = CreateMenuGroupLabel("NHÂN VIÊN");
            lblEmployee.Location = new Point(0, yPos);
            menuContainer.Controls.Add(lblEmployee);
            yPos += 30;

            var btnPositions = CreateMenuButton("📋 Vị trí");
            btnPositions.Location = new Point(0, yPos);
            btnPositions.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnPositions.Click += (s, e) => OpenChildForm(new PositionListForm(), "Quản lý Vị trí");
            menuContainer.Controls.Add(btnPositions);
            yPos += UITheme.ButtonHeightMenu;

            var btnDepartments = CreateMenuButton("🏢 Phòng ban");
            btnDepartments.Location = new Point(0, yPos);
            btnDepartments.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnDepartments.Click += (s, e) => OpenChildForm(new DepartmentListForm(), "Quản lý Phòng ban");
            menuContainer.Controls.Add(btnDepartments);
            yPos += UITheme.ButtonHeightMenu;

            var btnEmployees = CreateMenuButton("👨‍💼 Nhân viên");
            btnEmployees.Location = new Point(0, yPos);
            btnEmployees.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnEmployees.Click += (s, e) => OpenChildForm(new EmployeeListForm(), "Quản lý Nhân viên");
            menuContainer.Controls.Add(btnEmployees);
            yPos += UITheme.ButtonHeightMenu;

            var btnHeadcount = CreateMenuButton("📊 Định biên");
            btnHeadcount.Location = new Point(0, yPos);
            btnHeadcount.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnHeadcount.Click += (s, e) => OpenChildForm(new HeadcountManagementForm(), "Quản lý Định biên");
            menuContainer.Controls.Add(btnHeadcount);
            yPos += UITheme.ButtonHeightMenu + 10;

            // === TUYỂN DỤNG ===
            var lblRecruitment = CreateMenuGroupLabel("TUYỂN DỤNG");
            lblRecruitment.Location = new Point(0, yPos);
            menuContainer.Controls.Add(lblRecruitment);
            yPos += 30;

            var btnCampaigns = CreateMenuButton("📅 Đợt tuyển dụng");
            btnCampaigns.Location = new Point(0, yPos);
            btnCampaigns.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnCampaigns.Click += (s, e) => OpenChildForm(new RecruitmentCampaignListForm(), "Đợt tuyển dụng");
            menuContainer.Controls.Add(btnCampaigns);
            yPos += UITheme.ButtonHeightMenu;

            var btnJobPostings = CreateMenuButton("📰 Tin tuyển dụng");
            btnJobPostings.Location = new Point(0, yPos);
            btnJobPostings.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnJobPostings.Click += (s, e) => OpenChildForm(new JobPostingListForm(), "Tin tuyển dụng");
            menuContainer.Controls.Add(btnJobPostings);
            yPos += UITheme.ButtonHeightMenu;

            var btnCandidates = CreateMenuButton("👔 Hồ sơ ứng viên");
            btnCandidates.Location = new Point(0, yPos);
            btnCandidates.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnCandidates.Click += (s, e) => OpenChildForm(new CandidateListForm(), "Hồ sơ ứng viên");
            menuContainer.Controls.Add(btnCandidates);
            yPos += UITheme.ButtonHeightMenu;

            var btnApplications = CreateMenuButton("📝 Đơn ứng tuyển");
            btnApplications.Location = new Point(0, yPos);
            btnApplications.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnApplications.Click += (s, e) => OpenChildForm(new ApplicationListForm(), "Đơn ứng tuyển");
            menuContainer.Controls.Add(btnApplications);
            yPos += UITheme.ButtonHeightMenu;

            var btnInterviews = CreateMenuButton("🎤 Lịch phỏng vấn");
            btnInterviews.Location = new Point(0, yPos);
            btnInterviews.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnInterviews.Click += (s, e) => OpenChildForm(new InterviewListForm(), "Lịch phỏng vấn");
            menuContainer.Controls.Add(btnInterviews);
            yPos += UITheme.ButtonHeightMenu + 10;

            // === BÁO CÁO ===
            var lblReport = CreateMenuGroupLabel("BÁO CÁO");
            lblReport.Location = new Point(0, yPos);
            menuContainer.Controls.Add(lblReport);
            yPos += 30;

            var btnReportRecruitment = CreateMenuButton("📈 Báo cáo Tuyển dụng");
            btnReportRecruitment.Location = new Point(0, yPos);
            btnReportRecruitment.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnReportRecruitment.Click += (s, e) => ShowRecruitmentReport();
            menuContainer.Controls.Add(btnReportRecruitment);
            yPos += UITheme.ButtonHeightMenu;

            var btnReportHeadcount = CreateMenuButton("📊 Báo cáo Định biên");
            btnReportHeadcount.Location = new Point(0, yPos);
            btnReportHeadcount.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnReportHeadcount.Click += (s, e) => ShowHeadcountReport();
            menuContainer.Controls.Add(btnReportHeadcount);
            yPos += UITheme.ButtonHeightMenu;

            var btnReportEfficiency = CreateMenuButton("⚡ Báo cáo Hiệu quả");
            btnReportEfficiency.Location = new Point(0, yPos);
            btnReportEfficiency.Size = new Size(UITheme.SidebarWidth, UITheme.ButtonHeightMenu);
            btnReportEfficiency.Click += (s, e) => ShowEfficiencyReport();
            menuContainer.Controls.Add(btnReportEfficiency);
            yPos += UITheme.ButtonHeightMenu;
        }

        /// <summary>
        /// Tạo menu button không dùng Dock để có thể set Location
        /// </summary>
        private Guna2Button CreateMenuButton(string text)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Height = UITheme.ButtonHeightMenu,
                BorderRadius = 0,
                FillColor = UITheme.BackgroundPanel,
                ForeColor = UITheme.TextSecondary,
                Font = UITheme.BodyRegular,
                TextAlign = HorizontalAlignment.Left,
                Padding = new Padding(13, 0, 0, 0),
                Cursor = Cursors.Hand,
                ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton
            };
            
            btn.CheckedState.FillColor = UITheme.PrimaryLight;
            btn.CheckedState.ForeColor = UITheme.PrimaryHover;
            btn.HoverState.FillColor = UITheme.BackgroundHover;
            
            return btn;
        }

        private Label CreateMenuGroupLabel(string text)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UITheme.TextLight,
                AutoSize = false,
                Size = new Size(UITheme.SidebarWidth, 25),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 5, 0, 0)
            };
            return lbl;
        }

        private void CreateWelcomePanel()
        {
            if (contentPanel == null)
            {
                return;
            }

            welcomePanel = UITheme.CreateCardPanel(12);
            welcomePanel.Size = new Size(800, 500);
            welcomePanel.BackColor = UITheme.BackgroundPanel;

            // Center in content panel
            CenterWelcomePanel();
            welcomePanel.Anchor = AnchorStyles.None;
            
            // Update position when content panel resizes
            contentPanel.Resize += (s, e) => CenterWelcomePanel();

            // Title
            var lblWelcomeTitle = new Label
            {
                Text = "HỆ THỐNG QUẢN LÝ NHÂN SỰ",
                Font = UITheme.LargeTitle,
                ForeColor = UITheme.SecondaryColor,
                AutoSize = false,
                Size = new Size(760, 50),
                Location = new Point(20, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            // Welcome message
            var lblWelcome = new Label
            {
                Text = $"Xin chào, {CurrentUser?.Username}!",
                Font = UITheme.Title,
                ForeColor = UITheme.TextPrimary,
                AutoSize = false,
                Size = new Size(760, 40),
                Location = new Point(20, 110),
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            // Role info
            var lblRole = new Label
            {
                Text = $"Vai trò: {CurrentUser?.Role}",
                Font = UITheme.SubTitle,
                ForeColor = UITheme.TextSecondary,
                AutoSize = false,
                Size = new Size(760, 40),
                Location = new Point(20, 160),
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            
            // Version label
            var lblVersion = new Label
            {
                Text = "Phiên bản 1.0 - © 2026",
                Font = UITheme.BodySmall,
                ForeColor = UITheme.TextLight,
                AutoSize = false,
                Size = new Size(760, 20),
                Location = new Point(20, 450),
                TextAlign = ContentAlignment.MiddleCenter
            };

            welcomePanel.Controls.Add(lblWelcomeTitle);
            welcomePanel.Controls.Add(lblWelcome);
            welcomePanel.Controls.Add(lblRole);
            welcomePanel.Controls.Add(lblVersion);

            if (contentPanel != null)
            {
                contentPanel.Controls.Add(welcomePanel);
                welcomePanel.BringToFront();
                // Center again after adding to panel
                CenterWelcomePanel();
            }
        }

        /// <summary>
        /// Căn giữa welcome panel trong content panel
        /// </summary>
        private void CenterWelcomePanel()
        {
            if (welcomePanel != null && contentPanel != null)
            {
                welcomePanel.Location = new Point(
                    Math.Max(0, (contentPanel.ClientSize.Width - welcomePanel.Width) / 2),
                    Math.Max(0, (contentPanel.ClientSize.Height - welcomePanel.Height) / 2)
                );
            }
        }

        protected override void BaseForm_Load(object? sender, EventArgs e)
        {
            base.BaseForm_Load(sender, e);
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                // Dừng timer
                if (statusTimer != null)
                {
                    statusTimer.Stop();
                    statusTimer.Dispose();
                }

                // Close active child form
                if (activeChildForm != null && !activeChildForm.IsDisposed)
                {
                    if (!Confirm("Có cửa sổ đang mở. Bạn có chắc muốn đóng?"))
                    {
                        e.Cancel = true;
                        return;
                    }
                    activeChildForm.Close();
                    activeChildForm.Dispose();
                }
                else if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (!Confirm("Bạn có chắc muốn thoát ứng dụng?"))
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't prevent closing
                System.Diagnostics.Debug.WriteLine($"Error closing form: {ex.Message}");
            }
        }

        private void SetupPermissions()
        {
            // Ẩn/hiện menu theo quyền
            // Có thể implement sau nếu cần
        }

        private void MenuChangePassword_Click(object? sender, EventArgs e)
        {
            var form = new ChangePasswordForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                // Đăng xuất sau khi đổi mật khẩu thành công
                MenuLogout_Click(sender, e);
            }
        }

        private void MenuLogout_Click(object? sender, EventArgs e)
        {
            if (Confirm("Bạn có chắc muốn đăng xuất?"))
            {
                var authService = new AuthenticationService();
                authService.Logout();
                
                this.Close();
                
                // Hiển thị LoginForm lại
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    var mainForm = new MainForm();
                    mainForm.Show();
                }
            }
        }

        private void OpenChildForm(Form childForm, string pageTitle)
        {
            try
            {
                // Hide welcome panel
                if (welcomePanel != null && welcomePanel.Visible)
                {
                    welcomePanel.Visible = false;
                }

                // Close previous child form
                if (activeChildForm != null && !activeChildForm.IsDisposed)
                {
                    try
                    {
                        activeChildForm.FormClosed -= ChildForm_FormClosed;
                        activeChildForm.Close();
                        activeChildForm.Dispose();
                    }
                    catch { }
                }

                if (contentPanel == null)
                {
                    ShowError("Content panel chưa được khởi tạo!");
                    return;
                }

                // Setup child form properties BEFORE adding to parent
                childForm.TopLevel = false;
                childForm.FormBorderStyle = FormBorderStyle.None;
                childForm.Dock = DockStyle.Fill;
                childForm.Visible = false; // Hide initially
                
                // Update page title first
                if (lblPageTitle != null)
                {
                    lblPageTitle.Text = pageTitle;
                }
                
                // Store reference BEFORE adding to panel
                activeChildForm = childForm;
                
                // Attach FormClosed event handler BEFORE showing
                childForm.FormClosed += ChildForm_FormClosed;
                
                // Add to content panel
                contentPanel.Controls.Add(childForm);
                childForm.BringToFront();
                
                // Show form AFTER it's been added and configured
                // Use BeginInvoke to ensure form is fully initialized
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (childForm != null && !childForm.IsDisposed)
                        {
                            childForm.Visible = true;
                            childForm.Show();
                            childForm.BringToFront();
                            
                            // Force layout update
                            childForm.PerformLayout();
                            childForm.Update();
                            
                            // Try to focus, but don't fail if it doesn't work
                            try
                            {
                                childForm.Focus();
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Lỗi khi hiển thị form: {ex.Message}\n\n{ex.StackTrace}");
                    }
                }));
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi mở form: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            }
        }

        private void ChildForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            try
            {
                if (welcomePanel != null)
                {
                    welcomePanel.Visible = true;
                    welcomePanel.BringToFront();
                }
                if (lblPageTitle != null)
                {
                    lblPageTitle.Text = "Trang chủ";
                }
                activeChildForm = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ChildForm_FormClosed: {ex.Message}");
            }
        }

        private void ShowRecruitmentReport()
        {
            try
            {
                var reportService = new ReportService();
                var fromDate = DateTime.Now.AddMonths(-1);
                var toDate = DateTime.Now;

                var report = reportService.GetOverviewReport(fromDate, toDate);

                var message = $"=== BÁO CÁO TUYỂN DỤNG TỔNG QUAN ===\n\n" +
                             $"Thời gian: {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}\n\n" +
                             $"Tin tuyển dụng:\n" +
                             $"  - Tổng số: {report.TotalJobPostings}\n" +
                             $"  - Đang mở: {report.ActiveJobPostings}\n" +
                             $"  - Vị trí cần tuyển: {report.TotalPositionsRequired}\n\n" +
                             $"Ứng viên:\n" +
                             $"  - Tổng số đơn: {report.TotalApplications}\n" +
                             $"  - Số ứng viên riêng biệt: {report.UniqueApplicants}\n" +
                             $"  - Đã đạt: {report.PassedApplications}\n" +
                             $"  - Không đạt: {report.RejectedApplications}\n\n" +
                             $"Phỏng vấn:\n" +
                             $"  - Tổng số: {report.TotalInterviews}\n" +
                             $"  - Đã hoàn thành: {report.CompletedInterviews}\n\n" +
                             $"Hiệu quả:\n" +
                             $"  - Trung bình đơn/tin: {report.AverageApplicationsPerJob:F1}";

                MessageBox.Show(message, "Báo cáo Tuyển dụng", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi tạo báo cáo: {ex.Message}");
            }
        }

        private void ShowHeadcountReport()
        {
            try
            {
                var reportService = new ReportService();
                var year = DateTime.Now.Year;

                var reports = reportService.GetDepartmentReport(year);

                var message = $"=== BÁO CÁO ĐỊNH BIÊN {year} ===\n\n";

                if (reports != null && reports.Any())
                {
                    int totalApproved = reports.Sum(r => r.ApprovedHeadcount);
                    int totalFilled = reports.Sum(r => r.FilledHeadcount);
                    int totalRemaining = reports.Sum(r => r.RemainingHeadcount);
                    double overallFillRate = totalApproved > 0 ? (double)totalFilled / totalApproved * 100 : 0;

                    message += $"Tổng quan:\n" +
                             $"  - Tổng số phòng ban: {reports.Count}\n" +
                             $"  - Tổng định biên: {totalApproved}\n" +
                             $"  - Đã tuyển: {totalFilled}\n" +
                             $"  - Còn thiếu: {totalRemaining}\n" +
                             $"  - Tỷ lệ lấp đầy: {overallFillRate:F1}%\n\n" +
                             $"Chi tiết từng phòng ban:\n";

                    foreach (var dept in reports)
                    {
                        message += $"\n{dept.DepartmentName}:\n" +
                                  $"  Định biên: {dept.ApprovedHeadcount}, Đã tuyển: {dept.FilledHeadcount}, " +
                                  $"Còn thiếu: {dept.RemainingHeadcount} (Tỷ lệ: {dept.FillRate:F1}%)\n";
                    }
                }
                else
                {
                    message += "Không có dữ liệu định biên cho năm này.";
                }

                MessageBox.Show(message, "Báo cáo Định biên", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi tạo báo cáo: {ex.Message}");
            }
        }

        private void ShowEfficiencyReport()
        {
            try
            {
                var reportService = new ReportService();
                var fromDate = DateTime.Now.AddMonths(-3);
                var toDate = DateTime.Now;

                var report = reportService.GetEfficiencyReport(fromDate, toDate);

                var message = $"=== BÁO CÁO HIỆU QUẢ TUYỂN DỤNG ===\n\n" +
                             $"Thời gian: {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}\n\n" +
                             $"Số lượng:\n" +
                             $"  - Tổng số đơn ứng tuyển: {report.TotalApplications}\n" +
                             $"  - Đơn/ngày: {report.ApplicationsPerDay:F1}\n" +
                             $"  - Phỏng vấn/ngày: {report.InterviewsPerDay:F1}\n\n" +
                             $"Tỷ lệ:\n" +
                             $"  - Tỷ lệ đến phỏng vấn: {report.ApplicationToInterviewRate:F1}%\n" +
                             $"  - Tỷ lệ đạt phỏng vấn: {report.InterviewToOfferRate:F1}%\n" +
                             $"  - Tỷ lệ chuyển đổi tổng: {report.OverallConversionRate:F1}%\n\n" +
                             $"Thời gian:\n" +
                             $"  - Thời gian tuyển trung bình: {report.AverageTimeToHire:F1} ngày\n\n" +
                             $"Chất lượng:\n" +
                             $"  - Điểm PV trung bình: {report.AverageInterviewScore:F1}/100";

                MessageBox.Show(message, "Báo cáo Hiệu quả", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi tạo báo cáo: {ex.Message}");
            }
        }
    }
}
