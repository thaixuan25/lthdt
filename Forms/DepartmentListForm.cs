using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    /// <summary>
    /// DepartmentListForm - Danh sách phòng ban
    /// Kế thừa BaseListForm<Department> (Inheritance)
    /// </summary>
    public class DepartmentListForm : BaseListForm<Department>
    {
        private readonly DepartmentRepository _repository;

        public DepartmentListForm()
        {
            _repository = new DepartmentRepository();
        }

        protected override string GetFormTitle()
        {
            return "Quản lý Phòng ban";
        }

        protected override void SetupDataGridView()
        {
            // Setup columns
            dataGridView.AutoGenerateColumns = false;
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "DepartmentCode",
                HeaderText = "Mã phòng ban",
                Name = "DepartmentCode",
                Width = 120
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "DepartmentName",
                HeaderText = "Tên phòng ban",
                Name = "DepartmentName",
                Width = 200
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "ManagerName",
                HeaderText = "Quản lý",
                Name = "ManagerName",
                Width = 150
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "Location",
                HeaderText = "Địa điểm",
                Name = "Location",
                Width = 150
            });
        }

        protected override void LoadData()
        {
            try
            {
                var departments = _repository.GetAll().ToList();
                
                // Map to display model
                var displayData = departments.Select(d => new
                {
                    d.Id,
                    d.DepartmentCode,
                    d.DepartmentName,
                    ManagerName = string.IsNullOrWhiteSpace(d.ManagerName) ? "(Chưa có)" : d.ManagerName,
                    Location = string.IsNullOrWhiteSpace(d.Location) ? "(Chưa cập nhật)" : d.Location,
                    Department = d // Keep reference
                }).ToList();

                allData = departments;
                filteredData = new List<Department>(departments);
                dataGridView.DataSource = displayData;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        protected override void OnSearch(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                filteredData = new List<Department>(allData);
            }
            else
            {
                var lowerKeyword = keyword.ToLower();
                filteredData = allData.Where(d =>
                    d.DepartmentCode.ToLower().Contains(lowerKeyword) ||
                    d.DepartmentName.ToLower().Contains(lowerKeyword) ||
                    (d.ManagerName?.ToLower().Contains(lowerKeyword) ?? false) ||
                    (d.Location?.ToLower().Contains(lowerKeyword) ?? false)
                ).ToList();
            }

            // Update display
            var displayData = filteredData.Select(d => new
            {
                d.Id,
                d.DepartmentCode,
                d.DepartmentName,
                ManagerName = string.IsNullOrWhiteSpace(d.ManagerName) ? "(Chưa có)" : d.ManagerName,
                Location = string.IsNullOrWhiteSpace(d.Location) ? "(Chưa cập nhật)" : d.Location,
                Department = d
            }).ToList();

            dataGridView.DataSource = displayData;
        }

        /// <summary>
        /// Override to extract Department from anonymous object
        /// </summary>
        protected override Department? GetSelectedEntity()
        {
            if (dataGridView.CurrentRow?.DataBoundItem != null)
            {
                dynamic item = dataGridView.CurrentRow.DataBoundItem;
                return item.Department as Department;
            }
            return null;
        }

        protected override void OnAdd()
        {
            using (var form = new DepartmentEditForm())
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnEdit(Department entity)
        {
            using (var form = new DepartmentEditForm(entity))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnDelete(Department entity)
        {
            try
            {
                if (_repository.Delete(entity.Id))
                {
                    ShowSuccess("Xóa phòng ban thành công!");
                    LoadData();
                }
                else
                {
                    ShowError("Không thể xóa phòng ban. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi xóa: {ex.Message}\n\nLưu ý: Không thể xóa phòng ban đang có nhân viên.");
            }
        }
    }
}
