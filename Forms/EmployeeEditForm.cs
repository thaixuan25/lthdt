using System;
using System.Linq;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    /// <summary>
    /// EmployeeEditForm - Form thêm/sửa nhân viên
    /// Kế thừa BaseEditForm<Employee> (Inheritance)
    /// </summary>
    public class EmployeeEditForm : BaseEditForm<Employee>
    {
        private readonly EmployeeRepository _repository;
        private readonly DepartmentRepository _departmentRepository;
        private readonly PositionRepository _positionRepository;

        private TextBox txtCode = null!;
        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private ComboBox cmbDepartment = null!;
        private ComboBox cmbPosition = null!;
        private DateTimePicker dtpHireDate = null!;
        private ComboBox cmbStatus = null!;

        public EmployeeEditForm() : base()
        {
            _repository = new EmployeeRepository();
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
        }

        public EmployeeEditForm(Employee employee) : base(employee)
        {
            _repository = new EmployeeRepository();
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
        }

        protected override string GetEntityName()
        {
            return "Nhân viên";
        }

        protected override void InitializeFormControls()
        {
            int startY = 20;
            int spacing = 45;
            int currentY = startY;

            // Employee Code
            AddLabelAndTextBox("Mã nhân viên:", ref txtCode, currentY);
            txtCode.MaxLength = 20;
            currentY += spacing;

            // Full Name
            AddLabelAndTextBox("Họ tên:", ref txtFullName, currentY);
            txtFullName.MaxLength = 100;
            currentY += spacing;

            // Email
            AddLabelAndTextBox("Email:", ref txtEmail, currentY);
            txtEmail.MaxLength = 100;
            currentY += spacing;

            // Phone
            AddLabelAndTextBox("Điện thoại:", ref txtPhone, currentY);
            txtPhone.MaxLength = 15;
            txtPhone.PlaceholderText = "VD: 0901234567";
            currentY += spacing;

            // Department
            AddLabelAndComboBox("Phòng ban:", ref cmbDepartment, currentY, 300);
            LoadDepartments();
            currentY += spacing;

            // Position
            AddLabelAndComboBox("Vị trí:", ref cmbPosition, currentY, 300);
            LoadPositions();
            currentY += spacing;

            // Hire Date
            AddLabelAndDateTimePicker("Ngày vào làm:", ref dtpHireDate, currentY, 200);
            dtpHireDate.Value = DateTime.Now;
            currentY += spacing;

            // Status
            AddLabelAndComboBox("Trạng thái:", ref cmbStatus, currentY, 200);
            cmbStatus.Items.AddRange(new[] { "Active", "Resigned", "Terminated" });
            cmbStatus.SelectedIndex = 0;
            currentY += spacing;

            // Info label
            var lblInfo = new Label
            {
                Text = "💡 Số điện thoại: 10-11 số, bắt đầu bằng 0",
                Location = new System.Drawing.Point(180, currentY),
                Size = new System.Drawing.Size(400, 25),
                ForeColor = System.Drawing.Color.FromArgb(127, 140, 141),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic)
            };
            mainPanel.Controls.Add(lblInfo);
        }

        private void LoadDepartments()
        {
            try
            {
                var departments = _departmentRepository.GetAll().ToList();
                cmbDepartment.Items.Clear();
                foreach (var dept in departments)
                {
                    cmbDepartment.Items.Add(new { Id = dept.Id, Name = dept.GetDisplayName() });
                }
                cmbDepartment.DisplayMember = "Name";
                cmbDepartment.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load phòng ban: {ex.Message}");
            }
        }

        private void LoadPositions()
        {
            try
            {
                var positions = _positionRepository.GetAll().ToList();
                cmbPosition.Items.Clear();
                foreach (var pos in positions)
                {
                    cmbPosition.Items.Add(new { Id = pos.Id, Name = pos.GetDisplayName() });
                }
                cmbPosition.DisplayMember = "Name";
                cmbPosition.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load vị trí: {ex.Message}");
            }
        }

        protected override void LoadEntity()
        {
            try
            {
                txtCode.Text = Entity.EmployeeCode;
                txtFullName.Text = Entity.FullName;
                txtEmail.Text = Entity.Email;
                txtPhone.Text = Entity.Phone ?? "";
                dtpHireDate.Value = Entity.HireDate;

                // Select department
                for (int i = 0; i < cmbDepartment.Items.Count; i++)
                {
                    dynamic item = cmbDepartment.Items[i]!;
                    if (item.Id == Entity.DepartmentId)
                    {
                        cmbDepartment.SelectedIndex = i;
                        break;
                    }
                }

                // Select position
                for (int i = 0; i < cmbPosition.Items.Count; i++)
                {
                    dynamic item = cmbPosition.Items[i]!;
                    if (item.Id == Entity.PositionId)
                    {
                        cmbPosition.SelectedIndex = i;
                        break;
                    }
                }

                // Select status
                for (int i = 0; i < cmbStatus.Items.Count; i++)
                {
                    if (cmbStatus.Items[i].ToString() == Entity.Status)
                    {
                        cmbStatus.SelectedIndex = i;
                        break;
                    }
                }

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
            // Validate Employee Code
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                ShowWarning("Vui lòng nhập mã nhân viên!");
                txtCode.Focus();
                return false;
            }

            // Validate Full Name
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || txtFullName.Text.Trim().Length < 2)
            {
                ShowWarning("Họ tên phải có ít nhất 2 ký tự!");
                txtFullName.Focus();
                return false;
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowWarning("Vui lòng nhập email!");
                txtEmail.Focus();
                return false;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                ShowWarning("Email không hợp lệ!");
                txtEmail.Focus();
                return false;
            }

            // Validate Phone (if provided)
            if (!string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                var phone = txtPhone.Text.Replace(" ", "").Replace("-", "");
                if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9,10}$"))
                {
                    ShowWarning("Số điện thoại không hợp lệ!\nPhải là 10-11 số, bắt đầu bằng 0");
                    txtPhone.Focus();
                    return false;
                }
            }

            // Validate Department
            if (cmbDepartment.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn phòng ban!");
                cmbDepartment.Focus();
                return false;
            }

            // Validate Position
            if (cmbPosition.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn vị trí!");
                cmbPosition.Focus();
                return false;
            }

            // Validate Status
            if (cmbStatus.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn trạng thái!");
                cmbStatus.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        protected override void SaveEntity()
        {
            try
            {
                // Map data from controls to entity
                Entity.EmployeeCode = txtCode.Text.Trim().ToUpper();
                Entity.FullName = txtFullName.Text.Trim();
                Entity.Email = txtEmail.Text.Trim().ToLower();
                Entity.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
                
                dynamic selectedDept = cmbDepartment.SelectedItem!;
                Entity.DepartmentId = (int)selectedDept.Id;
                
                dynamic selectedPos = cmbPosition.SelectedItem!;
                Entity.PositionId = (int)selectedPos.Id;
                
                Entity.HireDate = dtpHireDate.Value.Date;
                Entity.Status = cmbStatus.SelectedItem!.ToString()!;

                // Save to database
                if (IsEditMode)
                {
                    if (!_repository.Update(Entity))
                    {
                        throw new Exception("Không thể cập nhật nhân viên");
                    }
                }
                else
                {
                    var id = _repository.Add(Entity);
                    if (id <= 0)
                    {
                        throw new Exception("Không thể thêm nhân viên");
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
