// ── UC_Semester.Designer.cs ──────────────────────────────────
namespace QLSV
{
    partial class UC_Semester
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.dgv            = new System.Windows.Forms.DataGridView();
            this.lblTotal       = new System.Windows.Forms.Label();
            this.txbCode        = new System.Windows.Forms.TextBox();
            this.txbName        = new System.Windows.Forms.TextBox();
            this.txbYear        = new System.Windows.Forms.TextBox();
            this.dtpStart       = new System.Windows.Forms.DateTimePicker();
            this.dtpEnd         = new System.Windows.Forms.DateTimePicker();
            this.chkRegOpen     = new System.Windows.Forms.CheckBox();
            this.chkActive      = new System.Windows.Forms.CheckBox();
            this.btnNew         = new System.Windows.Forms.Button();
            this.btnSave        = new System.Windows.Forms.Button();
            this.btnDelete      = new System.Windows.Forms.Button();
            this.btnToggleReg   = new System.Windows.Forms.Button();
            this.btnSetActive   = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // dgv
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.AllowUserToAddRows = false;
            this.dgv.BackgroundColor = System.Drawing.Color.White;
            this.dgv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgv.Font = new System.Drawing.Font("Segoe UI", 9.5f);
            this.dgv.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellClick);

            // Form panel
            var pnl = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Right, Width = 280, BackColor = System.Drawing.Color.FromArgb(245,247,252), Padding = new System.Windows.Forms.Padding(12) };
            var font9b = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            var font10 = new System.Drawing.Font("Segoe UI", 10f);

            int y = 10;
            void Row(string lbl, System.Windows.Forms.Control ctl, int h = 28) {
                var l = new System.Windows.Forms.Label { Text = lbl, Left = 12, Top = y, Width = 255, Font = font9b };
                ctl.Left = 12; ctl.Top = y + 20; ctl.Width = 250; ctl.Height = h;
                if (ctl is System.Windows.Forms.TextBox t) t.Font = font10;
                pnl.Controls.Add(l); pnl.Controls.Add(ctl); y += h + 30;
            }

            Row("Mã học kỳ *", this.txbCode);
            Row("Tên học kỳ *", this.txbName);
            Row("Năm học", this.txbYear);
            Row("Ngày bắt đầu", this.dtpStart);
            Row("Ngày kết thúc", this.dtpEnd);

            this.chkRegOpen.Text = "Mở đăng ký học phần";
            this.chkRegOpen.Left = 12; this.chkRegOpen.Top = y; this.chkRegOpen.Width = 200;
            this.chkRegOpen.Font = font9b; y += 28;
            this.chkActive.Text  = "Học kỳ hiện tại";
            this.chkActive.Left  = 12; this.chkActive.Top = y; this.chkActive.Width = 200;
            this.chkActive.Font  = font9b; y += 40;
            pnl.Controls.Add(this.chkRegOpen);
            pnl.Controls.Add(this.chkActive);

            void Btn(System.Windows.Forms.Button b, string txt, System.Drawing.Color bg, int left, int width = 118) {
                b.Text = txt; b.Left = left; b.Top = y; b.Width = width; b.Height = 34;
                b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                b.BackColor = bg; b.ForeColor = System.Drawing.Color.White;
                b.Font = font9b; b.FlatAppearance.BorderSize = 0;
                pnl.Controls.Add(b);
            }
            Btn(this.btnNew,  "➕ Mới",  System.Drawing.Color.FromArgb(59,130,246),   12, 56);
            Btn(this.btnSave, "💾 Lưu",  System.Drawing.Color.FromArgb(34,197,94),    74, 56);
            Btn(this.btnDelete,"🗑 Xóa", System.Drawing.Color.FromArgb(239,68,68),   136, 56); y += 44;
            Btn(this.btnToggleReg, "🔓 Mở/Đóng ĐK", System.Drawing.Color.FromArgb(245,158,11), 12, 118); 
            Btn(this.btnSetActive, "⭐ Đặt hiện tại", System.Drawing.Color.FromArgb(99,102,241), 136, 118);

            this.btnNew.Click        += new System.EventHandler(this.btnNew_Click);
            this.btnSave.Click       += new System.EventHandler(this.btnSave_Click);
            this.btnDelete.Click     += new System.EventHandler(this.btnDelete_Click);
            this.btnToggleReg.Click  += new System.EventHandler(this.btnToggleReg_Click);
            this.btnSetActive.Click  += new System.EventHandler(this.btnSetActive_Click);
            this.btnDelete.Enabled    = false;
            this.btnToggleReg.Enabled = false;
            this.btnSetActive.Enabled = false;

            // lblTotal top bar
            var topPnl = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Top, Height = 36, BackColor = System.Drawing.Color.FromArgb(240,244,255) };
            var title = new System.Windows.Forms.Label { Text = "📅 Quản lý Học kỳ", Left = 12, Top = 8, Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold), Width = 200 };
            this.lblTotal.Left = 220; this.lblTotal.Top = 10; this.lblTotal.Width = 200;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic);
            this.lblTotal.ForeColor = System.Drawing.Color.Gray;
            topPnl.Controls.Add(title); topPnl.Controls.Add(this.lblTotal);

            this.Controls.Add(this.dgv);
            this.Controls.Add(pnl);
            this.Controls.Add(topPnl);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.TextBox txbCode, txbName, txbYear;
        private System.Windows.Forms.DateTimePicker dtpStart, dtpEnd;
        private System.Windows.Forms.CheckBox chkRegOpen, chkActive;
        private System.Windows.Forms.Button btnNew, btnSave, btnDelete, btnToggleReg, btnSetActive;
    }
}
