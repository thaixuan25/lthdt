using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Services;

namespace LTHDT2.Forms
{
    /// <summary>
    /// JobPostingListForm - Danh sách tin tuyển dụng
    /// Kế thừa BaseListForm<JobPosting> (Inheritance)
    /// </summary>
    public class JobPostingListForm : BaseListForm<JobPosting>
    {
        private readonly JobPostingService _jobPostingService;
        private readonly DepartmentRepository _departmentRepository;
        private readonly PositionRepository _positionRepository;

        private ComboBox cmbStatusFilter = null!;

        public JobPostingListForm()
        {
            var headcountService = new HeadcountService();
            _jobPostingService = new JobPostingService(headcountService);
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
        }

        protected override string GetFormTitle()
        {
            return "Quản lý Tin tuyển dụng";
        }

        protected override void SetupDataGridView()
        {
            // Add status filter
            var lblStatus = CreateLabel("Trạng thái:", 540, 18, 80);
            cmbStatusFilter = new ComboBox
            {
                Location = new System.Drawing.Point(625, 15),
                Size = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatusFilter.Items.AddRange(new[] { "(Tất cả)", "Draft", "Active", "Closed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            searchPanel.Controls.Add(lblStatus);
            searchPanel.Controls.Add(cmbStatusFilter);

            // Setup columns
            dataGridView.AutoGenerateColumns = false;
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "JobCode",
                HeaderText = "Mã tin",
                Name = "JobCode",
                Width = 100
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "JobTitle",
                HeaderText = "Tiêu đề",
                Name = "JobTitle",
                Width = 250
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
                DataPropertyName = "NumberOfPositions",
                HeaderText = "Số lượng",
                Name = "NumberOfPositions",
                Width = 80
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PostDateFormatted",
                HeaderText = "Ngày đăng",
                Name = "PostDate",
                Width = 100
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DeadlineFormatted",
                HeaderText = "Hạn nộp",
                Name = "Deadline",
                Width = 100
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
                var jobPostings = _jobPostingService.GetAll().ToList();
                
                allData = jobPostings;
                filteredData = new List<JobPosting>(jobPostings);
                
                UpdateDisplayData(filteredData);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = allData.AsEnumerable();

            // Filter by status
            var selectedStatus = cmbStatusFilter.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "(Tất cả)")
            {
                filtered = filtered.Where(j => j.Status == selectedStatus);
            }

            // Apply search keyword
            var keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                filtered = filtered.Where(j =>
                    j.JobCode.ToLower().Contains(lowerKeyword) ||
                    j.JobTitle.ToLower().Contains(lowerKeyword)
                );
            }

            filteredData = filtered.ToList();
            UpdateDisplayData(filteredData);
        }

        private void UpdateDisplayData(List<JobPosting> jobPostings)
        {
            // Get reference data
            var departments = _departmentRepository.GetAll().ToDictionary(d => d.Id, d => d.DepartmentName);
            var positions = _positionRepository.GetAll().ToDictionary(p => p.Id, p => p.PositionName);

            var displayData = jobPostings.Select(j => new
            {
                j.Id,
                j.JobCode,
                j.JobTitle,
                DepartmentName = departments.ContainsKey(j.DepartmentId) ? departments[j.DepartmentId] : "N/A",
                PositionName = positions.ContainsKey(j.PositionId) ? positions[j.PositionId] : "N/A",
                j.NumberOfPositions,
                PostDateFormatted = j.PostDate.ToString("dd/MM/yyyy"),
                DeadlineFormatted = j.Deadline.ToString("dd/MM/yyyy"),
                j.Status,
                JobPosting = j // Keep reference
            }).ToList();

            dataGridView.DataSource = displayData;
        }

        protected override void OnSearch(string keyword)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Override to extract JobPosting from anonymous object
        /// </summary>
        protected override JobPosting? GetSelectedEntity()
        {
            if (dataGridView.CurrentRow?.DataBoundItem != null)
            {
                dynamic item = dataGridView.CurrentRow.DataBoundItem;
                return item.JobPosting as JobPosting;
            }
            return null;
        }

        protected override void OnAdd()
        {
            using (var form = new JobPostingEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnEdit(JobPosting entity)
        {
            using (var form = new JobPostingEditForm(entity))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnDelete(JobPosting entity)
        {
            try
            {
                if (_jobPostingService.DeleteJobPosting(entity.Id))
                {
                    ShowSuccess("Xóa tin tuyển dụng thành công!");
                    LoadData();
                }
                else
                {
                    ShowError("Không thể xóa tin tuyển dụng. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi xóa: {ex.Message}\n\nLưu ý: Không thể xóa tin tuyển dụng đã có ứng viên nộp đơn.");
            }
        }
    }
}
