using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Services;
using LTHDT2.Utils;

namespace LTHDT2.Forms
{
    /// <summary>
    /// UserManagementForm - Quản lý User (Admin only)
    /// Kế thừa BaseForm
    /// Áp dụng design theo PROMPT_UI_DESIGN_GUIDE.md
    /// </summary>
    public class UserManagementForm : BaseForm
    {
        private readonly UserRepository _userRepository;
        private readonly EmployeeRepository _employeeRepository;
        private readonly IAuthenticationService _authService;

        private Guna2DataGridView dgvUsers = null!;
        private Guna2Panel toolbarPanel = null!;
        private Guna2Button btnCreateUser = null!;
        private Guna2Button btnResetPassword = null!;
        private Guna2Button btnToggleActive = null!;
        private Guna2Button btnRefresh = null!;

        private List<User> users = new List<User>();

        public UserManagementForm()
        {
            _userRepository = new UserRepository();
            _employeeRepository = new EmployeeRepository();
            _authService = new AuthenticationService();
            
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Quản lý User (Admin)";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.BackgroundMain;

            // Toolbar
            toolbarPanel = UITheme.CreatePanel(withBorder: false);
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Height = 70;
            toolbarPanel.BackColor = UITheme.BackgroundMain;
            toolbarPanel.Padding = new Padding(15, 15, 15, 15);

            btnCreateUser = UITheme.CreatePrimaryButton("➕ Tạo User", 130, UITheme.ButtonHeight);
            btnCreateUser.Location = new Point(15, 15);
            btnCreateUser.Click += BtnCreateUser_Click;

            btnResetPassword = UITheme.CreatePrimaryButton("🔑 Reset Password", 160, UITheme.ButtonHeight);
            btnResetPassword.FillColor = UITheme.WarningColor;
            btnResetPassword.HoverState.FillColor = Color.FromArgb(230, 126, 34);
            btnResetPassword.Location = new Point(155, 15);
            btnResetPassword.Click += BtnResetPassword_Click;

            btnToggleActive = UITheme.CreatePrimaryButton("🔓 Khóa/Mở", 130, UITheme.ButtonHeight);
            btnToggleActive.FillColor = Color.FromArgb(142, 68, 173);
            btnToggleActive.HoverState.FillColor = Color.FromArgb(122, 48, 153);
            btnToggleActive.Location = new Point(325, 15);
            btnToggleActive.Click += BtnToggleActive_Click;

            btnRefresh = UITheme.CreateSuccessButton("🔄 Làm mới", 130, UITheme.ButtonHeight);
            btnRefresh.Location = new Point(465, 15);
            btnRefresh.Click += (s, e) => LoadUsers();

            toolbarPanel.Controls.Add(btnCreateUser);
            toolbarPanel.Controls.Add(btnResetPassword);
            toolbarPanel.Controls.Add(btnToggleActive);
            toolbarPanel.Controls.Add(btnRefresh);

            // DataGridView Container Panel
            var gridPanel = UITheme.CreatePanel(withBorder: false);
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.Padding = new Padding(15, 10, 15, 15);
            gridPanel.BackColor = UITheme.BackgroundMain;

            // DataGridView
            dgvUsers = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            UITheme.ApplyDataGridViewStyle(dgvUsers);

            SetupDataGridView();
            gridPanel.Controls.Add(dgvUsers);

            // Add to form
            this.Controls.Add(gridPanel);
            this.Controls.Add(toolbarPanel);
        }

