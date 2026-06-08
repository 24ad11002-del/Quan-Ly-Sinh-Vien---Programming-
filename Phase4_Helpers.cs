// ============================================================
//  AppSession_Upgrade.cs
//  Nâng cấp AppSession: thêm property IsAdmin, IsGiaoVien,
//  CurrentEmail, CurrentAccountID để dùng khắp project
// ============================================================
namespace QLSV
{
    // Thêm các property mới vào AppSession (partial hoặc sửa trực tiếp)
    // Nếu AppSession.cs đã tồn tại, merge các dòng sau vào:
    public partial class AppSession
    {
        // Các property tiện ích phân quyền
        public static bool IsAdmin      => CurrentRole == "Admin";
        public static bool IsGiaoVien   => CurrentRole == "GiaoVien";
        public static bool IsSinhVien   => CurrentRole == "SinhVien";
        public static string CurrentEmail { get; set; }
        public static int    CurrentAccountID { get; set; }
        public static int    CurrentMSSV      { get; set; }
        public static int    CurrentSemesterID { get; set; } = -1;
    }
}

// ============================================================
//  f_ChangePasswordFirst.cs – Bắt đổi mật khẩu lần đầu
// ============================================================
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QLSV
{
    public partial class f_ChangePasswordFirst : Form
    {
        private readonly int _accountID;

        public f_ChangePasswordFirst(int accountID)
        {
            InitializeComponent();
            _accountID = accountID;
            lblWarn.Text = "⚠️ Đây là lần đầu đăng nhập. Bạn phải đổi mật khẩu trước khi sử dụng!";
            this.FormClosing += (s, e) => {
                if (e.CloseReason == CloseReason.UserClosing)
                    e.Cancel = true; // Không cho tắt form này
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string oldPw  = txbOld.Text;
            string newPw  = txbNew.Text;
            string confPw = txbConfirm.Text;

            if (string.IsNullOrEmpty(oldPw) || string.IsNullOrEmpty(newPw))
            { MessageBox.Show("Vui lòng nhập đầy đủ!", "⚠️"); return; }

            if (newPw != confPw)
            { MessageBox.Show("Mật khẩu xác nhận không khớp!", "⚠️"); return; }

            if (newPw.Length < 8)
            { MessageBox.Show("Mật khẩu mới phải ít nhất 8 ký tự!", "⚠️"); return; }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPw, @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$"))
            { MessageBox.Show("Mật khẩu cần có: chữ hoa, số, ký tự đặc biệt!", "⚠️"); return; }

            // Kiểm tra mật khẩu cũ
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var check = new SqlCommand(
                    "SELECT COUNT(*) FROM Account WHERE ID=@id AND Pass=HASHBYTES('SHA2_256',@old)", db.conn);
                check.Parameters.AddWithValue("@id",  _accountID);
                check.Parameters.AddWithValue("@old", oldPw);
                if ((int)check.ExecuteScalar() == 0)
                { MessageBox.Show("Mật khẩu hiện tại không đúng!", "❌"); return; }

                // Cập nhật mật khẩu mới
                var upd = new SqlCommand(
                    "UPDATE Account SET Pass=HASHBYTES('SHA2_256',@new), IsFirstLogin=0 WHERE ID=@id", db.conn);
                upd.Parameters.AddWithValue("@new", newPw);
                upd.Parameters.AddWithValue("@id",  _accountID);
                upd.ExecuteNonQuery();

                MessageBox.Show("✅ Đổi mật khẩu thành công! Vui lòng đăng nhập lại.", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Restart();
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}", "❌"); }
            finally { db.closeConnection(); }
        }
    }
}

// ============================================================
//  ExcelHelper.cs – Import/Export Excel dùng ClosedXML
//  NuGet: Install-Package ClosedXML
// ============================================================
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace QLSV
{
    public static class ExcelHelper
    {
        // ── EXPORT DataTable → Excel ──────────────────────────────
        public static bool ExportToExcel(DataTable dt, string sheetName = "Sheet1", string title = "")
        {
            using var sfd = new SaveFileDialog {
                Filter   = "Excel|*.xlsx",
                FileName = $"{sheetName}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return false;

            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add(sheetName);

                int startRow = 1;
                if (!string.IsNullOrEmpty(title))
                {
                    ws.Cell(1, 1).Value = title;
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 14;
                    ws.Range(1, 1, 1, dt.Columns.Count).Merge();
                    startRow = 3;
                }

                // Header
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    var cell = ws.Cell(startRow, c + 1);
                    cell.Value = dt.Columns[c].ColumnName;
                    cell.Style.Font.Bold     = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(60, 90, 160);
                    cell.Style.Font.FontColor       = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Data
                for (int r = 0; r < dt.Rows.Count; r++)
                    for (int c = 0; c < dt.Columns.Count; c++)
                        ws.Cell(startRow + 1 + r, c + 1).Value = dt.Rows[r][c]?.ToString() ?? "";

                // Alternate rows
                for (int r = 0; r < dt.Rows.Count; r++)
                    if (r % 2 == 1)
                        ws.Range(startRow + 1 + r, 1, startRow + 1 + r, dt.Columns.Count)
                          .Style.Fill.BackgroundColor = XLColor.FromArgb(240, 245, 255);

                ws.Columns().AdjustToContents();

                // Border
                var tableRange = ws.Range(startRow, 1, startRow + dt.Rows.Count, dt.Columns.Count);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                tableRange.Style.Border.InsideBorder  = XLBorderStyleValues.Thin;

                wb.SaveAs(sfd.FileName);
                MessageBox.Show($"✅ Xuất Excel thành công!\n{sfd.FileName}", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Mở file ngay
                if (MessageBox.Show("Mở file vừa xuất?", "Câu hỏi",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                    System.Diagnostics.Process.Start(sfd.FileName);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "❌",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ── EXPORT Bảng điểm SV ──────────────────────────────────
        public static bool ExportScoreSheet(int mssv, string studentName)
        {
            DataTable dtSem = GpaCalculator.GetSummaryByStudent(mssv);

            using var sfd = new SaveFileDialog {
                Filter   = "Excel|*.xlsx",
                FileName = $"BangDiem_{mssv}_{DateTime.Now:yyyyMMdd}.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return false;

            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Bảng điểm");

                // Tiêu đề
                ws.Cell(1, 1).Value = "BẢNG ĐIỂM HỌC TẬP";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Range(1, 1, 1, 5).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(2, 1).Value = $"Sinh viên: {studentName}  |  MSSV: {mssv}";
                ws.Range(2, 1, 2, 5).Merge();

                ws.Cell(3, 1).Value = $"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Range(3, 1, 3, 5).Merge();

                // Header bảng
                int hr = 5;
                string[] headers = { "Học kỳ", "Số môn", "Tổng TC", "GPA (thang 10)", "Xếp loại" };
                for (int c = 0; c < headers.Length; c++)
                {
                    var cell = ws.Cell(hr, c + 1);
                    cell.Value = headers[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(60, 90, 160);
                    cell.Style.Font.FontColor       = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row = hr + 1;
                foreach (DataRow dr in dtSem.Rows)
                {
                    double gpa = Convert.ToDouble(dr["GPA thang 10"]);
                    ws.Cell(row, 1).Value = dr["Học kỳ"].ToString();
                    ws.Cell(row, 2).Value = dr["Số môn"].ToString();
                    ws.Cell(row, 3).Value = dr["Tổng TC"].ToString();
                    ws.Cell(row, 4).Value = gpa.ToString("F2");
                    ws.Cell(row, 5).Value = GpaCalculator.GetRank(gpa);

                    var rankCell = ws.Cell(row, 5);
                    rankCell.Style.Fill.BackgroundColor =
                        gpa >= 8.0 ? XLColor.FromArgb(200, 255, 200) :
                        gpa >= 6.5 ? XLColor.FromArgb(255, 255, 200) :
                                     XLColor.FromArgb(255, 220, 220);
                    row++;
                }

                // GPA tổng
                double overallGpa = GpaCalculator.CalcGPA10(mssv);
                ws.Cell(row, 1).Value = "TỔNG KẾT";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 4).Value = overallGpa.ToString("F2");
                ws.Cell(row, 4).Style.Font.Bold = true;
                ws.Cell(row, 5).Value = GpaCalculator.GetRank(overallGpa);
                ws.Cell(row, 5).Style.Font.Bold = true;

                ws.Columns().AdjustToContents();
                wb.SaveAs(sfd.FileName);

                MessageBox.Show($"✅ Xuất bảng điểm thành công!\n{sfd.FileName}", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "❌", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ── IMPORT ClassRoom từ Excel ─────────────────────────────
        public static List<ClassRoom> ImportClassRooms(out string errors)
        {
            errors = "";
            var list = new List<ClassRoom>();

            using var ofd = new OpenFileDialog { Filter = "Excel|*.xlsx;*.xls" };
            if (ofd.ShowDialog() != DialogResult.OK) return list;

            try
            {
                using var wb = new XLWorkbook(ofd.FileName);
                var ws   = wb.Worksheets.First();
                int rows = ws.LastRowUsed().RowNumber();

                int activeSemID = Semester.GetActiveSemesterID();

                for (int r = 2; r <= rows; r++)
                {
                    try
                    {
                        string code    = ws.Cell(r, 1).GetString().Trim().ToUpper();
                        string courseID = ws.Cell(r, 2).GetString().Trim();
                        string teacher = ws.Cell(r, 3).GetString().Trim();
                        string room    = ws.Cell(r, 4).GetString().Trim();
                        int dow        = int.Parse(ws.Cell(r, 5).GetString().Trim());
                        int start      = int.Parse(ws.Cell(r, 6).GetString().Trim());
                        int num        = int.Parse(ws.Cell(r, 7).GetString().Trim());
                        int maxSlot    = ws.Cell(r, 8).GetString().Trim() != "" ?
                                         int.Parse(ws.Cell(r, 8).GetString().Trim()) : 40;

                        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(courseID)) continue;

                        list.Add(new ClassRoom {
                            ClassCode   = code,
                            SemesterID  = activeSemID,
                            CourseID    = courseID,
                            TeacherName = teacher,
                            Room        = room,
                            DayOfWeek   = dow,
                            StartPeriod = start,
                            NumPeriod   = num,
                            MaxSlot     = maxSlot,
                            Status      = "Đang mở"
                        });
                    }
                    catch { errors += $"Lỗi dòng {r}\n"; }
                }

                MessageBox.Show($"✅ Đọc được {list.Count} lớp từ Excel{(errors != "" ? "\n\n" + errors : "")}", "Import");
            }
            catch (Exception ex) { errors = ex.Message; }

            return list;
        }

        // ── IMPORT Student từ Excel ───────────────────────────────
        public static (int ok, int fail, string errors) ImportStudents(string filePath)
        {
            int ok = 0, fail = 0;
            string errors = "";

            try
            {
                using var wb = new XLWorkbook(filePath);
                var ws   = wb.Worksheets.First();
                int rows = ws.LastRowUsed().RowNumber();

                for (int r = 2; r <= rows; r++)
                {
                    try
                    {
                        int    mssv   = int.Parse(ws.Cell(r, 1).GetString());
                        string fname  = ws.Cell(r, 2).GetString().Trim();
                        string lname  = ws.Cell(r, 3).GetString().Trim();
                        string email  = ws.Cell(r, 4).GetString().Trim();
                        DateTime dob  = ws.Cell(r, 5).GetDateTime();
                        string gender = ws.Cell(r, 6).GetString().Trim();

                        if (Student.Exists(mssv)) { errors += $"MSSV {mssv} đã tồn tại\n"; fail++; continue; }

                        var s = new Student(mssv, fname, lname, dob, gender, "", "", "", email, "Đang học", null);
                        if (s.AddStudent())
                        {
                            // Tạo tài khoản tự động
                            CreateAccountForStudent(mssv, email);
                            ok++;
                        }
                        else { errors += $"Lỗi thêm MSSV {mssv}\n"; fail++; }
                    }
                    catch { errors += $"Lỗi dòng {r}\n"; fail++; }
                }
            }
            catch (Exception ex) { errors = ex.Message; }

            return (ok, fail, errors);
        }

        private static void CreateAccountForStudent(int mssv, string email)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                string defaultPass = $"Abc@{mssv}";
                var cmd = new SqlCommand(
                    "IF NOT EXISTS(SELECT 1 FROM Account WHERE Username=@u) " +
                    "INSERT INTO Account(Username,Pass,Email,Role,MSSV,IsFirstLogin) " +
                    "VALUES(@u, HASHBYTES('SHA2_256',@p), @em, 'SinhVien', @mssv, 1)", db.conn);
                cmd.Parameters.AddWithValue("@u",    mssv.ToString());
                cmd.Parameters.AddWithValue("@p",    defaultPass);
                cmd.Parameters.AddWithValue("@em",   email);
                cmd.Parameters.AddWithValue("@mssv", mssv);
                cmd.ExecuteNonQuery();
            }
            catch { }
            finally { db.closeConnection(); }
        }
    }

    // ============================================================
    //  BackupHelper.cs – Backup & Restore SQL Server
    // ============================================================
    public static class BackupHelper
    {
        private static readonly string DB_NAME = "QLSV_Simple";

        public static bool Backup(string outputFolder = null)
        {
            if (string.IsNullOrEmpty(outputFolder))
            {
                using var fbd = new FolderBrowserDialog { Description = "Chọn thư mục lưu backup" };
                if (fbd.ShowDialog() != DialogResult.OK) return false;
                outputFolder = fbd.SelectedPath;
            }

            string fileName = $"{DB_NAME}_backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            string fullPath = System.IO.Path.Combine(outputFolder, fileName);

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    $"BACKUP DATABASE [{DB_NAME}] TO DISK='{fullPath}' WITH FORMAT, STATS=10", db.conn);
                cmd.CommandTimeout = 300;
                cmd.ExecuteNonQuery();

                AuditHelper.Log("BACKUP", "Database", DB_NAME, null, fullPath);
                MessageBox.Show($"✅ Backup thành công!\nFile: {fullPath}", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Backup thất bại!\n{ex.Message}", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally { db.closeConnection(); }
        }

        public static bool Restore(string bakFilePath = null)
        {
            if (string.IsNullOrEmpty(bakFilePath))
            {
                using var ofd = new OpenFileDialog { Filter = "Backup files|*.bak" };
                if (ofd.ShowDialog() != DialogResult.OK) return false;
                bakFilePath = ofd.FileName;
            }

            if (MessageBox.Show(
                $"⚠️ Restore sẽ THAY THẾ toàn bộ dữ liệu hiện tại!\nFile: {bakFilePath}\n\nTiếp tục?",
                "CẢNH BÁO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return false;

            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                // Đóng kết nối khác
                new SqlCommand(
                    $"ALTER DATABASE [{DB_NAME}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", db.conn).ExecuteNonQuery();

                var cmd = new SqlCommand(
                    $"RESTORE DATABASE [{DB_NAME}] FROM DISK='{bakFilePath}' WITH REPLACE, STATS=10", db.conn);
                cmd.CommandTimeout = 600;
                cmd.ExecuteNonQuery();

                new SqlCommand($"ALTER DATABASE [{DB_NAME}] SET MULTI_USER", db.conn).ExecuteNonQuery();

                MessageBox.Show("✅ Restore thành công! Ứng dụng sẽ khởi động lại.", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Restart();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Restore thất bại!\n{ex.Message}", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                try
                {
                    new SqlCommand($"ALTER DATABASE [{DB_NAME}] SET MULTI_USER", db.conn).ExecuteNonQuery();
                }
                catch { }
                return false;
            }
            finally { db.closeConnection(); }
        }

        // Backup tự động theo lịch
        private static System.Threading.Timer _autoTimer;

        public static void StartAutoBackup(string folder, int intervalHours = 24)
        {
            StopAutoBackup();
            var interval = TimeSpan.FromHours(intervalHours);
            _autoTimer = new System.Threading.Timer(_ => Backup(folder),
                null, interval, interval);
        }

        public static void StopAutoBackup()
        {
            _autoTimer?.Dispose();
            _autoTimer = null;
        }
    }
}
