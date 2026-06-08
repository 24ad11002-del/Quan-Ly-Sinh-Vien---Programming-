namespace QLSV
{
    partial class UC_ClassRoom
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.dgv            = new System.Windows.Forms.DataGridView();
            this.txbSearch      = new System.Windows.Forms.TextBox();
            this.cbSemFilter    = new System.Windows.Forms.ComboBox();
            this.cbSemForm      = new System.Windows.Forms.ComboBox();
            this.cbCourse       = new System.Windows.Forms.ComboBox();
            this.cbStatus       = new System.Windows.Forms.ComboBox();
            this.txbClassCode   = new System.Windows.Forms.TextBox();
            this.txbTeacher     = new System.Windows.Forms.TextBox();
            this.txbTeacherEmail= new System.Windows.Forms.TextBox();
            this.txbRoom        = new System.Windows.Forms.TextBox();
            this.txbNote        = new System.Windows.Forms.TextBox();
            this.nudMaxSlot     = new System.Windows.Forms.NumericUpDown();
            this.nudStartPeriod = new System.Windows.Forms.NumericUpDown();
            this.nudNumPeriod   = new System.Windows.Forms.NumericUpDown();
            this.nudDay         = new System.Windows.Forms.NumericUpDown();
            this.lblTotal       = new System.Windows.Forms.Label();
            this.pnlForm        = new System.Windows.Forms.Panel();
            this.btnNew         = new System.Windows.Forms.Button();
            this.btnSave        = new System.Windows.Forms.Button();
            this.btnDelete      = new System.Windows.Forms.Button();
            this.btnDetail      = new System.Windows.Forms.Button();
            this.SuspendLayout();

            var font9b = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            var font10 = new System.Drawing.Font("Segoe UI", 10f);

            // Top bar
            var topPnl = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Top, Height = 44, BackColor = System.Drawing.Color.FromArgb(240,244,255), Padding = new System.Windows.Forms.Padding(8,6,8,6) };
            var lblTitle = new System.Windows.Forms.Label { Text = "🏫 Quản lý Lớp môn học", Left=8, Top=10, Width=220, Font=new System.Drawing.Font("Segoe UI",10.5f,System.Drawing.FontStyle.Bold) };
            var lblSr = new System.Windows.Forms.Label { Text="🔍", Left=240, Top=12, Width=20 };
            this.txbSearch.Left=262; this.txbSearch.Top=10; this.txbSearch.Width=180; this.txbSearch.Font=font10;
            this.txbSearch.PlaceholderText = "Tìm mã lớp, GV, môn học...";
            var lblSemF = new System.Windows.Forms.Label { Text="Học kỳ:", Left=455, Top=12, Width=55, Font=font9b };
            this.cbSemFilter.Left=512; this.cbSemFilter.Top=9; this.cbSemFilter.Width=160; this.cbSemFilter.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lblTotal.Left=685; this.lblTotal.Top=13; this.lblTotal.Width=150; this.lblTotal.Font=new System.Drawing.Font("Segoe UI",9f,System.Drawing.FontStyle.Italic); this.lblTotal.ForeColor=System.Drawing.Color.Gray;
            this.txbSearch.TextChanged += new System.EventHandler(this.txbSearch_TextChanged);
            this.cbSemFilter.SelectedIndexChanged += new System.EventHandler(this.cbSemFilter_SelectedIndexChanged);
            topPnl.Controls.AddRange(new System.Windows.Forms.Control[]{ lblTitle, lblSr, this.txbSearch, lblSemF, this.cbSemFilter, this.lblTotal });

            // dgv
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.AllowUserToAddRows = false;
            this.dgv.BackgroundColor = System.Drawing.Color.White;
            this.dgv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgv.Font = font10;
            this.dgv.RowTemplate.Height = 30;
            this.dgv.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellClick);

            // Form panel right
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlForm.Width = 300;
            this.pnlForm.BackColor = System.Drawing.Color.FromArgb(245,247,252);
            this.pnlForm.Padding = new System.Windows.Forms.Padding(10);
            this.pnlForm.AutoScroll = true;

            int y = 8;
            void F(string lbl, System.Windows.Forms.Control ctl, int h=28) {
                var l = new System.Windows.Forms.Label { Text=lbl, Left=10, Top=y, Width=275, Font=font9b };
                ctl.Left=10; ctl.Top=y+18; ctl.Width=275; ctl.Height=h;
                if(ctl is System.Windows.Forms.TextBox tb) tb.Font=font10;
                if(ctl is System.Windows.Forms.ComboBox cb) cb.Font=font10;
                this.pnlForm.Controls.Add(l); this.pnlForm.Controls.Add(ctl); y+=h+28;
            }
            F("Mã lớp *", this.txbClassCode);
            F("Học kỳ *", this.cbSemForm, 26); this.cbSemForm.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDownList;
            F("Môn học *", this.cbCourse, 26); this.cbCourse.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDownList;
            F("Giảng viên *", this.txbTeacher);
            F("Email GV", this.txbTeacherEmail);
            F("Phòng học", this.txbRoom);

            // Numeric row
            var numLbl = new System.Windows.Forms.Label { Text="Thứ  |  Tiết BĐ  |  Số tiết  |  Sĩ số TĐ", Left=10, Top=y, Width=275, Font=font9b };
            this.pnlForm.Controls.Add(numLbl); y+=18;
            this.nudDay.Left=10;  this.nudDay.Top=y; this.nudDay.Width=55; this.nudDay.Minimum=2; this.nudDay.Maximum=8; this.nudDay.Value=2;
            this.nudStartPeriod.Left=71; this.nudStartPeriod.Top=y; this.nudStartPeriod.Width=55; this.nudStartPeriod.Minimum=1; this.nudStartPeriod.Maximum=12; this.nudStartPeriod.Value=1;
            this.nudNumPeriod.Left=132; this.nudNumPeriod.Top=y; this.nudNumPeriod.Width=55; this.nudNumPeriod.Minimum=1; this.nudNumPeriod.Maximum=6; this.nudNumPeriod.Value=3;
            this.nudMaxSlot.Left=193; this.nudMaxSlot.Top=y; this.nudMaxSlot.Width=65; this.nudMaxSlot.Minimum=1; this.nudMaxSlot.Maximum=200; this.nudMaxSlot.Value=40;
            this.pnlForm.Controls.AddRange(new System.Windows.Forms.Control[]{ this.nudDay, this.nudStartPeriod, this.nudNumPeriod, this.nudMaxSlot }); y+=40;

            var cLbl = new System.Windows.Forms.Label { Text="Trạng thái", Left=10, Top=y, Width=275, Font=font9b };
            this.pnlForm.Controls.Add(cLbl); y+=18;
            this.cbStatus.Left=10; this.cbStatus.Top=y; this.cbStatus.Width=275; this.cbStatus.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbStatus.Items.AddRange(new object[]{"Đang mở","Đã đóng","Tạm ngưng"});
            this.cbStatus.SelectedIndex=0;
            this.pnlForm.Controls.Add(this.cbStatus); y+=40;

            var nLbl = new System.Windows.Forms.Label { Text="Ghi chú", Left=10, Top=y, Width=275, Font=font9b };
            this.pnlForm.Controls.Add(nLbl); y+=18;
            this.txbNote.Left=10; this.txbNote.Top=y; this.txbNote.Width=275; this.txbNote.Height=50; this.txbNote.Multiline=true;
            this.pnlForm.Controls.Add(this.txbNote); y+=66;

            void BtnF(System.Windows.Forms.Button b, string txt, System.Drawing.Color bg, int left, int w=65) {
                b.Text=txt; b.Left=left; b.Top=y; b.Width=w; b.Height=32;
                b.FlatStyle=System.Windows.Forms.FlatStyle.Flat; b.BackColor=bg;
                b.ForeColor=System.Drawing.Color.White; b.Font=font9b; b.FlatAppearance.BorderSize=0;
                this.pnlForm.Controls.Add(b);
            }
            BtnF(this.btnNew,    "➕ Mới",   System.Drawing.Color.FromArgb(59,130,246),  10, 60);
            BtnF(this.btnSave,   "💾 Lưu",   System.Drawing.Color.FromArgb(34,197,94),   76, 60);
            BtnF(this.btnDelete, "🗑 Xóa",   System.Drawing.Color.FromArgb(239,68,68),  142, 60);
            BtnF(this.btnDetail, "📋 Chi tiết",System.Drawing.Color.FromArgb(99,102,241),208, 80);

            this.btnNew.Click    += new System.EventHandler(this.btnNew_Click);
            this.btnSave.Click   += new System.EventHandler(this.btnSave_Click);
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            this.btnDetail.Click += new System.EventHandler(this.btnDetail_Click);
            this.btnDelete.Enabled = false;

            this.Controls.Add(this.dgv);
            this.Controls.Add(this.pnlForm);
            this.Controls.Add(topPnl);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.TextBox txbSearch, txbClassCode, txbTeacher, txbTeacherEmail, txbRoom, txbNote;
        private System.Windows.Forms.ComboBox cbSemFilter, cbSemForm, cbCourse, cbStatus;
        private System.Windows.Forms.NumericUpDown nudMaxSlot, nudStartPeriod, nudNumPeriod, nudDay;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Button btnNew, btnSave, btnDelete, btnDetail;
    }
}
