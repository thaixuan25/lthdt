using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LTHDT2.Services;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    /// <summary>
    /// CandidateListForm - Danh sách ứng viên
    /// Kế thừa BaseListForm<Candidate> (Inheritance)
    /// </summary>
    public class CandidateListForm : BaseListForm<Candidate>
    {
        private readonly CandidateService _service;
        private Button btnViewCV = null!;

        public CandidateListForm()
        {
            _service = new CandidateService();
        }

        protected override string GetFormTitle()
        {
            return "Quản lý Hồ sơ Ứng viên";
        }

        protected override void SetupDataGridView()
        {
            // Add View CV button to toolbar
            btnViewCV = CreateStyledButton("📄 Xem CV", 560, 7, 120, 35);
            btnViewCV.Click += BtnViewCV_Click;
            btnViewCV.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            toolbarPanel.Controls.Add(btnViewCV);

            // Setup columns
            dataGridView.AutoGenerateColumns = false;
            
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
                DataPropertyName = "YearsOfExperience",
                HeaderText = "Kinh nghiệm (năm)",
                Name = "YearsOfExperience",
                Width = 120
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Skills",
                HeaderText = "Kĩ năng",
                Name = "Skills",
                Width = 150
            });
            
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Education",
                HeaderText = "Học vấn",
                Name = "Education",
                Width = 150
            });
            
            dataGridView.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "HasCV",
                HeaderText = "Có CV",
                Name = "HasCV",
                Width = 70
            });
        }

        protected override void LoadData()
        {
            try
            {
                var candidates = _service.GetAll().ToList();
                
                // Map to display model
                var displayData = candidates.Select(c => new
                {
                    c.Id,
                    c.FullName,
                    c.Email,
                    Phone = string.IsNullOrWhiteSpace(c.Phone) ? "-" : c.Phone,
                    YearsOfExperience = c.WorkExperience?.ToString() ?? "0",
                    Skills = string.IsNullOrWhiteSpace(c.Skills) ? "-" : c.Skills,
                    Education = string.IsNullOrWhiteSpace(c.Education) ? "-" : c.Education,
                    HasCV = c.HasResume(),
                    Candidate = c // Keep reference
                }).ToList();

                allData = candidates;
                filteredData = new List<Candidate>(candidates);
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
                filteredData = new List<Candidate>(allData);
            }
            else
            {
                var lowerKeyword = keyword.ToLower();
                filteredData = allData.Where(c =>
                    c.FullName.ToLower().Contains(lowerKeyword) ||
                    c.Email.ToLower().Contains(lowerKeyword) ||
                    (c.Phone?.ToLower().Contains(lowerKeyword) ?? false) ||
                    (c.Skills?.ToLower().Contains(lowerKeyword) ?? false)
                ).ToList();
            }

            // Update display
            var displayData = filteredData.Select(c => new
            {
                c.Id,
                c.FullName,
                c.Email,
                Phone = string.IsNullOrWhiteSpace(c.Phone) ? "-" : c.Phone,
                YearsOfExperience = c.WorkExperience?.ToString() ?? "0",
                Skills = string.IsNullOrWhiteSpace(c.Skills) ? "-" : c.Skills,
                Education = string.IsNullOrWhiteSpace(c.Education) ? "-" : c.Education,
                HasCV = c.HasResume(),
                Candidate = c
            }).ToList();

            dataGridView.DataSource = displayData;
        }

        /// <summary>
        /// Override to extract Candidate from anonymous object
        /// </summary>
        protected override Candidate? GetSelectedEntity()
        {
            if (dataGridView.CurrentRow?.DataBoundItem != null)
            {
                dynamic item = dataGridView.CurrentRow.DataBoundItem;
                return item.Candidate as Candidate;
            }
            return null;
        }

        protected override void OnAdd()
        {
            using (var form = new CandidateEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnEdit(Candidate entity)
        {
            using (var form = new CandidateEditForm(entity))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnDelete(Candidate entity)
        {
            try
            {
                if (_service.DeleteCandidate(entity.Id))
                {
                    ShowSuccess("Xóa ứng viên thành công!");
                    LoadData();
                }
                else
                {
                    ShowError("Không thể xóa ứng viên. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi xóa: {ex.Message}");
            }
        }

        private void BtnViewCV_Click(object? sender, EventArgs e)
        {
            var candidate = GetSelectedEntity();
            if (candidate == null)
            {
                ShowWarning("Vui lòng chọn một ứng viên!");
                return;
            }

            if (!candidate.HasResume())
            {
                ShowWarning("Ứng viên này chưa có CV!");
                return;
            }

            try
            {
                if (File.Exists(candidate.ResumeFilePath))
                {
                    // Open CV file with default application
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = candidate.ResumeFilePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    ShowError($"Không tìm thấy file CV:\n{candidate.ResumeFilePath}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi mở file CV: {ex.Message}");
            }
        }
    }
}
