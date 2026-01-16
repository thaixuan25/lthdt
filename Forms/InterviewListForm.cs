using System;
using System.Linq;
using LTHDT2.Models;
using LTHDT2.Services;

namespace LTHDT2.Forms
{
    /// <summary>
    /// Form quản lý Lịch phỏng vấn (Interviews)
    /// Kế thừa BaseListForm với Polymorphism
    /// </summary>
    public class InterviewListForm : BaseListForm<Interview>
    {
        private readonly InterviewService _service;

        public InterviewListForm()
        {
            var emailService = new EmailService();
            _service = new InterviewService(emailService);
        }

        protected override string GetFormTitle()
        {
            return "Quản lý Lịch phỏng vấn";
        }

        protected override void SetupDataGridView()
        {
            dataGridView.AutoGenerateColumns = false;
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "ID",
                Name = "Id",
                Width = 50,
                Visible = false
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "ApplicationId",
                HeaderText = "Mã đơn",
                Name = "ApplicationId",
                Width = 80
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "InterviewDate",
                HeaderText = "Ngày PV",
                Name = "InterviewDate",
                Width = 120
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "InterviewType",
                HeaderText = "Loại PV",
                Name = "InterviewType",
                Width = 100
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "InterviewRound",
                HeaderText = "Vòng PV",
                Name = "InterviewRound",
                Width = 80
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "InterviewerName",
                HeaderText = "Người PV",
                Name = "InterviewerName",
                Width = 150
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "Location",
                HeaderText = "Địa điểm",
                Name = "Location",
                Width = 150
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "Score",
                HeaderText = "Điểm",
                Name = "Score",
                Width = 70
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "Result",
                HeaderText = "Kết quả",
                Name = "Result",
                Width = 100
            });
        }

        protected override void LoadData()
        {
            try
            {
                var interviews = _service.GetAllInterviews()
                    .OrderByDescending(i => i.InterviewDate)
                    .ToList();

                allData = interviews;
                filteredData = new System.Collections.Generic.List<Interview>(interviews);
                dataGridView.DataSource = interviews;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        protected override void OnAdd()
        {
            using (var form = new InterviewEditForm())
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnEdit(Interview entity)
        {
            using (var form = new InterviewEditForm(entity))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnDelete(Interview entity)
        {
            try
            {
                bool deleted = _service.DeleteInterview(entity.Id);
                if (deleted)
                {
                    ShowSuccess("Xóa lịch phỏng vấn thành công!");
                    LoadData();
                }
                else
                {
                    ShowError("Không thể xóa lịch phỏng vấn!");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
            }
        }
    }
}