        private void SetupDataGridView()
        {
            dgvUsers.AutoGenerateColumns = false;
            dgvUsers.Columns.Clear();

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "ID",
                Name = "Id",
                Width = 60,
                Visible = false
            });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Username",
                HeaderText = "Username",
                Name = "Username",
                Width = 150
            });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EmployeeName",
                HeaderText = "Nhân viên",
                Name = "EmployeeName",
                Width = 200
            });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Role",
                HeaderText = "Role",
                Name = "Role",
                Width = 100
            });
            dgvUsers.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsActive",
                HeaderText = "Active",
                Name = "IsActive",
                Width = 80
            });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LastLoginFormatted",
                HeaderText = "Last Login",
                Name = "LastLogin",
                Width = 150
            });
        }

        protected override void BaseForm_Load(object? sender, EventArgs e)
        {
            base.BaseForm_Load(sender, e);

            // Check permission
            if (!CheckPermission(IsAdmin(), "Chỉ Admin mới có quyền quản lý User!"))
            {
                this.Close();
                return;
            }

            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var allUsers = _userRepository.GetAll().ToList();
                
                // Map to display model
                users = allUsers;
                var displayData = allUsers.Select(u => new
                {
                    u.Id,
                    u.Username,
                    EmployeeName = u.Employee?.FullName ?? "N/A",
                    u.Role,
                    u.IsActive,
                    LastLoginFormatted = u.LastLogin.HasValue 
                        ? u.LastLogin.Value.ToString("dd/MM/yyyy HH:mm")
                        : "Chưa đăng nhập"
                }).ToList();

                dgvUsers.DataSource = displayData;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load danh sách user: {ex.Message}");
            }
        }

        private void BtnCreateUser_Click(object? sender, EventArgs e)
        {
            try
            {
                // Load employees chưa có user
                var allEmployees = _employeeRepository.GetAll().ToList();
                var employeesWithUsers = users.Select(u => u.EmployeeId).ToList();
                var availableEmployees = allEmployees
                    .Where(emp => !employeesWithUsers.Contains(emp.Id) && emp.IsActive())
                    .ToList();

                if (!availableEmployees.Any())
                {
                    ShowWarning("Không còn nhân viên nào chưa có user!\n\nTất cả nhân viên active đã được tạo user.");
                    return;
                }

                // Show create user dialog
                using (var dialog = new CreateUserDialog(availableEmployees))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var (employeeId, username, password, role) = dialog.GetUserData();

                        try
                        {
                            var userId = _authService.CreateUser(username, password, employeeId, role);
                            if (userId > 0)
                            {
                                ShowSuccess($"Tạo user '{username}' thành công!");
                                LoadUsers();
                            }
                            else
                            {
                                ShowError("Không thể tạo user. Vui lòng thử lại.");
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowError($"Lỗi tạo user: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
            }
        }

        private void BtnResetPassword_Click(object? sender, EventArgs e)
        {
            var selectedUser = GetSelectedUser();
            if (selectedUser == null)
            {
                ShowWarning("Vui lòng chọn một user để reset password!");
                return;
            }

            if (!Confirm($"Bạn có chắc muốn reset password cho user '{selectedUser.Username}'?\n\nPassword mới sẽ là: 123456"))
            {
                return;
            }

            try
            {
                // Reset password to default: 123456
                var (newHash, newSalt) = PasswordHasher.Hash("123456");
                selectedUser.PasswordHash = newHash;
                selectedUser.Salt = newSalt;

                if (_userRepository.Update(selectedUser))
                {
                    ShowSuccess($"Reset password thành công!\n\nUser: {selectedUser.Username}\nPassword mới: 123456\n\nVui lòng thông báo cho user đổi password sau khi đăng nhập.");
                }
                else
                {
                    ShowError("Không thể reset password. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi reset password: {ex.Message}");
            }
        }

        private void BtnToggleActive_Click(object? sender, EventArgs e)
        {
            var selectedUser = GetSelectedUser();
            if (selectedUser == null)
            {
                ShowWarning("Vui lòng chọn một user!");
                return;
            }

            // Không cho phép tự khóa chính mình
            if (selectedUser.Id == CurrentUser?.Id)
            {
                ShowWarning("Bạn không thể khóa chính mình!");
                return;
            }

            var action = selectedUser.IsActive ? "khóa" : "mở khóa";
            if (!Confirm($"Bạn có chắc muốn {action} user '{selectedUser.Username}'?"))
            {
                return;
            }

            try
            {
                selectedUser.IsActive = !selectedUser.IsActive;

                if (_userRepository.Update(selectedUser))
                {
                    ShowSuccess($"{(selectedUser.IsActive ? "Mở khóa" : "Khóa")} user thành công!");
                    LoadUsers();
                }
                else
                {
                    ShowError("Không thể cập nhật trạng thái. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
            }
        }

        private User? GetSelectedUser()
        {
            if (dgvUsers.CurrentRow != null && dgvUsers.CurrentRow.Index >= 0)
            {
                var index = dgvUsers.CurrentRow.Index;
                if (index < users.Count)
                {
                    return users[index];
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Dialog tạo user mới
    /// Áp dụng design theo PROMPT_UI_DESIGN_GUIDE.md
    /// </summary>
    internal class CreateUserDialog : Form
    {
        private Guna2Panel mainPanel = null!;
        private Guna2ComboBox cmbEmployee = null!;
        private Guna2TextBox txtUsername = null!;
        private Guna2TextBox txtPassword = null!;
        private Guna2ComboBox cmbRole = null!;
        private Guna2Button btnOK = null!;
        private Guna2Button btnCancel = null!;

        private List<Employee> employees;

        public CreateUserDialog(List<Employee> availableEmployees)
        {
            employees = availableEmployees;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Tạo User mới";
            this.Size = new Size(560, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.BackgroundMain;

            // Main Panel
            mainPanel = UITheme.CreateCardPanel(12);
            mainPanel.Size = new Size(560, 520);
            mainPanel.Location = new Point(0, 0);
            mainPanel.Padding = new Padding(30);

            int yPos = 30;

            // Employee
            var lblEmployee = UITheme.CreateLabel("Chọn nhân viên:", UITheme.BodyBold);
            lblEmployee.Location = new Point(30, yPos);
            lblEmployee.Size = new Size(450, 25);

            cmbEmployee = UITheme.CreateComboBox(40);
            cmbEmployee.Location = new Point(30, yPos + 30);
            cmbEmployee.Size = new Size(450, 40);
            cmbEmployee.DisplayMember = "DisplayText";
            cmbEmployee.ValueMember = "Id";
            foreach (var emp in employees)
            {
                cmbEmployee.Items.Add(new { Id = emp.Id, DisplayText = $"{emp.EmployeeCode} - {emp.FullName}" });
            }
            yPos += 90;

            // Username
            var lblUsername = UITheme.CreateLabel("Username:", UITheme.BodyBold);
            lblUsername.Location = new Point(30, yPos);
            lblUsername.Size = new Size(450, 25);

            txtUsername = UITheme.CreateTextBox("Nhập username (tối thiểu 3 ký tự)");
            txtUsername.Location = new Point(30, yPos + 30);
            txtUsername.Size = new Size(450, UITheme.InputHeight);
            yPos += 90;

            // Password
            var lblPassword = UITheme.CreateLabel("Password:", UITheme.BodyBold);
            lblPassword.Location = new Point(30, yPos);
            lblPassword.Size = new Size(450, 25);

            txtPassword = UITheme.CreateTextBox("Nhập password (tối thiểu 6 ký tự)");
            txtPassword.Location = new Point(30, yPos + 30);
            txtPassword.Size = new Size(450, UITheme.InputHeight);
            txtPassword.PasswordChar = '●';
            yPos += 90;

            // Role
            var lblRole = UITheme.CreateLabel("Role:", UITheme.BodyBold);
            lblRole.Location = new Point(30, yPos);
            lblRole.Size = new Size(450, 25);

            cmbRole = UITheme.CreateComboBox(40);
            cmbRole.Location = new Point(30, yPos + 30);
            cmbRole.Size = new Size(200, 40);
            cmbRole.Items.AddRange(new[] { "Admin", "HRManager", "Staff" });
            cmbRole.SelectedIndex = 2; // Default: Staff
            yPos += 90;

            // Buttons
            yPos += 20; // Add spacing before buttons
            btnOK = UITheme.CreatePrimaryButton("Tạo User", 150, UITheme.ButtonHeight);
            btnOK.Location = new Point(180, yPos);
            btnOK.Click += BtnOK_Click;

            btnCancel = UITheme.CreateSecondaryButton("Hủy", 150, UITheme.ButtonHeight);
            btnCancel.Location = new Point(340, yPos);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Add controls to main panel
            mainPanel.Controls.Add(lblEmployee);
            mainPanel.Controls.Add(cmbEmployee);
            mainPanel.Controls.Add(lblUsername);
            mainPanel.Controls.Add(txtUsername);
            mainPanel.Controls.Add(lblPassword);
            mainPanel.Controls.Add(txtPassword);
            mainPanel.Controls.Add(lblRole);
            mainPanel.Controls.Add(cmbRole);
            mainPanel.Controls.Add(btnOK);
            mainPanel.Controls.Add(btnCancel);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Enter key handling
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnOK_Click(s, e);
                }
            };
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            // Validate
            if (cmbEmployee.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn nhân viên!");
                return;
            }

            var username = txtUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                ShowWarning("Username phải có ít nhất 3 ký tự!");
                return;
            }

            var password = txtPassword.Text;
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ShowWarning("Password phải có ít nhất 6 ký tự!");
                return;
            }

            if (cmbRole.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn role!");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public (int employeeId, string username, string password, string role) GetUserData()
        {
            dynamic selectedEmployee = cmbEmployee.SelectedItem!;
            return (
                (int)selectedEmployee.Id,
                txtUsername.Text.Trim(),
                txtPassword.Text,
                cmbRole.SelectedItem!.ToString()!
            );
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
