using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Forms
{
    /// <summary>
    /// BaseListForm - Form cha cho tất cả các form danh sách
    /// Áp dụng OOP: Generic Type, Abstract Methods, Virtual Methods
    /// Template pattern - định nghĩa khung sườn, class con implement chi tiết
    /// </summary>
    public abstract class BaseListForm<T> : BaseForm where T : BaseEntity
    {
        // Protected fields - cho phép class con truy cập
        protected Guna2DataGridView dataGridView = null!;
        protected Guna2TextBox txtSearch = null!;
        protected Guna2Button btnAdd = null!;
        protected Guna2Button btnEdit = null!;
        protected Guna2Button btnDelete = null!;
        protected Guna2Button btnRefresh = null!;
        protected Guna2Panel toolbarPanel = null!;
        protected Guna2Panel searchPanel = null!;
        
        protected List<T> allData = new List<T>();
        protected List<T> filteredData = new List<T>();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseListForm()
        {
            InitializeBaseComponents();
        }

        /// <summary>
        /// Khởi tạo các components cơ bản
        /// </summary>
        private void InitializeBaseComponents()
        {
            this.Text = GetFormTitle();
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.BackgroundMain;

            // Search Panel (Top)
            searchPanel = UITheme.CreatePanel(withBorder: false);
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 70;
            searchPanel.Padding = new Padding(15);

            var lblSearch = UITheme.CreateLabel("Tìm kiếm:", UITheme.BodyBold);
            lblSearch.Location = new Point(15, 23);
            lblSearch.Size = new Size(100, 25);
            
            txtSearch = UITheme.CreateTextBox("Nhập từ khóa tìm kiếm...");
            txtSearch.Location = new Point(120, 18);
            txtSearch.Size = new Size(400, UITheme.InputHeight);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);

            // Toolbar Panel (Top, below search)
            toolbarPanel = UITheme.CreatePanel(withBorder: false);
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Height = 70;
            toolbarPanel.BackColor = UITheme.BackgroundMain;
            toolbarPanel.Padding = new Padding(15, 15, 15, 15);

            btnAdd = UITheme.CreatePrimaryButton("➕ Thêm", 120, UITheme.ButtonHeight);
            btnAdd.Location = new Point(15, 15);
            btnAdd.Click += BtnAdd_Click;

            btnEdit = UITheme.CreateSecondaryButton("✏️ Sửa", 120, UITheme.ButtonHeight);
            btnEdit.Location = new Point(145, 15);
            btnEdit.Click += BtnEdit_Click;

            btnDelete = UITheme.CreateDangerButton("🗑️ Xóa", 120, UITheme.ButtonHeight);
            btnDelete.Location = new Point(275, 15);
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = UITheme.CreateSuccessButton("🔄 Làm mới", 130, UITheme.ButtonHeight);
            btnRefresh.Location = new Point(405, 15);
            btnRefresh.Click += BtnRefresh_Click;

            toolbarPanel.Controls.Add(btnAdd);
            toolbarPanel.Controls.Add(btnEdit);
            toolbarPanel.Controls.Add(btnDelete);
            toolbarPanel.Controls.Add(btnRefresh);

            // DataGridView Container Panel
            var gridPanel = UITheme.CreatePanel(withBorder: false);
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.Padding = new Padding(15, 10, 15, 15);
            gridPanel.BackColor = UITheme.BackgroundMain;

            // DataGridView (Fill container panel)
            dataGridView = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
            UITheme.ApplyDataGridViewStyle(dataGridView);
            dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;

            gridPanel.Controls.Add(dataGridView);

            // Add controls to form
            this.Controls.Add(gridPanel);
            this.Controls.Add(toolbarPanel);
            this.Controls.Add(searchPanel);
        }

        /// <summary>
        /// Override BaseForm_Load
        /// </summary>
        protected override void BaseForm_Load(object? sender, EventArgs e)
        {
            base.BaseForm_Load(sender, e);
            
            try
            {
                // Ensure all controls are initialized
                if (dataGridView == null || searchPanel == null || toolbarPanel == null)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Some controls are null in BaseListForm_Load");
                    return;
                }

                SetupDataGridView();
                SetupPermissions();
                
                // Use BeginInvoke to ensure form is fully loaded before loading data
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Lỗi load dữ liệu: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                    }
                }));
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khởi tạo form: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            }
        }

        #region Abstract Methods - Class con BẮT BUỘC phải implement

        /// <summary>
        /// Load dữ liệu từ database
        /// </summary>
        protected abstract void LoadData();

        /// <summary>
        /// Xử lý khi nhấn nút Thêm
        /// </summary>
        protected abstract void OnAdd();

        /// <summary>
        /// Xử lý khi nhấn nút Sửa
        /// </summary>
        protected abstract void OnEdit(T entity);

        /// <summary>
        /// Xử lý khi nhấn nút Xóa
        /// </summary>
        protected abstract void OnDelete(T entity);

        /// <summary>
        /// Lấy tiêu đề form
        /// </summary>
        protected abstract string GetFormTitle();

        /// <summary>
        /// Setup columns cho DataGridView
        /// </summary>
        protected abstract void SetupDataGridView();

        #endregion

        #region Virtual Methods - Class con CÓ THỂ override

        /// <summary>
        /// Setup quyền cho các nút
        /// Virtual - class con có thể override để customize
        /// </summary>
        protected virtual void SetupPermissions()
        {
            // Default: tất cả đều được phép
            // Class con override để giới hạn quyền
        }

        /// <summary>
        /// Xử lý tìm kiếm
        /// Virtual - class con có thể override để customize logic search
        /// </summary>
        protected virtual void OnSearch(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                filteredData = new List<T>(allData);
            }
            else
            {
                // Default: không làm gì, class con phải override
                filteredData = new List<T>(allData);
            }
            
            RefreshGrid();
        }

        /// <summary>
        /// Lấy entity hiện tại đang chọn
        /// </summary>
        protected virtual T? GetSelectedEntity()
        {
            if (dataGridView.CurrentRow?.DataBoundItem is T entity)
            {
                return entity;
            }
            return null;
        }

        #endregion

        #region Event Handlers

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            try
            {
                OnAdd();
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi thêm: {ex.Message}");
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            try
            {
                var entity = GetSelectedEntity();
                if (entity == null)
                {
                    ShowWarning("Vui lòng chọn một dòng để sửa!");
                    return;
                }
                OnEdit(entity);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi sửa: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            try
            {
                var entity = GetSelectedEntity();
                if (entity == null)
                {
                    ShowWarning("Vui lòng chọn một dòng để xóa!");
                    return;
                }

                if (Confirm($"Bạn có chắc muốn xóa bản ghi này?\n(ID: {entity.Id})"))
                {
                    OnDelete(entity);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi xóa: {ex.Message}");
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            try
            {
                txtSearch.Clear();
                LoadData();
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khi làm mới: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                if (txtSearch != null)
                {
                    OnSearch(txtSearch.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TxtSearch_TextChanged: {ex.Message}");
            }
        }

        private void DataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                BtnEdit_Click(sender, e);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Refresh DataGridView
        /// </summary>
        protected void RefreshGrid()
        {
            if (dataGridView == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: dataGridView is null in RefreshGrid");
                return;
            }

            try
            {
                dataGridView.DataSource = null;
                dataGridView.DataSource = filteredData;
                
                if (dataGridView.Columns.Contains("Id"))
                    dataGridView.Columns["Id"]!.Visible = false;
                if (dataGridView.Columns.Contains("CreatedDate"))
                    dataGridView.Columns["CreatedDate"]!.Visible = false;
                if (dataGridView.Columns.Contains("UpdatedDate"))
                    dataGridView.Columns["UpdatedDate"]!.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RefreshGrid: {ex.Message}");
                ShowError($"Lỗi refresh grid: {ex.Message}");
            }
        }

        /// <summary>
        /// Set data source và refresh
        /// </summary>
        protected void SetDataSource(List<T> data)
        {
            allData = data ?? new List<T>();
            filteredData = new List<T>(allData);
            RefreshGrid();
        }

        #endregion
    }
}
