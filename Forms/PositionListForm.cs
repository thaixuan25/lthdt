using System;
using System.Collections.Generic;
using System.Linq;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    /// <summary>
    /// PositionListForm - Danh sách vị trí công việc
    /// Kế thừa BaseListForm<Position> (Inheritance)
    /// </summary>
    public class PositionListForm : BaseListForm<Position>
    {
        private readonly PositionRepository _repository;

        public PositionListForm()
        {
            _repository = new PositionRepository();
        }

        protected override string GetFormTitle()
        {
            return "Quản lý Vị trí";
        }

        protected override void SetupDataGridView()
        {
            // Setup columns
            dataGridView.AutoGenerateColumns = false;
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "PositionCode",
                HeaderText = "Mã vị trí",
                Name = "PositionCode",
                Width = 120
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "PositionName",
                HeaderText = "Tên vị trí",
                Name = "PositionName",
                Width = 200
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "LevelName",
                HeaderText = "Cấp bậc",
                Name = "LevelName",
                Width = 100
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "MinSalaryFormatted",
                HeaderText = "Lương tối thiểu",
                Name = "MinSalary",
                Width = 150
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaxSalaryFormatted",
                HeaderText = "Lương tối đa",
                Name = "MaxSalary",
                Width = 150
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = "Mô tả",
                Name = "Description",
                Width = 200
            });
        }

        protected override void LoadData()
        {
            try
            {
                var positions = _repository.GetAll().ToList();
                
                // Map to display model
                var displayData = positions.Select(p => new
                {
                    p.Id,
                    p.PositionCode,
                    p.PositionName,
                    p.Description,
                    LevelName = p.GetLevelName(),
                    MinSalaryFormatted = p.MinSalary > 0 ? $"{p.MinSalary:N0} VNĐ" : "0 VNĐ",
                    MaxSalaryFormatted = p.MaxSalary > 0 ? $"{p.MaxSalary:N0} VNĐ" : "0 VNĐ",
                    Position = p // Keep reference for edit/delete
                }).ToList();

                allData = positions;
                filteredData = new List<Position>(positions);
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
                filteredData = new List<Position>(allData);
            }
            else
            {
                var lowerKeyword = keyword.ToLower();
                filteredData = allData.Where(p =>
                    p.PositionCode.ToLower().Contains(lowerKeyword) ||
                    p.PositionName.ToLower().Contains(lowerKeyword) ||
                    (p.Description?.ToLower().Contains(lowerKeyword) ?? false)
                ).ToList();
            }

            // Update display
            var displayData = filteredData.Select(p => new
            {
                p.Id,
                p.PositionCode,
                p.PositionName,
                p.Description,
                LevelName = p.GetLevelName(),
                MinSalaryFormatted = p.MinSalary > 0 ? $"{p.MinSalary:N0} VNĐ" : "0 VNĐ",
                MaxSalaryFormatted = p.MaxSalary > 0 ? $"{p.MaxSalary:N0} VNĐ" : "0 VNĐ",
                Position = p
            }).ToList();

            dataGridView.DataSource = displayData;
        }

        /// <summary>
        /// Override to extract Position from anonymous object
        /// </summary>
        protected override Position? GetSelectedEntity()
        {
            if (dataGridView.CurrentRow?.DataBoundItem != null)
            {
                dynamic item = dataGridView.CurrentRow.DataBoundItem;
                return item.Position as Position;
            }
            return null;
        }

        protected override void OnAdd()
        {
            using (var form = new PositionEditForm())
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnEdit(Position entity)
        {
            using (var form = new PositionEditForm(entity))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnDelete(Position entity)
        {
            try
            {
                if (_repository.Delete(entity.Id))
                {
                    ShowSuccess("Xóa vị trí thành công!");
                    LoadData();
                }
                else
                {
                    ShowError("Không thể xóa vị trí. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi xóa: {ex.Message}\n\nLưu ý: Không thể xóa vị trí đang được sử dụng.");
            }
        }
    }
}
