// ── UC_Enrollment.Designer.cs ────────────────────────────────
namespace QLSV
{
    partial class UC_Enrollment
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }
        private void InitializeComponent()
        {
            this.components     = new System.ComponentModel.Container();
            this.dgvMy          = new System.Windows.Forms.DataGridView();
            this.dgvAvail       = new System.Windows.Forms.DataGridView();
            this.pnlTimetable   = new System.Windows.Forms.Panel();
            this.txbSearch      = new System.Windows.Forms.TextBox();
            this.cbSem          = new System.Windows.Forms.ComboBox();
            this.lblMyCount     = new System.Windows.Forms.Label();
            this.lblCredits     = new System.Windows.Forms.Label();
            this.lblAvail       = new System.Windows.Forms.Label();
            this.btnRegister    = new System.Windows.Forms.Button();
            this.btnCancel      = new System.Windows.Forms.Button();
            this.btnRefresh     = new System.Windows.Forms.Button();
            this.toolTip        = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();

            var font9b = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            var font10 = new System.Drawing.Font("Segoe UI", 10f);

            // Top bar
            var top = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Top, Height=44, BackColor=System.Drawing.Color.FromArgb(240,244,255) };
            var title = new System.Windows.Forms.Label { Text="📋 Đăng ký Học phần", Left=10, Top=10, Width=210, Font=new System.Drawing.Font("Segoe UI",10.5f,System.Drawing.FontStyle.Bold) };
            var lblHK = new System.Windows.Forms.Label { Text="Học kỳ:", Left=225, Top=13, Width=60, Font=font9b };
            this.cbSem.Left=286; this.cbSem.Top=10; this.cbSem.Width=200; this.cbSem.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDownList; this.cbSem.Font=font10;
            this.cbSem.SelectedIndexChanged += new System.EventHandler(this.cbSem_SelectedIndexChanged);
            this.btnRefresh.Left=500; this.btnRefresh.Top=9; this.btnRefresh.Width=80; this.btnRefresh.Height=28;
            this.btnRefresh.Text="🔄 Làm mới"; this.btnRefresh.FlatStyle=System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.BackColor=System.Drawing.Color.FromArgb(99,102,241); this.btnRefresh.ForeColor=System.Drawing.Color.White;
            this.btnRefresh.Font=font9b; this.btnRefresh.FlatAppearance.BorderSize=0;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            top.Controls.AddRange(new System.Windows.Forms.Control[]{ title, lblHK, this.cbSem, this.btnRefresh });

            // TabControl
            var tab = new System.Windows.Forms.TabControl { Dock=System.Windows.Forms.DockStyle.Fill, Font=font9b };
            var t1 = new System.Windows.Forms.TabPage("📚 Lớp đã đăng ký");
            var t2 = new System.Windows.Forms.TabPage("➕ Đăng ký mới");
            var t3 = new System.Windows.Forms.TabPage("🗓 Thời khóa biểu");

            // Tab 1 – Lớp đã đăng ký
            var topT1 = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Top, Height=36 };
            this.lblMyCount.Left=8; this.lblMyCount.Top=10; this.lblMyCount.Width=150; this.lblMyCount.Font=font9b;
            this.lblCredits.Left=165; this.lblCredits.Top=10; this.lblCredits.Width=150; this.lblCredits.Font=font9b; this.lblCredits.ForeColor=System.Drawing.Color.FromArgb(59,130,246);
            this.btnCancel.Left=360; this.btnCancel.Top=5; this.btnCancel.Width=100; this.btnCancel.Height=28;
            this.btnCancel.Text="❌ Hủy đăng ký"; this.btnCancel.FlatStyle=System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.BackColor=System.Drawing.Color.FromArgb(239,68,68); this.btnCancel.ForeColor=System.Drawing.Color.White;
            this.btnCancel.Font=font9b; this.btnCancel.FlatAppearance.BorderSize=0;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            topT1.Controls.AddRange(new System.Windows.Forms.Control[]{ this.lblMyCount, this.lblCredits, this.btnCancel });
            this.dgvMy.Dock=System.Windows.Forms.DockStyle.Fill; this.dgvMy.AllowUserToAddRows=false;
            this.dgvMy.BackgroundColor=System.Drawing.Color.White; this.dgvMy.BorderStyle=System.Windows.Forms.BorderStyle.None; this.dgvMy.Font=font10; this.dgvMy.RowTemplate.Height=28;
            t1.Controls.Add(this.dgvMy); t1.Controls.Add(topT1);

            // Tab 2 – Đăng ký mới
            var topT2 = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Top, Height=36 };
            var lblSr = new System.Windows.Forms.Label { Text="🔍", Left=8, Top=10, Width=20 };
            this.txbSearch.Left=30; this.txbSearch.Top=7; this.txbSearch.Width=200; this.txbSearch.Font=font10;
            this.txbSearch.PlaceholderText="Tìm môn học, mã lớp...";
            this.txbSearch.TextChanged += new System.EventHandler(this.txbSearch_TextChanged);
            this.lblAvail.Left=240; this.lblAvail.Top=10; this.lblAvail.Width=160; this.lblAvail.Font=font9b; this.lblAvail.ForeColor=System.Drawing.Color.Gray;
            this.btnRegister.Left=415; this.btnRegister.Top=5; this.btnRegister.Width=100; this.btnRegister.Height=28;
            this.btnRegister.Text="✅ Đăng ký"; this.btnRegister.FlatStyle=System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.BackColor=System.Drawing.Color.FromArgb(34,197,94); this.btnRegister.ForeColor=System.Drawing.Color.White;
            this.btnRegister.Font=font9b; this.btnRegister.FlatAppearance.BorderSize=0;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            topT2.Controls.AddRange(new System.Windows.Forms.Control[]{ lblSr, this.txbSearch, this.lblAvail, this.btnRegister });
            this.dgvAvail.Dock=System.Windows.Forms.DockStyle.Fill; this.dgvAvail.AllowUserToAddRows=false;
            this.dgvAvail.BackgroundColor=System.Drawing.Color.White; this.dgvAvail.BorderStyle=System.Windows.Forms.BorderStyle.None; this.dgvAvail.Font=font10; this.dgvAvail.RowTemplate.Height=28;
            t2.Controls.Add(this.dgvAvail); t2.Controls.Add(topT2);

            // Tab 3 – TKB
            var note = new System.Windows.Forms.Label { Text="🟥 = Trùng giờ", Dock=System.Windows.Forms.DockStyle.Bottom, Height=22, TextAlign=System.Drawing.ContentAlignment.MiddleLeft, ForeColor=System.Drawing.Color.DarkRed, Font=font9b, BackColor=System.Drawing.Color.FromArgb(255,245,245) };
            this.pnlTimetable.Dock=System.Windows.Forms.DockStyle.Fill; this.pnlTimetable.BackColor=System.Drawing.Color.White; this.pnlTimetable.AutoScroll=true;
            t3.Controls.Add(this.pnlTimetable); t3.Controls.Add(note);

            tab.Controls.Add(t1); tab.Controls.Add(t2); tab.Controls.Add(t3);
            this.Controls.Add(tab); this.Controls.Add(top);
            this.Dock=System.Windows.Forms.DockStyle.Fill;
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgvMy, dgvAvail;
        private System.Windows.Forms.Panel pnlTimetable;
        private System.Windows.Forms.TextBox txbSearch;
        private System.Windows.Forms.ComboBox cbSem;
        private System.Windows.Forms.Label lblMyCount, lblCredits, lblAvail;
        private System.Windows.Forms.Button btnRegister, btnCancel, btnRefresh;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
