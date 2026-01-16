using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Forms
{
    /// <summary>
    /// BaseForm - Form cha cho tất cả forms trong hệ thống
    /// Áp dụng OOP: Inheritance, Encapsulation
    /// Cung cấp các chức năng chung: Authentication, Helpers, Permission checks
    /// </summary>
    public class BaseForm : Form
    {
        /// <summary>
        /// User hiện tại đang đăng nhập
        /// Protected - cho phép class con truy cập
        /// </summary>
        protected User? CurrentUser => SessionManager.CurrentUser;

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseForm()
        {
            // Subscribe events
            this.Load += BaseForm_Load;
            this.FormClosing += BaseForm_FormClosing;
            
            // Default settings
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
        }

        /// <summary>
        /// Virtual method - Xử lý khi form load
        /// Class con có thể override để customize
        /// </summary>
        protected virtual void BaseForm_Load(object? sender, EventArgs e)
        {
            try
            {
                // Kiểm tra đăng nhập (trừ LoginForm)
                if (CurrentUser == null && !(this is LoginForm))
                {
                    // Only show message if form is TopLevel (not a child form)
                    if (this.TopLevel)
                    {
                        MessageBox.Show(
                            "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại!",
                            "Chưa đăng nhập",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - child forms might not have full context
                System.Diagnostics.Debug.WriteLine($"Error in BaseForm_Load: {ex.Message}");
            }
        }

        /// <summary>
        /// Virtual method - Xử lý khi đóng form
        /// </summary>
        protected virtual void BaseForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Override trong class con nếu cần xử lý đặc biệt
        }

        #region Helper Methods

        /// <summary>
        /// Hiển thị thông báo lỗi
        /// </summary>
        protected void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        /// <summary>
        /// Hiển thị thông báo thành công
        /// </summary>
        protected void ShowSuccess(string message)
        {
            MessageBox.Show(
                message,
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Hiển thị cảnh báo
        /// </summary>
        protected void ShowWarning(string message)
        {
            MessageBox.Show(
                message,
                "Cảnh báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        /// <summary>
        /// Hiển thị hộp thoại xác nhận
        /// </summary>
        protected bool Confirm(string message)
        {
            return MessageBox.Show(
                message,
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            ) == DialogResult.Yes;
        }

        /// <summary>
        /// Hiển thị thông tin
        /// </summary>
        protected void ShowInfo(string message)
        {
            MessageBox.Show(
                message,
                "Thông tin",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        #endregion

        #region Permission Helpers

        /// <summary>
        /// Kiểm tra user có phải Admin không
        /// </summary>
        protected bool IsAdmin()
        {
            return CurrentUser?.IsAdmin() == true;
        }

        /// <summary>
        /// Kiểm tra user có phải HR Manager không (hoặc Admin)
        /// </summary>
        protected bool IsHRManager()
        {
            return CurrentUser?.IsHRManager() == true;
        }

        /// <summary>
        /// Kiểm tra user có phải Staff không
        /// </summary>
        protected bool IsStaff()
        {
            return CurrentUser?.Role == "Staff";
        }

        /// <summary>
        /// Kiểm tra user có role cụ thể không
        /// </summary>
        protected bool HasRole(string role)
        {
            return CurrentUser?.Role.Equals(role, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Kiểm tra quyền và hiển thị thông báo nếu không có quyền
        /// </summary>
        protected bool CheckPermission(bool hasPermission, string message = "Bạn không có quyền thực hiện chức năng này!")
        {
            if (!hasPermission)
            {
                ShowWarning(message);
                return false;
            }
            return true;
        }

        #endregion

        #region UI Helper Methods - Guna2 Components

        /// <summary>
        /// Tạo Guna2 Primary Button với style chuẩn
        /// </summary>
        protected Guna2Button CreateGuna2Button(string text, int x, int y, int width = 120, int height = 40, bool isPrimary = true)
        {
            var btn = isPrimary 
                ? UITheme.CreatePrimaryButton(text, width, height)
                : UITheme.CreateSecondaryButton(text, width, height);
            btn.Location = new Point(x, y);
            return btn;
        }

        /// <summary>
        /// Tạo Success Button
        /// </summary>
        protected Guna2Button CreateSuccessButton(string text, int x, int y, int width = 120, int height = 40)
        {
            var btn = UITheme.CreateSuccessButton(text, width, height);
            btn.Location = new Point(x, y);
            return btn;
        }

        /// <summary>
        /// Tạo Danger Button
        /// </summary>
        protected Guna2Button CreateDangerButton(string text, int x, int y, int width = 120, int height = 40)
        {
            var btn = UITheme.CreateDangerButton(text, width, height);
            btn.Location = new Point(x, y);
            return btn;
        }

        /// <summary>
        /// Tạo Guna2 TextBox với style chuẩn
        /// </summary>
        protected Guna2TextBox CreateGuna2TextBox(int x, int y, int width = 400, bool multiline = false, string placeholder = "")
        {
            var txt = UITheme.CreateTextBox(placeholder, multiline);
            txt.Location = new Point(x, y);
            txt.Width = width;
            return txt;
        }

        /// <summary>
        /// Tạo Guna2 ComboBox với style chuẩn
        /// </summary>
        protected Guna2ComboBox CreateGuna2ComboBox(int x, int y, int width = 400, int height = 40)
        {
            var cmb = UITheme.CreateComboBox(height);
            cmb.Location = new Point(x, y);
            cmb.Width = width;
            return cmb;
        }

        /// <summary>
        /// Tạo Guna2 DateTimePicker với style chuẩn
        /// </summary>
        protected Guna2DateTimePicker CreateGuna2DateTimePicker(int x, int y, int width = 200)
        {
            var dtp = UITheme.CreateDateTimePicker();
            dtp.Location = new Point(x, y);
            dtp.Width = width;
            return dtp;
        }

        /// <summary>
        /// Tạo Guna2 Panel với style chuẩn
        /// </summary>
        protected Guna2Panel CreateGuna2Panel(bool withBorder = false, int borderRadius = 8)
        {
            return UITheme.CreatePanel(withBorder, borderRadius);
        }

        /// <summary>
        /// Tạo Label với style chuẩn từ UITheme
        /// </summary>
        protected Label CreateThemedLabel(string text, int x, int y, int width = 150, bool isTitle = false)
        {
            var label = isTitle 
                ? UITheme.CreateTitleLabel(text)
                : UITheme.CreateLabel(text);
            label.Location = new Point(x, y);
            label.Size = new Size(width, 25);
            return label;
        }

        #endregion

        #region Legacy Helper Methods - Backward Compatibility

        /// <summary>
        /// Tạo button với style chuẩn (Legacy - backward compatibility)
        /// Internally uses Guna2Button
        /// </summary>
        protected Button CreateStyledButton(string text, int x, int y, int width = 100, int height = 35)
        {
            // For backward compatibility, create a regular Button but styled
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = UITheme.PrimaryColor,
                ForeColor = UITheme.TextWhite,
                FlatStyle = FlatStyle.Flat,
                Font = UITheme.BodyBold,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
        }

        /// <summary>
        /// Tạo label với style chuẩn (Legacy - backward compatibility)
        /// </summary>
        protected Label CreateLabel(string text, int x, int y, int width = 150)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = UITheme.BodyRegular,
                ForeColor = UITheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        /// <summary>
        /// Tạo textbox với style chuẩn (Legacy - backward compatibility)
        /// </summary>
        protected TextBox CreateTextBox(int x, int y, int width = 400, bool multiline = false)
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, multiline ? 100 : 25),
                Font = UITheme.BodyRegular,
                Multiline = multiline
            };

            if (multiline)
            {
                textBox.ScrollBars = ScrollBars.Vertical;
            }

            return textBox;
        }

        #endregion
    }
}

