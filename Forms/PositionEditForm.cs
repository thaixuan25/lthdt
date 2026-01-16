using System;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    /// <summary>
    /// PositionEditForm - Form thêm/sửa vị trí
    /// Kế thừa BaseEditForm<Position> (Inheritance)
    /// </summary>
    public class PositionEditForm : BaseEditForm<Position>
    {
        private readonly PositionRepository _repository;

        private TextBox txtCode = null!;
        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private ComboBox cmbLevel = null!;
        private NumericUpDown numMinSalary = null!;
        private NumericUpDown numMaxSalary = null!;

        public PositionEditForm() : base()
        {
            _repository = new PositionRepository();
        }

        public PositionEditForm(Position position) : base(position)
        {
            _repository = new PositionRepository();
        }

        protected override string GetEntityName()
        {
            return "Vị trí";
        }

        protected override void InitializeFormControls()
        {
            int startY = 20;
            int spacing = 45;
            int currentY = startY;

            // Position Code
            AddLabelAndTextBox("Mã vị trí:", ref txtCode, currentY);
            txtCode.MaxLength = 20;
            currentY += spacing;

            // Position Name
            AddLabelAndTextBox("Tên vị trí:", ref txtName, currentY);
            txtName.MaxLength = 100;
            currentY += spacing;

            // Description
            AddLabelAndTextBox("Mô tả:", ref txtDescription, currentY, true, 80);
            txtDescription.MaxLength = 500;
            currentY += 80 + 10;

            // Level
            AddLabelAndComboBox("Cấp bậc:", ref cmbLevel, currentY, 200);
            cmbLevel.Items.AddRange(new object[]
            {
                new { Value = 1, Text = "1 - Junior" },
                new { Value = 2, Text = "2 - Middle" },
                new { Value = 3, Text = "3 - Senior" },
                new { Value = 4, Text = "4 - Lead" },
                new { Value = 5, Text = "5 - Manager" }
            });
            cmbLevel.DisplayMember = "Text";
            cmbLevel.ValueMember = "Value";
            cmbLevel.SelectedIndex = 0;
            currentY += spacing;

            // Min Salary
            AddLabelAndNumericUpDown("Lương tối thiểu:", ref numMinSalary, currentY, 0, 999999999, 200);
            numMinSalary.Increment = 1000000;
            numMinSalary.ThousandsSeparator = true;
            currentY += spacing;

            // Max Salary
            AddLabelAndNumericUpDown("Lương tối đa:", ref numMaxSalary, currentY, 0, 999999999, 200);
            numMaxSalary.Increment = 1000000;
            numMaxSalary.ThousandsSeparator = true;
            currentY += spacing;

            // Info label
            var lblInfo = new Label
            {
                Text = "💡 Lương có thể để 0 nếu chưa xác định",
                Location = new System.Drawing.Point(180, currentY),
                Size = new System.Drawing.Size(400, 25),
                ForeColor = System.Drawing.Color.FromArgb(127, 140, 141),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic)
            };
            mainPanel.Controls.Add(lblInfo);
        }

        protected override void LoadEntity()
        {
            try
            {
                txtCode.Text = Entity.PositionCode;
                txtName.Text = Entity.PositionName;
                txtDescription.Text = Entity.Description ?? "";
                
                // Select level in combo box
                for (int i = 0; i < cmbLevel.Items.Count; i++)
                {
                    dynamic item = cmbLevel.Items[i]!;
                    if (item.Value == Entity.Level)
                    {
                        cmbLevel.SelectedIndex = i;
                        break;
                    }
                }

                numMinSalary.Value = Entity.MinSalary;
                numMaxSalary.Value = Entity.MaxSalary;

                // Disable code edit in edit mode
                txtCode.ReadOnly = true;
                txtCode.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        protected override bool ValidateInput()
        {
            // Validate Position Code
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                ShowWarning("Vui lòng nhập mã vị trí!");
                txtCode.Focus();
                return false;
            }

            // Validate Position Name
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text.Trim().Length < 2)
            {
                ShowWarning("Tên vị trí phải có ít nhất 2 ký tự!");
                txtName.Focus();
                return false;
            }

            // Validate Level
            if (cmbLevel.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn cấp bậc!");
                cmbLevel.Focus();
                return false;
            }

            // Validate Salary Range
            if (numMaxSalary.Value > 0 && numMaxSalary.Value < numMinSalary.Value)
            {
                ShowWarning("Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu!");
                numMaxSalary.Focus();
                return false;
            }

            return true;
        }

        protected override void SaveEntity()
        {
            try
            {
                // Map data from controls to entity
                Entity.PositionCode = txtCode.Text.Trim().ToUpper();
                Entity.PositionName = txtName.Text.Trim();
                Entity.Description = txtDescription.Text.Trim();
                
                dynamic selectedLevel = cmbLevel.SelectedItem!;
                Entity.Level = (int)selectedLevel.Value;
                
                Entity.MinSalary = numMinSalary.Value;
                Entity.MaxSalary = numMaxSalary.Value;

                // Save to database
                if (IsEditMode)
                {
                    if (!_repository.Update(Entity))
                    {
                        throw new Exception("Không thể cập nhật vị trí");
                    }
                }
                else
                {
                    var id = _repository.Add(Entity);
                    if (id <= 0)
                    {
                        throw new Exception("Không thể thêm vị trí");
                    }
                    Entity.Id = id;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lưu dữ liệu: {ex.Message}", ex);
            }
        }
    }
}
