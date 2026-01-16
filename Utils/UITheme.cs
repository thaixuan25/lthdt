using System;
using System.Drawing;
using Guna.UI2.WinForms;
using System.Windows.Forms;

namespace LTHDT2.Utils
{
    /// <summary>
    /// UITheme - Quản lý tập trung tất cả màu sắc, fonts và styling
    /// Theo PROMPT_UI_DESIGN_GUIDE.md
    /// </summary>
    public static class UITheme
    {
        #region Colors - Primary

        public static readonly Color PrimaryColor = Color.FromArgb(0, 158, 247);        // #009EF7
        public static readonly Color PrimaryHover = Color.FromArgb(0, 123, 255);        // #007BFF
        public static readonly Color PrimaryLight = Color.FromArgb(235, 245, 255);      // #EBF5FF

        #endregion

        #region Colors - Secondary

        public static readonly Color SecondaryColor = Color.FromArgb(15, 155, 220);     // #0F9BDC
        public static readonly Color SuccessColor = Color.FromArgb(80, 205, 137);       // #50CD89
        public static readonly Color WarningColor = Color.FromArgb(255, 168, 0);        // #FFA800
        public static readonly Color DangerColor = Color.FromArgb(220, 53, 69);         // #DC3545
        public static readonly Color InfoColor = Color.FromArgb(0, 188, 212);           // #00BCD4

        #endregion

        #region Colors - Background

        public static readonly Color BackgroundMain = Color.FromArgb(244, 246, 248);    // #F4F6F8
        public static readonly Color BackgroundPanel = Color.White;                      // #FFFFFF
        public static readonly Color BackgroundInput = Color.FromArgb(250, 250, 250);   // #FAFAFA
        public static readonly Color BackgroundHover = Color.FromArgb(249, 249, 249);   // #F9F9F9

        #endregion

        #region Colors - Text

        public static readonly Color TextPrimary = Color.FromArgb(24, 28, 50);          // #181C32
        public static readonly Color TextSecondary = Color.FromArgb(64, 64, 64);        // #404040
        public static readonly Color TextLight = Color.FromArgb(161, 165, 183);         // #A1A5B7
        public static readonly Color TextGray = Color.Gray;                             // #808080
        public static readonly Color TextWhite = Color.White;                           // #FFFFFF

        #endregion

        #region Colors - Border

        public static readonly Color BorderColor = Color.FromArgb(213, 218, 223);       // #D5DAE3
        public static readonly Color BorderLight = Color.FromArgb(239, 242, 245);       // #EFF2F5
        public static readonly Color BorderFocus = Color.FromArgb(94, 148, 255);        // #5E94FF

        #endregion

        #region Fonts

        public static readonly Font LargeTitle = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font Title = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font PageTitle = new Font("Segoe UI", 14.25F, FontStyle.Bold);
        public static readonly Font SubTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
        
        public static readonly Font BodyBold = new Font("Segoe UI", 10.8F, FontStyle.Bold);
        public static readonly Font BodyRegular = new Font("Segoe UI", 9.75F, FontStyle.Regular);
        public static readonly Font BodySmall = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font BodyTiny = new Font("Segoe UI", 8.25F, FontStyle.Regular);
        
        public static readonly Font LabelBold = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        public static readonly Font LabelRegular = new Font("Segoe UI", 9F, FontStyle.Regular);

        #endregion

        #region Sizing

        public static readonly int SidebarWidth = 347;
        public static readonly int HeaderHeight = 108;
        public static readonly int ButtonHeight = 40;
        public static readonly int ButtonHeightLarge = 58;
        public static readonly int ButtonHeightMenu = 60;
        public static readonly int InputHeight = 45;
        public static readonly int RowHeight = 50;
        public static readonly int HeaderRowHeight = 50;

        #endregion

        #region Helper Methods - Buttons

