// ============================================================
//  UC_Semester.cs – Quản lý Học kỳ
// ============================================================
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLSV
{
    public partial class UC_Semester : UserControl
    {
        private int  _selID    = -1;
        private bool _isEditing = false;

        public UC_Semester() { InitializeComponent(); }

        public void LoadData() => LoadGrid();

        private void LoadGrid()
        {
            DataTable dt = Semester.GetAll();
            dgv.DataSource = dt;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible   = false;
            dgv.ReadOnly            = true;
            dgv.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;

            if (dgv.Columns.Contains("SemesterID")) dgv.Columns["SemesterID"].Visible = false;
            if (dgv.Columns.Contains("SemCode"))      dgv.Columns["SemCode"].HeaderText      = "Mã HK";
            if (dgv.Columns.Contains("SemName"))      dgv.Columns["SemName"].HeaderText      = "Tên học kỳ";
            if (dgv.Columns.Contains("AcademicYear")) dgv.Columns["AcademicYear"].HeaderText = "Năm học";
            if (dgv.Columns.Contains("StartDate"))    dgv.Columns["StartDate"].HeaderText    = "Bắt đầu";
            if (dgv.Columns.Contains("EndDate"))      dgv.Columns["EndDate"].HeaderText      = "Kết thúc";
            if (dgv.Columns.Contains("IsRegOpen"))    dgv.Columns["IsRegOpen"].HeaderText    = "Mở ĐK";
            if (dgv.Columns.Contains("IsActive"))     dgv.Columns["IsActive"].HeaderText     = "Hiện tại";

            foreach (DataGridViewRow row in dgv.Rows)
            {
                bool isActive = Convert.ToBoolean(row.Cells["IsActive"].Value);
                bool isOpen   = Convert.ToBoolean(row.Cells["IsRegOpen"].Value);
                row.DefaultCellStyle.BackColor =
                    isActive ? Color.FromArgb(220, 255, 220) :
                    isOpen   ? Color.FromArgb(220, 235, 255) :
                               Color.White;
            }

            lblTotal.Text = $"Tổng: {dt.Rows.Count} học kỳ";
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            _selID    = Convert.ToInt32(row.Cells["SemesterID"].Value);
            _isEditing = true;

            txbCode.Text    = row.Cells["SemCode"].Value?.ToString();
            txbName.Text    = row.Cells["SemName"].Value?.ToString();
            txbYear.Text    = row.Cells["AcademicYear"].Value?.ToString();
            dtpStart.Value  = Convert.ToDateTime(row.Cells["StartDate"].Value);
            dtpEnd.Value    = Convert.ToDateTime(row.Cells["EndDate"].Value);
            chkRegOpen.Checked = Convert.ToBoolean(row.Cells["IsRegOpen"].Value);
            chkActive.Checked  = Convert.ToBoolean(row.Cells["IsActive"].Value);

            txbCode.Enabled = false;
            SetButtons(true);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _selID    = -1;
            _isEditing = false;
            txbCode.Clear(); txbName.Clear(); txbYear.Clear();
            dtpStart.Value = DateTime.Today;
            dtpEnd.Value   = DateTime.Today.AddMonths(5);
            chkRegOpen.Checked = false;
            chkActive.Checked  = false;
            txbCode.Enabled = true;
            txbCode.Focus();
            SetButtons(false);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbCode.Text) || string.IsNullOrWhiteSpace(txbName.Text))
            { MessageBox.Show("Vui lòng nhập đầy đủ mã và tên học kỳ!", "⚠️"); return; }
            if (dtpEnd.Value <= dtpStart.Value)
            { MessageBox.Show("Ngày kết thúc phải sau ngày bắt đầu!", "⚠️"); return; }

            var sem = new Semester
            {
                SemesterID   = _selID,
                SemCode      = txbCode.Text.Trim(),
                SemName      = txbName.Text.Trim(),
                AcademicYear = txbYear.Text.Trim(),
                StartDate    = dtpStart.Value,
                EndDate      = dtpEnd.Value,
                IsRegOpen    = chkRegOpen.Checked,
                IsActive     = chkActive.Checked
            };

            bool ok = _isEditing ? sem.Update() : sem.Add();
            if (ok)
            {
                if (chkActive.Checked) Semester.SetActive(_selID > 0 ? _selID :
                    GetLastInsertedID());

                AuditHelper.Log(_isEditing ? "UPDATE" : "INSERT", "Semester",
                                sem.SemCode, null, sem.SemName);
                MessageBox.Show("Lưu thành công!", "✅", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnNew_Click(null, null);
                LoadGrid();
            }
            else MessageBox.Show("Lưu thất bại (mã học kỳ có thể đã tồn tại)!", "❌",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selID == -1) return;
            if (MessageBox.Show("Xóa học kỳ này?\n(Sẽ xóa toàn bộ lớp môn học liên quan!)",
                "⚠️ Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            if (Semester.Delete(_selID))
            {
                AuditHelper.Log("DELETE", "Semester", _selID.ToString());
                MessageBox.Show("Đã xóa!", "✅"); btnNew_Click(null, null); LoadGrid();
            }
            else MessageBox.Show("Không thể xóa!", "❌", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnToggleReg_Click(object sender, EventArgs e)
        {
            if (_selID == -1) return;
            bool currentState = chkRegOpen.Checked;
            bool newState     = !currentState;
            if (Semester.ToggleRegistration(_selID, newState))
            {
                chkRegOpen.Checked = newState;
                string msg = newState ? "✅ Đã MỞ đăng ký học phần!" : "🔒 Đã ĐÓNG đăng ký học phần!";
                MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrid();
            }
        }

        private void btnSetActive_Click(object sender, EventArgs e)
        {
            if (_selID == -1) return;
            if (Semester.SetActive(_selID))
            {
                MessageBox.Show("Đã đặt làm học kỳ hiện tại!", "✅");
                LoadGrid();
            }
        }

        private void SetButtons(bool editing)
        {
            btnDelete.Enabled     = editing;
            btnToggleReg.Enabled  = editing;
            btnSetActive.Enabled  = editing;
        }

        private int GetLastInsertedID()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                object r = new System.Data.SqlClient.SqlCommand(
                    "SELECT TOP 1 SemesterID FROM Semester ORDER BY SemesterID DESC", db.conn).ExecuteScalar();
                return r != null ? (int)r : -1;
            }
            finally { db.closeConnection(); }
        }
    }
}
