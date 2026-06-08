// ============================================================
//  Phase3_Phase4_Forms.Designer.cs
//  Designer cho UC_Notification, UC_Library, UC_Request, UC_Dashboard
// ============================================================
using System.Drawing;
using System.Windows.Forms;

namespace QLSV
{
    // ── UC_Notification Designer ──────────────────────────────
    partial class UC_Notification
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private DataGridView dgv;
        private RichTextBox rtbContent, rtbNew;
        private TextBox txbTitle, txbSearch;
        private ComboBox cbType, cbTargetRole, cbFilterType;
        private Label lblUnread, lblDetailTitle, lblDetailType;
        private Panel pnlForm;
        private Button btnSend, btnDelete;

        private void InitializeComponent()
        {
            this.dgv           = new DataGridView();
            this.rtbContent    = new RichTextBox();
            this.rtbNew        = new RichTextBox();
            this.txbTitle      = new TextBox();
            this.txbSearch     = new TextBox();
            this.cbType        = new ComboBox();
            this.cbTargetRole  = new ComboBox();
            this.cbFilterType  = new ComboBox();
            this.lblUnread     = new Label();
            this.lblDetailTitle= new Label();
            this.lblDetailType = new Label();
            this.pnlForm       = new Panel();
            this.btnSend       = new Button();
            this.btnDelete     = new Button();

            var font9b = new Font("Segoe UI", 9f, FontStyle.Bold);
            var font10 = new Font("Segoe UI", 10f);

            // Top bar
            var top = new Panel { Dock=DockStyle.Top, Height=44, BackColor=Color.FromArgb(240,244,255) };
            var title = new Label { Text="🔔 Thông báo", Left=10, Top=10, Width=140, Font=new Font("Segoe UI",10.5f,FontStyle.Bold) };
            var lblSr = new Label { Text="🔍", Left=155, Top=13, Width=20, Font=font9b };
            txbSearch.Left=175; txbSearch.Top=10; txbSearch.Width=160; txbSearch.Font=font10; txbSearch.PlaceholderText="Tìm thông báo...";
            txbSearch.TextChanged += new System.EventHandler(this.txbSearch_TextChanged);
            var lblF = new Label { Text="Loại:", Left=345, Top=13, Width=40, Font=font9b };
            cbFilterType.Left=387; cbFilterType.Top=9; cbFilterType.Width=140; cbFilterType.DropDownStyle=ComboBoxStyle.DropDownList;
            cbFilterType.Items.AddRange(new object[]{"Tất cả","Chung","Thông báo nghỉ","Thông báo bù","Bài đăng mới"});
            cbFilterType.SelectedIndex=0;
            cbFilterType.SelectedIndexChanged += new System.EventHandler(this.cbFilterType_SelectedIndexChanged);
            lblUnread.Left=540; lblUnread.Top=13; lblUnread.Width=130; lblUnread.Font=new Font("Segoe UI",9.5f,FontStyle.Bold); lblUnread.ForeColor=Color.FromArgb(220,50,50);
            top.Controls.AddRange(new Control[]{ title, lblSr, txbSearch, lblF, cbFilterType, lblUnread });

            // Main split
            var split = new SplitContainer { Dock=DockStyle.Fill, SplitterDistance=420, BorderStyle=BorderStyle.None };

            // Left: danh sách
            dgv.Dock=DockStyle.Fill; dgv.AllowUserToAddRows=false; dgv.BackgroundColor=Color.White;
            dgv.BorderStyle=BorderStyle.None; dgv.Font=font10; dgv.RowTemplate.Height=30;
            dgv.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            dgv.CellClick += new DataGridViewCellEventHandler(this.dgv_CellClick);
            split.Panel1.Controls.Add(dgv);

            // Right: chi tiết + form gửi
            lblDetailTitle.Dock=DockStyle.Top; lblDetailTitle.Height=30; lblDetailTitle.Font=new Font("Segoe UI",11f,FontStyle.Bold);
            lblDetailTitle.ForeColor=Color.FromArgb(30,60,140); lblDetailTitle.Padding=new Padding(8,5,0,0);
            lblDetailType.Dock=DockStyle.Top; lblDetailType.Height=22; lblDetailType.Font=new Font("Segoe UI",8.5f,FontStyle.Italic);
            lblDetailType.ForeColor=Color.Gray; lblDetailType.Padding=new Padding(8,0,0,0);
            rtbContent.Dock=DockStyle.Fill; rtbContent.ReadOnly=true; rtbContent.Font=font10; rtbContent.BackColor=Color.White; rtbContent.BorderStyle=BorderStyle.None;

            btnDelete.Dock=DockStyle.Top; btnDelete.Height=32; btnDelete.Text="🗑 Xóa thông báo này";
            btnDelete.FlatStyle=FlatStyle.Flat; btnDelete.BackColor=Color.FromArgb(239,68,68);
            btnDelete.ForeColor=Color.White; btnDelete.Font=font9b; btnDelete.FlatAppearance.BorderSize=0;
            btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            split.Panel2.Controls.Add(rtbContent);
            split.Panel2.Controls.Add(lblDetailTitle);
            split.Panel2.Controls.Add(lblDetailType);
            split.Panel2.Controls.Add(btnDelete);

            // Form gửi thông báo (Admin/GV)
            pnlForm.Dock=DockStyle.Bottom; pnlForm.Height=180; pnlForm.BackColor=Color.FromArgb(245,247,252); pnlForm.Padding=new Padding(8);
            var sep = new Label { Dock=DockStyle.Top, Height=1, BackColor=Color.FromArgb(200,210,230) };
            var fTitle = new Label { Dock=DockStyle.Top, Height=24, Text="✉️ Gửi thông báo mới", Font=new Font("Segoe UI",9.5f,FontStyle.Bold), Padding=new Padding(0,4,0,0) };
            var rowTop = new Panel { Dock=DockStyle.Top, Height=32 };
            txbTitle.Dock=DockStyle.Fill; txbTitle.Font=font10; txbTitle.PlaceholderText="Tiêu đề...";
            var lblRole = new Label { Text="Gửi đến:", Dock=DockStyle.Right, Width=65, Font=font9b, TextAlign=ContentAlignment.MiddleRight };
            cbTargetRole.Dock=DockStyle.Right; cbTargetRole.Width=110; cbTargetRole.DropDownStyle=ComboBoxStyle.DropDownList;
            cbTargetRole.Items.AddRange(new object[]{"Tất cả","SinhVien","GiaoVien","Admin"});
            cbTargetRole.SelectedIndex=0;
            rowTop.Controls.Add(txbTitle); rowTop.Controls.Add(lblRole); rowTop.Controls.Add(cbTargetRole);
            var rowType = new Panel { Dock=DockStyle.Top, Height=30 };
            var lblType = new Label { Text="Loại:", Left=0, Top=7, Width=40, Font=font9b };
            cbType.Left=44; cbType.Top=4; cbType.Width=180; cbType.DropDownStyle=ComboBoxStyle.DropDownList;
            rowType.Controls.Add(lblType); rowType.Controls.Add(cbType);
            rtbNew.Dock=DockStyle.Fill; rtbNew.Font=font10; rtbNew.Height=60;
            btnSend.Dock=DockStyle.Bottom; btnSend.Height=34; btnSend.Text="📤 Gửi thông báo";
            btnSend.FlatStyle=FlatStyle.Flat; btnSend.BackColor=Color.FromArgb(59,130,246);
            btnSend.ForeColor=Color.White; btnSend.Font=new Font("Segoe UI",9.5f,FontStyle.Bold); btnSend.FlatAppearance.BorderSize=0;
            btnSend.Click += new System.EventHandler(this.btnSend_Click);
            pnlForm.Controls.Add(rtbNew); pnlForm.Controls.Add(rowType); pnlForm.Controls.Add(rowTop); pnlForm.Controls.Add(fTitle); pnlForm.Controls.Add(sep); pnlForm.Controls.Add(btnSend);

            split.Panel2.Controls.Add(pnlForm);
            Controls.Add(split); Controls.Add(top);
            Dock=DockStyle.Fill;
        }
    }

    // ── UC_Library Designer ───────────────────────────────────
    partial class UC_Library
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private DataGridView dgvBooks, dgvBorrow;
        private TextBox txbSearch, txbBookTitle, txbBookAuthor, txbBookPub, txbBookCat;
        private NumericUpDown nudQty;
        private Label lblBookCount;
        private Panel pnlAdmin;
        private Button btnBorrow, btnReturn, btnAddBook;

        private void InitializeComponent()
        {
            this.dgvBooks      = new DataGridView();
            this.dgvBorrow     = new DataGridView();
            this.txbSearch     = new TextBox();
            this.txbBookTitle  = new TextBox();
            this.txbBookAuthor = new TextBox();
            this.txbBookPub    = new TextBox();
            this.txbBookCat    = new TextBox();
            this.nudQty        = new NumericUpDown();
            this.lblBookCount  = new Label();
            this.pnlAdmin      = new Panel();
            this.btnBorrow     = new Button();
            this.btnReturn     = new Button();
            this.btnAddBook    = new Button();

            var font9b = new Font("Segoe UI", 9f, FontStyle.Bold);
            var font10 = new Font("Segoe UI", 10f);

            var top = new Panel { Dock=DockStyle.Top, Height=44, BackColor=Color.FromArgb(240,244,255) };
            var title = new Label { Text="📖 Thư viện", Left=10, Top=10, Width=120, Font=new Font("Segoe UI",10.5f,FontStyle.Bold) };
            txbSearch.Left=140; txbSearch.Top=10; txbSearch.Width=200; txbSearch.Font=font10; txbSearch.PlaceholderText="Tìm sách...";
            txbSearch.TextChanged += new System.EventHandler(this.txbSearch_TextChanged);
            lblBookCount.Left=355; lblBookCount.Top=13; lblBookCount.Width=150; lblBookCount.Font=new Font("Segoe UI",9f,FontStyle.Italic); lblBookCount.ForeColor=Color.Gray;
            void Btn(Button b, string txt, Color bg, int left) {
                b.Left=left; b.Top=9; b.Width=90; b.Height=28; b.Text=txt;
                b.FlatStyle=FlatStyle.Flat; b.BackColor=bg; b.ForeColor=Color.White;
                b.Font=font9b; b.FlatAppearance.BorderSize=0;
            }
            Btn(btnBorrow,"📥 Mượn sách",Color.FromArgb(59,130,246),520);
            Btn(btnReturn,"📤 Trả sách",Color.FromArgb(34,197,94),620);
            btnBorrow.Click += new System.EventHandler(this.btnBorrow_Click);
            btnReturn.Click += new System.EventHandler(this.btnReturn_Click);
            top.Controls.AddRange(new Control[]{ title, txbSearch, lblBookCount, btnBorrow, btnReturn });

            var tab = new TabControl { Dock=DockStyle.Fill, Font=font9b };
            var t1 = new TabPage("📚 Danh sách sách");
            var t2 = new TabPage("📋 Sách đang mượn");

            dgvBooks.Dock=DockStyle.Fill; dgvBooks.AllowUserToAddRows=false; dgvBooks.BackgroundColor=Color.White;
            dgvBooks.BorderStyle=BorderStyle.None; dgvBooks.Font=font10; dgvBooks.RowTemplate.Height=28; dgvBooks.ReadOnly=true;
            dgvBooks.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            dgvBooks.CellClick += new DataGridViewCellEventHandler(this.dgvBooks_CellClick);

            pnlAdmin.Dock=DockStyle.Bottom; pnlAdmin.Height=50; pnlAdmin.BackColor=Color.FromArgb(245,247,252);
            var addRow = new Panel { Dock=DockStyle.Fill };
            int ax=8;
            void AF(TextBox t, string ph, int w) { t.Left=ax; t.Top=10; t.Width=w; t.Font=font10; t.PlaceholderText=ph; addRow.Controls.Add(t); ax+=w+6; }
            AF(txbBookTitle,"Tên sách *",160); AF(txbBookAuthor,"Tác giả *",120); AF(txbBookPub,"NXB",90); AF(txbBookCat,"Thể loại",90);
            var lblQ = new Label { Text="SL:", Left=ax, Top=13, Width=25, Font=font9b };
            nudQty.Left=ax+28; nudQty.Top=10; nudQty.Width=55; nudQty.Minimum=1; nudQty.Maximum=100; nudQty.Value=1;
            addRow.Controls.Add(lblQ); addRow.Controls.Add(nudQty); ax+=90;
            btnAddBook.Left=ax; btnAddBook.Top=8; btnAddBook.Width=90; btnAddBook.Height=30;
            btnAddBook.Text="➕ Thêm sách"; btnAddBook.FlatStyle=FlatStyle.Flat;
            btnAddBook.BackColor=Color.FromArgb(245,158,11); btnAddBook.ForeColor=Color.White;
            btnAddBook.Font=font9b; btnAddBook.FlatAppearance.BorderSize=0;
            btnAddBook.Click += new System.EventHandler(this.btnAddBook_Click);
            addRow.Controls.Add(btnAddBook);
            pnlAdmin.Controls.Add(addRow);

            t1.Controls.Add(dgvBooks); t1.Controls.Add(pnlAdmin);

            dgvBorrow.Dock=DockStyle.Fill; dgvBorrow.AllowUserToAddRows=false; dgvBorrow.BackgroundColor=Color.White;
            dgvBorrow.BorderStyle=BorderStyle.None; dgvBorrow.Font=font10; dgvBorrow.RowTemplate.Height=28; dgvBorrow.ReadOnly=true;
            dgvBorrow.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            dgvBorrow.CellClick += new DataGridViewCellEventHandler(this.dgvBorrow_CellClick);
            t2.Controls.Add(dgvBorrow);

            tab.Controls.Add(t1); tab.Controls.Add(t2);
            Controls.Add(tab); Controls.Add(top);
            Dock=DockStyle.Fill;
        }
    }

    // ── UC_Request Designer ───────────────────────────────────
    partial class UC_Request
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private DataGridView dgv;
        private RichTextBox rtbReason, rtbResult;
        private ComboBox cbClass, cbStatusFilter;
        private RadioButton rbApprove, rbReject;
        private Panel pnlSend, pnlProcess;
        private Button btnSend, btnProcess;

        private void InitializeComponent()
        {
            this.dgv            = new DataGridView();
            this.rtbReason      = new RichTextBox();
            this.rtbResult      = new RichTextBox();
            this.cbClass        = new ComboBox();
            this.cbStatusFilter = new ComboBox();
            this.rbApprove      = new RadioButton();
            this.rbReject       = new RadioButton();
            this.pnlSend        = new Panel();
            this.pnlProcess     = new Panel();
            this.btnSend        = new Button();
            this.btnProcess     = new Button();

            var font9b = new Font("Segoe UI", 9f, FontStyle.Bold);
            var font10 = new Font("Segoe UI", 10f);

            var top = new Panel { Dock=DockStyle.Top, Height=44, BackColor=Color.FromArgb(240,244,255) };
            var title = new Label { Text="📝 Phúc khảo điểm", Left=10, Top=10, Width=180, Font=new Font("Segoe UI",10.5f,FontStyle.Bold) };
            var lblF = new Label { Text="Trạng thái:", Left=200, Top=13, Width=80, Font=font9b };
            cbStatusFilter.Left=282; cbStatusFilter.Top=9; cbStatusFilter.Width=140; cbStatusFilter.DropDownStyle=ComboBoxStyle.DropDownList;
            cbStatusFilter.Items.AddRange(new object[]{"Tất cả","Chờ duyệt","Đã duyệt","Từ chối"});
            cbStatusFilter.SelectedIndex=0;
            cbStatusFilter.SelectedIndexChanged += new System.EventHandler(this.cbStatusFilter_SelectedIndexChanged);
            top.Controls.AddRange(new Control[]{ title, lblF, cbStatusFilter });

            var split = new SplitContainer { Dock=DockStyle.Fill, SplitterDistance=500 };
            dgv.Dock=DockStyle.Fill; dgv.AllowUserToAddRows=false; dgv.BackgroundColor=Color.White;
            dgv.BorderStyle=BorderStyle.None; dgv.Font=font10; dgv.RowTemplate.Height=28; dgv.ReadOnly=true;
            dgv.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            dgv.CellClick += new DataGridViewCellEventHandler(this.dgv_CellClick);
            split.Panel1.Controls.Add(dgv);

            // SV: form gửi yêu cầu
            pnlSend.Dock=DockStyle.Fill; pnlSend.Padding=new Padding(10);
            var sv1 = new Label { Text="Gửi yêu cầu phúc khảo", Dock=DockStyle.Top, Height=28, Font=new Font("Segoe UI",10f,FontStyle.Bold), ForeColor=Color.FromArgb(30,60,140) };
            var sv2 = new Label { Text="Lớp môn học:", Dock=DockStyle.Top, Height=22, Font=font9b };
            cbClass.Dock=DockStyle.Top; cbClass.Height=28; cbClass.Font=font10; cbClass.DropDownStyle=ComboBoxStyle.DropDownList;
            var sv3 = new Label { Text="Lý do phúc khảo:", Dock=DockStyle.Top, Height=22, Font=font9b };
            rtbReason.Dock=DockStyle.Fill; rtbReason.Font=font10;
            btnSend.Dock=DockStyle.Bottom; btnSend.Height=36; btnSend.Text="📤 Gửi yêu cầu phúc khảo";
            btnSend.FlatStyle=FlatStyle.Flat; btnSend.BackColor=Color.FromArgb(59,130,246);
            btnSend.ForeColor=Color.White; btnSend.Font=new Font("Segoe UI",9.5f,FontStyle.Bold); btnSend.FlatAppearance.BorderSize=0;
            btnSend.Click += new System.EventHandler(this.btnSend_Click);
            pnlSend.Controls.Add(rtbReason); pnlSend.Controls.Add(sv3); pnlSend.Controls.Add(cbClass); pnlSend.Controls.Add(sv2); pnlSend.Controls.Add(sv1); pnlSend.Controls.Add(btnSend);

            // Admin/GV: form duyệt
            pnlProcess.Dock=DockStyle.Fill; pnlProcess.Padding=new Padding(10);
            var pa1 = new Label { Text="Xử lý yêu cầu", Dock=DockStyle.Top, Height=28, Font=new Font("Segoe UI",10f,FontStyle.Bold), ForeColor=Color.FromArgb(30,60,140) };
            var rbPnl = new Panel { Dock=DockStyle.Top, Height=30 };
            rbApprove.Text="✅ Duyệt"; rbApprove.Left=0; rbApprove.Top=5; rbApprove.Width=90; rbApprove.Checked=true; rbApprove.Font=font9b;
            rbReject.Text="❌ Từ chối"; rbReject.Left=96; rbReject.Top=5; rbReject.Width=100; rbReject.Font=font9b;
            rbPnl.Controls.Add(rbApprove); rbPnl.Controls.Add(rbReject);
            var pa2 = new Label { Text="Kết quả / Ghi chú:", Dock=DockStyle.Top, Height=22, Font=font9b };
            rtbResult.Dock=DockStyle.Fill; rtbResult.Font=font10;
            btnProcess.Dock=DockStyle.Bottom; btnProcess.Height=36; btnProcess.Text="✔️ Xác nhận xử lý";
            btnProcess.FlatStyle=FlatStyle.Flat; btnProcess.BackColor=Color.FromArgb(34,197,94);
            btnProcess.ForeColor=Color.White; btnProcess.Font=new Font("Segoe UI",9.5f,FontStyle.Bold); btnProcess.FlatAppearance.BorderSize=0; btnProcess.Enabled=false;
            btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            pnlProcess.Controls.Add(rtbResult); pnlProcess.Controls.Add(pa2); pnlProcess.Controls.Add(rbPnl); pnlProcess.Controls.Add(pa1); pnlProcess.Controls.Add(btnProcess);

            split.Panel2.Controls.Add(pnlSend);
            split.Panel2.Controls.Add(pnlProcess);
            Controls.Add(split); Controls.Add(top);
            Dock=DockStyle.Fill;
        }
    }

    // ── UC_Dashboard Designer ─────────────────────────────────
    partial class UC_Dashboard
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private DataGridView dgvDept, dgvWeak, dgvAudit;
        private Label lblTotalSV, lblTotalClass, lblRecentNotif, lblPendingReq;
        private Button btnRefresh;

        private void InitializeComponent()
        {
            this.dgvDept      = new DataGridView();
            this.dgvWeak      = new DataGridView();
            this.dgvAudit     = new DataGridView();
            this.lblTotalSV   = new Label();
            this.lblTotalClass= new Label();
            this.lblRecentNotif=new Label();
            this.lblPendingReq= new Label();
            this.btnRefresh   = new Button();

            var font9b = new Font("Segoe UI", 9f, FontStyle.Bold);
            var font10 = new Font("Segoe UI", 10f);
            var fontBig= new Font("Segoe UI", 24f, FontStyle.Bold);

            // Top bar
            var top = new Panel { Dock=DockStyle.Top, Height=44, BackColor=Color.FromArgb(30,60,140) };
            var title = new Label { Text="📊 Dashboard Tổng quan", Left=12, Top=10, Width=280, Font=new Font("Segoe UI",11f,FontStyle.Bold), ForeColor=Color.White };
            btnRefresh.Left=310; btnRefresh.Top=9; btnRefresh.Width=90; btnRefresh.Height=28;
            btnRefresh.Text="🔄 Làm mới"; btnRefresh.FlatStyle=FlatStyle.Flat;
            btnRefresh.BackColor=Color.FromArgb(99,102,241); btnRefresh.ForeColor=Color.White;
            btnRefresh.Font=font9b; btnRefresh.FlatAppearance.BorderSize=0;
            btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            top.Controls.Add(title); top.Controls.Add(btnRefresh);

            // KPI cards
            var kpiPnl = new Panel { Dock=DockStyle.Top, Height=100, BackColor=Color.FromArgb(245,247,252), Padding=new Padding(10,8,10,8) };
            var kpiData = new[]{
                ("👥 Sinh viên đang học", lblTotalSV,   Color.FromArgb(59,130,246)),
                ("🏫 Lớp đang mở",        lblTotalClass, Color.FromArgb(34,197,94)),
                ("🔔 TB trong 7 ngày",     lblRecentNotif,Color.FromArgb(245,158,11)),
                ("📝 Chờ phúc khảo",       lblPendingReq, Color.FromArgb(239,68,68))
            };
            int kx=10;
            foreach(var (txt, lbl, clr) in kpiData)
            {
                var card = new Panel { Left=kx, Top=8, Width=160, Height=80, BackColor=Color.White };
                card.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0,0,160,80,10,10));
                var icon = new Label { Text=txt, Left=10, Top=8, Width=140, Font=new Font("Segoe UI",8.5f,FontStyle.Bold), ForeColor=Color.Gray };
                lbl.Left=10; lbl.Top=28; lbl.Width=140; lbl.Font=new Font("Segoe UI",22f,FontStyle.Bold); lbl.ForeColor=clr; lbl.Text="—";
                card.Controls.Add(icon); card.Controls.Add(lbl);
                var bar = new Panel { Dock=DockStyle.Bottom, Height=4, BackColor=clr };
                card.Controls.Add(bar);
                kpiPnl.Controls.Add(card);
                kx+=170;
            }

            // Bottom grid: 3 columns
            var mainSplit = new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=3, RowCount=1 };
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,40));
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,30));
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,30));

            void AddGrid(DataGridView dg, Panel p, string headerTxt) {
                var h = new Label { Text=headerTxt, Dock=DockStyle.Top, Height=28, Font=font9b, BackColor=Color.FromArgb(240,244,255), Padding=new Padding(8,6,0,0) };
                dg.Dock=DockStyle.Fill; dg.AllowUserToAddRows=false; dg.BackgroundColor=Color.White; dg.BorderStyle=BorderStyle.None; dg.Font=new Font("Segoe UI",9.5f); dg.ReadOnly=true; dg.RowTemplate.Height=26; dg.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
                p.Dock=DockStyle.Fill; p.Padding=new Padding(4);
                p.Controls.Add(dg); p.Controls.Add(h);
            }
            var p1=new Panel(); AddGrid(dgvDept, p1, "📊 GPA trung bình theo Khoa");
            var p2=new Panel(); AddGrid(dgvWeak,  p2, "⚠️ Top 5 môn điểm thấp nhất");
            var p3=new Panel(); AddGrid(dgvAudit, p3, "🕵️ Hoạt động gần đây");
            mainSplit.Controls.Add(p1,0,0); mainSplit.Controls.Add(p2,1,0); mainSplit.Controls.Add(p3,2,0);

            Controls.Add(mainSplit); Controls.Add(kpiPnl); Controls.Add(top);
            Dock=DockStyle.Fill;
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint="CreateRoundRectRgn")]
        private static extern System.IntPtr CreateRoundRectRgn(int nLeftRect,int nTopRect,int nRightRect,int nBottomRect,int nWidthEllipse,int nHeightEllipse);
    }
}
