// ============================================================
//  f_Main_Complete.cs  –  Form chính HOÀN CHỈNH
//  Thay thế f_Main.cs hiện có bằng file này
// ============================================================
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace QLSV
{
    public partial class f_Main : Form
    {
        private Panel     pnlSidebar;
        private Panel     pnlContent;
        private Panel     pnlHeader;
        private Label     lblNotifBadge;
        private Label     lblUserInfo;
        private Timer     timerNotif;

        public f_Main() { InitializeComponent(); }

        private void f_Main_Load(object sender, EventArgs e)
        {
            BuildUI();
            WireUpModules();
            ShowWelcome();
            StartNotifTimer();
        }

        // ═══════════════════════════════════════════════════════
        //  BUILD UI
        // ═══════════════════════════════════════════════════════
        private void BuildUI()
        {
            this.Text            = "QLSV – Quản lý Sinh viên";
            this.Size            = new Size(1366, 768);
            this.MinimumSize     = new Size(1100, 660);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = Color.FromArgb(245, 247, 252);
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── HEADER ──────────────────────────────────────────
            pnlHeader = new Panel {
                Dock=DockStyle.Top, Height=56,
                BackColor=Color.FromArgb(18, 42, 100)
            };
            var logoLbl = new Label {
                Text="🎓 QLSV", Left=18, Top=12, Width=110,
                Font=new Font("Segoe UI",15f,FontStyle.Bold),
                ForeColor=Color.White
            };
            var subLbl = new Label {
                Text="Hệ thống quản lý sinh viên", Left=130, Top=20, Width=260,
                Font=new Font("Segoe UI",9f), ForeColor=Color.FromArgb(160,180,220)
            };
            lblUserInfo = new Label {
                Left=420, Top=16, Width=400, Font=new Font("Segoe UI",9.5f,FontStyle.Bold),
                ForeColor=Color.FromArgb(180,210,255)
            };
            UpdateUserInfo();

            // Nút thông báo + badge
            var btnNotifHeader = new Button {
                Text="🔔", Left=850, Top=12, Width=40, Height=32,
                FlatStyle=FlatStyle.Flat, BackColor=Color.Transparent,
                ForeColor=Color.White, Font=new Font("Segoe UI",13f),
                Cursor=Cursors.Hand
            };
            btnNotifHeader.FlatAppearance.BorderSize=0;
            btnNotifHeader.Click += (s,e) => OpenModule("notification");
            lblNotifBadge = new Label {
                Left=878, Top=8, Width=24, Height=18,
                BackColor=Color.FromArgb(220,50,50), ForeColor=Color.White,
                Font=new Font("Segoe UI",7.5f,FontStyle.Bold),
                TextAlign=ContentAlignment.MiddleCenter, Visible=false
            };
            MakeRound(lblNotifBadge);

            var btnLogout = new Button {
                Text="🚪 Đăng xuất", Left=910, Top=12, Width=110, Height=32,
                FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(200,50,50),
                ForeColor=Color.White, Font=new Font("Segoe UI",9f,FontStyle.Bold),
                Cursor=Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize=0;
            btnLogout.Click += BtnLogout_Click;

            pnlHeader.Controls.AddRange(new Control[]{ logoLbl, subLbl, lblUserInfo, btnNotifHeader, lblNotifBadge, btnLogout });

            // ── SIDEBAR ─────────────────────────────────────────
            pnlSidebar = new Panel {
                Dock=DockStyle.Left, Width=210,
                BackColor=Color.FromArgb(22,52,120),
                Padding=new Padding(0,8,0,8)
            };

            // ── CONTENT ─────────────────────────────────────────
            pnlContent = new Panel {
                Dock=DockStyle.Fill, BackColor=Color.FromArgb(245,247,252),
                Padding=new Padding(12)
            };

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlHeader);

            BuildSidebar();
        }

        private void BuildSidebar()
        {
            pnlSidebar.Controls.Clear();
            int y = 8;

            void Section(string txt) {
                var l = new Label {
                    Text=txt.ToUpper(), Left=12, Top=y, Width=186,
                    Font=new Font("Segoe UI",7.5f,FontStyle.Bold),
                    ForeColor=Color.FromArgb(100,140,200)
                };
                pnlSidebar.Controls.Add(l);
                y += 22;
            }

            void SideBtn(string icon, string label, string moduleKey, bool visible=true) {
                if(!visible) return;
                var btn = new Button {
                    Text=$"  {icon}  {label}", Left=0, Top=y, Width=210, Height=38,
                    FlatStyle=FlatStyle.Flat, BackColor=Color.Transparent,
                    ForeColor=Color.FromArgb(200,215,245),
                    Font=new Font("Segoe UI",9.5f), TextAlign=ContentAlignment.MiddleLeft,
                    Cursor=Cursors.Hand, Tag=moduleKey, Padding=new Padding(8,0,0,0)
                };
                btn.FlatAppearance.BorderSize=0;
                btn.MouseEnter += (s,e) => { if(((Button)s).BackColor==Color.Transparent) ((Button)s).BackColor=Color.FromArgb(35,70,150); };
                btn.MouseLeave += (s,e) => { if(((Button)s).Tag?.ToString()!=_activeKey) ((Button)s).BackColor=Color.Transparent; };
                btn.Click += (s,e) => OpenModule(((Button)s).Tag.ToString());
                pnlSidebar.Controls.Add(btn);
                y += 38;
            }

            bool isA  = AppSession.IsAdmin;
            bool isGV = AppSession.IsGiaoVien;
            bool isSV = AppSession.IsSinhVien;

            // Dashboard
            if(isA) { Section("Tổng quan"); SideBtn("📊","Dashboard","dashboard"); y+=4; }

            // Học vụ
            Section("Học vụ");
            SideBtn("🏛️","Khoa & Hệ đào tạo","department", isA);
            SideBtn("📅","Quản lý học kỳ","semester", isA);
            SideBtn("📚","Quản lý môn học","course", isA||isGV);
            SideBtn("🏫","Lớp môn học","classroom", isA||isGV);
            SideBtn("📋","Đăng ký học phần","enrollment", isSV);
            y+=4;

            // Sinh viên & điểm
            Section("Sinh viên & Điểm");
            SideBtn("👥","Quản lý sinh viên","student", isA||isGV);
            SideBtn("🎯","Điểm số","score");
            SideBtn("⭐","Điểm rèn luyện","trainingscore");
            SideBtn("📝","Phúc khảo điểm","request");
            y+=4;

            // Lớp học
            if(isGV||isSV) { Section("Lớp học của tôi"); SideBtn("📰","Chi tiết lớp học","classdetail"); y+=4; }

            // Thông tin
            Section("Thông tin");
            SideBtn("🔔","Thông báo","notification");
            SideBtn("📖","Thư viện","library");
            SideBtn("👤","Thông tin cá nhân","account");
            y+=4;

            // Admin tools
            if(isA) {
                Section("Quản trị");
                SideBtn("👥","Tài khoản người dùng","users");
                SideBtn("🕵️","Nhật ký thao tác","auditlog");
                SideBtn("🔐","Lịch sử đăng nhập","loginhistory");
                SideBtn("💾","Backup & Restore","backup");
            }
        }

        private string _activeKey = "";
        private void HighlightSideBtn(string key)
        {
            _activeKey = key;
            foreach(Control c in pnlSidebar.Controls)
            {
                if(c is Button b)
                {
                    bool isActive = b.Tag?.ToString()==key;
                    b.BackColor = isActive ? Color.FromArgb(59,130,246) : Color.Transparent;
                    b.ForeColor = Color.FromArgb(200,215,245);
                    b.Font      = new Font("Segoe UI", isActive?9.5f:9.5f, isActive?FontStyle.Bold:FontStyle.Regular);
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        //  MODULE ROUTING
        // ═══════════════════════════════════════════════════════
        private void OpenModule(string key)
        {
            HighlightSideBtn(key);
            switch(key)
            {
                // Phase 1
                case "dashboard":    ShowUC(CreateDashboard()); break;
                case "department":   ShowUC(CreateDepartment()); break;
                case "semester":     ShowUC(CreateSemester()); break;
                case "course":       ShowUC(CreateCourse()); break;
                case "classroom":    ShowUC(CreateClassRoom()); break;

                // Phase 2
                case "enrollment":   ShowUC(CreateEnrollment()); break;
                case "classdetail":  ShowClassDetail(); break;

                // Phase 3
                case "notification": ShowUC(CreateNotification()); break;
                case "library":      ShowUC(CreateLibrary()); break;
                case "request":      ShowUC(CreateRequest()); break;
                case "trainingscore":ShowUC(CreateTrainingScore()); break;

                // Existing
                case "student":      ShowExistingUC("UC_Student"); break;
                case "score":        ShowExistingUC("UC_Score"); break;
                case "account":      ShowExistingUC("UC_Account"); break;
                case "users":        ShowExistingUC("UC_Account"); break;

                // Phase 4
                case "auditlog":     ShowAuditLog(); break;
                case "loginhistory": ShowLoginHistory(); break;
                case "backup":       ShowBackupPanel(); break;
            }
        }

        // ── Creators ────────────────────────────────────────────
        private UserControl CreateDashboard()    { var u=new UC_Dashboard();    u.LoadData(); return u; }
        private UserControl CreateDepartment()   { var u=new UC_Department();   u.LoadData(); return u; }
        private UserControl CreateSemester()     { var u=new UC_Semester();     u.LoadData(); return u; }
        private UserControl CreateCourse()       { var u=new UC_Course();       u.LoadData(); return u; }
        private UserControl CreateClassRoom()    { var u=new UC_ClassRoom();    u.LoadData(); return u; }
        private UserControl CreateEnrollment()   { var u=new UC_Enrollment();   u.LoadData(AppSession.CurrentMSSV); return u; }
        private UserControl CreateNotification() { var u=new UC_Notification(); u.LoadData(AppSession.CurrentAccountID, AppSession.CurrentRole); UpdateNotifBadge(); return u; }
        private UserControl CreateLibrary()      { var u=new UC_Library();      u.LoadData(AppSession.CurrentMSSV); return u; }
        private UserControl CreateRequest()      { var u=new UC_Request();      u.LoadData(AppSession.CurrentMSSV, AppSession.CurrentRole); return u; }
        private UserControl CreateTrainingScore(){ var u=new UC_TrainingScore(); u.LoadData(AppSession.CurrentMSSV, AppSession.CurrentRole); return u; }

        private void ShowClassDetail()
        {
            // Mở picker lớp rồi load UC_ClassDetail
            var pick = PickClassRoom();
            if(pick <= 0) return;
            var u = new UC_ClassDetail();
            u.LoadData(pick, AppSession.CurrentAccountID, AppSession.CurrentRole);
            ShowUC(u);
        }

        private int PickClassRoom()
        {
            int semID = AppSession.IsGiaoVien
                ? Semester.GetActiveSemesterID()
                : Semester.GetActiveSemesterID();
            var dt = AppSession.IsGiaoVien
                ? ClassRoom.GetForTeacher(AppSession.CurrentEmail, semID)
                : ClassRoom.GetForStudent(AppSession.CurrentMSSV, semID);

            if(dt.Rows.Count==0) { MessageBox.Show("Không có lớp nào trong học kỳ hiện tại!","⚠️"); return -1; }
            if(dt.Rows.Count==1) return Convert.ToInt32(dt.Rows[0]["ClassID"]);

            var frm  = new Form { Text="Chọn lớp", Size=new Size(480,400), StartPosition=FormStartPosition.CenterParent, FormBorderStyle=FormBorderStyle.FixedDialog };
            var dgv2 = new DataGridView { Dock=DockStyle.Fill, DataSource=dt, ReadOnly=true, AllowUserToAddRows=false, SelectionMode=DataGridViewSelectionMode.FullRowSelect };
            dgv2.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
            var btn = new Button { Dock=DockStyle.Bottom, Height=36, Text="✅ Mở lớp này", FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(59,130,246), ForeColor=Color.White, Font=new Font("Segoe UI",9.5f,FontStyle.Bold) };
            btn.FlatAppearance.BorderSize=0;
            int result=-1;
            btn.Click+=(s,e)=>{ if(dgv2.SelectedRows.Count>0){ result=Convert.ToInt32(dgv2.SelectedRows[0].Cells["ClassID"].Value); frm.Close(); } };
            frm.Controls.Add(dgv2); frm.Controls.Add(btn);
            frm.ShowDialog();
            return result;
        }

        private void ShowExistingUC(string ucName)
        {
            // Tìm UC đã tồn tại theo tên
            switch(ucName)
            {
                case "UC_Student": var s=new UC_Student(); s.LoadData(); ShowUC(s); break;
                case "UC_Score":   var sc=new UC_Score();  sc.LoadData(AppSession.CurrentMSSV, AppSession.CurrentRole); ShowUC(sc); break;
                case "UC_Account": var a=new UC_Account(); a.LoadData(AppSession.CurrentAccountID); ShowUC(a); break;
            }
        }

        private void ShowAuditLog()
        {
            var dt = AuditHelper.GetLogs();
            var u  = new UserControl { Dock=DockStyle.Fill };
            var top = new Panel { Dock=DockStyle.Top, Height=44, BackColor=Color.FromArgb(240,244,255) };
            var title = new Label { Text="🕵️ Nhật ký thao tác hệ thống", Left=12, Top=12, Width=280, Font=new Font("Segoe UI",10.5f,FontStyle.Bold) };
            top.Controls.Add(title);
            var dgv2 = new DataGridView { Dock=DockStyle.Fill, DataSource=dt, ReadOnly=true, AllowUserToAddRows=false, BackgroundColor=Color.White, BorderStyle=BorderStyle.None, Font=new Font("Segoe UI",9.5f), RowTemplate={Height=26}, SelectionMode=DataGridViewSelectionMode.FullRowSelect };
            dgv2.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
            u.Controls.Add(dgv2); u.Controls.Add(top);
            ShowUC(u);
        }

        private void ShowLoginHistory()
        {
            var dt = LoginHelper.GetLoginHistory();
            var u  = new UserControl { Dock=DockStyle.Fill };
            var top = new Panel { Dock=DockStyle.Top, Height=44, BackColor=Color.FromArgb(240,244,255) };
            var title = new Label { Text="🔐 Lịch sử đăng nhập", Left=12, Top=12, Width=250, Font=new Font("Segoe UI",10.5f,FontStyle.Bold) };
            top.Controls.Add(title);
            var dgv2 = new DataGridView { Dock=DockStyle.Fill, DataSource=dt, ReadOnly=true, AllowUserToAddRows=false, BackgroundColor=Color.White, BorderStyle=BorderStyle.None, Font=new Font("Segoe UI",9.5f), RowTemplate={Height=26}, SelectionMode=DataGridViewSelectionMode.FullRowSelect };
            dgv2.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
            u.Controls.Add(dgv2); u.Controls.Add(top);
            ShowUC(u);
        }

        private void ShowBackupPanel()
        {
            var u   = new UserControl { Dock=DockStyle.Fill, BackColor=Color.FromArgb(245,247,252) };
            var pnl = new Panel { Left=60, Top=60, Width=500, Height=340, BackColor=Color.White };
            pnl.Region=System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0,0,500,340,12,12));
            var t = new Label { Text="💾 Backup & Restore Database", Left=20, Top=20, Width=460, Font=new Font("Segoe UI",12f,FontStyle.Bold), ForeColor=Color.FromArgb(30,60,140) };
            var d1= new Label { Text="Backup tạo file .bak lưu toàn bộ dữ liệu. Restore khôi phục từ file .bak đã có.", Left=20, Top=52, Width=460, Font=new Font("Segoe UI",9f), ForeColor=Color.Gray };
            var sep=new Label { Left=20, Top=78, Width=460, Height=1, BackColor=Color.FromArgb(220,225,240) };
            void BigBtn(Button b, string txt, string sub, Color bg, int top2) {
                b.Left=20; b.Top=top2; b.Width=460; b.Height=60; b.FlatStyle=FlatStyle.Flat;
                b.BackColor=bg; b.ForeColor=Color.White; b.Font=new Font("Segoe UI",10.5f,FontStyle.Bold);
                b.FlatAppearance.BorderSize=0; b.Text=$"{txt}\n{sub}"; b.TextAlign=ContentAlignment.MiddleCenter;
                b.Cursor=Cursors.Hand; pnl.Controls.Add(b);
            }
            var btnB = new Button(); BigBtn(btnB,"💾 Backup ngay","Lưu file .bak vào thư mục bạn chọn",Color.FromArgb(59,130,246),90);
            btnB.Click+=(s,e)=>BackupHelper.Backup();
            var btnAuto = new Button(); BigBtn(btnAuto,"⏰ Bật Backup tự động","Backup hàng ngày vào 23:00",Color.FromArgb(34,197,94),162);
            bool autoOn=false;
            btnAuto.Click+=(s,e)=>{
                if(!autoOn){
                    using var fbd=new FolderBrowserDialog{Description="Chọn thư mục backup"};
                    if(fbd.ShowDialog()==DialogResult.OK){
                        BackupHelper.StartAutoBackup(fbd.SelectedPath,24);
                        autoOn=true; btnAuto.Text="⏰ Backup tự động: BẬT\nClick để tắt";
                        btnAuto.BackColor=Color.FromArgb(245,158,11);
                    }
                } else { BackupHelper.StopAutoBackup(); autoOn=false; btnAuto.Text="⏰ Bật Backup tự động\nBackup hàng ngày vào 23:00"; btnAuto.BackColor=Color.FromArgb(34,197,94); }
            };
            var btnR=new Button(); BigBtn(btnR,"🔄 Restore từ file .bak","⚠️ Sẽ xóa toàn bộ dữ liệu hiện tại!",Color.FromArgb(239,68,68),234);
            btnR.Click+=(s,e)=>BackupHelper.Restore();
            pnl.Controls.Add(t); pnl.Controls.Add(d1); pnl.Controls.Add(sep);
            u.Controls.Add(pnl);
            ShowUC(u);
        }

        // ═══════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════
        private void ShowUC(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock=DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }

        private void ShowWelcome()
        {
            var welcome = new UserControl { Dock=DockStyle.Fill, BackColor=Color.FromArgb(245,247,252) };
            var lbl = new Label {
                Text=$"👋 Chào mừng, {AppSession.CurrentFullName ?? AppSession.CurrentUsername}!",
                Font=new Font("Segoe UI",18f,FontStyle.Bold),
                ForeColor=Color.FromArgb(30,60,140),
                AutoSize=true
            };
            lbl.Left=40; lbl.Top=60;
            var sub = new Label {
                Text=$"Role: {AppSession.CurrentRole}  |  Học kỳ hiện tại: {GetActiveSemName()}  |  {DateTime.Now:dddd, dd/MM/yyyy}",
                Font=new Font("Segoe UI",10f), ForeColor=Color.Gray, AutoSize=true
            };
            sub.Left=40; sub.Top=100;

            // Quick actions
            var quickTitle = new Label { Text="Truy cập nhanh", Left=40, Top=150, Font=new Font("Segoe UI",11f,FontStyle.Bold), ForeColor=Color.FromArgb(60,80,120), AutoSize=true };
            welcome.Controls.Add(lbl); welcome.Controls.Add(sub); welcome.Controls.Add(quickTitle);

            var quickItems = AppSession.IsAdmin
                ? new[]{("📊","Dashboard","dashboard"),("🏫","Lớp môn học","classroom"),("👥","Sinh viên","student"),("🔔","Thông báo","notification")}
                : AppSession.IsGiaoVien
                ? new[]{("🏫","Lớp của tôi","classdetail"),("🎯","Điểm số","score"),("🔔","Thông báo","notification"),("📚","Môn học","course")}
                : new[]{("📋","Đăng ký HP","enrollment"),("🎯","Điểm của tôi","score"),("⭐","Rèn luyện","trainingscore"),("📖","Thư viện","library")};

            int qx=40;
            foreach(var (ico,lbl2,key) in quickItems)
            {
                var card = new Button {
                    Left=qx, Top=180, Width=130, Height=100,
                    Text=$"{ico}\n{lbl2}", FlatStyle=FlatStyle.Flat,
                    BackColor=Color.White, ForeColor=Color.FromArgb(30,60,140),
                    Font=new Font("Segoe UI",9.5f,FontStyle.Bold),
                    Cursor=Cursors.Hand, Tag=key
                };
                card.FlatAppearance.BorderColor=Color.FromArgb(200,210,235);
                card.FlatAppearance.BorderSize=1;
                card.Click+=(s,e)=>OpenModule(((Button)s).Tag.ToString());
                card.MouseEnter+=(s,e)=>((Button)s).BackColor=Color.FromArgb(235,240,255);
                card.MouseLeave+=(s,e)=>((Button)s).BackColor=Color.White;
                welcome.Controls.Add(card);
                qx+=145;
            }
            ShowUC(welcome);
        }

        private string GetActiveSemName()
        {
            My_DB db = new My_DB();
            try { db.openConnection(); return new SqlCommand("SELECT TOP 1 SemName FROM Semester WHERE IsActive=1",db.conn).ExecuteScalar()?.ToString()??"Chưa thiết lập"; }
            finally { db.closeConnection(); }
        }

        private void UpdateUserInfo()
        {
            if(lblUserInfo!=null)
                lblUserInfo.Text = $"👤 {AppSession.CurrentUsername}  |  {AppSession.CurrentRole}";
        }

        private void UpdateNotifBadge()
        {
            if(lblNotifBadge==null) return;
            try {
                int n = Notification.GetUnreadCount(AppSession.CurrentAccountID, AppSession.CurrentRole);
                lblNotifBadge.Visible = n>0;
                lblNotifBadge.Text    = n>99?"99+":n.ToString();
            } catch {}
        }

        private void StartNotifTimer()
        {
            timerNotif = new Timer { Interval=60000 };
            timerNotif.Tick+=(s,e)=>UpdateNotifBadge();
            timerNotif.Start();
            UpdateNotifBadge();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Đăng xuất?","Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes) return;
            timerNotif?.Stop();
            AppSession.Clear();
            this.Hide();
            var login = new f_Login();
            login.Show();
        }

        private void WireUpModules() { /* routing handled in OpenModule */ }
        private static void MakeRound(Label l) { l.Region=System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0,0,l.Width,l.Height,l.Height,l.Height)); }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint="CreateRoundRectRgn")]
        private static extern System.IntPtr CreateRoundRectRgn(int nLeftRect,int nTopRect,int nRightRect,int nBottomRect,int nWidthEllipse,int nHeightEllipse);

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions=new SizeF(7F,15F);
            this.AutoScaleMode=AutoScaleMode.Font;
            this.Load += new EventHandler(this.f_Main_Load);
            this.ResumeLayout(false);
        }
    }
}
