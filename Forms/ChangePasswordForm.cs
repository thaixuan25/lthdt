using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.Services;
using LTHDT2.Utils;

namespace LTHDT2.Forms
{
    /// <summary>
    /// Form đổi mật khẩu
    /// Áp dụng design theo PROMPT_UI_DESIGN_GUIDE.md
    /// </summary>
    public class ChangePasswordForm : BaseForm
    {
        private readonly AuthenticationService _authService;

        private Guna2Panel mainPanel = null!;
        private Label lblTitle = null!;
        private Label lblCurrentPassword = null!;
        private Label lblNewPassword = null!;
        private Label lblConfirmPassword = null!;
        private Guna2TextBox txtCurrentPassword = null!;
        private Guna2TextBox txtNewPassword = null!;
        private Guna2TextBox txtConfirmPassword = null!;
        private Guna2Button btnSave = null!;
        private Guna2Button btnCancel = null!;
        private Guna2CheckBox chkShowPassword = null!;

        public ChangePasswordForm()
        {
            _authService = new AuthenticationService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Đổi mật khẩu";
            this.Size = new Size(530, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.BackgroundMain;

            // Main Panel
            mainPanel = UITheme.CreateCardPanel(12);
            mainPanel.Size = new Size(460, 500);
            mainPanel.Location = new Point(20, 20);
            mainPanel.Padding = new Padding(30);

            // Title
            lblTitle = UITheme.CreateTitleLabel("ĐỔI MẬT KHẨU");
            lblTitle.Location = new Point(30, 30);
            lblTitle.Size = new Size(400, 40);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.ForeColor = UITheme.SecondaryColor;

            int yPos = 90;

            // Current Password
            lblCurrentPassword = UITheme.CreateLabel("Mật khẩu hiện tại:", UITheme.BodyBold);
            lblCurrentPassword.Location = new Point(30, yPos);
            lblCurrentPassword.Size = new Size(400, 25);

            txtCurrentPassword = UITheme.CreateTextBox("Nhập mật khẩu hiện tại...");
            txtCurrentPassword.Location = new Point(30, yPos + 30);
            txtCurrentPassword.Size = new Size(400, UITheme.InputHeight);
            txtCurrentPassword.PasswordChar = '●';
            yPos += 90;

            // New Password
            lblNewPassword = UITheme.CreateLabel("Mật khẩu mới:", UITheme.BodyBold);
            lblNewPassword.Location = new Point(30, yPos);
            lblNewPassword.Size = new Size(400, 25);

            txtNewPassword = UITheme.CreateTextBox("Nhập mật khẩu mới (tối thiểu 6 ký tự)...");
            txtNewPassword.Location = new Point(30, yPos + 30);
            txtNewPassword.Size = new Size(400, UITheme.InputHeight);
            txtNewPassword.PasswordChar = '●';
            yPos += 90;

            // Confirm Password
            lblConfirmPassword = UITheme.CreateLabel("Xác nhận mật khẩu mới:", UITheme.BodyBold);
            lblConfirmPassword.Location = new Point(30, yPos);
            lblConfirmPassword.Size = new Size(400, 25);

            txtConfirmPassword = UITheme.CreateTextBox("Nhập lại mật khẩu mới...");
            txtConfirmPassword.Location = new Point(30, yPos + 30);
            txtConfirmPassword.Size = new Size(400, UITheme.InputHeight);
            txtConfirmPassword.PasswordChar = '●';
            yPos += 90;

            // Show Password
            chkShowPassword = new Guna2CheckBox
            {
                Text = "Hiển thị mật khẩu",
                Location = new Point(30, yPos),
                Size = new Size(200, 25),
                Font = UITheme.BodyRegular,
                CheckedState = { BorderColor = UITheme.BorderFocus, FillColor = UITheme.PrimaryColor }
            };
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;
            yPos += 50;

            // Buttons Panel
            var buttonPanel = new Guna2Panel
            {
                Location = new Point(30, yPos),
                Size = new Size(400, 50),
                BackColor = Color.Transparent
            };

            btnSave = UITheme.CreatePrimaryButton("💾 Lưu thay đổi", 150, UITheme.ButtonHeight);
            btnSave.Location = new Point(100, 10);
            btnSave.Click += BtnSave_Click;

            btnCancel = UITheme.CreateSecondaryButton("❌ Hủy", 150, UITheme.ButtonHeight);
            btnCancel.Location = new Point(260, 10);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.CancelButton = btnCancel;

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);

            // Add controls to main panel
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(lblCurrentPassword);
            mainPanel.Controls.Add(txtCurrentPassword);
            mainPanel.Controls.Add(lblNewPassword);
            mainPanel.Controls.Add(txtNewPassword);
            mainPanel.Controls.Add(lblConfirmPassword);
            mainPanel.Controls.Add(txtConfirmPassword);
            mainPanel.Controls.Add(chkShowPassword);
            mainPanel.Controls.Add(buttonPanel);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Enter key handling
            txtConfirmPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnSave_Click(s, e);
                }
            };
        }

        private void ChkShowPassword_CheckedChanged(object? sender, EventArgs e)
        {
            bool show = chkShowPassword.Checked;
            txtCurrentPassword.PasswordChar = show ? '\0' : '●';
            txtNewPassword.PasswordChar = show ? '\0' : '●';
            txtConfirmPassword.PasswordChar = show ? '\0' : '●';
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(txtCurrentPassword.Text))
                {
                    ShowWarning("Vui lòng nhập mật khẩu hiện tại!");
                    txtCurrentPassword.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
                {
                    ShowWarning("Vui lòng nhập mật khẩu mới!");
                    txtNewPassword.Focus();
                    return;
                }

                if (txtNewPassword.Text.Length < 6)
                {
                    ShowWarning("Mật khẩu mới phải có ít nhất 6 ký tự!");
                    txtNewPassword.Focus();
                    return;
                }

                if (txtNewPassword.Text != txtConfirmPassword.Text)
                {
                    ShowWarning("Xác nhận mật khẩu không khớp!");
                    txtConfirmPassword.Focus();
                    return;
                }

                if (txtNewPassword.Text == txtCurrentPassword.Text)
                {
                    ShowWarning("Mật khẩu mới phải khác mật khẩu hiện tại!");
                    txtNewPassword.Focus();
                    return;
                }

                // Disable button during processing
                btnSave.Enabled = false;
                btnSave.Text = "⏳ Đang xử lý...";

                // Change password
                var success = _authService.ChangePassword(
                    CurrentUser!.Id,
                    txtCurrentPassword.Text,
                    txtNewPassword.Text
                );

                if (success)
                {
                    ShowSuccess("Đổi mật khẩu thành công!\n\nVui lòng đăng nhập lại với mật khẩu mới.");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError("Đổi mật khẩu thất bại!\n\nVui lòng kiểm tra lại mật khẩu hiện tại.");
                    txtCurrentPassword.SelectAll();
                    txtCurrentPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
            }
            finally
            {
                btnSave.Enabled = true;
                btnSave.Text = "💾 Lưu thay đổi";
            }
        }
    }
}
