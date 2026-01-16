using System;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    /// <summary>
    /// RecruitmentCampaignEditForm - Form thêm/sửa đợt tuyển dụng
    /// Kế thừa BaseEditForm<RecruitmentCampaign>
    /// </summary>
    public class RecruitmentCampaignEditForm : BaseEditForm<RecruitmentCampaign>
    {
        private readonly RecruitmentCampaignRepository _repository;

        private TextBox txtCode = null!;
        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private DateTimePicker dtpStartDate = null!;
        private DateTimePicker dtpEndDate = null!;
        private ComboBox cmbStatus = null!;
        private NumericUpDown numBudget = null!;

        public RecruitmentCampaignEditForm() : base()
        {
            _repository = new RecruitmentCampaignRepository();
        }

        public RecruitmentCampaignEditForm(RecruitmentCampaign campaign) : base(campaign)
        {
            _repository = new RecruitmentCampaignRepository();
        }

        protected override string GetEntityName()
        {
            return "Đợt tuyển dụng";
        }

        protected override void InitializeFormControls()
        {
            int startY = 20;
            int spacing = 45;
            int currentY = startY;

            // Campaign Code
            AddLabelAndTextBox("Mã đợt:", ref txtCode, currentY);
            txtCode.MaxLength = 20;
            txtCode.PlaceholderText = "VD: RC-2026-Q1";
            currentY += spacing;

            // Campaign Name
            AddLabelAndTextBox("Tên đợt:", ref txtName, currentY);
            txtName.MaxLength = 200;
            txtName.PlaceholderText = "VD: Tuyển dụng quý 1/2026";
            currentY += spacing;

            // Description
            AddLabelAndTextBox("Mô tả:", ref txtDescription, currentY, true, 80);
            txtDescription.MaxLength = 500;
            txtDescription.PlaceholderText = "Mô tả chi tiết về đợt tuyển dụng...";
            currentY += 80 + 10;

            // Start Date
            AddLabelAndDateTimePicker("Ngày bắt đầu:", ref dtpStartDate, currentY);
            dtpStartDate.Value = DateTime.Today;
            currentY += spacing;

            // End Date
            AddLabelAndDateTimePicker("Ngày kết thúc:", ref dtpEndDate, currentY);
            dtpEndDate.Value = DateTime.Today.AddMonths(3);
            currentY += spacing;

            // Budget
            AddLabelAndNumericUpDown("Ngân sách (VNĐ):", ref numBudget, currentY, 0, 999999999999, 200);
            numBudget.Increment = 10000000; // 10 triệu
            numBudget.ThousandsSeparator = true;
            numBudget.Value = 0;
            currentY += spacing;

            // Status
            AddLabelAndComboBox("Trạng thái:", ref cmbStatus, currentY, 200);
            cmbStatus.Items.AddRange(new[] { "Draft", "Active", "Completed", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            currentY += spacing;

            // Info label
            var lblInfo = new Label
            {
                Text = "💡 Ngân sách có thể để 0 nếu chưa xác định",
                Location = new System.Drawing.Point(180, currentY),
                Size = new System.Drawing.Size(400, 25),
                ForeColor = System.Drawing.Color.FromArgb(127, 140, 141),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic)
            };
            mainPanel.Controls.Add(lblInfo);
        }

        protected override void LoadEntity()
        {
            try
            {
                txtCode.Text = Entity.CampaignCode;
                txtName.Text = Entity.CampaignName;
                txtDescription.Text = Entity.Description ?? "";
                dtpStartDate.Value = Entity.StartDate;
                dtpEndDate.Value = Entity.EndDate;
                numBudget.Value = Entity.Budget;
                
                // Select status
                int statusIndex = cmbStatus.Items.IndexOf(Entity.Status);
                if (statusIndex >= 0)
                    cmbStatus.SelectedIndex = statusIndex;

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
            // Validate Code
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                ShowWarning("Vui lòng nhập mã đợt tuyển dụng!");
                txtCode.Focus();
                return false;
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text.Trim().Length < 5)
            {
                ShowWarning("Tên đợt tuyển dụng phải có ít nhất 5 ký tự!");
                txtName.Focus();
                return false;
            }

            // Validate Date Range
            if (dtpEndDate.Value <= dtpStartDate.Value)
            {
                ShowWarning("Ngày kết thúc phải sau ngày bắt đầu!");
                dtpEndDate.Focus();
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

        protected override void SaveEntity()
        {
            try
            {
                // Map data from controls to entity
                Entity.CampaignCode = txtCode.Text.Trim().ToUpper();
                Entity.CampaignName = txtName.Text.Trim();
                Entity.Description = string.IsNullOrWhiteSpace(txtDescription.Text) 
                    ? null 
                    : txtDescription.Text.Trim();
                Entity.StartDate = dtpStartDate.Value.Date;
                Entity.EndDate = dtpEndDate.Value.Date;
                Entity.Budget = numBudget.Value;
                Entity.Status = cmbStatus.SelectedItem!.ToString()!;

                // Save to database
                if (IsEditMode)
                {
                    if (!_repository.Update(Entity))
                    {
                        throw new Exception("Không thể cập nhật đợt tuyển dụng");
                    }
                }
                else
                {
                    // Set CreatedBy for new campaign
                    Entity.CreatedBy = CurrentUser?.EmployeeId ?? 0;
                    
                    if (Entity.CreatedBy <= 0)
                    {
                        throw new Exception("Không tìm thấy thông tin Employee của user hiện tại.\nVui lòng liên hệ Admin để gắn User với Employee.");
                    }

                    var id = _repository.Add(Entity);
                    if (id <= 0)
                    {
                        throw new Exception("Không thể thêm đợt tuyển dụng");
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

