using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Utils;
using AppModel = LTHDT2.Models.Application;

namespace LTHDT2.Forms
{
    /// <summary>
    /// ApplicationListForm - Danh sách đơn ứng tuyển
    /// Form phức tạp nhất, core của hệ thống recruitment
    /// Kế thừa BaseListForm<Application>
    /// </summary>
    public class ApplicationListForm : BaseListForm<AppModel>
    {
        private readonly ApplicationRepository _repository;
        private readonly JobPostingRepository _jobPostingRepository;
        
        private Guna2ComboBox cmbStatusFilter = null!;
        private Guna2ComboBox cmbJobPostingFilter = null!;

        public ApplicationListForm()
        {
            _repository = new ApplicationRepository();
            _jobPostingRepository = new JobPostingRepository();
        }

        protected override string GetFormTitle()
        {
            return "Quản lý Đơn ứng tuyển";
        }

        protected override void SetupDataGridView()
        {
            searchPanel.Height = 120;
            searchPanel.Padding = new Padding(15, 15, 15, 15);

            searchPanel.Controls.Clear();

            var lblSearch = UITheme.CreateLabel("Tìm kiếm:", UITheme.BodyBold);
            lblSearch.Location = new Point(15, 20);
            lblSearch.Size = new Size(100, 25);
            searchPanel.Controls.Add(lblSearch);

            // Move txtSearch to proper position
            txtSearch.Location = new Point(120, 18);
            txtSearch.Size = new Size(400, UITheme.InputHeight);
            searchPanel.Controls.Add(txtSearch);

            var lblJobPosting = UITheme.CreateLabel("Tin tuyển dụng:", UITheme.BodyBold);
            lblJobPosting.Location = new Point(15, 75);
            lblJobPosting.Size = new Size(120, 25);
            searchPanel.Controls.Add(lblJobPosting);

            cmbJobPostingFilter = UITheme.CreateComboBox(40);
            cmbJobPostingFilter.Location = new Point(140, 72);
            cmbJobPostingFilter.Size = new Size(300, 40);
            cmbJobPostingFilter.SelectedIndexChanged += (s, e) => ApplyFilters();
            searchPanel.Controls.Add(cmbJobPostingFilter);

            var lblStatus = UITheme.CreateLabel("Trạng thái:", UITheme.BodyBold);
            lblStatus.Location = new Point(450, 75);
            lblStatus.Size = new Size(80, 25);
            searchPanel.Controls.Add(lblStatus);

            cmbStatusFilter = UITheme.CreateComboBox(40);
            cmbStatusFilter.Location = new Point(535, 72);
            cmbStatusFilter.Size = new Size(200, 40);
            cmbStatusFilter.Items.AddRange(new[] { 
                "(Tất cả)", "Nộp đơn", "Sơ tuyển", "Phỏng vấn vòng 1", 
                "Phỏng vấn vòng 2", "Phỏng vấn vòng 3", "Đạt", "Không đạt" 
            });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => ApplyFilters();
            searchPanel.Controls.Add(cmbStatusFilter);

            dataGridView.AutoGenerateColumns = false;
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ApplyDate",
                HeaderText = "Ngày nộp",
                Name = "ApplyDate",
                Width = 100,
                DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CandidateName",
                HeaderText = "Ứng viên",
                Name = "CandidateName",
                Width = 180
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CandidateEmail",
                HeaderText = "Email",
                Name = "CandidateEmail",
                Width = 180
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "JobTitle",
                HeaderText = "Vị trí ứng tuyển",
                Name = "JobTitle",
                Width = 200
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CurrentStatus",
                HeaderText = "Trạng thái",
                Name = "CurrentStatus",
                Width = 130
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Score",
                HeaderText = "Điểm",
                Name = "Score",
                Width = 80,
                DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle { Format = "F0" }
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Source",
                HeaderText = "Nguồn",
                Name = "Source",
                Width = 100
            });
        }

        protected override void LoadData()
        {
            try
            {
                var applications = _repository.GetAll().ToList();
                
                LoadFilters();

                allData = applications;
                filteredData = new List<AppModel>(applications);
                
                UpdateDisplayData(filteredData);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}\n\nStack trace:\n{ex.StackTrace}");
            }
        }

        private void LoadFilters()
        {
            try
            {
                var jobPostings = _jobPostingRepository.GetAll().ToList();
                cmbJobPostingFilter.Items.Clear();
                cmbJobPostingFilter.Items.Add(new { Id = 0, Display = "(Tất cả)" });
                foreach (var job in jobPostings)
                {
                    cmbJobPostingFilter.Items.Add(new { Id = job.Id, Display = $"{job.JobCode} - {job.JobTitle}" });
                }
                cmbJobPostingFilter.DisplayMember = "Display";
                cmbJobPostingFilter.ValueMember = "Id";
                cmbJobPostingFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load filters: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = allData.AsEnumerable();

            if (cmbJobPostingFilter.SelectedItem != null)
            {
                dynamic jobItem = cmbJobPostingFilter.SelectedItem;
                if (jobItem.Id > 0)
                {
                    filtered = filtered.Where(a => a.JobPostingId == jobItem.Id);
                }
            }

            var selectedStatus = cmbStatusFilter.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "(Tất cả)")
            {
                filtered = filtered.Where(a => a.CurrentStatus == selectedStatus);
            }

            var keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                filtered = filtered.Where(a =>
                    (a.Candidate?.FullName?.ToLower().Contains(lowerKeyword) ?? false) ||
                    (a.Candidate?.Email?.ToLower().Contains(lowerKeyword) ?? false) ||
                    (a.JobPosting?.JobTitle?.ToLower().Contains(lowerKeyword) ?? false)
                );
            }

            filteredData = filtered.ToList();
            UpdateDisplayData(filteredData);
        }

        private void UpdateDisplayData(List<AppModel> applications)
        {
            dataGridView.DataSource = null;
            dataGridView.DataSource = applications;
        }
        
        /// <summary>
        /// Override to handle Application entity correctly
        /// </summary>
        protected override AppModel? GetSelectedEntity()
        {
            if (dataGridView.CurrentRow?.DataBoundItem is AppModel entity)
            {
                return entity;
            }
            return null;
        }

        protected override void OnSearch(string keyword)
        {
            ApplyFilters();
        }

        protected override void OnAdd()
        {
            using (var form = new ApplicationEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnEdit(AppModel entity)
        {
            using (var form = new ApplicationEditForm(entity))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnDelete(AppModel entity)
        {
            try
            {
                if (_repository.Delete(entity.Id))
                {
                    ShowSuccess("Xóa đơn ứng tuyển thành công!");
                    LoadData();
                }
                else
                {
                    ShowError("Không thể xóa đơn ứng tuyển. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi xóa: {ex.Message}");
            }
        }
    }
}
