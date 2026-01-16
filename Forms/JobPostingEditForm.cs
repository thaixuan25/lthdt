using System;
using System.Linq;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Services;

namespace LTHDT2.Forms
{
    /// <summary>
    /// JobPostingEditForm - Form thêm/sửa tin tuyển dụng
    /// Kế thừa BaseEditForm<JobPosting> (Inheritance)
    /// </summary>
    public class JobPostingEditForm : BaseEditForm<JobPosting>
    {
        private readonly JobPostingService _jobPostingService;
        private readonly DepartmentRepository _departmentRepository;
        private readonly PositionRepository _positionRepository;
        private readonly RecruitmentCampaignRepository _campaignRepository;
        private readonly IHeadcountService _headcountService;

        private TextBox txtCode = null!;
        private TextBox txtTitle = null!;
        private TextBox txtDescription = null!;
        private TextBox txtResponsibilities = null!;
        private TextBox txtRequirements = null!;
        private ComboBox cmbDepartment = null!;
        private ComboBox cmbPosition = null!;
        private ComboBox cmbCampaign = null!;
        private NumericUpDown numVacancyCount = null!;
        private NumericUpDown numMinSalary = null!;
        private NumericUpDown numMaxSalary = null!;
        private TextBox txtLocation = null!;
        private DateTimePicker dtpPostDate = null!;
        private DateTimePicker dtpDeadline = null!;
        private ComboBox cmbStatus = null!;
        private CheckBox chkHeadcountApproved = null!;

        public JobPostingEditForm() : base()
        {
            _headcountService = new HeadcountService();
            _jobPostingService = new JobPostingService(_headcountService);
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
            _campaignRepository = new RecruitmentCampaignRepository();
        }

        public JobPostingEditForm(JobPosting jobPosting) : base(jobPosting)
        {
            _headcountService = new HeadcountService();
            _jobPostingService = new JobPostingService(_headcountService);
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
            _campaignRepository = new RecruitmentCampaignRepository();
        }

        protected override string GetEntityName()
        {
            return "Tin tuyển dụng";
        }

