// ============================================================
//  UC_ClassRoom.cs – Quản lý Lớp môn học
// ============================================================
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace QLSV
{
    public partial class UC_ClassRoom : UserControl
    {
        private int  _selClassID = -1;
        private bool _isEditing  = false;

        public UC_ClassRoom() { InitializeComponent(); }

        public void LoadData()
        {
            LoadSemesterCombo();
            LoadCourseCombo();
            LoadGrid();
        }

        private void LoadSemesterCombo()
        {
            DataTable dt = Semester.GetComboSource();
            cbSemFilter.DataSource    = dt.Copy();
            cbSemFilter.DisplayMember = "Name";
            cbSemFilter.ValueMember   = "ID";
            cbSemFilter.Items.Insert(0, "-- Tất cả --");

            cbSemForm.DataSource    = Semester.GetComboSource();
            cbSemForm.DisplayMember = "Name";
            cbSemForm.ValueMember   = "ID";

            int activeID = Semester.GetActiveSemesterID();
            if (activeID > 0) cbSemForm.SelectedValue = activeID;
        }

        private void LoadCourseCombo()
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                new SqlDataAdapter(
                    "SELECT CourseID AS ID, CourseName+' ('+CourseID+')' AS Name FROM Course ORDER BY CourseName",
                    db.conn).Fill(dt);
            }
            finally { db.closeConnection(); }
            cbCourse.DataSource    = dt;
            cbCourse.DisplayMember = "Name";
            cbCourse.ValueMember   = "ID";
        }

        private void LoadGrid(string kw = "", int semID = -1)
        {
            DataTable dt;
            if (AppSession.IsGiaoVien)
                dt = ClassRoom.GetForTeacher(AppSession.CurrentEmail, semID);
            else
                dt = ClassRoom.GetAll(kw, semID);

            dgv.DataSource = dt;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible   = false;
            dgv.ReadOnly            = true;
            dgv.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;

            if (dgv.Columns.Contains("ClassID"))    dgv.Columns["ClassID"].Visible    = false;
            if (dgv.Columns.Contains("DayOfWeek"))  dgv.Columns["DayOfWeek"].Visible  = false;
            if (dgv.Columns.Contains("StartPeriod")) dgv.Columns["StartPeriod"].Visible = false;
            if (dgv.Columns.Contains("NumPeriod"))  dgv.Columns["NumPeriod"].Visible  = false;

            // Tô màu trạng thái
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string status = row.Cells["Trạng thái"]?.Value?.ToString();
                row.DefaultCellStyle.BackColor =
                    status == "Đang mở"  ? Color.FromArgb(235, 255, 235) :
                    status == "Đã đóng"  ? Color.FromArgb(255, 240, 240) :
                                           Color.White;
            }

            lblTotal.Text = $"Tổng: {dt.Rows.Count} lớp";

            // Phân quyền
            bool canEdit = AppSession.IsAdmin;
            btnNew.Visible    = canEdit;
            btnDelete.Visible = canEdit;
            pnlForm.Visible   = canEdit;
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            _selClassID = Convert.ToInt32(row.Cells["ClassID"].Value);
            _isEditing  = true;

            // Fill form từ DB
            FillFormFromDB(_selClassID);
            btnDelete.Enabled = true;
        }

        private void FillFormFromDB(int classID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT * FROM ClassRoom WHERE ClassID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", classID);
                var dr = cmd.ExecuteReader();
                if (!dr.Read()) return;

                txbClassCode.Text   = dr["ClassCode"].ToString();
                txbTeacher.Text     = dr["TeacherName"].ToString();
                txbTeacherEmail.Text= dr["TeacherEmail"]?.ToString();
                txbRoom.Text        = dr["Room"]?.ToString();
                txbNote.Text        = dr["Note"]?.ToString();
                nudMaxSlot.Value    = Convert.ToInt32(dr["MaxSlot"]);
                nudStartPeriod.Value = Convert.ToInt32(dr["StartPeriod"]);
                nudNumPeriod.Value  = Convert.ToInt32(dr["NumPeriod"]);
                nudDay.Value        = Convert.ToInt32(dr["DayOfWeek"]);
                cbSemForm.SelectedValue = dr["SemesterID"];
                cbCourse.SelectedValue  = dr["CourseID"].ToString();
                cbStatus.Text           = dr["Status"]?.ToString();

                txbClassCode.Enabled = false;
            }
            finally { db.closeConnection(); }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _selClassID = -1;
            _isEditing  = false;
            txbClassCode.Clear(); txbTeacher.Clear(); txbTeacherEmail.Clear();
            txbRoom.Clear(); txbNote.Clear();
            nudMaxSlot.Value     = 40;
            nudStartPeriod.Value = 1;
            nudNumPeriod.Value   = 3;
            nudDay.Value         = 2;
            cbStatus.SelectedIndex = 0;
            txbClassCode.Enabled = true;
            txbClassCode.Focus();
            btnDelete.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            var cr = new ClassRoom
            {
                ClassID      = _selClassID,
                ClassCode    = txbClassCode.Text.Trim().ToUpper(),
                SemesterID   = Convert.ToInt32(cbSemForm.SelectedValue),
                CourseID     = cbCourse.SelectedValue?.ToString(),
                TeacherName  = txbTeacher.Text.Trim(),
                TeacherEmail = txbTeacherEmail.Text.Trim(),
                MaxSlot      = Convert.ToInt32(nudMaxSlot.Value),
                Room         = txbRoom.Text.Trim(),
                DayOfWeek    = Convert.ToInt32(nudDay.Value),
                StartPeriod  = Convert.ToInt32(nudStartPeriod.Value),
                NumPeriod    = Convert.ToInt32(nudNumPeriod.Value),
                Status       = cbStatus.Text,
                Note         = txbNote.Text.Trim()
            };

            bool ok = _isEditing ? cr.Update() : cr.Add();
            if (ok)
            {
                AuditHelper.Log(_isEditing ? "UPDATE" : "INSERT", "ClassRoom",
                                cr.ClassCode, null, cr.TeacherName);
                MessageBox.Show("Lưu thành công!", "✅", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnNew_Click(null, null);
                LoadGrid(txbSearch.Text.Trim(), GetFilterSemID());
            }
            else MessageBox.Show("Lưu thất bại (mã lớp có thể đã tồn tại)!", "❌",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selClassID == -1) return;
            if (MessageBox.Show("Xóa lớp môn học này?\n(Dữ liệu đăng ký, điểm, bài đăng sẽ bị xóa theo!)",
                "⚠️ Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            if (ClassRoom.Delete(_selClassID))
            {
                AuditHelper.Log("DELETE", "ClassRoom", _selClassID.ToString());
                MessageBox.Show("Đã xóa!", "✅"); btnNew_Click(null, null);
                LoadGrid(txbSearch.Text.Trim(), GetFilterSemID());
            }
            else MessageBox.Show("Không thể xóa!", "❌", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnDetail_Click(object sender, EventArgs e)
        {
            if (_selClassID == -1) return;
            // Mở form chi tiết lớp (sẽ triển khai ở Phase 2)
            MessageBox.Show($"Chức năng chi tiết lớp ClassID={_selClassID}\nsẽ mở UC_ClassDetail (Phase 2).",
                            "ℹ️ Thông tin");
        }

        private void txbSearch_TextChanged(object sender, EventArgs e)
            => LoadGrid(txbSearch.Text.Trim(), GetFilterSemID());

        private void cbSemFilter_SelectedIndexChanged(object sender, EventArgs e)
            => LoadGrid(txbSearch.Text.Trim(), GetFilterSemID());

        private int GetFilterSemID()
        {
            if (cbSemFilter.SelectedItem?.ToString() == "-- Tất cả --") return -1;
            if (cbSemFilter.SelectedValue is int id) return id;
            return -1;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txbClassCode.Text))
            { MessageBox.Show("Vui lòng nhập mã lớp!", "⚠️"); return false; }
            if (string.IsNullOrWhiteSpace(txbTeacher.Text))
            { MessageBox.Show("Vui lòng nhập tên giảng viên!", "⚠️"); return false; }
            if (cbCourse.SelectedValue == null)
            { MessageBox.Show("Vui lòng chọn môn học!", "⚠️"); return false; }
            if (cbSemForm.SelectedValue == null)
            { MessageBox.Show("Vui lòng chọn học kỳ!", "⚠️"); return false; }
            if (nudStartPeriod.Value + nudNumPeriod.Value - 1 > 12)
            { MessageBox.Show("Tiết học vượt quá 12 tiết/ngày!", "⚠️"); return false; }
            return true;
        }
    }
}
