// ============================================================
//  AppSession_Full.cs  –  Thay thế AppSession.cs hiện có
// ============================================================
namespace QLSV
{
    public static class AppSession
    {
        // ── Core ──────────────────────────────────────────────
        public static int    CurrentAccountID   { get; set; }
        public static string CurrentUsername     { get; set; }
        public static string CurrentRole         { get; set; }
        public static string CurrentEmail        { get; set; }
        public static string CurrentFullName     { get; set; }
        public static int    CurrentMSSV         { get; set; }
        public static int    CurrentSemesterID   { get; set; } = -1;

        // ── Permission shortcuts ──────────────────────────────
        public static bool IsAdmin     => CurrentRole == "Admin";
        public static bool IsGiaoVien  => CurrentRole == "GiaoVien";
        public static bool IsSinhVien  => CurrentRole == "SinhVien";

        public static bool CanEdit     => IsAdmin || IsGiaoVien;
        public static bool CanViewAll  => IsAdmin;

        // ── Clear on logout ───────────────────────────────────
        public static void Clear()
        {
            CurrentAccountID  = 0;
            CurrentUsername   = null;
            CurrentRole       = null;
            CurrentEmail      = null;
            CurrentFullName   = null;
            CurrentMSSV       = 0;
            CurrentSemesterID = -1;
        }

        // ── Set from login ────────────────────────────────────
        public static void SetFromLogin(int id, string username, string role,
                                        string email, string fullName, int mssv)
        {
            CurrentAccountID  = id;
            CurrentUsername   = username;
            CurrentRole       = role;
            CurrentEmail      = email;
            CurrentFullName   = fullName;
            CurrentMSSV       = mssv;
            CurrentSemesterID = Semester.GetActiveSemesterID();
        }
    }
}

