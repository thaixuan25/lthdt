using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Forms
{
    /// <summary>
    /// BaseEditForm - Form cha cho tất cả các form thêm/sửa
    /// Áp dụng OOP: Generic Type, Abstract Methods, Encapsulation
    /// Template pattern - định nghĩa khung sườn, class con implement chi tiết
    /// </summary>
    public abstract class BaseEditForm<T> : BaseForm where T : BaseEntity, new()
    {
        protected T Entity { get; set; } = null!;
        protected bool IsEditMode => Entity?.Id > 0;
        
        protected Guna2Panel mainPanel = null!;
        protected Guna2Panel buttonPanel = null!;
        protected Guna2Button btnSave = null!;
        protected Guna2Button btnCancel = null!;

        /// <summary>
        /// Constructor cho Add mode
        /// </summary>
        public BaseEditForm() : this(null)
        {
        }

        /// <summary>
        /// Constructor cho Edit mode
        /// </summary>
        public BaseEditForm(T? entity)
        {
            Entity = entity ?? new T();
            InitializeBaseComponents();
        }

        /// <summary>
        /// Khởi tạo các components cơ bản
        /// </summary>
        private void InitializeBaseComponents()
        {
            this.Text = IsEditMode ? $"Sửa {GetEntityName()}" : $"Thêm {GetEntityName()}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.BackgroundMain;

            mainPanel = UITheme.CreatePanel(withBorder: false);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);
            mainPanel.AutoScroll = true;

            buttonPanel = UITheme.CreatePanel(withBorder: false);
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 70;
            buttonPanel.BackColor = UITheme.BackgroundMain;
            buttonPanel.Padding = new Padding(15);

            btnSave = UITheme.CreatePrimaryButton("💾 Lưu", 120, UITheme.ButtonHeight);
            btnSave.Click += BtnSave_Click;

            btnCancel = UITheme.CreateSecondaryButton("❌ Hủy", 120, UITheme.ButtonHeight);
            btnCancel.Click += BtnCancel_Click;

            buttonPanel.Resize += (s, e) =>
            {
                int totalWidth = 250;
                int startX = (buttonPanel.Width - totalWidth) / 2;
                btnSave.Location = new Point(startX, 15);
                btnCancel.Location = new Point(startX + 130, 15);
            };

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);
        }

        /// <summary>
        /// Override BaseForm_Load
        /// </summary>
        protected override void BaseForm_Load(object? sender, EventArgs e)
        {
            base.BaseForm_Load(sender, e);

            try
            {
                InitializeFormControls();

                if (IsEditMode)
                {
                    LoadEntity();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khởi tạo form: {ex.Message}");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        #region Abstract Methods - Class con BẮT BUỘC phải implement

        /// <summary>
        /// Khởi tạo các controls cụ thể (TextBox, ComboBox, etc.)
        /// Class con phải tạo và add controls vào mainPanel
        /// </summary>
        protected abstract void InitializeFormControls();

        /// <summary>
        /// Load dữ liệu của entity vào các controls
        /// Chỉ gọi khi IsEditMode = true
        /// </summary>
        protected abstract void LoadEntity();

        /// <summary>
        /// Lưu dữ liệu từ controls vào entity và save vào database
        /// </summary>
        protected abstract void SaveEntity();

        /// <summary>
        /// Validate dữ liệu người dùng nhập
        /// Return true nếu hợp lệ, false nếu không
        /// </summary>
        protected abstract bool ValidateInput();

        /// <summary>
        /// Lấy tên entity để hiển thị trên title
        /// </summary>
        protected abstract string GetEntityName();

        #endregion

        #region Virtual Methods - Class con CÓ THỂ override

        /// <summary>
        /// Xử lý trước khi save
        /// Virtual - class con có thể override
        /// </summary>
        protected virtual bool BeforeSave()
        {
            return true;
        }

        /// <summary>
        /// Xử lý sau khi save thành công
        /// Virtual - class con có thể override
        /// </summary>
        protected virtual void AfterSave()
        {
        }

        #endregion

        #region Event Handlers

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput())
                {
                    return;
                }

                if (!BeforeSave())
                {
                    return;
                }

                SaveEntity();

                AfterSave();

                ShowSuccess(IsEditMode 
                    ? $"Cập nhật {GetEntityName()} thành công!" 
                    : $"Thêm {GetEntityName()} thành công!");

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi lưu: {ex.Message}\n\nChi tiết: {ex.StackTrace}");
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (HasChanges())
            {
                if (!Confirm("Bạn có thay đổi chưa lưu. Bạn có chắc muốn hủy?"))
                {
                    return;
                }
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Kiểm tra có thay đổi chưa lưu không
        /// Virtual - class con có thể override để implement logic phức tạp hơn
        /// </summary>
        protected virtual bool HasChanges()
        {
            return true;
        }

        /// <summary>
        /// Helper: Tạo label và Guna2TextBox theo cặp
        /// </summary>
        protected void AddLabelAndTextBox(string labelText, ref Guna2TextBox textBox, int y, bool multiline = false, int height = 55)
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            textBox = UITheme.CreateTextBox("", multiline, multiline ? height : UITheme.InputHeight);
            textBox.Location = new Point(180, y);
            textBox.Width = 400;
            
            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(textBox);
        }

        /// <summary>
        /// Legacy Helper: Backward compatibility cho TextBox thông thường
        /// </summary>
        protected void AddLabelAndTextBox(string labelText, ref TextBox textBox, int y, bool multiline = false, int height = 25)
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            textBox = new TextBox
            {
                Location = new Point(180, y),
                Size = new Size(400, multiline ? height : 25),
                Font = UITheme.BodyRegular,
                Multiline = multiline
            };
            if (multiline)
            {
                textBox.ScrollBars = ScrollBars.Vertical;
            }

            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(textBox);
        }

        /// <summary>
        /// Helper: Tạo label và Guna2ComboBox theo cặp
        /// </summary>
        protected void AddLabelAndComboBox(string labelText, ref Guna2ComboBox comboBox, int y, int width = 400)
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            comboBox = UITheme.CreateComboBox(40);
            comboBox.Location = new Point(180, y);
            comboBox.Width = width;

            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(comboBox);
        }

        /// <summary>
        /// Legacy Helper: Backward compatibility cho ComboBox thông thường
        /// </summary>
        protected void AddLabelAndComboBox(string labelText, ref ComboBox comboBox, int y, int width = 400)
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            comboBox = new ComboBox
            {
                Location = new Point(180, y),
                Size = new Size(width, 25),
                Font = UITheme.BodyRegular,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(comboBox);
        }

        /// <summary>
        /// Helper: Tạo label và numeric updown theo cặp
        /// </summary>
        protected void AddLabelAndNumericUpDown(string labelText, ref NumericUpDown numericUpDown, int y, decimal min = 0, decimal max = 999999999, int width = 200)
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            numericUpDown = new NumericUpDown
            {
                Location = new Point(180, y),
                Size = new Size(width, 35),
                Font = UITheme.BodyRegular,
                Minimum = min,
                Maximum = max,
                DecimalPlaces = 0,
                BorderStyle = BorderStyle.FixedSingle
            };
            UITheme.ApplyNumericUpDownStyle(numericUpDown);

            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(numericUpDown);
        }

        /// <summary>
        /// Helper: Tạo label và Guna2DateTimePicker theo cặp
        /// </summary>
        protected void AddLabelAndDateTimePicker(string labelText, ref Guna2DateTimePicker dateTimePicker, int y, int width = 200)
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            dateTimePicker = UITheme.CreateDateTimePicker();
            dateTimePicker.Location = new Point(180, y);
            dateTimePicker.Width = width;

            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(dateTimePicker);
        }

        /// <summary>
        /// Legacy Helper: Backward compatibility cho DateTimePicker thông thường
        /// </summary>
        protected void AddLabelAndDateTimePicker(string labelText, ref DateTimePicker dateTimePicker, int y, int width = 200)
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            dateTimePicker = new DateTimePicker
            {
                Location = new Point(180, y),
                Size = new Size(width, 25),
                Font = UITheme.BodyRegular,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy"
            };

            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(dateTimePicker);
        }

        /// <summary>
        /// Helper: Tạo label và Guna2CheckBox theo cặp
        /// </summary>
        protected void AddLabelAndCheckBox(string labelText, ref Guna2CheckBox checkBox, int y, string checkBoxText = "")
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            checkBox = new Guna2CheckBox
            {
                Location = new Point(180, y),
                Size = new Size(400, 25),
                Font = UITheme.BodyRegular,
                Text = checkBoxText,
                CheckedState = { BorderColor = UITheme.BorderFocus, FillColor = UITheme.PrimaryColor }
            };

            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(checkBox);
        }
        
        /// <summary>
        /// Legacy Helper: Backward compatibility cho CheckBox thông thường
        /// </summary>
        protected void AddLabelAndCheckBox(string labelText, ref CheckBox checkBox, int y, string checkBoxText = "")
        {
            var label = UITheme.CreateLabel(labelText, UITheme.BodyBold);
            label.Location = new Point(20, y);
            label.Size = new Size(150, 25);
            
            checkBox = new CheckBox
            {
                Location = new Point(180, y),
                Size = new Size(400, 25),
                Font = UITheme.BodyRegular,
                Text = checkBoxText
            };

            mainPanel.Controls.Add(label);
            mainPanel.Controls.Add(checkBox);
        }

        #endregion
    }
}
