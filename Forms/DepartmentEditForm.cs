using System;
using System.Linq;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    /// <summary>
    /// DepartmentEditForm - Form thêm/sửa phòng ban
    /// Kế thừa BaseEditForm<Department> (Inheritance)
    /// </summary>
    public class DepartmentEditForm : BaseEditForm<Department>
    {
        private readonly DepartmentRepository _repository;
        private readonly EmployeeRepository _employeeRepository;

        private TextBox txtCode = null!;
        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private ComboBox cmbManager = null!;
        private TextBox txtLocation = null!;
        private NumericUpDown numMaxHeadcount = null!;

        public DepartmentEditForm() : base()
        {
            _repository = new DepartmentRepository();
            _employeeRepository = new EmployeeRepository();
        }

        public DepartmentEditForm(Department department) : base(department)
        {
            _repository = new DepartmentRepository();
            _employeeRepository = new EmployeeRepository();
        }

        protected override string GetEntityName()
        {
            return "Phòng ban";
        }

        protected override void InitializeFormControls()
        {
            int startY = 20;
            int spacing = 45;
            int currentY = startY;

            // Department Code
            AddLabelAndTextBox("Mã phòng ban:", ref txtCode, currentY);
            txtCode.MaxLength = 20;
            txtCode.CharacterCasing = CharacterCasing.Upper;
            currentY += spacing;

            // Department Name
            AddLabelAndTextBox("Tên phòng ban:", ref txtName, currentY);
            txtName.MaxLength = 100;
            currentY += spacing;

            // Description
            var lblDescription = CreateLabel("Mô tả:", 20, currentY, 150);
            txtDescription = CreateTextBox(180, currentY, 400, true);
            txtDescription.Height = 60;
            mainPanel.Controls.Add(lblDescription);
            mainPanel.Controls.Add(txtDescription);
            currentY += 75;

            // Manager (ComboBox)
            AddLabelAndComboBox("Quản lý:", ref cmbManager, currentY, 300);
            LoadManagerList();
            currentY += spacing;

            // Location
            AddLabelAndTextBox("Địa điểm:", ref txtLocation, currentY);
            txtLocation.MaxLength = 200;
            txtLocation.PlaceholderText = "VD: Tầng 5, Tòa nhà ABC";
            currentY += spacing;

            // Max Headcount
            AddLabelAndNumericUpDown("Biên chế tối đa:", ref numMaxHeadcount, currentY, 0, 1000, 100);
            numMaxHeadcount.Value = 0;
            currentY += spacing;

            // Info label
            var lblInfo = new Label
            {
                Text = "💡 Quản lý và địa điểm có thể để trống",
                Location = new System.Drawing.Point(180, currentY),
                Size = new System.Drawing.Size(400, 25),
                ForeColor = System.Drawing.Color.FromArgb(127, 140, 141),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic)
            };
            mainPanel.Controls.Add(lblInfo);
        }

        private void LoadManagerList()
        {
            try
            {
                var employees = _employeeRepository.GetAll()
                    .Where(e => e.Status == "Active")
                    .OrderBy(e => e.FullName)
                    .ToList();

                cmbManager.Items.Clear();
                cmbManager.Items.Add(new { Id = 0, DisplayName = "-- Chọn quản lý --" });
                
                foreach (var emp in employees)
                {
                    cmbManager.Items.Add(new { Id = emp.Id, DisplayName = $"{emp.EmployeeCode} - {emp.FullName}" });
                }

                cmbManager.DisplayMember = "DisplayName";
                cmbManager.ValueMember = "Id";
                cmbManager.SelectedIndex = 0;
            }
            catch
            {
                // Nếu chưa có employee nào, để trống
                cmbManager.Items.Clear();
                cmbManager.Items.Add(new { Id = 0, DisplayName = "-- Không có nhân viên --" });
                cmbManager.SelectedIndex = 0;
            }
        }

        protected override void LoadEntity()
        {
            try
            {
                txtCode.Text = Entity.DepartmentCode;
                txtName.Text = Entity.DepartmentName;
                txtDescription.Text = Entity.Description ?? "";
                txtLocation.Text = Entity.Location ?? "";
                numMaxHeadcount.Value = Entity.MaxHeadcount;

                // Set selected manager
                if (Entity.ManagerId.HasValue && Entity.ManagerId.Value > 0)
                {
                    for (int i = 0; i < cmbManager.Items.Count; i++)
                    {
                        dynamic item = cmbManager.Items[i];
                        if (item.Id == Entity.ManagerId.Value)
                        {
                            cmbManager.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // Disable code edit in edit mode
                if (IsEditMode)
                {
                    txtCode.ReadOnly = true;
                    txtCode.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        protected override bool ValidateInput()
        {
            // Validate Department Code
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                ShowWarning("Vui lòng nhập mã phòng ban!");
                txtCode.Focus();
                return false;
            }

            // Validate Department Name
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text.Trim().Length < 2)
            {
                ShowWarning("Tên phòng ban phải có ít nhất 2 ký tự!");
                txtName.Focus();
                return false;
            }

            return true;
        }

        protected override void SaveEntity()
        {
            try
            {
                // Map data from controls to entity
                Entity.DepartmentCode = txtCode.Text.Trim().ToUpper();
                Entity.DepartmentName = txtName.Text.Trim();
                Entity.Description = string.IsNullOrWhiteSpace(txtDescription.Text) 
                    ? null 
                    : txtDescription.Text.Trim();
                Entity.Location = string.IsNullOrWhiteSpace(txtLocation.Text) 
                    ? null 
                    : txtLocation.Text.Trim();
                Entity.MaxHeadcount = (int)numMaxHeadcount.Value;

                // Get selected manager ID
                if (cmbManager.SelectedItem != null)
                {
                    dynamic selectedItem = cmbManager.SelectedItem;
                    int managerId = selectedItem.Id;
                    Entity.ManagerId = managerId > 0 ? managerId : (int?)null;
                }
                else
                {
                    Entity.ManagerId = null;
                }

                // Save to database
                if (IsEditMode)
                {
                    if (!_repository.Update(Entity))
                    {
                        throw new Exception("Không thể cập nhật phòng ban");
                    }
                }
                else
                {
                    var id = _repository.Add(Entity);
                    if (id <= 0)
                    {
                        throw new Exception("Không thể thêm phòng ban");
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
