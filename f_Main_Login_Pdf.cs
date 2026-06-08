// ============================================================
//  f_Main_Integration.cs
//  Paste đoạn này vào f_Main.cs hiện có
// ============================================================
// 1. Thêm helper ShowUC vào f_Main
// 2. Thêm wire-up các UC mới vào các nút sidebar

using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLSV
{
    // ── Thêm vào class f_Main ────────────────────────────────
    public partial class f_Main : Form
    {
        // Helper: hiển thị UserControl vào vùng content
        private void ShowUC(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }

        // Gọi hàm này trong Form_Load hoặc sau login thành công
        private void WireUpNewModules()
        {
            // ─ Phase 1 ─
            if (btnDepartment != null)
                btnDepartment.Click += (s, e) => {
                    var uc = new UC_Department();
                    ShowUC(uc);
                    uc.LoadData();
                };

            if (btnSemester != null)
                btnSemester.Click += (s, e) => {
                    var uc = new UC_Semester();
                    ShowUC(uc);
                    uc.LoadData();
                };

            if (btnClassRoom != null)
                btnClassRoom.Click += (s, e) => {
                    var uc = new UC_ClassRoom();
                    ShowUC(uc);
                    uc.LoadData();
                };

            if (btnCourse != null)
                btnCourse.Click += (s, e) => {
                    var uc = new UC_Course();
                    ShowUC(uc);
                    uc.LoadData();
                };

            // ─ Phase 2 ─
            if (btnEnrollment != null)
                btnEnrollment.Click += (s, e) => {
                    var uc = new UC_Enrollment();
                    ShowUC(uc);
                    uc.LoadData(AppSession.CurrentMSSV);
                };

            // ─ Phase 3 ─
            if (btnNotification != null)
                btnNotification.Click += (s, e) => {
                    var uc = new UC_Notification();
                    ShowUC(uc);
                    uc.LoadData(AppSession.CurrentAccountID, AppSession.CurrentRole);
                    // Cập nhật badge
                    UpdateNotifBadge();
                };

            if (btnLibrary != null)
                btnLibrary.Click += (s, e) => {
                    var uc = new UC_Library();
                    ShowUC(uc);
                    uc.LoadData(AppSession.CurrentMSSV);
                };

            if (btnRequest != null)
                btnRequest.Click += (s, e) => {
                    var uc = new UC_Request();
                    ShowUC(uc);
                    uc.LoadData(AppSession.CurrentMSSV, AppSession.CurrentRole);
                };

            if (btnTrainingScore != null)
                btnTrainingScore.Click += (s, e) => {
                    var uc = new UC_TrainingScore();
                    ShowUC(uc);
                    uc.LoadData(AppSession.CurrentMSSV, AppSession.CurrentRole);
                };

            // ─ Phase 4 (Admin only) ─
            if (AppSession.IsAdmin)
            {
                if (btnDashboard != null)
                    btnDashboard.Click += (s, e) => {
                        var uc = new UC_Dashboard();
                        ShowUC(uc);
                        uc.LoadData();
                    };

                if (btnBackup != null)
                    btnBackup.Click += (s, e) => BackupHelper.Backup();

                if (btnAuditLog != null)
                    btnAuditLog.Click += (s, e) => {
                        var dt = AuditHelper.GetLogs();
                        var frm = new Form { Text="📋 Nhật ký thao tác", Size=new Size(1000,600), StartPosition=FormStartPosition.CenterScreen };
                        var dgv = new DataGridView { Dock=DockStyle.Fill, ReadOnly=true, AllowUserToAddRows=false, BackgroundColor=Color.White, Font=new Font("Segoe UI",9.5f) };
                        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        frm.Controls.Add(dgv);
                        dgv.DataSource = dt;
                        frm.Show();
                    };

                if (btnLoginHistory != null)
                    btnLoginHistory.Click += (s, e) => {
                        var dt = LoginHelper.GetLoginHistory();
                        var frm = new Form { Text="🔐 Lịch sử đăng nhập", Size=new Size(900,500), StartPosition=FormStartPosition.CenterScreen };
                        var dgv = new DataGridView { Dock=DockStyle.Fill, ReadOnly=true, AllowUserToAddRows=false, BackgroundColor=Color.White, Font=new Font("Segoe UI",9.5f) };
                        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        frm.Controls.Add(dgv);
                        dgv.DataSource = dt;
                        frm.Show();
                    };
            }

            // Ẩn/hiện theo role
            ApplyRoleVisibility();

            // Cập nhật badge thông báo mỗi 60s
            var timer = new Timer { Interval = 60000 };
            timer.Tick += (s, e) => UpdateNotifBadge();
            timer.Start();

            // Lần đầu load
            UpdateNotifBadge();
        }

        private void UpdateNotifBadge()
        {
            if (lblNotifBadge == null) return;
            int count = Notification.GetUnreadCount(AppSession.CurrentAccountID, AppSession.CurrentRole);
            lblNotifBadge.Visible = count > 0;
            lblNotifBadge.Text    = count > 99 ? "99+" : count.ToString();
        }

        private void ApplyRoleVisibility()
        {
            bool isAdmin = AppSession.IsAdmin;
            bool isGV    = AppSession.IsGiaoVien;
            bool isSV    = AppSession.IsSinhVien;

            // Admin-only
            if (btnDashboard    != null) btnDashboard.Visible    = isAdmin;
            if (btnDepartment   != null) btnDepartment.Visible   = isAdmin;
            if (btnSemester     != null) btnSemester.Visible     = isAdmin;
            if (btnBackup       != null) btnBackup.Visible       = isAdmin;
            if (btnAuditLog     != null) btnAuditLog.Visible     = isAdmin;
            if (btnLoginHistory != null) btnLoginHistory.Visible = isAdmin;

            // GV + Admin
            if (btnClassRoom != null) btnClassRoom.Visible = isAdmin || isGV;

            // SV only
            if (btnEnrollment != null) btnEnrollment.Visible = isSV;
        }

        // Declare các button mới (nếu chưa có trong Designer)
        // Thêm vào Designer của f_Main hoặc khai báo ở đây:
        private Button btnDepartment, btnSemester, btnClassRoom, btnCourse;
        private Button btnEnrollment, btnNotification, btnLibrary, btnRequest;
        private Button btnTrainingScore, btnDashboard, btnBackup, btnAuditLog, btnLoginHistory;
        private Label  lblNotifBadge;
        private Panel  pnlContent;
    }

    // ============================================================
    //  f_Login_Upgrade.cs
    //  Thêm vào f_Login.cs hiện có sau khi xác thực thành công
    // ============================================================
    public static class LoginUpgradeHelper
    {
        /// <summary>
        /// Gọi sau khi xác thực login thành công.
        /// Trả về true nếu được phép vào main, false nếu cần đổi mật khẩu.
        /// </summary>
        public static bool PostLoginSetup(int accountID, string username, string role,
                                          string email, int mssv, out string redirectMsg)
        {
            redirectMsg = "";

            // Set session
            AppSession.CurrentAccountID  = accountID;
            AppSession.CurrentRole       = role;
            AppSession.CurrentEmail      = email;
            AppSession.CurrentMSSV       = mssv;
            AppSession.CurrentSemesterID = Semester.GetActiveSemesterID();

            // Ghi log đăng nhập thành công
            LoginHelper.LogLogin(accountID, true);

            // Kiểm tra lần đầu đăng nhập
            if (LoginHelper.IsFirstLogin(accountID))
            {
                redirectMsg = "first_login";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ghi log đăng nhập thất bại (sai mật khẩu).
        /// </summary>
        public static void LogFailedLogin(int accountID, string reason)
            => LoginHelper.LogLogin(accountID, false, reason);
    }

    // ============================================================
    //  PdfHelper.cs – Xuất PDF bảng điểm dùng iTextSharp
    //  NuGet: Install-Package iTextSharp
    // ============================================================
    public static class PdfHelper
    {
        public static bool ExportScorePdf(int mssv, string studentName)
        {
            using var sfd = new SaveFileDialog {
                Filter   = "PDF|*.pdf",
                FileName = $"BangDiem_{mssv}_{DateTime.Now:yyyyMMdd}.pdf"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return false;

            try
            {
                using var fs  = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create);
                var doc   = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 40, 40, 60, 40);
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fs);
                doc.Open();

                // Font
                var baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(
                    iTextSharp.text.pdf.BaseFont.HELVETICA, iTextSharp.text.pdf.BaseFont.CP1252, false);
                var titleFont  = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
                var headerFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);
                var bodyFont   = new iTextSharp.text.Font(baseFont, 10);
                var boldFont   = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD);

                // Tiêu đề
                var title = new iTextSharp.text.Paragraph("BANG DIEM HOC TAP", titleFont) {
                    Alignment = iTextSharp.text.Element.ALIGN_CENTER,
                    SpacingAfter = 6
                };
                doc.Add(title);
                doc.Add(new iTextSharp.text.Paragraph($"Sinh vien: {studentName}  |  MSSV: {mssv}", boldFont) { Alignment=iTextSharp.text.Element.ALIGN_CENTER });
                doc.Add(new iTextSharp.text.Paragraph($"Ngay in: {DateTime.Now:dd/MM/yyyy HH:mm}", bodyFont) { Alignment=iTextSharp.text.Element.ALIGN_CENTER, SpacingAfter=12 });

                // Bảng điểm
                var table = new iTextSharp.text.pdf.PdfPTable(5) { WidthPercentage=100 };
                table.SetWidths(new float[]{ 3f, 1f, 1f, 1.5f, 1.5f });

                var headerBg = new iTextSharp.text.BaseColor(30, 60, 140);
                void AddHeader(string txt) {
                    var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(txt, headerFont)) {
                        BackgroundColor=headerBg, HorizontalAlignment=iTextSharp.text.Element.ALIGN_CENTER,
                        Padding=6, BorderColor=iTextSharp.text.BaseColor.WHITE
                    };
                    table.AddCell(cell);
                }
                AddHeader("Hoc ky"); AddHeader("So mon"); AddHeader("Tong TC"); AddHeader("GPA (10)"); AddHeader("Xep loai");

                var dt = GpaCalculator.GetSummaryByStudent(mssv);
                bool alt = false;
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    double gpa = Convert.ToDouble(row["GPA thang 10"]);
                    string rank = GpaCalculator.GetRank(gpa);
                    var rowBg = alt ? new iTextSharp.text.BaseColor(240,244,255) : iTextSharp.text.BaseColor.WHITE;
                    var rankBg = gpa>=8.0 ? new iTextSharp.text.BaseColor(200,255,200) :
                                 gpa>=6.5 ? new iTextSharp.text.BaseColor(255,255,200) :
                                            new iTextSharp.text.BaseColor(255,210,210);

                    void AddCell(string txt, iTextSharp.text.BaseColor bg=null, bool center=false) {
                        var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(txt, bodyFont)) {
                            BackgroundColor=bg??rowBg, Padding=5,
                            HorizontalAlignment=center?iTextSharp.text.Element.ALIGN_CENTER:iTextSharp.text.Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    AddCell(row["Học kỳ"].ToString());
                    AddCell(row["Số môn"].ToString(), null, true);
                    AddCell(row["Tổng TC"].ToString(), null, true);
                    AddCell(gpa.ToString("F2"), null, true);
                    AddCell(rank, rankBg, true);
                    alt = !alt;
                }

                // Tổng kết
                double overall = GpaCalculator.CalcGPA10(mssv);
                string overallRank = GpaCalculator.GetRank(overall);
                void AddSummaryCell(string txt, bool bold=false) {
                    var f = bold ? boldFont : bodyFont;
                    var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(txt, f)) {
                        BackgroundColor=new iTextSharp.text.BaseColor(220,230,255), Padding=6,
                        HorizontalAlignment=iTextSharp.text.Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }
                AddSummaryCell("TONG KET", true);
                AddSummaryCell("—"); AddSummaryCell("—");
                AddSummaryCell(overall.ToString("F2"), true);
                AddSummaryCell(overallRank, true);

                doc.Add(table);

                // GPA thang 4
                double gpa4 = GpaCalculator.ToScale4(overall);
                doc.Add(new iTextSharp.text.Paragraph($"\nGPA thang 4: {gpa4:F1}   |   Xep loai: {overallRank}", boldFont) { SpacingBefore=10 });

                doc.Close();

                MessageBox.Show($"✅ Xuất PDF thành công!\n{sfd.FileName}", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (MessageBox.Show("Mở file PDF?", "Câu hỏi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    System.Diagnostics.Process.Start(sfd.FileName);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất PDF: {ex.Message}", "❌", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