        protected override void InitializeFormControls()
        {
            // Increase form height for more fields
            this.Size = new System.Drawing.Size(800, 800);

            int startY = 20;
            int spacing = 45;
            int currentY = startY;

            // Job Code
            AddLabelAndTextBox("Mã tin:", ref txtCode, currentY);
            txtCode.MaxLength = 20;
            currentY += spacing;

            // Job Title
            AddLabelAndTextBox("Tiêu đề:", ref txtTitle, currentY);
            txtTitle.MaxLength = 200;
            currentY += spacing;

            // Department
            AddLabelAndComboBox("Phòng ban:", ref cmbDepartment, currentY, 300);
            LoadDepartments();
            currentY += spacing;

            // Position
            AddLabelAndComboBox("Vị trí:", ref cmbPosition, currentY, 300);
            LoadPositions();
            currentY += spacing;

            // Campaign (optional)
            AddLabelAndComboBox("Đợt tuyển dụng:", ref cmbCampaign, currentY, 300);
            LoadCampaigns();
            currentY += spacing;

            // Vacancy Count
            AddLabelAndNumericUpDown("Số lượng cần tuyển:", ref numVacancyCount, currentY, 1, 100, 100);
            numVacancyCount.Value = 1;
            currentY += spacing;

            // Description
            AddLabelAndTextBox("Mô tả:", ref txtDescription, currentY, true, 60);
            currentY += 60 + 10;

            // Responsibilities
            AddLabelAndTextBox("Trách nhiệm:", ref txtResponsibilities, currentY, true, 60);
            currentY += 60 + 10;

            // Requirements
            AddLabelAndTextBox("Yêu cầu:", ref txtRequirements, currentY, true, 60);
            currentY += 60 + 10;

            // Min Salary
            AddLabelAndNumericUpDown("Lương tối thiểu:", ref numMinSalary, currentY, 0, 999999999, 200);
            numMinSalary.Increment = 1000000;
            numMinSalary.ThousandsSeparator = true;
            currentY += spacing;

            // Max Salary
            AddLabelAndNumericUpDown("Lương tối đa:", ref numMaxSalary, currentY, 0, 999999999, 200);
            numMaxSalary.Increment = 1000000;
            numMaxSalary.ThousandsSeparator = true;
            currentY += spacing;

            // Location
            AddLabelAndTextBox("Địa điểm làm việc:", ref txtLocation, currentY);
            txtLocation.MaxLength = 200;
            currentY += spacing;

            // Post Date
            AddLabelAndDateTimePicker("Ngày đăng:", ref dtpPostDate, currentY, 200);
            dtpPostDate.Value = DateTime.Now;
            currentY += spacing;

            // Deadline
            AddLabelAndDateTimePicker("Hạn nộp:", ref dtpDeadline, currentY, 200);
            dtpDeadline.Value = DateTime.Now.AddMonths(1);
            currentY += spacing;

            // Status
            AddLabelAndComboBox("Trạng thái:", ref cmbStatus, currentY, 200);
            cmbStatus.Items.AddRange(new[] { "Draft", "Active", "Closed", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            currentY += spacing;

            // Headcount Approved
            AddLabelAndCheckBox("Định biên:", ref chkHeadcountApproved, currentY, "Đã được phê duyệt");
            currentY += spacing*2;
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

        private void LoadCampaigns()
        {
            try
            {
                var campaigns = _campaignRepository.GetAll().ToList();
                cmbCampaign.Items.Clear();
                // Thêm option "Không có"
                cmbCampaign.Items.Add(new { Id = 0, Name = "(Không có)" });
                foreach (var campaign in campaigns)
                {
                    cmbCampaign.Items.Add(new { Id = campaign.Id, Name = $"{campaign.CampaignCode} - {campaign.CampaignName}" });
                }
                cmbCampaign.DisplayMember = "Name";
                cmbCampaign.ValueMember = "Id";
                // Mặc định chọn "Không có"
                cmbCampaign.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load đợt tuyển dụng: {ex.Message}");
            }
        }

        protected override void LoadEntity()
        {
            try
            {
                txtCode.Text = Entity.JobCode;
                txtTitle.Text = Entity.JobTitle;
                txtDescription.Text = Entity.JobDescription ?? "";
                txtResponsibilities.Text = Entity.Responsibilities ?? "";
                txtRequirements.Text = Entity.Requirements ?? "";
                numVacancyCount.Value = Entity.NumberOfPositions;
                numMinSalary.Value = Entity.MinSalary;
                numMaxSalary.Value = Entity.MaxSalary;
                txtLocation.Text = Entity.Location ?? "";
                dtpPostDate.Value = Entity.PostDate;
                dtpDeadline.Value = Entity.Deadline;
                chkHeadcountApproved.Checked = Entity.IsHeadcountApproved;

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

                // Select campaign
                if (Entity.CampaignId > 0)
                {
                    for (int i = 0; i < cmbCampaign.Items.Count; i++)
                    {
                        dynamic item = cmbCampaign.Items[i]!;
                        if (item.Id == Entity.CampaignId)
                        {
                            cmbCampaign.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    cmbCampaign.SelectedIndex = 0; // "(Không có)"
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
            // Validate Job Code
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                ShowWarning("Vui lòng nhập mã tin tuyển dụng!");
                txtCode.Focus();
                return false;
            }

            // Validate Job Title
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowWarning("Vui lòng nhập tiêu đề!");
                txtTitle.Focus();
                return false;
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

            // Validate Vacancy Count
            if (numVacancyCount.Value <= 0)
            {
                ShowWarning("Số lượng cần tuyển phải lớn hơn 0!");
                numVacancyCount.Focus();
                return false;
            }

            // Validate Salary Range
            if (numMaxSalary.Value > 0 && numMaxSalary.Value < numMinSalary.Value)
            {
                ShowWarning("Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu!");
                numMaxSalary.Focus();
                return false;
            }

            // Validate Deadline
            if (dtpDeadline.Value <= dtpPostDate.Value)
            {
                ShowWarning("Hạn nộp phải sau ngày đăng!");
                dtpDeadline.Focus();
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

        protected override bool BeforeSave()
        {
            // Check headcount when adding new or changing department/position/vacancy
            if (!IsEditMode || 
                (IsEditMode && (Entity.DepartmentId != ((dynamic)cmbDepartment.SelectedItem!).Id ||
                                Entity.PositionId != ((dynamic)cmbPosition.SelectedItem!).Id ||
                                Entity.NumberOfPositions != (int)numVacancyCount.Value)))
            {
                try
                {
                    dynamic dept = cmbDepartment.SelectedItem!;
                    dynamic pos = cmbPosition.SelectedItem!;
                    int deptId = dept.Id;
                    int posId = pos.Id;
                    int vacancyCount = (int)numVacancyCount.Value;

                    var headcountResult = _headcountService.CheckHeadcount(deptId, posId, vacancyCount);

                    if (!headcountResult.IsApproved)
                    {
                        var result = MessageBox.Show(
                            $"⚠️ CẢNH BÁO ĐỊNH BIÊN\n\n{headcountResult.Message}\n\n" +
                            $"Còn lại: {headcountResult.Remaining}/{headcountResult.Approved}\n" +
                            $"Yêu cầu: {vacancyCount}\n\n" +
                            $"Bạn có muốn tiếp tục đăng tin này không?\n" +
                            $"(Yêu cầu phê duyệt từ HR Manager)",
                            "Vượt định biên",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (result == DialogResult.No)
                        {
                            return false;
                        }

                        chkHeadcountApproved.Checked = false;
                    }
                    else
                    {
                        chkHeadcountApproved.Checked = true;
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Lỗi kiểm tra định biên: {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        protected override void SaveEntity()
        {
            try
            {
                // Map data from controls to entity
                Entity.JobCode = txtCode.Text.Trim().ToUpper();
                Entity.JobTitle = txtTitle.Text.Trim();
                Entity.JobDescription = txtDescription.Text.Trim();
                Entity.Responsibilities = txtResponsibilities.Text.Trim();
                Entity.Requirements = txtRequirements.Text.Trim();
                
                dynamic selectedDept = cmbDepartment.SelectedItem!;
                Entity.DepartmentId = (int)selectedDept.Id;
                
                dynamic selectedPos = cmbPosition.SelectedItem!;
                Entity.PositionId = (int)selectedPos.Id;
                
                // Campaign (có thể null/0)
                dynamic selectedCampaign = cmbCampaign.SelectedItem!;
                Entity.CampaignId = (int)selectedCampaign.Id; // 0 nếu chọn "(Không có)"
                
                Entity.NumberOfPositions = (int)numVacancyCount.Value;
                Entity.MinSalary = numMinSalary.Value;
                Entity.MaxSalary = numMaxSalary.Value;
                Entity.Location = txtLocation.Text.Trim();
                Entity.PostDate = dtpPostDate.Value.Date;
                Entity.Deadline = dtpDeadline.Value.Date;
                Entity.Status = cmbStatus.SelectedItem!.ToString()!;
                Entity.IsHeadcountApproved = chkHeadcountApproved.Checked;

                // Save to database using service
                if (IsEditMode)
                {
                    bool overrideHeadcount = !chkHeadcountApproved.Checked;
                    if (!_jobPostingService.UpdateJobPosting(Entity, overrideHeadcount))
                    {
                        throw new Exception("Không thể cập nhật tin tuyển dụng");
                    }
                }
                else
                {
                    bool overrideHeadcount = !chkHeadcountApproved.Checked;
                    var result = _jobPostingService.CreateJobPosting(Entity, overrideHeadcount);
                    
                    if (!result.Success)
                    {
                        throw new Exception(result.Message);
                    }
                    
                    if (result.JobPosting != null)
                    {
                        Entity.Id = result.JobPosting.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lưu dữ liệu: {ex.Message}", ex);
            }
        }
    }
}
