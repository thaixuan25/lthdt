using System;
using System.Linq;
using LTHDT2.Models;
using LTHDT2.Services;

namespace LTHDT2.Forms
{
    /// <summary>
    /// Form quản lý Đợt tuyển dụng (Recruitment Campaigns)
    /// Kế thừa BaseListForm với Polymorphism
    /// </summary>
    public class RecruitmentCampaignListForm : BaseListForm<RecruitmentCampaign>
    {
        private readonly RecruitmentCampaignService _service;

        public RecruitmentCampaignListForm()
        {
            _service = new RecruitmentCampaignService();
        }

        protected override string GetFormTitle()
        {
            return "Quản lý Đợt tuyển dụng";
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
                DataPropertyName = "CampaignCode",
                HeaderText = "Mã đợt",
                Name = "CampaignCode",
                Width = 100
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "CampaignName",
                HeaderText = "Tên đợt",
                Name = "CampaignName",
                Width = 250
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = "Mô tả",
                Name = "Description",
                Width = 200
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "StartDate",
                HeaderText = "Ngày bắt đầu",
                Name = "StartDate",
                Width = 120
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                DataPropertyName = "EndDate",
                HeaderText = "Ngày kết thúc",
                Name = "EndDate",
                Width = 120
            });
            
            dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
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
                var campaigns = _service.GetAllCampaigns().ToList();
                
                allData = campaigns;
                filteredData = new System.Collections.Generic.List<RecruitmentCampaign>(campaigns);
                dataGridView.DataSource = campaigns;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        protected override void OnAdd()
        {
            using (var form = new RecruitmentCampaignEditForm())
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnEdit(RecruitmentCampaign entity)
        {
            using (var form = new RecruitmentCampaignEditForm(entity))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        protected override void OnDelete(RecruitmentCampaign entity)
        {
            try
            {
                bool deleted = _service.DeleteCampaign(entity.Id);
                if (deleted)
                {
                    ShowSuccess("Xóa đợt tuyển dụng thành công!");
                    LoadData();
                }
                else
                {
                    ShowError("Không thể xóa đợt tuyển dụng!");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
            }
        }
    }
}

