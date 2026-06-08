// ============================================================
//  UC_ClassDetail.cs – Chi tiết lớp môn học
//  (Bảng tin · Lịch học · Tài liệu · Điểm thành phần)
// ============================================================
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace QLSV
{
    public partial class UC_ClassDetail : UserControl
    {
        private int    _classID;
        private int    _accountID;
        private string _role;
        private int    _selPostID  = -1;
        private int    _selCompID  = -1;
        private int    _selDocID   = -1;
        private int    _selFolderID = -1;

        public UC_ClassDetail() { InitializeComponent(); }

        public void LoadData(int classID, int accountID, string role)
        {
            _classID   = classID;
            _accountID = accountID;
            _role      = role;

            LoadClassInfo();
            LoadPosts();
            LoadSchedule();
            LoadDocuments();
            LoadScoreComponents();

            // Phân quyền
            bool canPost   = role != "SinhVien";
            bool canScore  = role != "SinhVien";
            btnAddPost.Visible    = canPost;
            btnAddMakeup.Visible  = canPost;
            btnAddComp.Visible    = canScore;
            pnlScoreEdit.Visible  = canScore;
        }

        // ── THÔNG TIN LỚP ────────────────────────────────────────
        private void LoadClassInfo()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT cr.ClassCode, co.CourseName, co.Credits, " +
                    "cr.TeacherName, cr.Room, cr.DayOfWeek, cr.StartPeriod, cr.NumPeriod, " +
                    "se.SemName, cr.MaxSlot, cr.Status, " +
                    "(SELECT COUNT(*) FROM Enrollment e WHERE e.ClassID=cr.ClassID AND e.Status=N'Đã đăng ký') AS Enrolled " +
                    "FROM ClassRoom cr " +
                    "JOIN Course   co ON cr.CourseID=co.CourseID " +
                    "JOIN Semester se ON cr.SemesterID=se.SemesterID " +
                    "WHERE cr.ClassID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", _classID);
                var dr = cmd.ExecuteReader();
                if (!dr.Read()) return;

                lblClassName.Text   = $"{dr["ClassCode"]} – {dr["CourseName"]} ({dr["Credits"]} TC)";
                lblTeacher.Text     = $"Giảng viên: {dr["TeacherName"]}";
                lblRoom.Text        = $"Phòng: {dr["Room"]}  |  {ClassRoom.DAY_NAMES[Convert.ToInt32(dr["DayOfWeek"])-1]}, Tiết {dr["StartPeriod"]}–{Convert.ToInt32(dr["StartPeriod"])+Convert.ToInt32(dr["NumPeriod"])-1}";
                lblSemester.Text    = $"Học kỳ: {dr["SemName"]}";
                lblSlot.Text        = $"Sĩ số: {dr["Enrolled"]}/{dr["MaxSlot"]}";
                lblStatus.Text      = $"Trạng thái: {dr["Status"]}";
            }
            finally { db.closeConnection(); }
        }

        // ══════════════════════════════════════════════
        //  BẢNG TIN
        // ══════════════════════════════════════════════
        private void LoadPosts()
        {
            DataTable dt = Post.GetByClass(_classID);
            lstPosts.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                string pin  = Convert.ToBoolean(row["IsPinned"]) ? "📌 " : "";
                string item = $"{pin}{row["Title"]}  –  {row["AuthorName"]}  [{Convert.ToDateTime(row["CreatedAt"]):dd/MM HH:mm}]  💬{row["CmtCount"]}";
                var li = new ListViewItem(item) { Tag = row };
                lstPosts.Items.Add(li);
            }
        }

        private void lstPosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPosts.SelectedItems.Count == 0) return;
            var row = (DataRow)lstPosts.SelectedItems[0].Tag;
            _selPostID = Convert.ToInt32(row["PostID"]);

            rtbPostContent.Text = row["Content"].ToString();
            if (row["ImageData"] != DBNull.Value)
            {
                using var ms = new MemoryStream((byte[])row["ImageData"]);
                picPost.Image = Image.FromStream(ms);
            }
            else picPost.Image = null;

            LoadComments(_selPostID);
        }

        private void LoadComments(int postID)
        {
            DataTable dt = Post.GetComments(postID);
            lstComments.Items.Clear();
            foreach (DataRow row in dt.Rows)
                lstComments.Items.Add(
                    new ListViewItem($"{row["Author"]}  [{Convert.ToDateTime(row["CreatedAt"]):dd/MM HH:mm}]:  {row["Content"]}")
                    { Tag = row });
        }

        private void btnAddPost_Click(object sender, EventArgs e)
        {
            string title   = txbPostTitle.Text.Trim();
            string content = rtbNewPost.Text.Trim();
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            { MessageBox.Show("Nhập tiêu đề và nội dung!", "⚠️"); return; }

            byte[] imgData = null;
            if (picNewPost.Image != null)
            {
                using var ms = new MemoryStream();
                picNewPost.Image.Save(ms, picNewPost.Image.RawFormat);
                imgData = ms.ToArray();
            }

            var post = new Post {
                ClassID   = _classID,
                AuthorID  = _accountID,
                Title     = title,
                Content   = content,
                ImageData = imgData,
                IsPinned  = chkPin.Checked
            };

            if (post.Add())
            {
                // Tạo thông báo
                new Notification {
                    Title    = $"[Bảng tin] {title}",
                    Content  = content.Length > 100 ? content.Substring(0, 100) + "..." : content,
                    NotifType = "Bài đăng mới",
                    ClassID  = _classID,
                    SenderID = _accountID,
                    TargetRole = "SinhVien"
                }.Add();

                txbPostTitle.Clear(); rtbNewPost.Clear(); chkPin.Checked = false; picNewPost.Image = null;
                LoadPosts();
                MessageBox.Show("✅ Đăng bài thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else MessageBox.Show("❌ Đăng bài thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnDeletePost_Click(object sender, EventArgs e)
        {
            if (_selPostID == -1) return;
            if (MessageBox.Show("Xóa bài đăng này?", "Xác nhận",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            if (Post.Delete(_selPostID))
            { _selPostID = -1; LoadPosts(); rtbPostContent.Clear(); lstComments.Items.Clear(); }
        }

        private void btnAddComment_Click(object sender, EventArgs e)
        {
            if (_selPostID == -1 || string.IsNullOrWhiteSpace(txbComment.Text)) return;
            if (Post.AddComment(_selPostID, _accountID, txbComment.Text.Trim()))
            {
                // Thông báo khi bình luận
                new Notification {
                    Title     = "[Bình luận mới]",
                    Content   = txbComment.Text.Trim(),
                    NotifType = "Bình luận",
                    ClassID   = _classID,
                    SenderID  = _accountID
                }.Add();
                txbComment.Clear();
                LoadComments(_selPostID);
            }
        }

        private void btnPickPostImage_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog {
                Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
                picNewPost.Image = Image.FromFile(ofd.FileName);
        }

        // ══════════════════════════════════════════════
        //  LỊCH HỌC · NGHỈ · BÙ
        // ══════════════════════════════════════════════
        private void LoadSchedule()
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT MdID, DayDate AS [Ngày], DayType AS [Loại], Note AS [Ghi chú], " +
                    "IsNotified AS [Đã thông báo] " +
                    "FROM MakeupDay WHERE ClassID=@cid ORDER BY DayDate DESC", db.conn);
                cmd.Parameters.AddWithValue("@cid", _classID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }

            dgvSchedule.DataSource = dt;
            dgvSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSchedule.RowHeadersVisible   = false;
            dgvSchedule.ReadOnly            = true;
            if (dgvSchedule.Columns.Contains("MdID")) dgvSchedule.Columns["MdID"].Visible = false;

            foreach (DataGridViewRow row in dgvSchedule.Rows)
            {
                string type = row.Cells["Loại"].Value?.ToString();
                row.DefaultCellStyle.BackColor =
                    type == "Nghỉ" ? Color.FromArgb(255, 230, 230) : Color.FromArgb(230, 255, 230);
            }
        }

        private void btnAddMakeup_Click(object sender, EventArgs e)
        {
            string type = rbNghi.Checked ? "Nghỉ" : "Bù";
            if (dtpMakeup.Value.Date < DateTime.Today.Date)
            { MessageBox.Show("Không thể thêm ngày trong quá khứ!", "⚠️"); return; }

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO MakeupDay(ClassID,DayDate,DayType,Note,IsNotified,CreatedByID,CreatedAt) " +
                    "VALUES(@cid,@d,@t,@n,0,@aid,GETDATE())", db.conn);
                cmd.Parameters.AddWithValue("@cid", _classID);
                cmd.Parameters.AddWithValue("@d",   dtpMakeup.Value.Date);
                cmd.Parameters.AddWithValue("@t",   type);
                cmd.Parameters.AddWithValue("@n",   txbMakeupNote.Text.Trim());
                cmd.Parameters.AddWithValue("@aid", _accountID);
                cmd.ExecuteNonQuery();

                // Thông báo tự động
                Notification.CreateMakeupNotification(
                    _classID, type, dtpMakeup.Value.Date, txbMakeupNote.Text.Trim(), _accountID);

                // Cập nhật cờ IsNotified
                new SqlCommand(
                    $"UPDATE MakeupDay SET IsNotified=1 WHERE ClassID={_classID} AND DayDate='{dtpMakeup.Value:yyyy-MM-dd}'",
                    db.conn).ExecuteNonQuery();

                MessageBox.Show($"✅ Đã thêm lịch {type.ToLower()} và gửi thông báo!", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                txbMakeupNote.Clear();
                LoadSchedule();
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}", "❌"); }
            finally { db.closeConnection(); }
        }

        private void btnDeleteMakeup_Click(object sender, EventArgs e)
        {
            if (dgvSchedule.SelectedRows.Count == 0) return;
            int mdID = Convert.ToInt32(dgvSchedule.SelectedRows[0].Cells["MdID"].Value);
            if (MessageBox.Show("Xóa lịch này?", "Xác nhận",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("DELETE FROM MakeupDay WHERE MdID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", mdID);
                cmd.ExecuteNonQuery();
                LoadSchedule();
            }
            finally { db.closeConnection(); }
        }

        // ══════════════════════════════════════════════
        //  TÀI LIỆU
        // ══════════════════════════════════════════════
        private void LoadDocuments()
        {
            My_DB db = new My_DB();
            DataTable dtFolders = new DataTable();
            try
            {
                db.openConnection();
                new SqlDataAdapter(
                    $"SELECT FolderID, FolderName FROM Folder WHERE ClassID={_classID} ORDER BY FolderName",
                    db.conn).Fill(dtFolders);
            }
            finally { db.closeConnection(); }

            tvDocs.Nodes.Clear();
            foreach (DataRow fr in dtFolders.Rows)
            {
                int    fid   = Convert.ToInt32(fr["FolderID"]);
                var    node  = new TreeNode(fr["FolderName"].ToString()) { Tag = fid, ImageIndex = 0 };

                My_DB db2 = new My_DB();
                DataTable dtDocs = new DataTable();
                try
                {
                    db2.openConnection();
                    var cmd = new SqlCommand(
                        "SELECT DocID, DocName, FileSize FROM Document WHERE FolderID=@fid", db2.conn);
                    cmd.Parameters.AddWithValue("@fid", fid);
                    new SqlDataAdapter(cmd).Fill(dtDocs);
                }
                finally { db2.closeConnection(); }

                foreach (DataRow dr in dtDocs.Rows)
                {
                    string sizeTxt = FormatSize(Convert.ToInt64(dr["FileSize"]));
                    var docNode = new TreeNode($"{dr["DocName"]}  ({sizeTxt})") {
                        Tag = dr["DocID"], ImageIndex = 1
                    };
                    node.Nodes.Add(docNode);
                }
                tvDocs.Nodes.Add(node);
            }
            tvDocs.ExpandAll();
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Tên thư mục:", "Thêm thư mục");
            if (string.IsNullOrWhiteSpace(name)) return;

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO Folder(ClassID,FolderName,CreatedAt) VALUES(@cid,@n,GETDATE())", db.conn);
                cmd.Parameters.AddWithValue("@cid", _classID);
                cmd.Parameters.AddWithValue("@n",   name.Trim());
                cmd.ExecuteNonQuery();
                LoadDocuments();
            }
            finally { db.closeConnection(); }
        }

        private void btnUploadDoc_Click(object sender, EventArgs e)
        {
            if (tvDocs.SelectedNode == null || !(tvDocs.SelectedNode.Tag is int folderID))
            { MessageBox.Show("Chọn thư mục trước!", "⚠️"); return; }

            using var ofd = new OpenFileDialog { Multiselect = true,
                Filter = "Tất cả|*.*|PDF|*.pdf|Word|*.docx;*.doc|Excel|*.xlsx;*.xls|Ảnh|*.jpg;*.png" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            foreach (string path in ofd.FileNames)
            {
                var fi     = new FileInfo(path);
                string dest = Path.Combine(Application.StartupPath, "ClassDocs",
                              _classID.ToString(), Path.GetFileName(path));
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(path, dest, true);

                My_DB db = new My_DB();
                try
                {
                    db.openConnection();
                    var cmd = new SqlCommand(
                        "INSERT INTO Document(FolderID,ClassID,DocName,FilePath,FileSize,UploadedBy,UploadedAt) " +
                        "VALUES(@fid,@cid,@dn,@fp,@fs,@uid,GETDATE())", db.conn);
                    cmd.Parameters.AddWithValue("@fid", folderID);
                    cmd.Parameters.AddWithValue("@cid", _classID);
                    cmd.Parameters.AddWithValue("@dn",  fi.Name);
                    cmd.Parameters.AddWithValue("@fp",  dest);
                    cmd.Parameters.AddWithValue("@fs",  fi.Length);
                    cmd.Parameters.AddWithValue("@uid", _accountID);
                    cmd.ExecuteNonQuery();
                }
                finally { db.closeConnection(); }
            }
            MessageBox.Show($"✅ Đã upload {ofd.FileNames.Length} file!", "Thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadDocuments();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (tvDocs.SelectedNode == null || tvDocs.SelectedNode.Tag is int) return; // folder selected
            int docID = Convert.ToInt32(tvDocs.SelectedNode.Tag);

            My_DB db = new My_DB();
            string filePath = "", docName = "";
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("SELECT FilePath, DocName FROM Document WHERE DocID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", docID);
                var dr = cmd.ExecuteReader();
                if (dr.Read()) { filePath = dr["FilePath"].ToString(); docName = dr["DocName"].ToString(); }
            }
            finally { db.closeConnection(); }

            if (!File.Exists(filePath)) { MessageBox.Show("File không tồn tại!", "❌"); return; }

            using var sfd = new SaveFileDialog { FileName = docName };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.Copy(filePath, sfd.FileName, true);
                MessageBox.Show("✅ Tải xuống thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDeleteDoc_Click(object sender, EventArgs e)
        {
            if (tvDocs.SelectedNode == null || tvDocs.SelectedNode.Tag is int) return;
            int docID = Convert.ToInt32(tvDocs.SelectedNode.Tag);
            if (MessageBox.Show("Xóa tài liệu này?", "Xác nhận",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var getPath = new SqlCommand("SELECT FilePath FROM Document WHERE DocID=@id", db.conn);
                getPath.Parameters.AddWithValue("@id", docID);
                string path = getPath.ExecuteScalar()?.ToString();
                if (File.Exists(path)) File.Delete(path);

                var del = new SqlCommand("DELETE FROM Document WHERE DocID=@id", db.conn);
                del.Parameters.AddWithValue("@id", docID);
                del.ExecuteNonQuery();
                LoadDocuments();
            }
            finally { db.closeConnection(); }
        }

        // ══════════════════════════════════════════════
        //  ĐIỂM THÀNH PHẦN
        // ══════════════════════════════════════════════
        private void LoadScoreComponents()
        {
            DataTable dtComp = ScoreComponent.GetByClass(_classID);
            dgvComp.DataSource = dtComp;
            dgvComp.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvComp.RowHeadersVisible   = false;
            dgvComp.ReadOnly            = true;
            if (dgvComp.Columns.Contains("CompID"))    dgvComp.Columns["CompID"].Visible    = false;
            if (dgvComp.Columns.Contains("ClassID"))   dgvComp.Columns["ClassID"].Visible   = false;
            if (dgvComp.Columns.Contains("CompName"))  dgvComp.Columns["CompName"].HeaderText  = "Thành phần";
            if (dgvComp.Columns.Contains("Weight"))    dgvComp.Columns["Weight"].HeaderText    = "Trọng số (%)";
            if (dgvComp.Columns.Contains("CompOrder")) dgvComp.Columns["CompOrder"].HeaderText = "Thứ tự";

            // Hiển thị bảng điểm
            DataTable dtSheet = ScoreComponent.GetScoreSheet(_classID);
            if (dtSheet.Rows.Count > 0) BuildPivotScoreGrid(dtSheet);
        }

        private void BuildPivotScoreGrid(DataTable raw)
        {
            // Pivot: hàng = SV, cột = thành phần + tổng
            DataTable pivot = new DataTable();
            pivot.Columns.Add("MSSV");
            pivot.Columns.Add("Họ tên");

            DataTable comps = ScoreComponent.GetByClass(_classID);
            foreach (DataRow cr in comps.Rows)
                pivot.Columns.Add($"{cr["CompName"]} ({cr["Weight"]}%)");
            pivot.Columns.Add("Điểm tổng kết");

            var students = new System.Collections.Generic.Dictionary<string, DataRow>();
            foreach (DataRow r in raw.Rows)
            {
                string key = r["MSSV"].ToString();
                if (!students.ContainsKey(key))
                {
                    var newRow = pivot.NewRow();
                    newRow["MSSV"]   = r["MSSV"];
                    newRow["Họ tên"] = r["Họ tên"];
                    students[key]    = newRow;
                    pivot.Rows.Add(newRow);
                }
                string colName = $"{r["CompName"]} ({r["Weight"]}%)";
                if (pivot.Columns.Contains(colName))
                    students[key][colName] = r["Điểm"];
            }

            // Tính điểm tổng
            foreach (DataRow r in pivot.Rows)
            {
                double total = ScoreComponent.GetTotalScore(_classID, Convert.ToInt32(r["MSSV"]));
                r["Điểm tổng kết"] = Math.Round(total, 2);
            }

            dgvScore.DataSource = pivot;
            dgvScore.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvScore.RowHeadersVisible   = false;
            dgvScore.EditMode = _role != "SinhVien"
                ? DataGridViewEditMode.EditOnDoubleClick
                : DataGridViewEditMode.EditProgrammatically;

            // Tô màu điểm tổng
            foreach (DataGridViewRow row in dgvScore.Rows)
            {
                if (row.Cells["Điểm tổng kết"].Value == null) continue;
                double gpa = Convert.ToDouble(row.Cells["Điểm tổng kết"].Value);
                row.Cells["Điểm tổng kết"].Style.BackColor =
                    gpa >= 8.5 ? Color.FromArgb(200, 255, 200) :
                    gpa >= 5.0 ? Color.FromArgb(255, 255, 200) :
                                 Color.FromArgb(255, 200, 200);
            }
        }

        private void btnAddComp_Click(object sender, EventArgs e)
        {
            string name = txbCompName.Text.Trim();
            if (string.IsNullOrEmpty(name) || nudWeight.Value <= 0) return;

            // Kiểm tra tổng trọng số
            DataTable existing = ScoreComponent.GetByClass(_classID);
            double totalW = 0;
            foreach (DataRow r in existing.Rows) totalW += Convert.ToDouble(r["Weight"]);
            if (totalW + (double)nudWeight.Value > 100)
            { MessageBox.Show($"Tổng trọng số vượt 100%! Còn lại: {100 - totalW}%", "⚠️"); return; }

            var comp = new ScoreComponent {
                ClassID   = _classID,
                CompName  = name,
                Weight    = (double)nudWeight.Value,
                CompOrder = existing.Rows.Count + 1
            };
            if (comp.Add())
            {
                txbCompName.Clear(); nudWeight.Value = 10;
                LoadScoreComponents();
                MessageBox.Show("✅ Thêm thành phần điểm thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSaveScore_Click(object sender, EventArgs e)
        {
            if (dgvScore.SelectedCells.Count == 0) return;
            var cell = dgvScore.SelectedCells[0];
            var row  = dgvScore.Rows[cell.RowIndex];
            int mssv = Convert.ToInt32(row.Cells["MSSV"].Value);
            string colName = dgvScore.Columns[cell.ColumnIndex].HeaderText;

            // Tìm CompID từ tên cột
            DataTable comps = ScoreComponent.GetByClass(_classID);
            foreach (DataRow cr in comps.Rows)
            {
                string expectedCol = $"{cr["CompName"]} ({cr["Weight"]}%)";
                if (expectedCol == colName)
                {
                    if (!double.TryParse(cell.Value?.ToString(), out double sc) || sc < 0 || sc > 10)
                    { MessageBox.Show("Điểm phải từ 0 đến 10!", "⚠️"); return; }

                    if (ScoreComponent.SetScore(Convert.ToInt32(cr["CompID"]), mssv, sc))
                    {
                        MessageBox.Show("✅ Đã lưu điểm!", "Thành công",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadScoreComponents();
                    }
                    return;
                }
            }
        }

        // ── HELPERS ──────────────────────────────────────────────
        private string FormatSize(long bytes)
        {
            if (bytes < 1024)       return $"{bytes} B";
            if (bytes < 1024*1024)  return $"{bytes/1024} KB";
            return $"{bytes/1024/1024} MB";
        }
    }
}