// ============================================================
//  f_Login_Full.cs  –  Thay thế f_Login.cs hiện có
//  Tích hợp: LoginLog, IsFirstLogin, AppSession.SetFromLogin
// ============================================================
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QLSV
{
    public partial class f_Login : Form
    {
        public f_Login() { InitializeComponent(); }

        private void f_Login_Load(object sender, EventArgs e)
        {
            txbUsername.Focus();
            CheckAndLoadRemember();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txbUsername.Text.Trim();
            string password = txbPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            { ShowError("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!"); return; }

            SetLoading(true);
            try { DoLogin(username, password); }
            finally { SetLoading(false); }
        }

        private void DoLogin(string username, string password)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT a.ID, a.Username, a.Role, a.Email, a.MSSV, a.IsFirstLogin, " +
                    "ISNULL(s.Fname+' '+s.Lname, a.Username) AS FullName " +
                    "FROM Account a " +
                    "LEFT JOIN Student s ON a.MSSV=s.MSSV " +
                    "WHERE a.Username=@u AND a.Pass=HASHBYTES('SHA2_256',@p)", db.conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                var dr = cmd.ExecuteReader();
                if (!dr.Read())
                {
                    // Thử tìm account để log thất bại
                    dr.Close();
                    var findCmd = new SqlCommand("SELECT ID FROM Account WHERE Username=@u", db.conn);
                    findCmd.Parameters.AddWithValue("@u", username);
                    object idObj = findCmd.ExecuteScalar();
                    if (idObj != null) LoginHelper.LogLogin((int)idObj, false, "Sai mật khẩu");

                    ShowError("Tên đăng nhập hoặc mật khẩu không đúng!");
                    txbPassword.Clear(); txbPassword.Focus();
                    return;
                }

                int    id          = (int)dr["ID"];
                string role        = dr["Role"].ToString();
                string email       = dr["Email"].ToString();
                int    mssv        = dr["MSSV"]==DBNull.Value ? 0 : Convert.ToInt32(dr["MSSV"]);
                bool   firstLogin  = Convert.ToBoolean(dr["IsFirstLogin"]);
                string fullName    = dr["FullName"].ToString();
                dr.Close();

                // Ghi log thành công
                LoginHelper.LogLogin(id, true);

                // Lưu remember
                if (chkRemember.Checked)
                {
                    Properties.Settings.Default.RememberedUser = username;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.RememberedUser = "";
                    Properties.Settings.Default.Save();
                }

                // Set session
                AppSession.SetFromLogin(id, username, role, email, fullName, mssv);

                db.closeConnection();

                // Lần đầu đăng nhập → bắt đổi mật khẩu
                if (firstLogin)
                {
                    var changePw = new f_ChangePasswordFirst(id);
                    changePw.ShowDialog();
                    return; // ShowDialog sẽ restart app
                }

                // Mở f_Main
                this.Hide();
                var main = new f_Main();
                main.FormClosed += (s, ev) => this.Close();
                main.Show();
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi kết nối: {ex.Message}");
            }
            finally { try { db.closeConnection(); } catch { } }
        }

        private void btnForgotPw_Click(object sender, EventArgs e)
        {
            // Gọi form forgot password hiện có
            new f_ForgotPassword().ShowDialog();
        }

        private void txbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btnLogin_Click(null, null);
        }

        private void txbUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) txbPassword.Focus();
        }

        private void ShowError(string msg)
        {
            lblError.Text    = msg;
            lblError.Visible = true;
            lblError.ForeColor = Color.FromArgb(220, 50, 50);
        }

        private void SetLoading(bool loading)
        {
            btnLogin.Enabled = !loading;
            btnLogin.Text    = loading ? "Đang đăng nhập..." : "🔑 Đăng nhập";
        }

        private void CheckAndLoadRemember()
        {
            try
            {
                string saved = Properties.Settings.Default.RememberedUser;
                if (!string.IsNullOrEmpty(saved))
                {
                    txbUsername.Text = saved;
                    chkRemember.Checked = true;
                    txbPassword.Focus();
                }
            }
            catch { }
        }

        private void InitializeComponent()
        {
            this.txbUsername = new System.Windows.Forms.TextBox();
            this.txbPassword = new System.Windows.Forms.TextBox();
            this.btnLogin    = new System.Windows.Forms.Button();
            this.btnForgotPw = new System.Windows.Forms.Button();
            this.lblError    = new System.Windows.Forms.Label();
            this.chkRemember = new System.Windows.Forms.CheckBox();

            var font9b = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            var font10 = new System.Drawing.Font("Segoe UI", 10f);
            var font12 = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);

            this.Text            = "QLSV – Đăng nhập";
            this.Size            = new System.Drawing.Size(460, 520);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = System.Drawing.Color.FromArgb(245, 247, 252);

            // Header
            var hdr = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Top, Height=120, BackColor=System.Drawing.Color.FromArgb(18,42,100) };
            var icon= new System.Windows.Forms.Label { Text="🎓", Left=170, Top=20, Width=60, Font=new System.Drawing.Font("Segoe UI",28f), ForeColor=System.Drawing.Color.White, TextAlign=System.Drawing.ContentAlignment.MiddleCenter };
            var ht  = new System.Windows.Forms.Label { Text="QLSV", Left=140, Top=70, Width=120, Font=new System.Drawing.Font("Segoe UI",16f,System.Drawing.FontStyle.Bold), ForeColor=System.Drawing.Color.White, TextAlign=System.Drawing.ContentAlignment.MiddleCenter };
            var hs  = new System.Windows.Forms.Label { Text="Quản lý Sinh viên", Left=100, Top=96, Width=200, Font=new System.Drawing.Font("Segoe UI",9f), ForeColor=System.Drawing.Color.FromArgb(160,185,230), TextAlign=System.Drawing.ContentAlignment.MiddleCenter };
            hdr.Controls.AddRange(new System.Windows.Forms.Control[]{ icon, ht, hs });

            // Form card
            var card = new System.Windows.Forms.Panel { Left=40, Top=140, Width=370, Height=300, BackColor=System.Drawing.Color.White };
            card.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0,0,370,300,12,12));

            int cy=24;
            void Field(string lbl, System.Windows.Forms.TextBox t, bool pw=false) {
                var l=new System.Windows.Forms.Label{Text=lbl,Left=24,Top=cy,Width=320,Font=font9b,ForeColor=System.Drawing.Color.FromArgb(60,80,120)};
                t.Left=24; t.Top=cy+20; t.Width=322; t.Height=34; t.Font=font10;
                t.BorderStyle=System.Windows.Forms.BorderStyle.FixedSingle;
                if(pw){t.PasswordChar='●';}
                card.Controls.Add(l); card.Controls.Add(t); cy+=68;
            }
            Field("Tên đăng nhập", txbUsername);
            Field("Mật khẩu", txbPassword, true);

            chkRemember.Text="Ghi nhớ đăng nhập"; chkRemember.Left=24; chkRemember.Top=cy; chkRemember.Width=200; chkRemember.Font=font9b; chkRemember.ForeColor=System.Drawing.Color.FromArgb(60,80,120); cy+=32;

            lblError.Left=24; lblError.Top=cy; lblError.Width=320; lblError.Height=36;
            lblError.Font=new System.Drawing.Font("Segoe UI",9f); lblError.Visible=false;
            card.Controls.Add(chkRemember); card.Controls.Add(lblError);

            btnLogin.Left=24; btnLogin.Top=232; btnLogin.Width=322; btnLogin.Height=42;
            btnLogin.Text="🔑 Đăng nhập"; btnLogin.FlatStyle=System.Windows.Forms.FlatStyle.Flat;
            btnLogin.BackColor=System.Drawing.Color.FromArgb(59,130,246); btnLogin.ForeColor=System.Drawing.Color.White;
            btnLogin.Font=new System.Drawing.Font("Segoe UI",10.5f,System.Drawing.FontStyle.Bold); btnLogin.FlatAppearance.BorderSize=0;
            btnLogin.Click+=new System.EventHandler(this.btnLogin_Click);
            card.Controls.Add(btnLogin);

            btnForgotPw.Left=130; btnForgotPw.Top=460; btnForgotPw.Width=160; btnForgotPw.Height=28;
            btnForgotPw.Text="Quên mật khẩu?"; btnForgotPw.FlatStyle=System.Windows.Forms.FlatStyle.Flat;
            btnForgotPw.BackColor=System.Drawing.Color.Transparent; btnForgotPw.ForeColor=System.Drawing.Color.FromArgb(59,130,246);
            btnForgotPw.Font=font9b; btnForgotPw.FlatAppearance.BorderSize=0; btnForgotPw.Cursor=System.Windows.Forms.Cursors.Hand;
            btnForgotPw.Click+=new System.EventHandler(this.btnForgotPw_Click);

            txbPassword.KeyDown+=new System.Windows.Forms.KeyEventHandler(this.txbPassword_KeyDown);
            txbUsername.KeyDown+=new System.Windows.Forms.KeyEventHandler(this.txbUsername_KeyDown);

            this.Controls.Add(card); this.Controls.Add(hdr); this.Controls.Add(btnForgotPw);
            this.Load+=new System.EventHandler(this.f_Login_Load);
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint="CreateRoundRectRgn")]
        private static extern System.IntPtr CreateRoundRectRgn(int nL,int nT,int nR,int nB,int nW,int nH);

        private System.Windows.Forms.TextBox txbUsername, txbPassword;
        private System.Windows.Forms.Button btnLogin, btnForgotPw;
        private System.Windows.Forms.Label  lblError;
        private System.Windows.Forms.CheckBox chkRemember;
    }
}
