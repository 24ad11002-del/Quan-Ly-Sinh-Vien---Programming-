namespace QLSV
{
    partial class UC_Department
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControl      = new System.Windows.Forms.TabControl();
            this.tabDept         = new System.Windows.Forms.TabPage();
            this.tabSys          = new System.Windows.Forms.TabPage();

            // ── Tab Khoa ──
            this.txbDeptSearch   = new System.Windows.Forms.TextBox();
            this.lblDeptTotal    = new System.Windows.Forms.Label();
            this.dgvDept         = new System.Windows.Forms.DataGridView();
            this.pnlDeptForm     = new System.Windows.Forms.Panel();
            this.lblDeptCode     = new System.Windows.Forms.Label();
            this.txbDeptCode     = new System.Windows.Forms.TextBox();
            this.lblDeptName     = new System.Windows.Forms.Label();
            this.txbDeptName     = new System.Windows.Forms.TextBox();
            this.lblDeptNote     = new System.Windows.Forms.Label();
            this.txbDeptNote     = new System.Windows.Forms.TextBox();
            this.btnDeptNew      = new System.Windows.Forms.Button();
            this.btnDeptSave     = new System.Windows.Forms.Button();
            this.btnDeptDelete   = new System.Windows.Forms.Button();

            // ── Tab Hệ đào tạo ──
            this.dgvSys          = new System.Windows.Forms.DataGridView();
            this.lblSysCode      = new System.Windows.Forms.Label();
            this.txbSysCode      = new System.Windows.Forms.TextBox();
            this.lblSysName      = new System.Windows.Forms.Label();
            this.txbSysName      = new System.Windows.Forms.TextBox();
            this.btnSysNew       = new System.Windows.Forms.Button();
            this.btnSysSave      = new System.Windows.Forms.Button();
            this.btnSysDelete    = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // tabControl
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Controls.Add(this.tabDept);
            this.tabControl.Controls.Add(this.tabSys);
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);

            // ── tabDept ──
            this.tabDept.Text = "🏛️ Quản lý Khoa";
            this.tabDept.Padding = new System.Windows.Forms.Padding(8);

            var searchPnl = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Top, Height = 40 };
            var lblSearch = new System.Windows.Forms.Label { Text = "🔍 Tìm:", Width = 50, Top = 8, Left = 0, Font = new System.Drawing.Font("Segoe UI", 9.5f) };
            this.txbDeptSearch.Width = 250; this.txbDeptSearch.Top = 6; this.txbDeptSearch.Left = 55;
            this.txbDeptSearch.Font = new System.Drawing.Font("Segoe UI", 10f);
            this.txbDeptSearch.TextChanged += new System.EventHandler(this.txbDeptSearch_TextChanged);
            this.lblDeptTotal.Top = 12; this.lblDeptTotal.Left = 320; this.lblDeptTotal.Width = 150;
            this.lblDeptTotal.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic);
            this.lblDeptTotal.ForeColor = System.Drawing.Color.Gray;
            searchPnl.Controls.AddRange(new System.Windows.Forms.Control[] { lblSearch, this.txbDeptSearch, this.lblDeptTotal });

            this.dgvDept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDept.AllowUserToAddRows = false;
            this.dgvDept.BackgroundColor = System.Drawing.Color.White;
            this.dgvDept.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDept.RowHeadersWidth = 30;
            this.dgvDept.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDept_CellClick);

            // Form panel bên phải
            this.pnlDeptForm.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlDeptForm.Width = 260;
            this.pnlDeptForm.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDeptForm.BackColor = System.Drawing.Color.FromArgb(245, 247, 252);

            int y = 10;
            Action<System.Windows.Forms.Label, System.Windows.Forms.TextBox, string, bool> addField = (lbl, txb, text, ml) => {
                lbl.Text = text; lbl.Top = y; lbl.Left = 10; lbl.Width = 240;
                lbl.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
                txb.Top = y + 20; txb.Left = 10; txb.Width = 230;
                txb.Font = new System.Drawing.Font("Segoe UI", 10f);
                if (ml) { txb.Multiline = true; txb.Height = 60; }
                this.pnlDeptForm.Controls.Add(lbl); this.pnlDeptForm.Controls.Add(txb);
                y += ml ? 95 : 55;
            };
            addField(this.lblDeptCode, this.txbDeptCode, "Mã khoa *", false);
            addField(this.lblDeptName, this.txbDeptName, "Tên khoa *", false);
            addField(this.lblDeptNote, this.txbDeptNote, "Ghi chú", true);

            void StyleBtn(System.Windows.Forms.Button b, string txt, System.Drawing.Color bg) {
                b.Text = txt; b.Top = y; b.Width = 72; b.Height = 34;
                b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                b.BackColor = bg; b.ForeColor = System.Drawing.Color.White;
                b.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
                b.FlatAppearance.BorderSize = 0;
                this.pnlDeptForm.Controls.Add(b);
            }
            this.btnDeptNew.Left   = 10;  StyleBtn(this.btnDeptNew,    "➕ Mới",  System.Drawing.Color.FromArgb(59, 130, 246));
            this.btnDeptSave.Left  = 88;  StyleBtn(this.btnDeptSave,   "💾 Lưu",  System.Drawing.Color.FromArgb(34, 197, 94));
            this.btnDeptDelete.Left = 166; StyleBtn(this.btnDeptDelete, "🗑 Xóa",  System.Drawing.Color.FromArgb(239, 68, 68));
            this.btnDeptNew.Click    += new System.EventHandler(this.btnDeptNew_Click);
            this.btnDeptSave.Click   += new System.EventHandler(this.btnDeptSave_Click);
            this.btnDeptDelete.Click += new System.EventHandler(this.btnDeptDelete_Click);
            this.btnDeptDelete.Enabled = false;

            var splitDept = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Fill };
            splitDept.Controls.Add(this.dgvDept);
            splitDept.Controls.Add(this.pnlDeptForm);

            this.tabDept.Controls.Add(splitDept);
            this.tabDept.Controls.Add(searchPnl);

            // ── tabSys ──
            this.tabSys.Text = "🎓 Hệ đào tạo";
            this.tabSys.Padding = new System.Windows.Forms.Padding(8);

            this.dgvSys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSys.AllowUserToAddRows = false;
            this.dgvSys.BackgroundColor = System.Drawing.Color.White;
            this.dgvSys.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvSys.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSys_CellClick);

            var pnlSysForm = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Right, Width = 260, Padding = new System.Windows.Forms.Padding(10), BackColor = System.Drawing.Color.FromArgb(245,247,252) };
            int sy = 10;
            void AddSysField(System.Windows.Forms.Label l, System.Windows.Forms.TextBox t, string txt) {
                l.Text = txt; l.Top = sy; l.Left = 10; l.Width = 240; l.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
                t.Top = sy+20; t.Left = 10; t.Width = 230; t.Font = new System.Drawing.Font("Segoe UI", 10f);
                pnlSysForm.Controls.Add(l); pnlSysForm.Controls.Add(t); sy += 55;
            }
            AddSysField(this.lblSysCode, this.txbSysCode, "Mã hệ *");
            AddSysField(this.lblSysName, this.txbSysName, "Tên hệ đào tạo *");

            void StyleSysBtn(System.Windows.Forms.Button b, string txt, System.Drawing.Color bg, int left) {
                b.Text = txt; b.Top = sy; b.Left = left; b.Width = 72; b.Height = 34;
                b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                b.BackColor = bg; b.ForeColor = System.Drawing.Color.White;
                b.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
                b.FlatAppearance.BorderSize = 0;
                pnlSysForm.Controls.Add(b);
            }
            StyleSysBtn(this.btnSysNew,    "➕ Mới", System.Drawing.Color.FromArgb(59,130,246), 10);
            StyleSysBtn(this.btnSysSave,   "💾 Lưu", System.Drawing.Color.FromArgb(34,197,94),  88);
            StyleSysBtn(this.btnSysDelete, "🗑 Xóa", System.Drawing.Color.FromArgb(239,68,68),  166);
            this.btnSysNew.Click    += new System.EventHandler(this.btnSysNew_Click);
            this.btnSysSave.Click   += new System.EventHandler(this.btnSysSave_Click);
            this.btnSysDelete.Click += new System.EventHandler(this.btnSysDelete_Click);
            this.btnSysDelete.Enabled = false;

            var splitSys = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Fill };
            splitSys.Controls.Add(this.dgvSys);
            splitSys.Controls.Add(pnlSysForm);
            this.tabSys.Controls.Add(splitSys);

            this.Controls.Add(this.tabControl);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDept, tabSys;
        private System.Windows.Forms.TextBox txbDeptSearch, txbDeptCode, txbDeptName, txbDeptNote;
        private System.Windows.Forms.TextBox txbSysCode, txbSysName;
        private System.Windows.Forms.Label lblDeptTotal, lblDeptCode, lblDeptName, lblDeptNote;
        private System.Windows.Forms.Label lblSysCode, lblSysName;
        private System.Windows.Forms.DataGridView dgvDept, dgvSys;
        private System.Windows.Forms.Panel pnlDeptForm;
        private System.Windows.Forms.Button btnDeptNew, btnDeptSave, btnDeptDelete;
        private System.Windows.Forms.Button btnSysNew, btnSysSave, btnSysDelete;
    }
}
