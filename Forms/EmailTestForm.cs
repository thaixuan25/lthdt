using System;
using System.Windows.Forms;
using LTHDT2.Services;

namespace LTHDT2.Forms
{
    /// <summary>
    /// Form đơn giản để test gửi email
    /// </summary>
    public partial class EmailTestForm : Form
    {
        private readonly EmailService _emailService;
        private TextBox txtEmail = null!;
        private Button btnTest = null!;
        private Label lblStatus = null!;

        public EmailTestForm()
        {
            _emailService = new EmailService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Test Gửi Email";
            this.Size = new System.Drawing.Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Email input
            var lblEmail = new Label
            {
                Text = "Email nhận test:",
                Location = new System.Drawing.Point(20, 30),
                Size = new System.Drawing.Size(150, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblEmail);

            txtEmail = new TextBox
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(440, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                PlaceholderText = "example@email.com"
            };
            this.Controls.Add(txtEmail);

            // Test button
            btnTest = new Button
            {
                Text = "Gửi Email Test",
                Location = new System.Drawing.Point(20, 110),
                Size = new System.Drawing.Size(150, 35),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(52, 152, 219),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest.FlatAppearance.BorderSize = 0;
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);

            // Status label
            lblStatus = new Label
            {
                Text = "",
                Location = new System.Drawing.Point(20, 160),
                Size = new System.Drawing.Size(440, 40),
                Font = new System.Drawing.Font("Segoe UI", 9F),
                ForeColor = System.Drawing.Color.FromArgb(44, 62, 80)
            };
            this.Controls.Add(lblStatus);
        }

        private void BtnTest_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validate email
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Vui lòng nhập email để test!", "Thông báo", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
                {
                    MessageBox.Show("Email không hợp lệ!", "Thông báo", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                // Disable button while sending
                btnTest.Enabled = false;
                btnTest.Text = "Đang gửi...";
                lblStatus.Text = "Đang gửi email test...";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(52, 152, 219);
                Application.DoEvents();

                // Send test email
                bool result = _emailService.TestSendEmail(txtEmail.Text.Trim());

                // Update UI
                btnTest.Enabled = true;
                btnTest.Text = "Gửi Email Test";

                if (result)
                {
                    lblStatus.Text = "✅ Email test đã được gửi thành công!\nVui lòng kiểm tra hộp thư đến (và cả thư mục Spam).";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(39, 174, 96);
                    MessageBox.Show("Email test đã được gửi thành công!\n\nVui lòng kiểm tra hộp thư đến của bạn.", 
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "❌ Gửi email thất bại!\nVui lòng kiểm tra cấu hình SMTP trong App.config.";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
                    MessageBox.Show("Gửi email thất bại!\n\nVui lòng kiểm tra:\n" +
                        "1. Cấu hình SMTP trong App.config\n" +
                        "2. Username và Password email\n" +
                        "3. Kết nối internet\n" +
                        "4. Xem log để biết chi tiết lỗi", 
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                btnTest.Enabled = true;
                btnTest.Text = "Gửi Email Test";
                lblStatus.Text = $"❌ Lỗi: {ex.Message}";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
                MessageBox.Show($"Lỗi khi gửi email test:\n\n{ex.Message}", 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
