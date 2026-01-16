using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Forms
{
    /// <summary>
    /// HeadcountManagementForm - Quản lý định biên (Custom form, không dùng BaseListForm)
    /// Chỉ HR Manager có quyền
    /// Kế thừa BaseForm (Inheritance)
    /// Áp dụng design theo PROMPT_UI_DESIGN_GUIDE.md
    /// </summary>
    public class HeadcountManagementForm : BaseForm
    {
        private readonly HeadcountRepository _repository;
        private readonly DepartmentRepository _departmentRepository;
        private readonly PositionRepository _positionRepository;

        private Guna2Panel topPanel = null!;
        private NumericUpDown numYear = null!;
        private Guna2Button btnLoadYear = null!;
        private Guna2Button btnAddHeadcount = null!;
        private Guna2Button btnRefresh = null!;
        private Guna2DataGridView dgvHeadcount = null!;
        private Guna2Panel summaryPanel = null!;
        private Label lblSummary = null!;

        private List<Headcount> headcounts = new List<Headcount>();
        private int currentYear;

        public HeadcountManagementForm()
        {
            _repository = new HeadcountRepository();
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
            currentYear = DateTime.Now.Year;
            
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Quản lý Định biên (HR Manager)";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.BackgroundMain;

            // Top Panel
            topPanel = UITheme.CreatePanel(withBorder: false);
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 70;
            topPanel.BackColor = UITheme.BackgroundMain;
            topPanel.Padding = new Padding(15, 15, 15, 15);

            var lblYear = UITheme.CreateLabel("Năm:", UITheme.BodyBold);
            lblYear.Location = new Point(15, 20);
            lblYear.Size = new Size(70, 25);
            lblYear.AutoSize = true;

            numYear = new NumericUpDown
            {
                Location = new Point(80, 18),
                Size = new Size(100, 35),
                Minimum = 2020,
                Maximum = 2100,
                Value = currentYear,
                Font = UITheme.BodyRegular,
                BorderStyle = BorderStyle.FixedSingle
            };
            UITheme.ApplyNumericUpDownStyle(numYear);

            btnAddHeadcount = UITheme.CreatePrimaryButton("➕ Thêm định biên", 160, UITheme.ButtonHeight);
            btnAddHeadcount.Location = new Point(190, 15);
            btnAddHeadcount.Click += BtnAddHeadcount_Click;

            btnRefresh = UITheme.CreateSuccessButton("🔄 Làm mới", 130, UITheme.ButtonHeight);
            btnRefresh.Location = new Point(360, 15);
            btnRefresh.Click += (s, e) => LoadHeadcounts(currentYear);

            topPanel.Controls.Add(lblYear);
            topPanel.Controls.Add(numYear);
            topPanel.Controls.Add(btnLoadYear);
            topPanel.Controls.Add(btnAddHeadcount);
            topPanel.Controls.Add(btnRefresh);

            // Summary Panel
            summaryPanel = UITheme.CreatePanel(withBorder: false);
            summaryPanel.Dock = DockStyle.Bottom;
            summaryPanel.Height = 60;
            summaryPanel.BackColor = UITheme.PrimaryLight;
            summaryPanel.Padding = new Padding(15);

            lblSummary = UITheme.CreateLabel("", UITheme.BodyBold);
            lblSummary.Dock = DockStyle.Fill;
            lblSummary.ForeColor = UITheme.TextPrimary;
            lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            summaryPanel.Controls.Add(lblSummary);

            // DataGridView Container Panel
            var gridPanel = UITheme.CreatePanel(withBorder: false);
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.Padding = new Padding(15, 10, 15, 15);
            gridPanel.BackColor = UITheme.BackgroundMain;

            // DataGridView
            dgvHeadcount = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = false
            };
            UITheme.ApplyDataGridViewStyle(dgvHeadcount);

            SetupDataGridView();
            dgvHeadcount.CellValueChanged += DgvHeadcount_CellValueChanged;
            dgvHeadcount.CellDoubleClick += DgvHeadcount_CellDoubleClick;

            gridPanel.Controls.Add(dgvHeadcount);

            // Add controls to form
            this.Controls.Add(gridPanel);
            this.Controls.Add(summaryPanel);
            this.Controls.Add(topPanel);
        }

        private void SetupDataGridView()
        {
            dgvHeadcount.AutoGenerateColumns = false;
            dgvHeadcount.Columns.Clear();

            dgvHeadcount.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DepartmentName",
                HeaderText = "Phòng ban",
                Name = "DepartmentName",
                Width = 200,
                ReadOnly = true
            });

            dgvHeadcount.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PositionName",
                HeaderText = "Vị trí",
                Name = "PositionName",
                Width = 200,
                ReadOnly = true
            });

            dgvHeadcount.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ApprovedCount",
                HeaderText = "Định biên phê duyệt",
                Name = "ApprovedCount",
                Width = 150,
                ReadOnly = false // Allow edit
            });

            dgvHeadcount.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FilledCount",
                HeaderText = "Đã tuyển",
                Name = "FilledCount",
                Width = 120,
                ReadOnly = true
            });

            dgvHeadcount.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Remaining",
                HeaderText = "Còn thiếu",
                Name = "Remaining",
                Width = 120,
                ReadOnly = true
            });

            dgvHeadcount.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PercentageFormatted",
                HeaderText = "Tỷ lệ lấp đầy",
                Name = "Percentage",
                Width = 120,
                ReadOnly = true
            });
        }

        protected override void BaseForm_Load(object? sender, EventArgs e)
        {
            base.BaseForm_Load(sender, e);

            // Check permission - Only HR Manager
            if (!CheckPermission(IsHRManager(), "Chỉ HR Manager mới có quyền quản lý định biên!"))
            {
                this.Close();
                return;
            }

            LoadHeadcounts(currentYear);
        }

        private void LoadHeadcounts(int year)
        {
            try
            {
                currentYear = year;
                headcounts = _repository.GetAll()
                    .Where(h => h.Year == year)
                    .ToList();

                // Get reference data
                var departments = _departmentRepository.GetAll().ToDictionary(d => d.Id, d => d.DepartmentName);
                var positions = _positionRepository.GetAll().ToDictionary(p => p.Id, p => p.PositionName);

                var displayData = headcounts.Select(h => new
                {
                    h.Id,
                    DepartmentName = departments.ContainsKey(h.DepartmentId) ? departments[h.DepartmentId] : "N/A",
                    PositionName = positions.ContainsKey(h.PositionId) ? positions[h.PositionId] : "N/A",
                    h.ApprovedCount,
                    FilledCount = h.FilledCount,
                    Remaining = h.GetRemainingCount(),
                    PercentageFormatted = h.ApprovedCount > 0 ? $"{h.GetFilledPercentage():F1}%" : "0%",
                    Headcount = h // Keep reference
                }).ToList();

                dgvHeadcount.DataSource = displayData;

                // Update summary
                UpdateSummary();
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi load dữ liệu: {ex.Message}");
            }
        }

        private void UpdateSummary()
        {
            if (headcounts.Any())
            {
                int totalApproved = headcounts.Sum(h => h.ApprovedCount);
                int totalFilled = headcounts.Sum(h => h.FilledCount);
                int totalRemaining = totalApproved - totalFilled;
                double fillRate = totalApproved > 0 ? (double)totalFilled / totalApproved * 100 : 0;

                lblSummary.Text = $"📊 Tổng quan năm {currentYear}: " +
                    $"Định biên: {totalApproved} | Đã tuyển: {totalFilled} | " +
                    $"Còn thiếu: {totalRemaining} | Tỷ lệ lấp đầy: {fillRate:F1}%";
            }
            else
            {
                lblSummary.Text = $"📊 Chưa có dữ liệu định biên cho năm {currentYear}";
            }
        }

        private void BtnAddHeadcount_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var dialog = new AddHeadcountDialog(
                    _departmentRepository.GetAll().ToList(),
                    _positionRepository.GetAll().ToList(),
                    currentYear))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var (deptId, posId, year, approvedCount) = dialog.GetHeadcountData();

                        // Check if already exists
                        var existing = _repository.GetByDepartmentPositionYear(deptId, posId, year);
                        if (existing != null)
                        {
                            ShowWarning("Định biên cho phòng ban và vị trí này trong năm đã tồn tại!\n\nVui lòng chỉnh sửa trực tiếp trong bảng.");
                            return;
                        }

                        var headcount = new Headcount
                        {
                            DepartmentId = deptId,
                            PositionId = posId,
                            Year = year,
                            ApprovedCount = approvedCount,
                            FilledCount = 0,
                            ApprovedDate = DateTime.Now,
                            ApprovedBy = CurrentUser?.EmployeeId ?? 0
                        };

                        // Validate ApprovedBy
                        if (headcount.ApprovedBy <= 0)
                        {
                            ShowError("Không tìm thấy thông tin Employee của user hiện tại.\nVui lòng liên hệ Admin để gắn User với Employee.");
                            return;
                        }

                        var id = _repository.Add(headcount);
                        if (id > 0)
                        {
                            ShowSuccess("Thêm định biên thành công!");
                            LoadHeadcounts(currentYear);
                        }
                        else
                        {
                            ShowError("Không thể thêm định biên. Vui lòng thử lại.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
            }
        }

        private void DgvHeadcount_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 2) // ApprovedCount column
            {
                try
                {
                    var row = dgvHeadcount.Rows[e.RowIndex];
                    var newValue = row.Cells[2].Value;

                    if (int.TryParse(newValue?.ToString(), out int approvedCount))
                    {
                        if (approvedCount < 0)
                        {
                            ShowWarning("Định biên không thể âm!");
                            LoadHeadcounts(currentYear);
                            return;
                        }

                        var headcount = headcounts[e.RowIndex];
                        headcount.ApprovedCount = approvedCount;

                        if (_repository.Update(headcount))
                        {
                            LoadHeadcounts(currentYear);
                        }
                        else
                        {
                            ShowError("Không thể cập nhật định biên.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Lỗi cập nhật: {ex.Message}");
                    LoadHeadcounts(currentYear);
                }
            }
        }

        private void DgvHeadcount_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var headcount = headcounts[e.RowIndex];
                var departments = _departmentRepository.GetAll().ToDictionary(d => d.Id, d => d.DepartmentName);
                var positions = _positionRepository.GetAll().ToDictionary(p => p.Id, p => p.PositionName);

                var info = $"Chi tiết định biên:\n\n" +
                    $"Phòng ban: {departments[headcount.DepartmentId]}\n" +
                    $"Vị trí: {positions[headcount.PositionId]}\n" +
                    $"Năm: {headcount.Year}\n\n" +
                    $"Định biên phê duyệt: {headcount.ApprovedCount}\n" +
                    $"Đã tuyển: {headcount.FilledCount}\n" +
                    $"Còn thiếu: {headcount.GetRemainingCount()}\n" +
                    $"Tỷ lệ lấp đầy: {headcount.GetFilledPercentage():F1}%";

                ShowInfo(info);
            }
        }
    }

    /// <summary>
    /// Dialog thêm định biên mới
    /// Áp dụng design theo PROMPT_UI_DESIGN_GUIDE.md
    /// </summary>
    internal class AddHeadcountDialog : Form
    {
        private Guna2Panel mainPanel = null!;
        private Guna2ComboBox cmbDepartment = null!;
        private Guna2ComboBox cmbPosition = null!;
        private NumericUpDown numYear = null!;
        private NumericUpDown numApprovedCount = null!;
        private Guna2Button btnOK = null!;
        private Guna2Button btnCancel = null!;

        public AddHeadcountDialog(List<Department> departments, List<Position> positions, int currentYear)
        {
            InitializeComponents(departments, positions, currentYear);
        }

        private void InitializeComponents(List<Department> departments, List<Position> positions, int currentYear)
        {
            this.Text = "Thêm định biên mới";
            this.Size = new Size(560, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.BackgroundMain;

            // Main Panel
            mainPanel = UITheme.CreateCardPanel(12);
            mainPanel.Size = new Size(560, 520);
            mainPanel.Location = new Point(0, 0);
            mainPanel.Padding = new Padding(30);

            int yPos = 30;

            // Department
            var lblDepartment = UITheme.CreateLabel("Phòng ban:", UITheme.BodyBold);
            lblDepartment.Location = new Point(30, yPos);
            lblDepartment.Size = new Size(450, 25);

            cmbDepartment = UITheme.CreateComboBox(40);
            cmbDepartment.Location = new Point(30, yPos + 30);
            cmbDepartment.Size = new Size(450, 40);
            cmbDepartment.DisplayMember = "DepartmentName";
            cmbDepartment.ValueMember = "Id";
            foreach (var dept in departments)
            {
                cmbDepartment.Items.Add(dept);
            }
            yPos += 90;

            // Position
            var lblPosition = UITheme.CreateLabel("Vị trí:", UITheme.BodyBold);
            lblPosition.Location = new Point(30, yPos);
            lblPosition.Size = new Size(450, 25);

            cmbPosition = UITheme.CreateComboBox(40);
            cmbPosition.Location = new Point(30, yPos + 30);
            cmbPosition.Size = new Size(450, 40);
            cmbPosition.DisplayMember = "PositionName";
            cmbPosition.ValueMember = "Id";
            foreach (var pos in positions)
            {
                cmbPosition.Items.Add(pos);
            }
            yPos += 90;

            // Year
            var lblYear = UITheme.CreateLabel("Năm:", UITheme.BodyBold);
            lblYear.Location = new Point(30, yPos);
            lblYear.Size = new Size(450, 25);

            numYear = new NumericUpDown
            {
                Location = new Point(30, yPos + 30),
                Size = new Size(150, 35),
                Minimum = 2020,
                Maximum = 2100,
                Value = currentYear,
                Font = UITheme.BodyRegular,
                BorderStyle = BorderStyle.FixedSingle
            };
            UITheme.ApplyNumericUpDownStyle(numYear);
            yPos += 90;

            // Approved Count
            var lblApproved = UITheme.CreateLabel("Định biên phê duyệt:", UITheme.BodyBold);
            lblApproved.Location = new Point(30, yPos);
            lblApproved.Size = new Size(450, 25);

            numApprovedCount = new NumericUpDown
            {
                Location = new Point(30, yPos + 30),
                Size = new Size(150, 35),
                Minimum = 0,
                Maximum = 1000,
                Value = 1,
                Font = UITheme.BodyRegular,
                BorderStyle = BorderStyle.FixedSingle
            };
            UITheme.ApplyNumericUpDownStyle(numApprovedCount);
            yPos += 90;

            // Buttons
            yPos += 20; // Add spacing before buttons
            btnOK = UITheme.CreatePrimaryButton("Thêm", 150, UITheme.ButtonHeight);
            btnOK.Location = new Point(180, yPos);
            btnOK.Click += BtnOK_Click;

            btnCancel = UITheme.CreateSecondaryButton("Hủy", 150, UITheme.ButtonHeight);
            btnCancel.Location = new Point(340, yPos);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Add controls to main panel
            mainPanel.Controls.Add(lblDepartment);
            mainPanel.Controls.Add(cmbDepartment);
            mainPanel.Controls.Add(lblPosition);
            mainPanel.Controls.Add(cmbPosition);
            mainPanel.Controls.Add(lblYear);
            mainPanel.Controls.Add(numYear);
            mainPanel.Controls.Add(lblApproved);
            mainPanel.Controls.Add(numApprovedCount);
            mainPanel.Controls.Add(btnOK);
            mainPanel.Controls.Add(btnCancel);

            // Add main panel to form
            this.Controls.Add(mainPanel);
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (cmbDepartment.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn phòng ban!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbPosition.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn vị trí!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public (int departmentId, int positionId, int year, int approvedCount) GetHeadcountData()
        {
            var dept = (Department)cmbDepartment.SelectedItem!;
            var pos = (Position)cmbPosition.SelectedItem!;
            return (dept.Id, pos.Id, (int)numYear.Value, (int)numApprovedCount.Value);
        }
    }
}
