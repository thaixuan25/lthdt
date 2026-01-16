using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.Services;
using LTHDT2.Utils;

namespace LTHDT2.Forms
{
    /// <summary>
    /// LoginForm - Form đăng nhập
    /// Kế thừa BaseForm (Inheritance)
    /// Áp dụng design theo PROMPT_UI_DESIGN_GUIDE.md
    /// </summary>
    public class LoginForm : BaseForm
    {
        private readonly IAuthenticationService _authService;
        
        private Guna2Panel headerPanel = null!;
        private Guna2Panel mainPanel = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Label lblUsername = null!;
        private Label lblPassword = null!;
        private Guna2TextBox txtUsername = null!;
        private Guna2TextBox txtPassword = null!;
        private Guna2Button btnLogin = null!;
        private Label lblVersion = null!;

        public LoginForm()
        {
            _authService = new AuthenticationService();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Đăng nhập - Hệ thống Quản lý Nhân sự";
            this.Size = new Size(533, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.BackgroundPanel;

            // Header Panel (Màu xanh #0F9BDC - 277px)
            headerPanel = new Guna2Panel
            {
                BackColor = UITheme.SecondaryColor,
                Dock = DockStyle.Top,
                Height = 277
            };

            // Title
            lblTitle = new Label
            {
                Text = "HỆ THỐNG QUẢN LÝ NHÂN SỰ",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = UITheme.TextWhite,
                AutoSize = false,
                Size = new Size(470, 50),
                Location = new Point(32, 79),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle
            lblSubtitle = new Label
            {
                Text = "Vui lòng đăng nhập để tiếp tục",
                Font = UITheme.BodyRegular,
                ForeColor = Color.FromArgb(230, 240, 250),
                AutoSize = false,
                Size = new Size(470, 30),
                Location = new Point(32, 135),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);

            // Main Content Panel
            mainPanel = UITheme.CreatePanel(withBorder: false);
            mainPanel.Location = new Point(53, 320);
            mainPanel.Size = new Size(427, 340);

            // Username Label
            lblUsername = UITheme.CreateLabel("Tên đăng nhập:", UITheme.BodyBold);
            lblUsername.Location = new Point(0, 0);
            lblUsername.Size = new Size(427, 25);

            // Username TextBox
            txtUsername = UITheme.CreateTextBox("Nhập tên đăng nhập...");
            txtUsername.Location = new Point(0, 30);
            txtUsername.Size = new Size(427, UITheme.InputHeight);

            // Password Label
            lblPassword = UITheme.CreateLabel("Mật khẩu:", UITheme.BodyBold);
            lblPassword.Location = new Point(0, 100);
            lblPassword.Size = new Size(427, 25);

            // Password TextBox
            txtPassword = UITheme.CreateTextBox("Nhập mật khẩu...");
            txtPassword.Location = new Point(0, 130);
            txtPassword.Size = new Size(427, UITheme.InputHeight);
            txtPassword.PasswordChar = '●';

            // Login Button
            btnLogin = UITheme.CreatePrimaryButton("🔐 Đăng nhập", 427, UITheme.ButtonHeightLarge);
            btnLogin.Location = new Point(0, 210);
            btnLogin.Click += BtnLogin_Click;

            mainPanel.Controls.Add(lblUsername);
            mainPanel.Controls.Add(txtUsername);
            mainPanel.Controls.Add(lblPassword);
            mainPanel.Controls.Add(txtPassword);
            mainPanel.Controls.Add(btnLogin);

            // Version Label
            lblVersion = new Label
            {
                Text = "Phiên bản 1.0 - © 2026",
                Font = UITheme.BodyTiny,
                ForeColor = UITheme.TextLight,
                AutoSize = false,
                Size = new Size(533, 20),
                Location = new Point(0, 670),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add controls to form
            this.Controls.Add(lblVersion);
            this.Controls.Add(mainPanel);
            this.Controls.Add(headerPanel);

            // Enter key handling
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnLogin_Click(s, e);
                }
            };

            txtUsername.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    txtPassword.Focus();
                }
            };

            // Focus on username when form loads
            this.Shown += (s, e) => txtUsername.Focus();

            // Add close button
            var btnClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = UITheme.TextWhite,
                AutoSize = false,
                Size = new Size(30, 30),
                Location = new Point(490, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(200, 15, 155, 220);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
            headerPanel.Controls.Add(btnClose);

            // Make form draggable
            bool isDragging = false;
            Point lastCursor = Point.Empty;
            Point lastForm = Point.Empty;

            MouseEventHandler headerMouseDown = (s, e) =>
            {
                isDragging = true;
                lastCursor = Cursor.Position;
                lastForm = this.Location;
            };

            MouseEventHandler headerMouseMove = (s, e) =>
            {
                if (isDragging)
                {
                    Point currentCursor = Cursor.Position;
                    Point offset = new Point(currentCursor.X - lastCursor.X, currentCursor.Y - lastCursor.Y);
                    this.Location = new Point(lastForm.X + offset.X, lastForm.Y + offset.Y);
                }
            };

            MouseEventHandler headerMouseUp = (s, e) =>
            {
                isDragging = false;
            };

            headerPanel.MouseDown += headerMouseDown;
            headerPanel.MouseMove += headerMouseMove;
            headerPanel.MouseUp += headerMouseUp;
            lblTitle.MouseDown += headerMouseDown;
            lblTitle.MouseMove += headerMouseMove;
            lblTitle.MouseUp += headerMouseUp;
            lblSubtitle.MouseDown += headerMouseDown;
            lblSubtitle.MouseMove += headerMouseMove;
            lblSubtitle.MouseUp += headerMouseUp;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validate input
                var username = txtUsername.Text.Trim();
                var password = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(username))
                {
                    ShowWarning("Vui lòng nhập tên đăng nhập!");
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    ShowWarning("Vui lòng nhập mật khẩu!");
                    txtPassword.Focus();
                    return;
                }

                // Disable button to prevent double click
                btnLogin.Enabled = false;
                btnLogin.Text = "⏳ Đang đăng nhập...";
                Application.DoEvents();

                // Attempt login
                var user = _authService.Login(username, password);

                if (user != null)
                {
                    // Success
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // Failed
                    ShowError("Đăng nhập thất bại!\n\nTên đăng nhập hoặc mật khẩu không đúng.");
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi đăng nhập:\n{ex.Message}");
            }
            finally
            {
                // Re-enable button
                btnLogin.Enabled = true;
                btnLogin.Text = "🔐 Đăng nhập";
            }
        }

        /// <summary>
        /// Override - Không check authentication cho LoginForm
        /// </summary>
        protected override void BaseForm_Load(object? sender, EventArgs e)
        {
            // Không gọi base.BaseForm_Load để không check authentication
            // LoginForm là form đầu tiên, không cần check
        }
    }
}
