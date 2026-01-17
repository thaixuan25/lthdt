using System;
using System.Linq;
using System.Windows.Forms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Utils;
using AppModel = LTHDT2.Models.Application;

namespace LTHDT2.Forms
{
    /// <summary>
    /// ApplicationEditForm - Form thêm/sửa đơn ứng tuyển
    /// Form phức tạp nhất trong hệ thống
    /// Kế thừa BaseEditForm<Application>
    /// </summary>
    public class ApplicationEditForm : BaseEditForm<AppModel>
    {
        private readonly ApplicationRepository _repository;
        private readonly CandidateRepository _candidateRepository;
        private readonly JobPostingRepository _jobPostingRepository;

        private ComboBox cmbCandidate = null!;
        private ComboBox cmbJobPosting = null!;
        private DateTimePicker dtpApplyDate = null!;
        private TextBox txtCoverLetter = null!;
        private ComboBox cmbSource = null!;
        private ComboBox cmbStatus = null!;
        private NumericUpDown numScore = null!;
        private TextBox txtNotes = null!;

        public ApplicationEditForm() : base()
        {
            _repository = new ApplicationRepository();
            _candidateRepository = new CandidateRepository();
            _jobPostingRepository = new JobPostingRepository();
        }

        public ApplicationEditForm(AppModel application) : base(application)
        {
            _repository = new ApplicationRepository();
            _candidateRepository = new CandidateRepository();
            _jobPostingRepository = new JobPostingRepository();
        }

        protected override string GetEntityName()
        {
            return "Đơn ứng tuyển";
        }

        protected override void InitializeFormControls()
        {
            this.Size = new System.Drawing.Size(800, 650);

            int startY = 20;
            int spacing = 45;
            int currentY = startY;

            AddLabelAndComboBox("Ứng viên:", ref cmbCandidate, currentY, 550);
            LoadCandidates();
            currentY += spacing;

            AddLabelAndComboBox("Vị trí ứng tuyển:", ref cmbJobPosting, currentY, 550);
            LoadJobPostings();
            currentY += spacing;

            AddLabelAndDateTimePicker("Ngày nộp:", ref dtpApplyDate, currentY, 200);
            dtpApplyDate.Value = DateTime.Now;
            currentY += spacing;

            AddLabelAndTextBox("Thư xin việc:", ref txtCoverLetter, currentY, true, 80);
            currentY += 80 + 10;

            AddLabelAndComboBox("Nguồn ứng tuyển:", ref cmbSource, currentY, 200);
            cmbSource.Items.AddRange(new[] { 
                "Website", "LinkedIn", "Facebook", "Referral", "Job Fair", "Email", "Other" 
            });
            cmbSource.SelectedIndex = 0;
            currentY += spacing;

            AddLabelAndComboBox("Trạng thái:", ref cmbStatus, currentY, 200);
            cmbStatus.Items.AddRange(new[] { 
                "Nộp đơn", "Sơ tuyển", "Phỏng vấn vòng 1", 
                "Phỏng vấn vòng 2", "Phỏng vấn vòng 3", "Đạt", "Không đạt" 
            });
            cmbStatus.SelectedIndex = 0;
            currentY += spacing;

            AddLabelAndNumericUpDown("Điểm đánh giá:", ref numScore, currentY, 0, 100, 100);
            numScore.Value = 0;
            numScore.Enabled = false;
            numScore.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            currentY += spacing;

            AddLabelAndTextBox("Ghi chú HR:", ref txtNotes, currentY, true, 80);
            currentY += 80 + 10;

            var lblInfo = new Label
            {
                Text = "💡 Điểm từ 0-100. Thay đổi trạng thái sẽ gửi email thông báo cho ứng viên",
                Location = new System.Drawing.Point(180, currentY),
                Size = new System.Drawing.Size(500, 25),
                ForeColor = System.Drawing.Color.FromArgb(127, 140, 141),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic)
            };
            mainPanel.Controls.Add(lblInfo);
        }

