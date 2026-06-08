// ============================================================
//  UC_Course.cs – Quản lý Môn học đầy đủ
// ============================================================
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace QLSV
{
    public partial class UC_Course : UserControl
    {
        private string _selCourseID = null;
        private bool   _isEditing   = false;

        public UC_Course() { InitializeComponent(); }

        public void LoadData()
        {
            LoadDeptCombo();
            LoadGrid();
        }

        private void LoadDeptCombo()
        {
            DataTable dt = Department.GetComboSource("Department");
            cbDeptFilter.DataSource    = dt.Copy();
            cbDeptFilter.DisplayMember = "Name";
            cbDeptFilter.ValueMember   = "ID";
            cbDeptFilter.Items.Insert(0, "-- Tất cả --");

            cbDeptForm.DataSource    = Department.GetComboSource("Department");
            cbDeptForm.DisplayMember = "Name";
            cbDeptForm.ValueMember   = "ID";
        }

        private void LoadGrid(string kw = "", int deptID = -1)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT c.CourseID AS [Mã môn], c.CourseName AS [Tên môn], c.Credits AS [TC], " +
                    "c.CourseType AS [Loại], c.LT_Hours AS [Tiết LT], c.TH_Hours AS [Tiết TH], " +
                    "d.DeptName AS [Khoa], c.Description AS [Mô tả] " +
                    "FROM Course c LEFT JOIN Department d ON c.DeptID=d.DeptID " +
                    "WHERE (@kw='' OR c.CourseID LIKE @kw OR c.CourseName LIKE @kw) " +
                    "AND (@did=-1 OR c.DeptID=@did) ORDER BY c.CourseName", db.conn);
                cmd.Parameters.AddWithValue("@kw",  string.IsNullOrWhiteSpace(kw) ? "" : "%" + kw + "%");
                cmd.Parameters.AddWithValue("@did", deptID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }

            dgv.DataSource = dt;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible   = false;
            dgv.ReadOnly            = true;
            dgv.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            if (dgv.Columns.Contains("Mô tả")) dgv.Columns["Mô tả"].Visible = false;
            lblTotal.Text = $"Tổng: {dt.Rows.Count} môn học";
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            _selCourseID = row.Cells["Mã môn"].Value?.ToString();
            _isEditing   = true;

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("SELECT * FROM Course WHERE CourseID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", _selCourseID);
                var dr = cmd.ExecuteReader();
                if (!dr.Read()) return;
                txbID.Text          = dr["CourseID"].ToString();
                txbName.Text        = dr["CourseName"].ToString();
                nudCredits.Value    = Convert.ToDecimal(dr["Credits"]);
                txbDesc.Text        = dr["Description"]?.ToString();
                cbType.Text         = dr["CourseType"]?.ToString() ?? "Lý thuyết";
                nudLT.Value         = Convert.ToDecimal(dr["LT_Hours"]);
                nudTH.Value         = Convert.ToDecimal(dr["TH_Hours"]);
                if (dr["DeptID"] != DBNull.Value) cbDeptForm.SelectedValue = dr["DeptID"];
            }
            finally { db.closeConnection(); }

            txbID.Enabled     = false;
            btnDelete.Enabled = AppSession.IsAdmin;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _selCourseID = null; _isEditing = false;
            txbID.Clear(); txbName.Clear(); txbDesc.Clear();
            nudCredits.Value = 3; nudLT.Value = 30; nudTH.Value = 0;
            cbType.SelectedIndex = 0;
            txbID.Enabled = true; txbID.Focus();
            btnDelete.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbID.Text) || string.IsNullOrWhiteSpace(txbName.Text))
            { MessageBox.Show("Nhập đầy đủ mã và tên môn!", "⚠️"); return; }

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                int? deptID = cbDeptForm.SelectedValue is int d ? d : (int?)null;
                SqlCommand cmd;
                if (_isEditing)
                    cmd = new SqlCommand(
                        "UPDATE Course SET CourseName=@n,Credits=@cr,Description=@ds,CourseType=@ct,LT_Hours=@lt,TH_Hours=@th,DeptID=@did WHERE CourseID=@id", db.conn);
                else
                    cmd = new SqlCommand(
                        "INSERT INTO Course(CourseID,CourseName,Credits,Description,CourseType,LT_Hours,TH_Hours,DeptID) VALUES(@id,@n,@cr,@ds,@ct,@lt,@th,@did)", db.conn);

                cmd.Parameters.AddWithValue("@id",  txbID.Text.Trim().ToUpper());
                cmd.Parameters.AddWithValue("@n",   txbName.Text.Trim());
                cmd.Parameters.AddWithValue("@cr",  (int)nudCredits.Value);
                cmd.Parameters.AddWithValue("@ds",  txbDesc.Text.Trim());
                cmd.Parameters.AddWithValue("@ct",  cbType.Text);
                cmd.Parameters.AddWithValue("@lt",  (int)nudLT.Value);
                cmd.Parameters.AddWithValue("@th",  (int)nudTH.Value);
                cmd.Parameters.AddWithValue("@did", (object)deptID ?? DBNull.Value);

                if (cmd.ExecuteNonQuery() > 0)
                {
                    AuditHelper.Log(_isEditing ? "UPDATE" : "INSERT", "Course", txbID.Text, null, txbName.Text);
                    MessageBox.Show("✅ Lưu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnNew_Click(null, null);
                    LoadGrid(txbSearch.Text.Trim(), GetDeptFilter());
                }
                else MessageBox.Show("❌ Lưu thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { db.closeConnection(); }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selCourseID)) return;
            if (MessageBox.Show($"Xóa môn [{_selCourseID}]?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("DELETE FROM Course WHERE CourseID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", _selCourseID);
                if (cmd.ExecuteNonQuery() > 0)
                { AuditHelper.Log("DELETE","Course",_selCourseID); MessageBox.Show("✅ Đã xóa!","Thông báo"); btnNew_Click(null,null); LoadGrid(); }
                else MessageBox.Show("❌ Không thể xóa (có lớp môn học liên quan)!","Lỗi",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            finally { db.closeConnection(); }
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Excel|*.xlsx;*.xls" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            int ok = 0, fail = 0; string errors = "";
            try
            {
                using var wb = new ClosedXML.Excel.XLWorkbook(ofd.FileName);
                var ws   = wb.Worksheets.First();
                int rows = ws.LastRowUsed().RowNumber();
                My_DB db = new My_DB();
                db.openConnection();
                for (int r = 2; r <= rows; r++)
                {
                    try
                    {
                        string cid  = ws.Cell(r,1).GetString().Trim().ToUpper();
                        string name = ws.Cell(r,2).GetString().Trim();
                        int    cred = int.Parse(ws.Cell(r,3).GetString().Trim());
                        if (string.IsNullOrEmpty(cid)||string.IsNullOrEmpty(name)) continue;
                        var cmd = new SqlCommand(
                            "IF NOT EXISTS(SELECT 1 FROM Course WHERE CourseID=@id) " +
                            "INSERT INTO Course(CourseID,CourseName,Credits) VALUES(@id,@n,@cr)", db.conn);
                        cmd.Parameters.AddWithValue("@id",cid); cmd.Parameters.AddWithValue("@n",name); cmd.Parameters.AddWithValue("@cr",cred);
                        if (cmd.ExecuteNonQuery()>0) ok++; else fail++;
                    }
                    catch { errors += $"Lỗi dòng {r}\n"; fail++; }
                }
                db.closeConnection();
            }
            catch (Exception ex) { errors = ex.Message; }

            MessageBox.Show($"✅ Thêm {ok} môn. Lỗi {fail}.\n{errors}", "Kết quả import");
            LoadGrid();
        }

        private void btnExport_Click(object sender, EventArgs e)
            => ExcelHelper.ExportToExcel(((DataTable)dgv.DataSource), "DanhSachMonHoc", "DANH SÁCH MÔN HỌC");

        private void txbSearch_TextChanged(object sender, EventArgs e) => LoadGrid(txbSearch.Text.Trim(), GetDeptFilter());
        private void cbDeptFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadGrid(txbSearch.Text.Trim(), GetDeptFilter());

        private int GetDeptFilter()
        {
            if (cbDeptFilter.SelectedItem?.ToString() == "-- Tất cả --") return -1;
            return cbDeptFilter.SelectedValue is int id ? id : -1;
        }

        private void InitializeComponent()
        {
            this.dgv           = new DataGridView();
            this.txbSearch     = new TextBox();
            this.cbDeptFilter  = new ComboBox();
            this.cbDeptForm    = new ComboBox();
            this.cbType        = new ComboBox();
            this.txbID         = new TextBox();
            this.txbName       = new TextBox();
            this.txbDesc       = new TextBox();
            this.nudCredits    = new NumericUpDown();
            this.nudLT         = new NumericUpDown();
            this.nudTH         = new NumericUpDown();
            this.lblTotal      = new Label();
            this.btnNew        = new Button();
            this.btnSave       = new Button();
            this.btnDelete     = new Button();
            this.btnImportExcel= new Button();
            this.btnExport     = new Button();

            var font9b = new Font("Segoe UI", 9f, FontStyle.Bold);
            var font10 = new Font("Segoe UI", 10f);

            var top = new Panel { Dock=DockStyle.Top, Height=44, BackColor=Color.FromArgb(240,244,255) };
            var title = new Label { Text="📚 Quản lý Môn học", Left=10, Top=10, Width=200, Font=new Font("Segoe UI",10.5f,FontStyle.Bold) };
            var lblSr = new Label { Text="🔍", Left=215, Top=13, Width=20, Font=font9b };
            txbSearch.Left=235; txbSearch.Top=10; txbSearch.Width=180; txbSearch.Font=font10; txbSearch.PlaceholderText="Tìm mã, tên môn...";
            txbSearch.TextChanged+=new EventHandler(txbSearch_TextChanged);
            var lblDf = new Label { Text="Khoa:", Left=425, Top=13, Width=45, Font=font9b };
            cbDeptFilter.Left=472; cbDeptFilter.Top=9; cbDeptFilter.Width=150; cbDeptFilter.DropDownStyle=ComboBoxStyle.DropDownList;
            cbDeptFilter.SelectedIndexChanged+=new EventHandler(cbDeptFilter_SelectedIndexChanged);
            lblTotal.Left=635; lblTotal.Top=13; lblTotal.Width=150; lblTotal.Font=new Font("Segoe UI",9f,FontStyle.Italic); lblTotal.ForeColor=Color.Gray;
            top.Controls.AddRange(new Control[]{ title, lblSr, txbSearch, lblDf, cbDeptFilter, lblTotal });

            dgv.Dock=DockStyle.Fill; dgv.AllowUserToAddRows=false; dgv.BackgroundColor=Color.White; dgv.BorderStyle=BorderStyle.None; dgv.Font=font10; dgv.RowTemplate.Height=28;
            dgv.CellClick+=new DataGridViewCellEventHandler(dgv_CellClick);

            var pnl = new Panel { Dock=DockStyle.Right, Width=290, BackColor=Color.FromArgb(245,247,252), AutoScroll=true, Padding=new Padding(10) };
            int y=10;
            void Row(string lbl, Control ctl, int h=28) {
                var l=new Label{Text=lbl,Left=10,Top=y,Width=265,Font=font9b};
                ctl.Left=10; ctl.Top=y+18; ctl.Width=265; ctl.Height=h;
                if(ctl is TextBox tb)tb.Font=font10;
                if(ctl is ComboBox cb)cb.Font=font10;
                pnl.Controls.Add(l); pnl.Controls.Add(ctl); y+=h+28;
            }
            Row("Mã môn *", txbID);
            Row("Tên môn *", txbName);
            nudCredits.Minimum=1; nudCredits.Maximum=10; nudCredits.Value=3;
            Row("Số tín chỉ", nudCredits, 26);
            cbType.Items.AddRange(new object[]{"Lý thuyết","Thực hành","Lý thuyết + Thực hành","Đồ án"});
            cbType.DropDownStyle=ComboBoxStyle.DropDownList; cbType.SelectedIndex=0;
            Row("Loại môn học", cbType, 26);
            nudLT.Minimum=0; nudLT.Maximum=200; nudLT.Value=30;
            Row("Số tiết LT", nudLT, 26);
            nudTH.Minimum=0; nudTH.Maximum=200; nudTH.Value=0;
            Row("Số tiết TH", nudTH, 26);
            Row("Thuộc khoa", cbDeptForm, 26); cbDeptForm.DropDownStyle=ComboBoxStyle.DropDownList;
            txbDesc.Multiline=true;
            Row("Mô tả", txbDesc, 55);

            void BtnF(Button b, string txt, Color bg, int left, int w=82) {
                b.Text=txt; b.Left=left; b.Top=y; b.Width=w; b.Height=30;
                b.FlatStyle=FlatStyle.Flat; b.BackColor=bg; b.ForeColor=Color.White;
                b.Font=font9b; b.FlatAppearance.BorderSize=0; pnl.Controls.Add(b);
            }
            BtnF(btnNew,"➕ Mới",Color.FromArgb(59,130,246),10,60);
            BtnF(btnSave,"💾 Lưu",Color.FromArgb(34,197,94),76,60);
            BtnF(btnDelete,"🗑 Xóa",Color.FromArgb(239,68,68),142,60); y+=40;
            BtnF(btnImportExcel,"📥 Import Excel",Color.FromArgb(245,158,11),10,130);
            BtnF(btnExport,"📤 Xuất Excel",Color.FromArgb(99,102,241),146,120);

            btnNew.Click+=new EventHandler(btnNew_Click);
            btnSave.Click+=new EventHandler(btnSave_Click);
            btnDelete.Click+=new EventHandler(btnDelete_Click);
            btnImportExcel.Click+=new EventHandler(btnImportExcel_Click);
            btnExport.Click+=new EventHandler(btnExport_Click);
            btnDelete.Enabled=false;

            Controls.Add(dgv); Controls.Add(pnl); Controls.Add(top);
            Dock=DockStyle.Fill;
        }

        private DataGridView dgv;
        private TextBox txbSearch, txbID, txbName, txbDesc;
        private ComboBox cbDeptFilter, cbDeptForm, cbType;
        private NumericUpDown nudCredits, nudLT, nudTH;
        private Label lblTotal;
        private Button btnNew, btnSave, btnDelete, btnImportExcel, btnExport;
    }

    // ============================================================
    //  UC_TrainingScore.cs – Điểm rèn luyện
    // ============================================================
    public partial class UC_TrainingScore : UserControl
    {
        private int    _mssv;
        private string _role;
        private int    _selMSSV = -1;

        public UC_TrainingScore() { InitializeComponent(); }

        public void LoadData(int mssv, string role)
        {
            _mssv = mssv; _role = role;
            LoadSemCombo();
            bool canEdit = role != "SinhVien";
            pnlEdit.Visible = canEdit;
            if (role == "SinhVien") LoadMyScore();
            else LoadAllScores();
        }

        private void LoadSemCombo()
        {
            DataTable dt = Semester.GetComboSource();
            cbSem.DataSource = dt; cbSem.DisplayMember = "Name"; cbSem.ValueMember = "ID";
            int aid = Semester.GetActiveSemesterID();
            if (aid > 0) cbSem.SelectedValue = aid;
        }

        private void LoadMyScore()
        {
            int semID = cbSem.SelectedValue is int s ? s : -1;
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT se.SemName AS [Học kỳ], ts.C1_Hocluc AS [Học lực (30)], " +
                    "ts.C2_Cando AS [Chấp hành (25)], ts.C3_Hoatdong AS [Hoạt động (20)], " +
                    "ts.C4_Kyluong AS [Kỷ luật (25)], ts.TotalScore AS [Tổng], ts.Rank AS [Xếp loại] " +
                    "FROM TrainingScore ts JOIN Semester se ON ts.SemesterID=se.SemesterID " +
                    "WHERE ts.MSSV=@m AND (@sid=-1 OR ts.SemesterID=@sid) ORDER BY se.StartDate DESC", db.conn);
                cmd.Parameters.AddWithValue("@m", _mssv); cmd.Parameters.AddWithValue("@sid", semID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            dgv.DataSource = dt;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false; dgv.ReadOnly = true;
            ColorRankRows();
        }

        private void LoadAllScores()
        {
            int semID = cbSem.SelectedValue is int s ? s : -1;
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT ts.MSSV, st.Fname+' '+st.Lname AS [Họ tên], " +
                    "ts.C1_Hocluc AS [Học lực], ts.C2_Cando AS [Chấp hành], " +
                    "ts.C3_Hoatdong AS [Hoạt động], ts.C4_Kyluong AS [Kỷ luật], " +
                    "ts.TotalScore AS [Tổng], ts.Rank AS [Xếp loại] " +
                    "FROM TrainingScore ts JOIN Student st ON ts.MSSV=st.MSSV " +
                    "WHERE (@sid=-1 OR ts.SemesterID=@sid) ORDER BY ts.TotalScore DESC", db.conn);
                cmd.Parameters.AddWithValue("@sid", semID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            dgv.DataSource = dt;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false; dgv.ReadOnly = true;
            if (dgv.Columns.Contains("MSSV")) dgv.Columns["MSSV"].Visible = false;
            ColorRankRows();
        }

        private void ColorRankRows()
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string rank = row.Cells["Xếp loại"].Value?.ToString();
                row.DefaultCellStyle.BackColor =
                    rank == "Xuất sắc" ? Color.FromArgb(200,255,200) :
                    rank == "Tốt"      ? Color.FromArgb(220,240,255) :
                    rank == "Khá"      ? Color.FromArgb(255,255,210) :
                    rank == "Yếu"      ? Color.FromArgb(255,210,210) : Color.White;
            }
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _role == "SinhVien") return;
            var row = dgv.Rows[e.RowIndex];
            if (!dgv.Columns.Contains("MSSV")) return;
            _selMSSV = Convert.ToInt32(row.Cells["MSSV"].Value);
            nudC1.Value = Convert.ToDecimal(row.Cells["Học lực"].Value);
            nudC2.Value = Convert.ToDecimal(row.Cells["Chấp hành"].Value);
            nudC3.Value = Convert.ToDecimal(row.Cells["Hoạt động"].Value);
            nudC4.Value = Convert.ToDecimal(row.Cells["Kỷ luật"].Value);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_selMSSV == -1) { MessageBox.Show("Chọn sinh viên trước!", "⚠️"); return; }
            int semID = cbSem.SelectedValue is int s ? s : -1;
            if (semID == -1) { MessageBox.Show("Chọn học kỳ!", "⚠️"); return; }

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "IF EXISTS(SELECT 1 FROM TrainingScore WHERE MSSV=@m AND SemesterID=@sid) " +
                    "  UPDATE TrainingScore SET C1_Hocluc=@c1,C2_Cando=@c2,C3_Hoatdong=@c3,C4_Kyluong=@c4 WHERE MSSV=@m AND SemesterID=@sid " +
                    "ELSE INSERT INTO TrainingScore(MSSV,SemesterID,C1_Hocluc,C2_Cando,C3_Hoatdong,C4_Kyluong) VALUES(@m,@sid,@c1,@c2,@c3,@c4)", db.conn);
                cmd.Parameters.AddWithValue("@m",  _selMSSV); cmd.Parameters.AddWithValue("@sid",semID);
                cmd.Parameters.AddWithValue("@c1",(double)nudC1.Value); cmd.Parameters.AddWithValue("@c2",(double)nudC2.Value);
                cmd.Parameters.AddWithValue("@c3",(double)nudC3.Value); cmd.Parameters.AddWithValue("@c4",(double)nudC4.Value);
                if (cmd.ExecuteNonQuery()>0)
                { MessageBox.Show("✅ Lưu điểm rèn luyện thành công!","Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Information); LoadAllScores(); }
            }
            finally { db.closeConnection(); }
        }

        private void cbSem_SelectedIndexChanged(object sender, EventArgs e)
        { if (_role=="SinhVien") LoadMyScore(); else LoadAllScores(); }

        private void btnExport_Click(object sender, EventArgs e)
            => ExcelHelper.ExportToExcel((DataTable)dgv.DataSource, "DiemRenLuyen", "BẢNG ĐIỂM RÈN LUYỆN");

        private void InitializeComponent()
        {
            this.dgv   = new DataGridView();
            this.cbSem = new ComboBox();
            this.nudC1 = new NumericUpDown(); this.nudC2 = new NumericUpDown();
            this.nudC3 = new NumericUpDown(); this.nudC4 = new NumericUpDown();
            this.pnlEdit   = new Panel();
            this.btnSave   = new Button();
            this.btnExport = new Button();

            var font9b = new Font("Segoe UI",9f,FontStyle.Bold);
            var font10 = new Font("Segoe UI",10f);

            var top = new Panel { Dock=DockStyle.Top, Height=44, BackColor=Color.FromArgb(240,244,255) };
            var title = new Label { Text="⭐ Điểm Rèn luyện", Left=10, Top=10, Width=200, Font=new Font("Segoe UI",10.5f,FontStyle.Bold) };
            var lblHK = new Label { Text="Học kỳ:", Left=215, Top=13, Width=60, Font=font9b };
            cbSem.Left=278; cbSem.Top=9; cbSem.Width=180; cbSem.DropDownStyle=ComboBoxStyle.DropDownList;
            cbSem.SelectedIndexChanged+=new EventHandler(cbSem_SelectedIndexChanged);
            btnExport.Left=475; btnExport.Top=9; btnExport.Width=100; btnExport.Height=28;
            btnExport.Text="📤 Xuất Excel"; btnExport.FlatStyle=FlatStyle.Flat;
            btnExport.BackColor=Color.FromArgb(99,102,241); btnExport.ForeColor=Color.White;
            btnExport.Font=font9b; btnExport.FlatAppearance.BorderSize=0;
            btnExport.Click+=new EventHandler(btnExport_Click);
            top.Controls.AddRange(new Control[]{ title, lblHK, cbSem, btnExport });

            dgv.Dock=DockStyle.Fill; dgv.AllowUserToAddRows=false; dgv.BackgroundColor=Color.White;
            dgv.BorderStyle=BorderStyle.None; dgv.Font=font10; dgv.RowTemplate.Height=28;
            dgv.CellClick+=new DataGridViewCellEventHandler(dgv_CellClick);

            pnlEdit.Dock=DockStyle.Bottom; pnlEdit.Height=60; pnlEdit.BackColor=Color.FromArgb(245,247,252);
            var pairs = new[]{("Học lực (0-30):",nudC1,30m),("Chấp hành (0-25):",nudC2,25m),("Hoạt động (0-20):",nudC3,20m),("Kỷ luật (0-25):",nudC4,25m)};
            int px=10;
            foreach(var (txt,nud,max) in pairs){
                var l=new Label{Text=txt,Left=px,Top=8,Width=120,Font=font9b};
                nud.Left=px+122; nud.Top=6; nud.Width=60; nud.Minimum=0; nud.Maximum=max; nud.DecimalPlaces=1;
                pnlEdit.Controls.Add(l); pnlEdit.Controls.Add(nud); px+=190;
            }
            btnSave.Left=px+10; btnSave.Top=8; btnSave.Width=90; btnSave.Height=30;
            btnSave.Text="💾 Lưu điểm"; btnSave.FlatStyle=FlatStyle.Flat;
            btnSave.BackColor=Color.FromArgb(34,197,94); btnSave.ForeColor=Color.White;
            btnSave.Font=font9b; btnSave.FlatAppearance.BorderSize=0;
            btnSave.Click+=new EventHandler(btnSave_Click);
            pnlEdit.Controls.Add(btnSave);

            Controls.Add(dgv); Controls.Add(pnlEdit); Controls.Add(top);
            Dock=DockStyle.Fill;
        }

        private DataGridView dgv;
        private ComboBox cbSem;
        private NumericUpDown nudC1, nudC2, nudC3, nudC4;
        private Panel pnlEdit;
        private Button btnSave, btnExport;
    }
}
