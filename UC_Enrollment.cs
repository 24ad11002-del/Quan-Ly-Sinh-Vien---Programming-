// ============================================================
//  UC_Enrollment.cs – Đăng ký học phần & Thời khóa biểu
// ============================================================
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace QLSV
{
    public partial class UC_Enrollment : UserControl
    {
        private int _mssv;
        private int _activeSemID;

        // Màu cho từng môn (tái sử dụng khi vẽ TKB)
        private readonly Color[] COLORS = {
            Color.FromArgb(180,220,255), Color.FromArgb(180,255,200),
            Color.FromArgb(255,220,180), Color.FromArgb(220,180,255),
            Color.FromArgb(255,180,200), Color.FromArgb(180,255,240),
            Color.FromArgb(255,255,180), Color.FromArgb(200,220,200)
        };

        public UC_Enrollment() { InitializeComponent(); }

        public void LoadData(int mssv)
        {
            _mssv       = mssv;
            _activeSemID = Semester.GetActiveSemesterID();

            LoadSemesterCombo();
            LoadMyClasses();
            LoadAvailableClasses();
            DrawTimetable();
        }

        private void LoadSemesterCombo()
        {
            DataTable dt = Semester.GetComboSource();
            cbSem.DataSource    = dt;
            cbSem.DisplayMember = "Name";
            cbSem.ValueMember   = "ID";
            if (_activeSemID > 0) cbSem.SelectedValue = _activeSemID;
        }

        // ── LỚP ĐÃ ĐĂNG KÝ ──────────────────────────────────────
        private void LoadMyClasses()
        {
            int semID = cbSem.SelectedValue is int sid ? sid : -1;
            DataTable dt = Enrollment.GetByStudent(_mssv, semID);
            dgvMy.DataSource = dt;
            dgvMy.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMy.RowHeadersVisible   = false;
            dgvMy.ReadOnly            = true;
            dgvMy.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;

            if (dgvMy.Columns.Contains("EnrollID"))   dgvMy.Columns["EnrollID"].Visible   = false;
            if (dgvMy.Columns.Contains("DayOfWeek"))  dgvMy.Columns["DayOfWeek"].Visible  = false;
            if (dgvMy.Columns.Contains("StartPeriod")) dgvMy.Columns["StartPeriod"].Visible = false;
            if (dgvMy.Columns.Contains("NumPeriod"))  dgvMy.Columns["NumPeriod"].Visible  = false;

            lblMyCount.Text = $"Đã đăng ký: {dt.Rows.Count} lớp";

            // Tính tổng tín chỉ
            int totalCredits = 0;
            foreach (DataRow row in dt.Rows)
                if (row["TC"] != DBNull.Value) totalCredits += Convert.ToInt32(row["TC"]);
            lblCredits.Text = $"Tổng tín chỉ: {totalCredits}";
        }

        // ── LỚP CÓ THỂ ĐĂNG KÝ ──────────────────────────────────
        private void LoadAvailableClasses(string kw = "")
        {
            if (_activeSemID <= 0) { lblAvail.Text = "Học kỳ chưa mở đăng ký"; return; }
            DataTable dt = Enrollment.GetAvailableClasses(_mssv, _activeSemID, kw);

            dgvAvail.DataSource = dt;
            dgvAvail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAvail.RowHeadersVisible   = false;
            dgvAvail.ReadOnly            = true;
            dgvAvail.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;

            if (dgvAvail.Columns.Contains("ClassID"))    dgvAvail.Columns["ClassID"].Visible    = false;
            if (dgvAvail.Columns.Contains("DayOfWeek"))  dgvAvail.Columns["DayOfWeek"].Visible  = false;
            if (dgvAvail.Columns.Contains("StartPeriod")) dgvAvail.Columns["StartPeriod"].Visible = false;
            if (dgvAvail.Columns.Contains("NumPeriod"))  dgvAvail.Columns["NumPeriod"].Visible  = false;

            // Tô đỏ lớp đã đủ sĩ số
            foreach (DataGridViewRow row in dgvAvail.Rows)
            {
                int max  = Convert.ToInt32(row.Cells["Sĩ số tối đa"].Value);
                int enr  = Convert.ToInt32(row.Cells["Đã đăng ký"].Value);
                if (enr >= max) row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
            }

            lblAvail.Text = $"Có thể đăng ký: {dt.Rows.Count} lớp";
        }

        // ── ĐĂNG KÝ ──────────────────────────────────────────────
        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (dgvAvail.SelectedRows.Count == 0) return;
            int classID = Convert.ToInt32(dgvAvail.SelectedRows[0].Cells["ClassID"].Value);
            string className = dgvAvail.SelectedRows[0].Cells["ClassCode"]?.Value?.ToString();

            if (MessageBox.Show($"Đăng ký lớp [{className}]?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            string error;
            if (Enrollment.Register(_mssv, classID, out error))
            {
                AuditHelper.Log("INSERT", "Enrollment", classID.ToString(), null, $"MSSV={_mssv}");
                MessageBox.Show("✅ Đăng ký thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMyClasses();
                LoadAvailableClasses(txbSearch.Text.Trim());
                DrawTimetable();
            }
            else MessageBox.Show($"❌ Đăng ký thất bại!\n{error}", "Lỗi",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ── HỦY ĐĂNG KÝ ──────────────────────────────────────────
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (dgvMy.SelectedRows.Count == 0) return;
            var row = dgvMy.SelectedRows[0];

            // Chỉ hủy được lớp trong học kỳ mở đăng ký
            if (row.Cells["Status"]?.Value?.ToString() != "Đã đăng ký") return;

            int classID = Convert.ToInt32(row.Cells["EnrollID"].Value); // dùng EnrollID để lấy ClassID
            string className = row.Cells["ClassCode"]?.Value?.ToString();

            if (MessageBox.Show($"Hủy đăng ký lớp [{className}]?", "⚠️ Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            // Lấy ClassID từ EnrollID
            My_DB db = new My_DB();
            int realClassID = -1;
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT ClassID FROM Enrollment WHERE EnrollID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", classID);
                realClassID = (int)cmd.ExecuteScalar();
            }
            finally { db.closeConnection(); }

            if (Enrollment.Cancel(_mssv, realClassID))
            {
                AuditHelper.Log("UPDATE", "Enrollment", realClassID.ToString(), "Đã đăng ký", "Đã hủy");
                MessageBox.Show("✅ Đã hủy đăng ký!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMyClasses();
                LoadAvailableClasses(txbSearch.Text.Trim());
                DrawTimetable();
            }
            else MessageBox.Show("❌ Hủy thất bại!", "Lỗi",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ── THỜI KHÓA BIỂU ───────────────────────────────────────
        private void DrawTimetable()
        {
            int semID = cbSem.SelectedValue is int sid ? sid : _activeSemID;
            DataTable dt = ClassRoom.GetForStudent(_mssv, semID);

            pnlTimetable.Controls.Clear();

            int colW  = 110, rowH = 32, headerH = 36, leftW = 60;
            string[] days = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN" };
            int maxPeriod = 12;

            // Vẽ header ngày
            for (int d = 0; d < 7; d++)
            {
                var lbl = new Label {
                    Text      = days[d],
                    Width     = colW, Height    = headerH,
                    Left      = leftW + d * colW, Top = 0,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    BackColor = Color.FromArgb(60, 90, 160),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };
                pnlTimetable.Controls.Add(lbl);
            }

            // Vẽ header tiết
            for (int p = 1; p <= maxPeriod; p++)
            {
                var lbl = new Label {
                    Text      = $"Tiết {p}",
                    Width     = leftW, Height = rowH,
                    Left      = 0, Top = headerH + (p - 1) * rowH,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font      = new Font("Segoe UI", 8),
                    BackColor = Color.FromArgb(230, 235, 245),
                    ForeColor = Color.FromArgb(50, 60, 80),
                    BorderStyle = BorderStyle.FixedSingle
                };
                pnlTimetable.Controls.Add(lbl);
            }

            // Vẽ ô nền
            for (int d = 0; d < 7; d++)
                for (int p = 1; p <= maxPeriod; p++)
                {
                    var cell = new Label {
                        Width = colW, Height = rowH,
                        Left  = leftW + d * colW,
                        Top   = headerH + (p - 1) * rowH,
                        BackColor   = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    pnlTimetable.Controls.Add(cell);
                }

            // Phát hiện trùng giờ
            DataTable conflicts = DetectConflicts(dt);

            // Vẽ lớp
            int colorIdx = 0;
            foreach (DataRow row in dt.Rows)
            {
                int dow   = Convert.ToInt32(row["DayOfWeek"]);   // 2–8
                int start = Convert.ToInt32(row["StartPeriod"]);
                int num   = Convert.ToInt32(row["NumPeriod"]);
                string code    = row["ClassCode"].ToString();
                string subject = row["Môn học"].ToString();

                int col = dow - 2; // 0-based
                if (col < 0 || col > 6) continue;

                bool isConflict = IsConflict(conflicts, dow, start, num);
                Color bg = isConflict ? Color.FromArgb(255, 180, 180) : COLORS[colorIdx % COLORS.Length];

                var card = new Label {
                    Text      = $"{code}\n{subject}",
                    Width     = colW,
                    Height    = rowH * num - 1,
                    Left      = leftW + col * colW,
                    Top       = headerH + (start - 1) * rowH,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    BackColor = bg,
                    ForeColor = isConflict ? Color.DarkRed : Color.FromArgb(30, 50, 100),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = row
                };
                card.MouseEnter += (s, ev) => {
                    var r = (DataRow)((Label)s).Tag;
                    toolTip.SetToolTip((Label)s,
                        $"Môn: {r["Môn học"]}\nGV: {r["Giảng viên"]}\nPhòng: {r["Phòng"]}\nTiết: {start}–{start+num-1}");
                };
                pnlTimetable.Controls.Add(card);
                card.BringToFront();
                colorIdx++;
            }

            pnlTimetable.AutoScroll   = true;
            pnlTimetable.AutoScrollMinSize = new Size(
                leftW + 7 * colW, headerH + maxPeriod * rowH);
        }

        private DataTable DetectConflicts(DataTable dt)
        {
            DataTable conflicts = new DataTable();
            conflicts.Columns.Add("dow", typeof(int));
            conflicts.Columns.Add("start", typeof(int));
            conflicts.Columns.Add("end", typeof(int));

            for (int i = 0; i < dt.Rows.Count; i++)
            for (int j = i + 1; j < dt.Rows.Count; j++)
            {
                int d1 = Convert.ToInt32(dt.Rows[i]["DayOfWeek"]);
                int d2 = Convert.ToInt32(dt.Rows[j]["DayOfWeek"]);
                if (d1 != d2) continue;
                int s1 = Convert.ToInt32(dt.Rows[i]["StartPeriod"]);
                int n1 = Convert.ToInt32(dt.Rows[i]["NumPeriod"]);
                int s2 = Convert.ToInt32(dt.Rows[j]["StartPeriod"]);
                int n2 = Convert.ToInt32(dt.Rows[j]["NumPeriod"]);
                if (s1 < s2 + n2 && s2 < s1 + n1)
                    conflicts.Rows.Add(d1, Math.Min(s1, s2), Math.Max(s1 + n1, s2 + n2));
            }
            return conflicts;
        }

        private bool IsConflict(DataTable conflicts, int dow, int start, int num)
        {
            foreach (DataRow r in conflicts.Rows)
                if (Convert.ToInt32(r["dow"]) == dow &&
                    Convert.ToInt32(r["start"]) <= start &&
                    Convert.ToInt32(r["end"]) >= start + num) return true;
            return false;
        }

        private void txbSearch_TextChanged(object sender, EventArgs e)
            => LoadAvailableClasses(txbSearch.Text.Trim());

        private void cbSem_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMyClasses();
            DrawTimetable();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadMyClasses();
            LoadAvailableClasses(txbSearch.Text.Trim());
            DrawTimetable();
        }
    }
}