        private void LoadCandidates()
        {
            try
            {
                var candidates = _candidateRepository.GetAll().ToList();
                cmbCandidate.Items.Clear();
                foreach (var candidate in candidates)
                {
                    cmbCandidate.Items.Add(new { 
                        Id = candidate.Id, 
                        Display = $"{candidate.FullName} ({candidate.Email})" 
                    });
                }
                cmbCandidate.DisplayMember = "Display";
                cmbCandidate.ValueMember = "Id";

                if (cmbCandidate.Items.Count > 0 && !IsEditMode)
                {
                    cmbCandidate.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load ứng viên: {ex.Message}");
            }
        }

        private void LoadJobPostings()
        {
            try
            {
                var jobPostings = _jobPostingRepository.GetAll()
                    .Where(j => j.Status == "Active")
                    .ToList();
                    
                cmbJobPosting.Items.Clear();
                foreach (var job in jobPostings)
                {
                    cmbJobPosting.Items.Add(new { 
                        Id = job.Id, 
                        Display = $"{job.JobCode} - {job.JobTitle}" 
                    });
                }
                cmbJobPosting.DisplayMember = "Display";
                cmbJobPosting.ValueMember = "Id";

                if (cmbJobPosting.Items.Count > 0 && !IsEditMode)
                {
                    cmbJobPosting.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load tin tuyển dụng: {ex.Message}");
            }
        }

        protected override void LoadEntity()
        {
            try
            {
                for (int i = 0; i < cmbCandidate.Items.Count; i++)
                {
                    dynamic item = cmbCandidate.Items[i]!;
                    if (item.Id == Entity.CandidateId)
                    {
                        cmbCandidate.SelectedIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < cmbJobPosting.Items.Count; i++)
                {
                    dynamic item = cmbJobPosting.Items[i]!;
                    if (item.Id == Entity.JobPostingId)
                    {
                        cmbJobPosting.SelectedIndex = i;
                        break;
                    }
                }

                dtpApplyDate.Value = Entity.ApplyDate;
                txtCoverLetter.Text = Entity.CoverLetter ?? "";
                
                if (!string.IsNullOrEmpty(Entity.Source))
                {
                    for (int i = 0; i < cmbSource.Items.Count; i++)
                    {
                        if (cmbSource.Items[i].ToString() == Entity.Source)
                        {
                            cmbSource.SelectedIndex = i;
                            break;
                        }
                    }
                }

                for (int i = 0; i < cmbStatus.Items.Count; i++)
                {
                    if (cmbStatus.Items[i].ToString() == Entity.CurrentStatus)
                    {
                        cmbStatus.SelectedIndex = i;
                        break;
                    }
                }

                numScore.Value = Entity.Score;
                txtNotes.Text = Entity.Notes ?? "";

                cmbCandidate.Enabled = false;
                cmbJobPosting.Enabled = false;
                cmbCandidate.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
                cmbJobPosting.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        protected override bool ValidateInput()
        {
            if (cmbCandidate.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn ứng viên!");
                cmbCandidate.Focus();
                return false;
            }

            if (cmbJobPosting.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn vị trí ứng tuyển!");
                cmbJobPosting.Focus();
                return false;
            }

            if (cmbStatus.SelectedItem == null)
            {
                ShowWarning("Vui lòng chọn trạng thái!");
                cmbStatus.Focus();
                return false;
            }

            if (!IsEditMode)
            {
                dynamic candidate = cmbCandidate.SelectedItem!;
                dynamic jobPosting = cmbJobPosting.SelectedItem!;
                
                if (_repository.CheckDuplicateApplication((int)candidate.Id, (int)jobPosting.Id))
                {
                    ShowWarning("Ứng viên này đã nộp đơn cho vị trí này rồi!\n\nVui lòng kiểm tra lại.");
                    return false;
                }
            }

            return true;
        }

        protected override void SaveEntity()
        {
            try
            {
                dynamic selectedCandidate = cmbCandidate.SelectedItem!;
                Entity.CandidateId = (int)selectedCandidate.Id;
                
                dynamic selectedJobPosting = cmbJobPosting.SelectedItem!;
                Entity.JobPostingId = (int)selectedJobPosting.Id;
                
                Entity.ApplyDate = dtpApplyDate.Value.Date;
                Entity.CoverLetter = string.IsNullOrWhiteSpace(txtCoverLetter.Text) 
                    ? null 
                    : txtCoverLetter.Text.Trim();
                    
                Entity.Source = cmbSource.SelectedItem?.ToString();
                Entity.CurrentStatus = cmbStatus.SelectedItem!.ToString()!;
                Entity.Score = numScore.Value;
                Entity.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) 
                    ? null 
                    : txtNotes.Text.Trim();
                    
                Entity.UpdatedBy = SessionManager.CurrentEmployeeId;

                if (IsEditMode)
                {
                    if (!_repository.Update(Entity))
                    {
                        throw new Exception("Không thể cập nhật đơn ứng tuyển");
                    }
                }
                else
                {
                    var id = _repository.Add(Entity);
                    if (id <= 0)
                    {
                        throw new Exception("Không thể thêm đơn ứng tuyển");
                    }
                    Entity.Id = id;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lưu dữ liệu: {ex.Message}", ex);
            }
        }

        protected override void AfterSave()
        {
            base.AfterSave();
            
        }
    }
}