        /// <summary>
        /// Tạo Primary Button với style chuẩn
        /// </summary>
        public static Guna2Button CreatePrimaryButton(string text, int width = 120, int height = 40)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Size = new Size(width, height),
                BorderRadius = 6,
                FillColor = PrimaryColor,
                ForeColor = TextWhite,
                Font = BodyBold,
                Cursor = Cursors.Hand,
                DisabledState = { BorderColor = BorderColor, FillColor = BorderColor }
            };
            btn.HoverState.FillColor = PrimaryHover;
            return btn;
        }

        /// <summary>
        /// Tạo Secondary Button với style chuẩn
        /// </summary>
        public static Guna2Button CreateSecondaryButton(string text, int width = 120, int height = 40)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Size = new Size(width, height),
                BorderRadius = 6,
                FillColor = BackgroundPanel,
                ForeColor = TextSecondary,
                BorderColor = BorderColor,
                BorderThickness = 1,
                Font = BodyBold,
                Cursor = Cursors.Hand
            };
            btn.HoverState.FillColor = BackgroundHover;
            return btn;
        }

        /// <summary>
        /// Tạo Success Button
        /// </summary>
        public static Guna2Button CreateSuccessButton(string text, int width = 120, int height = 40)
        {
            var btn = CreatePrimaryButton(text, width, height);
            btn.FillColor = SuccessColor;
            btn.HoverState.FillColor = Color.FromArgb(60, 185, 117);
            return btn;
        }

        /// <summary>
        /// Tạo Danger Button
        /// </summary>
        public static Guna2Button CreateDangerButton(string text, int width = 120, int height = 40)
        {
            var btn = CreatePrimaryButton(text, width, height);
            btn.FillColor = DangerColor;
            btn.HoverState.FillColor = Color.FromArgb(200, 43, 59);
            return btn;
        }

        /// <summary>
        /// Tạo Menu Button cho Sidebar
        /// </summary>
        public static Guna2Button CreateMenuButton(string text, Image? icon = null)
        {
            var btn = new Guna2Button
            {
                Text = text,
                Height = ButtonHeightMenu,
                Dock = DockStyle.Top,
                BorderRadius = 0,
                FillColor = BackgroundPanel,
                ForeColor = TextSecondary,
                Font = BodyRegular,
                TextAlign = HorizontalAlignment.Left,
                Padding = new Padding(13, 0, 0, 0),
                Cursor = Cursors.Hand,
                ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton
            };
            
            if (icon != null)
            {
                btn.Image = icon;
                btn.ImageAlign = HorizontalAlignment.Left;
                btn.ImageOffset = new Point(10, 0);
                btn.TextOffset = new Point(10, 0);
            }
            
            btn.CheckedState.FillColor = PrimaryLight;
            btn.CheckedState.ForeColor = PrimaryHover;
            btn.HoverState.FillColor = BackgroundHover;
            
            return btn;
        }

        #endregion

        #region Helper Methods - TextBoxes

        /// <summary>
        /// Tạo TextBox với style chuẩn
        /// </summary>
        public static Guna2TextBox CreateTextBox(string placeholder = "", bool multiline = false, int height = 55)
        {
            var txt = new Guna2TextBox
            {
                BorderRadius = 6,
                BorderColor = BorderColor,
                FillColor = BackgroundInput,
                Font = BodyRegular,
                PlaceholderForeColor = TextLight,
                PlaceholderText = placeholder,
                Height = multiline ? 100 : height,
                Multiline = multiline
            };
            
            txt.FocusedState.BorderColor = BorderFocus;
            txt.HoverState.BorderColor = SecondaryColor;
            
            if (multiline)
            {
                txt.ScrollBars = ScrollBars.Vertical;
            }
            
            return txt;
        }

        #endregion

        #region Helper Methods - ComboBoxes

        /// <summary>
        /// Tạo ComboBox với style chuẩn
        /// </summary>
        public static Guna2ComboBox CreateComboBox(int height = 40)
        {
            var cmb = new Guna2ComboBox
            {
                BorderRadius = 6,
                BorderColor = BorderColor,
                FillColor = BackgroundPanel,
                Font = BodyRegular,
                ForeColor = TextPrimary,
                ItemHeight = 30,
                Height = height,
                DrawMode = DrawMode.OwnerDrawFixed,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
            cmb.FocusedState.BorderColor = BorderFocus;
            cmb.HoverState.BorderColor = SecondaryColor;
            
            return cmb;
        }

        #endregion

        #region Helper Methods - Panels

        /// <summary>
        /// Tạo Panel với style chuẩn
        /// </summary>
        public static Guna2Panel CreatePanel(bool withBorder = false, int borderRadius = 8)
        {
            var pnl = new Guna2Panel
            {
                BackColor = BackgroundPanel,
                BorderRadius = borderRadius,
                BorderColor = withBorder ? BorderLight : Color.Transparent,
                BorderThickness = withBorder ? 1 : 0
            };
            
            return pnl;
        }

        /// <summary>
        /// Tạo Card Panel
        /// </summary>
        public static Guna2Panel CreateCardPanel(int borderRadius = 12)
        {
            var pnl = new Guna2Panel
            {
                BackColor = BackgroundPanel,
                BorderRadius = borderRadius,
                BorderColor = BorderLight,
                BorderThickness = 1,
                Padding = new Padding(20)
            };
            
            return pnl;
        }

        #endregion

        #region Helper Methods - DataGridView

        /// <summary>
        /// Apply style chuẩn cho DataGridView
        /// </summary>
        public static void ApplyDataGridViewStyle(Guna2DataGridView dgv)
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = BackgroundPanel;
            dgv.GridColor = BorderLight;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowTemplate.Height = RowHeight;
            dgv.ColumnHeadersHeight = HeaderRowHeight;
            dgv.Font = BodyRegular;
            
            // Header style
            dgv.ColumnHeadersDefaultCellStyle.BackColor = BorderFocus;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextWhite;
            dgv.ColumnHeadersDefaultCellStyle.Font = BodyBold;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgv.EnableHeadersVisualStyles = false;
            
            // Row style
            dgv.DefaultCellStyle.BackColor = BackgroundPanel;
            dgv.DefaultCellStyle.ForeColor = TextSecondary;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 240, 243);
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            
            // Alternating rows
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            
            // Theme
            dgv.ThemeStyle.HeaderStyle.BackColor = BorderFocus;
            dgv.ThemeStyle.HeaderStyle.ForeColor = TextWhite;
            dgv.ThemeStyle.HeaderStyle.Font = BodyBold;
            dgv.ThemeStyle.AlternatingRowsStyle.BackColor = Color.FromArgb(250, 250, 250);
            dgv.ThemeStyle.RowsStyle.BackColor = BackgroundPanel;
            dgv.ThemeStyle.RowsStyle.ForeColor = TextSecondary;
        }

        #endregion

        #region Helper Methods - DateTimePicker

        /// <summary>
        /// Tạo DateTimePicker với style chuẩn
        /// </summary>
        public static Guna2DateTimePicker CreateDateTimePicker()
        {
            var dtp = new Guna2DateTimePicker
            {
                BorderRadius = 6,
                BorderColor = BorderColor,
                BorderThickness = 1,
                FillColor = BackgroundPanel,
                Font = BodyRegular,
                ForeColor = TextPrimary,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Height = InputHeight
            };
            
            // Note: Guna2DateTimePicker may not have FocusedState/HoverState in some versions
            // Remove these if compilation errors occur
            
            return dtp;
        }

        #endregion

        #region Helper Methods - NumericUpDown

        /// <summary>
        /// Apply style chuẩn cho NumericUpDown
        /// </summary>
        public static void ApplyNumericUpDownStyle(NumericUpDown nud)
        {
            nud.Font = BodyRegular;
            nud.BorderStyle = BorderStyle.FixedSingle;
        }

        #endregion

        #region Helper Methods - Labels

        /// <summary>
        /// Tạo Label với style chuẩn
        /// </summary>
        public static Label CreateLabel(string text, Font? font = null, Color? foreColor = null)
        {
            return new Label
            {
                Text = text,
                Font = font ?? BodyRegular,
                ForeColor = foreColor ?? TextSecondary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        /// <summary>
        /// Tạo Title Label
        /// </summary>
        public static Label CreateTitleLabel(string text)
        {
            return CreateLabel(text, Title, TextPrimary);
        }

        /// <summary>
        /// Tạo SubTitle Label
        /// </summary>
        public static Label CreateSubTitleLabel(string text)
        {
            return CreateLabel(text, SubTitle, TextSecondary);
        }

        #endregion

        #region Helper Methods - Status Badge

        /// <summary>
        /// Lấy màu cho Status Badge
        /// </summary>
        public static (Color ForeColor, Color BackColor) GetStatusColors(string status)
        {
            return status.ToLower() switch
            {
                "active" or "đang hoạt động" or "đã duyệt" or "passed" => 
                    (Color.FromArgb(30, 150, 80), Color.FromArgb(220, 250, 230)),
                
                "pending" or "đang chờ" or "chờ duyệt" => 
                    (Color.FromArgb(210, 120, 0), Color.FromArgb(255, 245, 220)),
                
                "inactive" or "closed" or "đã đóng" or "rejected" or "failed" => 
                    (Color.FromArgb(100, 100, 100), Color.FromArgb(240, 242, 245)),
                
                "cancelled" or "đã hủy" => 
                    (DangerColor, Color.FromArgb(255, 220, 220)),
                
                _ => (TextSecondary, BackgroundInput)
            };
        }

        #endregion
    }
}
