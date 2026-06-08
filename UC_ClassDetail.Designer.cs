namespace QLSV
{
    partial class UC_ClassDetail
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }
        private void InitializeComponent()
        {
            this.lstPosts       = new System.Windows.Forms.ListView();
            this.rtbPostContent = new System.Windows.Forms.RichTextBox();
            this.picPost        = new System.Windows.Forms.PictureBox();
            this.lstComments    = new System.Windows.Forms.ListView();
            this.txbComment     = new System.Windows.Forms.TextBox();
            this.txbPostTitle   = new System.Windows.Forms.TextBox();
            this.rtbNewPost     = new System.Windows.Forms.RichTextBox();
            this.chkPin         = new System.Windows.Forms.CheckBox();
            this.picNewPost     = new System.Windows.Forms.PictureBox();
            this.dgvSchedule    = new System.Windows.Forms.DataGridView();
            this.dtpMakeup      = new System.Windows.Forms.DateTimePicker();
            this.rbNghi         = new System.Windows.Forms.RadioButton();
            this.rbBu           = new System.Windows.Forms.RadioButton();
            this.txbMakeupNote  = new System.Windows.Forms.TextBox();
            this.tvDocs         = new System.Windows.Forms.TreeView();
            this.dgvComp        = new System.Windows.Forms.DataGridView();
            this.dgvScore       = new System.Windows.Forms.DataGridView();
            this.txbCompName    = new System.Windows.Forms.TextBox();
            this.nudWeight      = new System.Windows.Forms.NumericUpDown();
            this.lblClassName   = new System.Windows.Forms.Label();
            this.lblTeacher     = new System.Windows.Forms.Label();
            this.lblRoom        = new System.Windows.Forms.Label();
            this.lblSemester    = new System.Windows.Forms.Label();
            this.lblSlot        = new System.Windows.Forms.Label();
            this.lblStatus      = new System.Windows.Forms.Label();
            this.btnAddPost     = new System.Windows.Forms.Button();
            this.btnDeletePost  = new System.Windows.Forms.Button();
            this.btnAddComment  = new System.Windows.Forms.Button();
            this.btnPickPostImage = new System.Windows.Forms.Button();
            this.btnAddMakeup   = new System.Windows.Forms.Button();
            this.btnDeleteMakeup= new System.Windows.Forms.Button();
            this.btnAddFolder   = new System.Windows.Forms.Button();
            this.btnUploadDoc   = new System.Windows.Forms.Button();
            this.btnDownload    = new System.Windows.Forms.Button();
            this.btnDeleteDoc   = new System.Windows.Forms.Button();
            this.btnAddComp     = new System.Windows.Forms.Button();
            this.btnSaveScore   = new System.Windows.Forms.Button();
            this.pnlScoreEdit   = new System.Windows.Forms.Panel();
            this.SuspendLayout();

            var font9b = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            var font10 = new System.Drawing.Font("Segoe UI", 10f);

            // Header info
            var hdr = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Top, Height=70, BackColor=System.Drawing.Color.FromArgb(30,60,140), Padding=new System.Windows.Forms.Padding(12,8,12,8) };
            this.lblClassName.Left=12; this.lblClassName.Top=6; this.lblClassName.Width=700; this.lblClassName.Font=new System.Drawing.Font("Segoe UI",12f,System.Drawing.FontStyle.Bold); this.lblClassName.ForeColor=System.Drawing.Color.White;
            this.lblTeacher.Left=12; this.lblTeacher.Top=30; this.lblTeacher.Width=350; this.lblTeacher.Font=font9b; this.lblTeacher.ForeColor=System.Drawing.Color.FromArgb(180,210,255);
            this.lblRoom.Left=12; this.lblRoom.Top=50; this.lblRoom.Width=400; this.lblRoom.Font=font9b; this.lblRoom.ForeColor=System.Drawing.Color.FromArgb(180,210,255);
            this.lblSemester.Left=400; this.lblSemester.Top=30; this.lblSemester.Width=200; this.lblSemester.Font=font9b; this.lblSemester.ForeColor=System.Drawing.Color.FromArgb(180,210,255);
            this.lblSlot.Left=400; this.lblSlot.Top=50; this.lblSlot.Width=150; this.lblSlot.Font=font9b; this.lblSlot.ForeColor=System.Drawing.Color.FromArgb(200,255,200);
            this.lblStatus.Left=560; this.lblStatus.Top=50; this.lblStatus.Width=150; this.lblStatus.Font=font9b; this.lblStatus.ForeColor=System.Drawing.Color.Yellow;
            hdr.Controls.AddRange(new System.Windows.Forms.Control[]{ this.lblClassName, this.lblTeacher, this.lblRoom, this.lblSemester, this.lblSlot, this.lblStatus });

            // TabControl
            var tab = new System.Windows.Forms.TabControl { Dock=System.Windows.Forms.DockStyle.Fill, Font=font9b };
            var tBoard    = new System.Windows.Forms.TabPage("📰 Bảng tin");
            var tSchedule = new System.Windows.Forms.TabPage("🗓 Lịch học");
            var tDocs     = new System.Windows.Forms.TabPage("📁 Tài liệu");
            var tScore    = new System.Windows.Forms.TabPage("🎯 Điểm số");

            // ── BẢNG TIN ──
            var split1 = new System.Windows.Forms.SplitContainer { Dock=System.Windows.Forms.DockStyle.Fill, Orientation=System.Windows.Forms.Orientation.Vertical, SplitterDistance=320 };
            // Left: danh sách bài + form đăng
            this.lstPosts.Dock=System.Windows.Forms.DockStyle.Fill; this.lstPosts.View=System.Windows.Forms.View.List; this.lstPosts.Font=font10; this.lstPosts.FullRowSelect=true;
            this.lstPosts.SelectedIndexChanged += new System.EventHandler(this.lstPosts_SelectedIndexChanged);
            var postBtnPnl = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Bottom, Height=200, BackColor=System.Drawing.Color.FromArgb(248,249,252), Padding=new System.Windows.Forms.Padding(6) };
            this.txbPostTitle.Dock=System.Windows.Forms.DockStyle.Top; this.txbPostTitle.Font=font10; this.txbPostTitle.PlaceholderText="Tiêu đề bài đăng...";
            this.rtbNewPost.Dock=System.Windows.Forms.DockStyle.Fill; this.rtbNewPost.Font=font10;
            var btnRow = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Bottom, Height=32 };
            this.chkPin.Text="📌 Ghim"; this.chkPin.Left=0; this.chkPin.Top=5; this.chkPin.Width=80; this.chkPin.Font=font9b;
            void SBtn(System.Windows.Forms.Button b, string t, System.Drawing.Color c, int l, int w=80) { b.Text=t; b.Left=l; b.Top=2; b.Width=w; b.Height=28; b.FlatStyle=System.Windows.Forms.FlatStyle.Flat; b.BackColor=c; b.ForeColor=System.Drawing.Color.White; b.Font=font9b; b.FlatAppearance.BorderSize=0; }
            SBtn(this.btnPickPostImage,"🖼 Ảnh",System.Drawing.Color.FromArgb(99,102,241),85,70);
            SBtn(this.btnAddPost,"📤 Đăng",System.Drawing.Color.FromArgb(34,197,94),160,80);
            SBtn(this.btnDeletePost,"🗑 Xóa",System.Drawing.Color.FromArgb(239,68,68),245,70);
            this.btnAddPost.Click       += new System.EventHandler(this.btnAddPost_Click);
            this.btnDeletePost.Click    += new System.EventHandler(this.btnDeletePost_Click);
            this.btnPickPostImage.Click += new System.EventHandler(this.btnPickPostImage_Click);
            btnRow.Controls.AddRange(new System.Windows.Forms.Control[]{ this.chkPin, this.btnPickPostImage, this.btnAddPost, this.btnDeletePost });
            postBtnPnl.Controls.Add(this.rtbNewPost); postBtnPnl.Controls.Add(this.txbPostTitle); postBtnPnl.Controls.Add(btnRow);
            this.picNewPost.Dock=System.Windows.Forms.DockStyle.Right; this.picNewPost.Width=90; this.picNewPost.SizeMode=System.Windows.Forms.PictureBoxSizeMode.Zoom; this.picNewPost.BackColor=System.Drawing.Color.FromArgb(230,235,245);
            split1.Panel1.Controls.Add(this.lstPosts); split1.Panel1.Controls.Add(postBtnPnl);
            // Right: nội dung + comment
            this.rtbPostContent.Dock=System.Windows.Forms.DockStyle.Fill; this.rtbPostContent.ReadOnly=true; this.rtbPostContent.Font=font10; this.rtbPostContent.BackColor=System.Drawing.Color.White;
            this.picPost.Dock=System.Windows.Forms.DockStyle.Right; this.picPost.Width=120; this.picPost.SizeMode=System.Windows.Forms.PictureBoxSizeMode.Zoom;
            var cmtPnl = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Bottom, Height=180, BackColor=System.Drawing.Color.FromArgb(248,249,252) };
            this.lstComments.Dock=System.Windows.Forms.DockStyle.Fill; this.lstComments.View=System.Windows.Forms.View.List; this.lstComments.Font=font10;
            var cmtRow = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Bottom, Height=32 };
            this.txbComment.Dock=System.Windows.Forms.DockStyle.Fill; this.txbComment.Font=font10; this.txbComment.PlaceholderText="Nhập bình luận...";
            SBtn(this.btnAddComment,"💬 Gửi",System.Drawing.Color.FromArgb(59,130,246),0,70);
            this.btnAddComment.Dock=System.Windows.Forms.DockStyle.Right;
            this.btnAddComment.Click += new System.EventHandler(this.btnAddComment_Click);
            cmtRow.Controls.Add(this.txbComment); cmtRow.Controls.Add(this.btnAddComment);
            cmtPnl.Controls.Add(this.lstComments); cmtPnl.Controls.Add(cmtRow);
            split1.Panel2.Controls.Add(this.rtbPostContent); split1.Panel2.Controls.Add(this.picPost); split1.Panel2.Controls.Add(cmtPnl);
            tBoard.Controls.Add(split1);

            // ── LỊCH HỌC ──
            this.dgvSchedule.Dock=System.Windows.Forms.DockStyle.Fill; this.dgvSchedule.AllowUserToAddRows=false; this.dgvSchedule.BackgroundColor=System.Drawing.Color.White; this.dgvSchedule.BorderStyle=System.Windows.Forms.BorderStyle.None; this.dgvSchedule.Font=font10;
            var mkPnl = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Bottom, Height=70, BackColor=System.Drawing.Color.FromArgb(248,249,252), Padding=new System.Windows.Forms.Padding(10) };
            var lblDate = new System.Windows.Forms.Label { Text="Ngày:", Left=10, Top=10, Width=45, Font=font9b };
            this.dtpMakeup.Left=55; this.dtpMakeup.Top=8; this.dtpMakeup.Width=130;
            this.rbNghi.Text="Nghỉ"; this.rbNghi.Left=195; this.rbNghi.Top=10; this.rbNghi.Width=55; this.rbNghi.Checked=true; this.rbNghi.Font=font9b;
            this.rbBu.Text="Bù"; this.rbBu.Left=255; this.rbBu.Top=10; this.rbBu.Width=40; this.rbBu.Font=font9b;
            this.txbMakeupNote.Left=300; this.txbMakeupNote.Top=8; this.txbMakeupNote.Width=180; this.txbMakeupNote.Font=font10; this.txbMakeupNote.PlaceholderText="Ghi chú...";
            SBtn(this.btnAddMakeup,"➕ Thêm",System.Drawing.Color.FromArgb(34,197,94),490,80);
            SBtn(this.btnDeleteMakeup,"🗑 Xóa",System.Drawing.Color.FromArgb(239,68,68),578,70);
            this.btnAddMakeup.Top=6; this.btnDeleteMakeup.Top=6;
            this.btnAddMakeup.Click    += new System.EventHandler(this.btnAddMakeup_Click);
            this.btnDeleteMakeup.Click += new System.EventHandler(this.btnDeleteMakeup_Click);
            mkPnl.Controls.AddRange(new System.Windows.Forms.Control[]{ lblDate, this.dtpMakeup, this.rbNghi, this.rbBu, this.txbMakeupNote, this.btnAddMakeup, this.btnDeleteMakeup });
            tSchedule.Controls.Add(this.dgvSchedule); tSchedule.Controls.Add(mkPnl);

            // ── TÀI LIỆU ──
            var docBtnPnl = new System.Windows.Forms.Panel { Dock=System.Windows.Forms.DockStyle.Bottom, Height=40, BackColor=System.Drawing.Color.FromArgb(248,249,252) };
            SBtn(this.btnAddFolder,"📂 Thư mục",System.Drawing.Color.FromArgb(245,158,11),8,100);
            SBtn(this.btnUploadDoc,"⬆ Upload",System.Drawing.Color.FromArgb(59,130,246),114,90);
            SBtn(this.btnDownload,"⬇ Tải về",System.Drawing.Color.FromArgb(34,197,94),210,90);
            SBtn(this.btnDeleteDoc,"🗑 Xóa",System.Drawing.Color.FromArgb(239,68,68),306,80);
            foreach(var b in new[]{this.btnAddFolder,this.btnUploadDoc,this.btnDownload,this.btnDeleteDoc}) b.Top=5;
            this.btnAddFolder.Click   += new System.EventHandler(this.btnAddFolder_Click);
            this.btnUploadDoc.Click   += new System.EventHandler(this.btnUploadDoc_Click);
            this.btnDownload.Click    += new System.EventHandler(this.btnDownload_Click);
            this.btnDeleteDoc.Click   += new System.EventHandler(this.btnDeleteDoc_Click);
            this.tvDocs.Dock=System.Windows.Forms.DockStyle.Fill; this.tvDocs.Font=font10; this.tvDocs.ShowLines=true; this.tvDocs.ShowPlusMinus=true;
            docBtnPnl.Controls.AddRange(new System.Windows.Forms.Control[]{ this.btnAddFolder, this.btnUploadDoc, this.btnDownload, this.btnDeleteDoc });
            tDocs.Controls.Add(this.tvDocs); tDocs.Controls.Add(docBtnPnl);

            // ── ĐIỂM SỐ ──
            this.pnlScoreEdit.Dock=System.Windows.Forms.DockStyle.Bottom; this.pnlScoreEdit.Height=50; this.pnlScoreEdit.BackColor=System.Drawing.Color.FromArgb(248,249,252);
            var lblCN = new System.Windows.Forms.Label { Text="Thành phần:", Left=8, Top=14, Width=90, Font=font9b };
            this.txbCompName.Left=100; this.txbCompName.Top=12; this.txbCompName.Width=120; this.txbCompName.Font=font10; this.txbCompName.PlaceholderText="VD: Giữa kỳ";
            var lblW = new System.Windows.Forms.Label { Text="Trọng số%:", Left=228, Top=14, Width=80, Font=font9b };
            this.nudWeight.Left=310; this.nudWeight.Top=12; this.nudWeight.Width=60; this.nudWeight.Minimum=1; this.nudWeight.Maximum=100; this.nudWeight.Value=10;
            SBtn(this.btnAddComp,"➕ Thêm TP",System.Drawing.Color.FromArgb(99,102,241),380,100); this.btnAddComp.Top=10;
            SBtn(this.btnSaveScore,"💾 Lưu điểm",System.Drawing.Color.FromArgb(34,197,94),486,100); this.btnSaveScore.Top=10;
            this.btnAddComp.Click  += new System.EventHandler(this.btnAddComp_Click);
            this.btnSaveScore.Click += new System.EventHandler(this.btnSaveScore_Click);
            this.pnlScoreEdit.Controls.AddRange(new System.Windows.Forms.Control[]{ lblCN, this.txbCompName, lblW, this.nudWeight, this.btnAddComp, this.btnSaveScore });
            var scoreSplit = new System.Windows.Forms.SplitContainer { Dock=System.Windows.Forms.DockStyle.Fill, SplitterDistance=160, Orientation=System.Windows.Forms.Orientation.Horizontal };
            this.dgvComp.Dock=System.Windows.Forms.DockStyle.Fill; this.dgvComp.AllowUserToAddRows=false; this.dgvComp.BackgroundColor=System.Drawing.Color.White; this.dgvComp.BorderStyle=System.Windows.Forms.BorderStyle.None; this.dgvComp.Font=font10;
            this.dgvScore.Dock=System.Windows.Forms.DockStyle.Fill; this.dgvScore.AllowUserToAddRows=false; this.dgvScore.BackgroundColor=System.Drawing.Color.White; this.dgvScore.BorderStyle=System.Windows.Forms.BorderStyle.None; this.dgvScore.Font=font10;
            scoreSplit.Panel1.Controls.Add(this.dgvComp);
            scoreSplit.Panel2.Controls.Add(this.dgvScore);
            tScore.Controls.Add(scoreSplit); tScore.Controls.Add(this.pnlScoreEdit);

            tab.Controls.Add(tBoard); tab.Controls.Add(tSchedule); tab.Controls.Add(tDocs); tab.Controls.Add(tScore);
            this.Controls.Add(tab); this.Controls.Add(hdr);
            this.Dock=System.Windows.Forms.DockStyle.Fill;
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ListView lstPosts, lstComments;
        private System.Windows.Forms.RichTextBox rtbPostContent, rtbNewPost;
        private System.Windows.Forms.PictureBox picPost, picNewPost;
        private System.Windows.Forms.TextBox txbComment, txbPostTitle, txbMakeupNote, txbCompName;
        private System.Windows.Forms.CheckBox chkPin;
        private System.Windows.Forms.DataGridView dgvSchedule, dgvComp, dgvScore;
        private System.Windows.Forms.DateTimePicker dtpMakeup;
        private System.Windows.Forms.RadioButton rbNghi, rbBu;
        private System.Windows.Forms.TreeView tvDocs;
        private System.Windows.Forms.NumericUpDown nudWeight;
        private System.Windows.Forms.Label lblClassName, lblTeacher, lblRoom, lblSemester, lblSlot, lblStatus;
        private System.Windows.Forms.Button btnAddPost, btnDeletePost, btnAddComment, btnPickPostImage;
        private System.Windows.Forms.Button btnAddMakeup, btnDeleteMakeup;
        private System.Windows.Forms.Button btnAddFolder, btnUploadDoc, btnDownload, btnDeleteDoc;
        private System.Windows.Forms.Button btnAddComp, btnSaveScore;
        private System.Windows.Forms.Panel pnlScoreEdit;
    }
}
