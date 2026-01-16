using System;
using System.Windows.Forms;
using LTHDT2.Services;
using LTHDT2.Models;

namespace LTHDT2.Forms
{
    public class CandidateEditForm : BaseEditForm<Candidate>
    {
        private readonly CandidateService _service;

        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private ComboBox cmbGender = null!;
        private DateTimePicker dtpDateOfBirth = null!;
        private NumericUpDown numYearsOfExperience = null!;
        private TextBox txtSkills = null!;
        private TextBox txtEducation = null!;
        private TextBox txtCVFilePath = null!;
        private Button btnBrowseCV = null!;
        private ComboBox cmbStatus = null!;

        public CandidateEditForm() : base()
        {
            _service = new CandidateService();
        }

        public CandidateEditForm(Candidate candidate) : base(candidate)
        {
            _service = new CandidateService();
        }

        protected override string GetEntityName()
        {
            return "Ứng viên";
        }

        protected override void InitializeFormControls()
        {
            int startY = 20, spacing = 45;
            int currentY = startY;

            // Full Name
            AddLabelAndTextBox("Họ và tên:", ref txtFullName, currentY);
            txtFullName.MaxLength = 100;
            currentY += spacing;

            // Gender
            AddLabelAndComboBox("Giới tính:", ref cmbGender, currentY, 200);
            cmbGender.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cmbGender.SelectedIndex = 0;
            currentY += spacing;

            // Email
            AddLabelAndTextBox("Email:", ref txtEmail, currentY);
            txtEmail.MaxLength = 100;
            currentY += spacing;

            // Phone
            AddLabelAndTextBox("Số điện thoại:", ref txtPhone, currentY);
            txtPhone.MaxLength = 15;
            currentY += spacing;

            // Date of Birth
            AddLabelAndDateTimePicker("Ngày sinh:", ref dtpDateOfBirth, currentY, 200);
            dtpDateOfBirth.Value = DateTime.Now.AddYears(-25);
            currentY += spacing;

            // Years of Experience
            AddLabelAndNumericUpDown("Kinh nghiệm (năm):", ref numYearsOfExperience, currentY, 0, 50, 100);
            currentY += spacing;

            // Skills
            AddLabelAndTextBox("Kĩ năng:", ref txtSkills, currentY);
            txtSkills.MaxLength = 100;
            currentY += spacing;

            // Education
            AddLabelAndTextBox("Học vấn:", ref txtEducation, currentY);
            txtEducation.MaxLength = 100;
            currentY += spacing;

            // CV File Path
            var lblCV = CreateLabel("CV:", 20, currentY, 150);
            txtCVFilePath = CreateTextBox(180, currentY, 280);
            txtCVFilePath.ReadOnly = true;

            btnBrowseCV = new Button
            {
                Text = "📎 Chọn file",
                Location = new System.Drawing.Point(470, currentY - 2),
                Size = new System.Drawing.Size(110, 28),
                BackColor = System.Drawing.Color.FromArgb(52, 152, 219),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseCV.Click += BtnBrowseCV_Click;
            
            mainPanel.Controls.Add(lblCV);
            mainPanel.Controls.Add(txtCVFilePath);
            mainPanel.Controls.Add(btnBrowseCV);
            currentY += spacing;

            // Status
            AddLabelAndComboBox("Trạng thái:", ref cmbStatus, currentY, 200);
            cmbStatus.Items.AddRange(new object[] { "Mới", "Đang ứng tuyển", "Đạt", "Không đạt", "Từ chối" });
            cmbStatus.SelectedIndex = 0;
        }

        private void BtnBrowseCV_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf|Word documents (*.doc;*.docx)|*.doc;*.docx|All files (*.*)|*.*",
                Title = "Chọn file CV"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtCVFilePath.Text = openFileDialog.FileName;
            }
        }

        protected override bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowWarning("Vui lòng nhập họ tên!");
                txtFullName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowWarning("Vui lòng nhập email!");
                txtEmail.Focus();
                return false;
            }

            if (!txtEmail.Text.Contains("@"))
            {
                ShowWarning("Email không hợp lệ!");
                txtEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                ShowWarning("Vui lòng nhập số điện thoại!");
                txtPhone.Focus();
                return false;
            }

            return true;
        }

        protected override void LoadEntity()
        {
            try
        {
            txtFullName.Text = Entity.FullName;
            txtEmail.Text = Entity.Email;
                txtPhone.Text = Entity.Phone ?? "";
                cmbGender.SelectedItem = Entity.Gender ?? "Nam";
            dtpDateOfBirth.Value = Entity.DateOfBirth ?? DateTime.Now.AddYears(-25);
                numYearsOfExperience.Value = int.Parse(Entity.WorkExperience ?? "0");
                txtSkills.Text = Entity.Skills ?? "";
                txtCVFilePath.Text = Entity.CVFilePath ?? "";
                txtEducation.Text = Entity.Education ?? "";
            if (!string.IsNullOrEmpty(Entity.Status))
                {
                    for (int i = 0; i < cmbStatus.Items.Count; i++)
                    {
                        if (cmbStatus.Items[i].ToString() == Entity.Status)
                        {
                            cmbStatus.SelectedIndex = i;
                            break;
        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        protected override void SaveEntity()
        {
            try
            {
                Entity.FullName = txtFullName.Text.Trim();
                Entity.Email = txtEmail.Text.Trim();
                Entity.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
                Entity.Gender = cmbGender.SelectedItem?.ToString() ?? "Nam";
                Entity.DateOfBirth = dtpDateOfBirth.Value.Date;
                Entity.WorkExperience = numYearsOfExperience.Value.ToString();
                Entity.Skills = string.IsNullOrWhiteSpace(txtSkills.Text) ? null : txtSkills.Text.Trim();
                Entity.CVFilePath = string.IsNullOrWhiteSpace(txtCVFilePath.Text) ? null : txtCVFilePath.Text.Trim();
                Entity.Status = cmbStatus.SelectedItem?.ToString() ?? "Mới";
                Entity.Education = string.IsNullOrWhiteSpace(txtEducation.Text) ? null : txtEducation.Text.Trim();
            if (IsEditMode)
                {
                    if (!_service.UpdateCandidate(Entity))
                    {
                        throw new Exception("Không thể cập nhật ứng viên");
                    }
                }
            else
            {
                var id = _service.CreateCandidate(Entity);
                    if (id <= 0)
                    {
                        throw new Exception("Không thể thêm ứng viên");
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

