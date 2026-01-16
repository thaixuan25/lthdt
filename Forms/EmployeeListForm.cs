using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    /// <summary>
    /// EmployeeListForm - Danh sách nhân viên
    /// Kế thừa BaseListForm<Employee> (Inheritance)
    /// </summary>
    public class EmployeeListForm : BaseListForm<Employee>
    {
        private readonly EmployeeRepository _repository;
        private readonly DepartmentRepository _departmentRepository;
        private readonly PositionRepository _positionRepository;

        private ComboBox cmbDepartmentFilter = null!;
        private ComboBox cmbPositionFilter = null!;
        private ComboBox cmbStatusFilter = null!;

        public EmployeeListForm()
        {
            _repository = new EmployeeRepository();
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
        }

        protected override string GetFormTitle()
        {
            return "Quản lý Nhân viên";
        }

        protected override void SetupDataGridView()
        {
            // Add filter controls to search panel
            var lblDept = CreateLabel("Phòng ban:", 540, 18, 80);
            cmbDepartmentFilter = new ComboBox
            {
                Location = new System.Drawing.Point(625, 15),
                Size = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDepartmentFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            var lblPos = CreateLabel("Vị trí:", 785, 18, 50);
            cmbPositionFilter = new ComboBox
            {
                Location = new System.Drawing.Point(835, 15),
                Size = new System.Drawing.Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPositionFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            searchPanel.Controls.Add(lblDept);
            searchPanel.Controls.Add(cmbDepartmentFilter);
            searchPanel.Controls.Add(lblPos);
            searchPanel.Controls.Add(cmbPositionFilter);

            // Setup columns
            dataGridView.AutoGenerateColumns = false;
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EmployeeCode",
                HeaderText = "Mã NV",
                Name = "EmployeeCode",
                Width = 100
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FullName",
                HeaderText = "Họ tên",
                Name = "FullName",
                Width = 180
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Email",
                HeaderText = "Email",
                Name = "Email",
                Width = 200
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Phone",
                HeaderText = "Điện thoại",
                Name = "Phone",
                Width = 120
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DepartmentName",
                HeaderText = "Phòng ban",
                Name = "DepartmentName",
                Width = 150
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PositionName",
                HeaderText = "Vị trí",
                Name = "PositionName",
                Width = 150
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HireDateFormatted",
                HeaderText = "Ngày vào làm",
                Name = "HireDate",
                Width = 120
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Trạng thái",
                Name = "Status",
                Width = 100
            });
        }

        protected override void LoadData()
        {
            try
            {
                var employees = _repository.GetAll().ToList();
                
                // Load filters
                LoadFilters();

                // Map to display model
                UpdateDisplayData(employees);

                allData = employees;
                filteredData = new List<Employee>(employees);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        private void LoadFilters()
        {
            // Load departments
            var departments = _departmentRepository.GetAll().ToList();
            cmbDepartmentFilter.Items.Clear();
            cmbDepartmentFilter.Items.Add(new { Id = 0, Name = "(Tất cả)" });
            foreach (var dept in departments)
            {
                cmbDepartmentFilter.Items.Add(new { Id = dept.Id, Name = dept.DepartmentName });
            }
            cmbDepartmentFilter.DisplayMember = "Name";
            cmbDepartmentFilter.ValueMember = "Id";
            cmbDepartmentFilter.SelectedIndex = 0;

            // Load positions
            var positions = _positionRepository.GetAll().ToList();
            cmbPositionFilter.Items.Clear();
            cmbPositionFilter.Items.Add(new { Id = 0, Name = "(Tất cả)" });
            foreach (var pos in positions)
            {
                cmbPositionFilter.Items.Add(new { Id = pos.Id, Name = pos.PositionName });
            }
            cmbPositionFilter.DisplayMember = "Name";
            cmbPositionFilter.ValueMember = "Id";
            cmbPositionFilter.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            var filtered = allData.AsEnumerable();

            // Filter by department
            if (cmbDepartmentFilter.SelectedItem != null)
            {
                dynamic deptItem = cmbDepartmentFilter.SelectedItem;
                if (deptItem.Id > 0)
                {
                    filtered = filtered.Where(e => e.DepartmentId == deptItem.Id);
                }
            }

            // Filter by position
            if (cmbPositionFilter.SelectedItem != null)
            {
                dynamic posItem = cmbPositionFilter.SelectedItem;
                if (posItem.Id > 0)
                {
                    filtered = filtered.Where(e => e.PositionId == posItem.Id);
                }
            }

            // Apply search keyword
            var keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                filtered = filtered.Where(e =>
                    e.EmployeeCode.ToLower().Contains(lowerKeyword) ||
                    e.FullName.ToLower().Contains(lowerKeyword) ||
                    e.Email.ToLower().Contains(lowerKeyword)
                );
            }

            filteredData = filtered.ToList();
            UpdateDisplayData(filteredData);
        }

        private void UpdateDisplayData(List<Employee> employees)
        {
            // Get reference data
            var departments = _departmentRepository.GetAll().ToDictionary(d => d.Id, d => d.DepartmentName);
            var positions = _positionRepository.GetAll().ToDictionary(p => p.Id, p => p.PositionName);

            var displayData = employees.Select(e => new
            {
                e.Id,
                e.EmployeeCode,
                e.FullName,
                e.Email,
                Phone = string.IsNullOrWhiteSpace(e.Phone) ? "-" : e.Phone,
                DepartmentName = departments.ContainsKey(e.DepartmentId) ? departments[e.DepartmentId] : "N/A",
                PositionName = positions.ContainsKey(e.PositionId) ? positions[e.PositionId] : "N/A",
                HireDateFormatted = e.HireDate.ToString("dd/MM/yyyy"),
                e.Status,
                Employee = e // Keep reference
            }).ToList();

            dataGridView.DataSource = displayData;
        }

        protected override void OnSearch(string keyword)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Override to extract Employee from anonymous object
        /// </summary>
        protected override Employee? GetSelectedEntity()
        {
            if (dataGridView.CurrentRow?.DataBoundItem != null)
            {
                dynamic item = dataGridView.CurrentRow.DataBoundItem;
                return item.Employee as Employee;
            }
            return null;
        }

        protected override void OnAdd()
        {
            using (var form = new EmployeeEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnEdit(Employee entity)
        {
            using (var form = new EmployeeEditForm(entity))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnDelete(Employee entity)
        {
            try
            {
                if (_repository.Delete(entity.Id))
                {
                    ShowSuccess("Xóa nhân viên thành công!");
                    LoadData();
                }
                else
                {
                    ShowError("Không thể xóa nhân viên. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi xóa: {ex.Message}\n\nLưu ý: Không thể xóa nhân viên đang có user hoặc liên kết dữ liệu khác.");
            }
        }
    }
}
