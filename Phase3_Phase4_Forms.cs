// ============================================================
//  UC_Notification.cs – Quản lý Thông báo
// ============================================================
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLSV
{
    public partial class UC_Notification : UserControl
    {
        private int    _accountID;
        private string _role;
        private int    _selNotifID = -1;

        public UC_Notification() { InitializeComponent(); }

        public void LoadData(int accountID, string role)
        {
            _accountID = accountID;
            _role      = role;

            // Phân quyền hiển thị
            pnlForm.Visible    = (role == "Admin" || role == "GiaoVien");
            btnDelete.Visible  = (role == "Admin");
            cbType.Items.AddRange(new[] { "Chung", "Thông báo nghỉ", "Thông báo bù", "Bài đăng mới", "Bình luận" });
            cbType.SelectedIndex = 0;

            LoadNotifications();
        }

        private void LoadNotifications(string kw = "", string type = "")
        {
            DataTable dt = Notification.GetForUser(_accountID, _role, kw, type);
            dgv.DataSource = dt;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible   = false;
            dgv.ReadOnly            = true;
            dgv.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;

            if (dgv.Columns.Contains("NotifID")) dgv.Columns["NotifID"].Visible = false;

            // Tô đậm chưa đọc
            foreach (DataGridViewRow row in dgv.Rows)
            {
                bool isRead = Convert.ToBoolean(row.Cells["IsRead"].Value);
                if (!isRead) row.DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
            }

            if (dgv.Columns.Contains("IsRead")) dgv.Columns["IsRead"].Visible = false;

            int unread = Notification.GetUnreadCount(_accountID, _role);
            lblUnread.Text = $"Chưa đọc: {unread}";
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            _selNotifID = Convert.ToInt32(row.Cells["NotifID"].Value);

            rtbContent.Text = row.Cells["Content"].Value?.ToString();
            lblDetailTitle.Text = row.Cells["Title"].Value?.ToString();
            lblDetailType.Text  = $"Loại: {row.Cells["NotifType"].Value}  |  {row.Cells["CreatedAt"].Value:dd/MM/yyyy HH:mm}";

            // Đánh dấu đã đọc
            Notification.MarkRead(_selNotifID, _accountID);
            row.DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Regular);

            int unread = Notification.GetUnreadCount(_accountID, _role);
            lblUnread.Text = $"Chưa đọc: {unread}";
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbTitle.Text) || string.IsNullOrWhiteSpace(rtbNew.Text))
            { MessageBox.Show("Nhập tiêu đề và nội dung!", "⚠️"); return; }

            var n = new Notification {
                Title      = txbTitle.Text.Trim(),
                Content    = rtbNew.Text.Trim(),
                NotifType  = cbType.SelectedItem?.ToString() ?? "Chung",
                SenderID   = _accountID,
                TargetRole = cbTargetRole.SelectedItem?.ToString()
            };

            if (n.Add())
            {
                MessageBox.Show("✅ Đã gửi thông báo!", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                txbTitle.Clear(); rtbNew.Clear();
                LoadNotifications();
            }
            else MessageBox.Show("❌ Gửi thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selNotifID == -1) return;
            if (MessageBox.Show("Xóa thông báo này?", "Xác nhận",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            if (Notification.Delete(_selNotifID))
            { _selNotifID = -1; rtbContent.Clear(); LoadNotifications(); }
        }

        private void txbSearch_TextChanged(object sender, EventArgs e)
            => LoadNotifications(txbSearch.Text.Trim(), cbFilterType.SelectedItem?.ToString() ?? "");

        private void cbFilterType_SelectedIndexChanged(object sender, EventArgs e)
            => LoadNotifications(txbSearch.Text.Trim(), cbFilterType.SelectedItem?.ToString() ?? "");
    }

    // ============================================================
    //  UC_Library.cs – Thư viện
    // ============================================================
    public partial class UC_Library : UserControl
    {
        private int _mssv;
        private int _selBookID  = -1;
        private int _selBorrowID = -1;

        public UC_Library() { InitializeComponent(); }

        public void LoadData(int mssv)
        {
            _mssv = mssv;
            bool isAdmin = AppSession.IsAdmin;
            pnlAdmin.Visible  = isAdmin;
            btnBorrow.Visible = !isAdmin;
            btnReturn.Visible = true;
            LoadBooks();
            if (!isAdmin) LoadMyBorrows();
        }

        private void LoadBooks(string kw = "")
        {
            DataTable dt = Book.GetAll(kw);
            dgvBooks.DataSource = dt;
            dgvBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBooks.RowHeadersVisible   = false;
            dgvBooks.ReadOnly            = true;
            dgvBooks.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            if (dgvBooks.Columns.Contains("BookID")) dgvBooks.Columns["BookID"].Visible = false;

            // Tô đỏ sách hết
            foreach (DataGridViewRow row in dgvBooks.Rows)
            {
                int avail = Convert.ToInt32(row.Cells["Còn lại"].Value);
                if (avail == 0) row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
            }
            lblBookCount.Text = $"Tổng: {dt.Rows.Count} đầu sách";
        }

        private void LoadMyBorrows()
        {
            DataTable dt = Book.GetBorrowByStudent(_mssv);
            dgvBorrow.DataSource = dt;
            dgvBorrow.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBorrow.RowHeadersVisible   = false;
            dgvBorrow.ReadOnly            = true;
            dgvBorrow.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            if (dgvBorrow.Columns.Contains("BorrowID")) dgvBorrow.Columns["BorrowID"].Visible = false;

            // Tô cam sách quá hạn
            foreach (DataGridViewRow row in dgvBorrow.Rows)
            {
                string warn = row.Cells["Cảnh báo"].Value?.ToString();
                if (!string.IsNullOrEmpty(warn))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 180);
            }
        }

        private void dgvBooks_CellClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _selBookID = Convert.ToInt32(dgvBooks.Rows[e.RowIndex].Cells["BookID"].Value);
        }

        private void dgvBorrow_CellClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _selBorrowID = Convert.ToInt32(dgvBorrow.Rows[e.RowIndex].Cells["BorrowID"].Value);
        }

        private void btnBorrow_Click(object sender, System.EventArgs e)
        {
            if (_selBookID == -1) { MessageBox.Show("Chọn sách trước!", "⚠️"); return; }
            if (MessageBox.Show("Mượn sách này? (14 ngày)", "Xác nhận",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            if (Book.Borrow(_selBookID, _mssv))
            {
                MessageBox.Show("✅ Mượn sách thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBooks(); LoadMyBorrows();
            }
            else MessageBox.Show("❌ Sách đã hết hoặc lỗi!", "Lỗi",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnReturn_Click(object sender, System.EventArgs e)
        {
            if (_selBorrowID == -1) { MessageBox.Show("Chọn phiếu mượn cần trả!", "⚠️"); return; }
            if (Book.Return(_selBorrowID))
            {
                MessageBox.Show("✅ Trả sách thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBooks(); LoadMyBorrows();
            }
            else MessageBox.Show("❌ Trả sách thất bại!", "Lỗi",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void txbSearch_TextChanged(object sender, System.EventArgs e)
            => LoadBooks(txbSearch.Text.Trim());

        // Admin: thêm sách mới
        private void btnAddBook_Click(object sender, System.EventArgs e)
        {
            var b = new Book {
                Title     = txbBookTitle.Text.Trim(),
                Author    = txbBookAuthor.Text.Trim(),
                Publisher = txbBookPub.Text.Trim(),
                TotalQty  = (int)nudQty.Value,
                AvailQty  = (int)nudQty.Value,
                Category  = txbBookCat.Text.Trim()
            };
            if (string.IsNullOrEmpty(b.Title) || string.IsNullOrEmpty(b.Author))
            { MessageBox.Show("Nhập tên sách và tác giả!", "⚠️"); return; }
            if (b.Add())
            {
                MessageBox.Show("✅ Thêm sách thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                txbBookTitle.Clear(); txbBookAuthor.Clear(); txbBookPub.Clear();
                nudQty.Value = 1;
                LoadBooks();
            }
        }
    }

    // ============================================================
    //  UC_Request.cs – Phúc khảo điểm
    // ============================================================
    public partial class UC_Request : UserControl
    {
        private int    _mssv;
        private string _role;
        private int    _selReqID = -1;

        public UC_Request() { InitializeComponent(); }

        public void LoadData(int mssv, string role)
        {
            _mssv = mssv;
            _role = role;

            pnlSend.Visible   = (role == "SinhVien");
            pnlProcess.Visible = (role != "SinhVien");
            btnProcess.Visible = (role != "SinhVien");

            if (role == "SinhVien") LoadMyClasses();
            LoadRequests();
        }

        private void LoadMyClasses()
        {
            int semID = Semester.GetActiveSemesterID();
            DataTable dt = ClassRoom.GetForStudent(_mssv, semID);
            cbClass.DataSource    = dt;
            cbClass.DisplayMember = "ClassCode";
            cbClass.ValueMember   = "ClassID";
        }

        private void LoadRequests(string status = "")
        {
            DataTable dt = _role == "SinhVien"
                ? Request.GetByStudent(_mssv)
                : Request.GetAll(status);
            dgv.DataSource = dt;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible   = false;
            dgv.ReadOnly            = true;
            dgv.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            if (dgv.Columns.Contains("ReqID")) dgv.Columns["ReqID"].Visible = false;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                string st = row.Cells["Trạng thái"].Value?.ToString();
                row.DefaultCellStyle.BackColor =
                    st == "Đã duyệt"    ? Color.FromArgb(220, 255, 220) :
                    st == "Từ chối"     ? Color.FromArgb(255, 220, 220) :
                    st == "Chờ duyệt"   ? Color.FromArgb(255, 250, 210) :
                                          Color.White;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (cbClass.SelectedValue == null || string.IsNullOrWhiteSpace(rtbReason.Text))
            { MessageBox.Show("Chọn lớp và nhập lý do phúc khảo!", "⚠️"); return; }

            var req = new Request {
                MSSV    = _mssv,
                ClassID = Convert.ToInt32(cbClass.SelectedValue),
                ReqType = "Phúc khảo",
                Reason  = rtbReason.Text.Trim()
            };
            if (req.Add())
            {
                MessageBox.Show("✅ Đã gửi yêu cầu phúc khảo!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                rtbReason.Clear();
                LoadRequests();
            }
            else MessageBox.Show("❌ Gửi thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _selReqID = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["ReqID"].Value);
            rtbResult.Text = dgv.Rows[e.RowIndex].Cells["Kết quả"].Value?.ToString();
            btnProcess.Enabled = true;
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (_selReqID == -1) return;
            string status = rbApprove.Checked ? "Đã duyệt" : "Từ chối";
            if (string.IsNullOrWhiteSpace(rtbResult.Text))
            { MessageBox.Show("Nhập kết quả xử lý!", "⚠️"); return; }

            if (Request.Process(_selReqID, AppSession.CurrentAccountID, status, rtbResult.Text.Trim()))
            {
                // Gửi thông báo kết quả
                new Notification {
                    Title    = $"[Phúc khảo] Kết quả: {status}",
                    Content  = rtbResult.Text.Trim(),
                    NotifType = "Phúc khảo",
                    SenderID = AppSession.CurrentAccountID
                }.Add();

                MessageBox.Show($"✅ Đã {status.ToLower()} yêu cầu!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRequests();
            }
        }

        private void cbStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
            => LoadRequests(cbStatusFilter.SelectedItem?.ToString() == "Tất cả" ? "" : cbStatusFilter.SelectedItem?.ToString() ?? "");
    }

    // ============================================================
    //  UC_Dashboard.cs – Dashboard thống kê Admin
    // ============================================================
    public partial class UC_Dashboard : UserControl
    {
        public UC_Dashboard() { InitializeComponent(); }

        public void LoadData()
        {
            LoadStats();
            LoadDeptGPA();
            LoadTopWeakCourses();
            LoadRecentActivity();
        }

        private void LoadStats()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();

                int totalSV = (int)new System.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM Student WHERE Status=N'Đang học'", db.conn).ExecuteScalar();
                int totalClass = (int)new System.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM ClassRoom WHERE Status=N'Đang mở'", db.conn).ExecuteScalar();
                int totalNotif = (int)new System.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM Notification WHERE IsActive=1 AND CreatedAt>=DATEADD(DAY,-7,GETDATE())", db.conn).ExecuteScalar();
                int totalReq = (int)new System.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM Request WHERE Status=N'Chờ duyệt'", db.conn).ExecuteScalar();

                lblTotalSV.Text      = totalSV.ToString("N0");
                lblTotalClass.Text   = totalClass.ToString("N0");
                lblRecentNotif.Text  = totalNotif.ToString("N0");
                lblPendingReq.Text   = totalReq.ToString("N0");
            }
            finally { db.closeConnection(); }
        }

        private void LoadDeptGPA()
        {
            DataTable dt = GpaCalculator.GetDashboardStats();
            dgvDept.DataSource = dt;
            dgvDept.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDept.RowHeadersVisible   = false;
            dgvDept.ReadOnly            = true;

            // Tô màu GPA
            foreach (DataGridViewRow row in dgvDept.Rows)
            {
                if (row.Cells["GPA TB"].Value == null || row.Cells["GPA TB"].Value == DBNull.Value) continue;
                double gpa = Convert.ToDouble(row.Cells["GPA TB"].Value);
                row.Cells["GPA TB"].Style.BackColor =
                    gpa >= 8.0 ? Color.FromArgb(200, 255, 200) :
                    gpa >= 6.5 ? Color.FromArgb(255, 255, 200) :
                                 Color.FromArgb(255, 220, 220);
            }
        }

        private void LoadTopWeakCourses()
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                new System.Data.SqlClient.SqlDataAdapter(
                    "SELECT TOP 5 co.CourseName AS [Môn học], " +
                    "ROUND(AVG(vs.TotalScore),2) AS [GPA TB], " +
                    "COUNT(vs.MSSV) AS [Số SV] " +
                    "FROM v_ScoreSummary vs " +
                    "JOIN ClassRoom cr ON vs.ClassID=cr.ClassID " +
                    "JOIN Course    co ON cr.CourseID=co.CourseID " +
                    "GROUP BY co.CourseID, co.CourseName " +
                    "ORDER BY [GPA TB] ASC", db.conn).Fill(dt);
            }
            finally { db.closeConnection(); }

            dgvWeak.DataSource = dt;
            dgvWeak.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvWeak.RowHeadersVisible   = false;
            dgvWeak.ReadOnly            = true;
        }

        private void LoadRecentActivity()
        {
            DataTable dt = AuditHelper.GetLogs(
                from: DateTime.Today.AddDays(-3));
            dgvAudit.DataSource = dt;
            dgvAudit.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAudit.RowHeadersVisible   = false;
            dgvAudit.ReadOnly            = true;
            if (dgvAudit.Columns.Contains("AuditID"))  dgvAudit.Columns["AuditID"].Visible  = false;
            if (dgvAudit.Columns.Contains("OldValue")) dgvAudit.Columns["OldValue"].Visible = false;
            if (dgvAudit.Columns.Contains("NewValue")) dgvAudit.Columns["NewValue"].Visible = false;
        }

        private void btnRefresh_Click(object sender, EventArgs e) => LoadData();
    }
}
