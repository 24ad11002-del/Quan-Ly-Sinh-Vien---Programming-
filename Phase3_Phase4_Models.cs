// ============================================================
//  Phase3_Phase4_Models.cs
//  Notification · AuditLog · LoginLog · Book · Request · TrainingScore · GpaCalculator
// ============================================================
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace QLSV
{
    // ============================================================
    //  Notification – Thông báo
    // ============================================================
    public class Notification
    {
        public int    NotifID   { get; set; }
        public string Title     { get; set; }
        public string Content   { get; set; }
        public string NotifType { get; set; } = "Chung";
        public string TargetRole { get; set; }
        public int?   ClassID   { get; set; }
        public int    SenderID  { get; set; }

        public bool Add()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO Notification(Title,Content,NotifType,TargetRole,ClassID,SenderID,CreatedAt,IsActive) " +
                    "VALUES(@t,@con,@nt,@tr,@cid,@sid,GETDATE(),1)", db.conn);
                cmd.Parameters.AddWithValue("@t",   Title);
                cmd.Parameters.AddWithValue("@con", Content);
                cmd.Parameters.AddWithValue("@nt",  NotifType);
                cmd.Parameters.AddWithValue("@tr",  (object)TargetRole ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cid", (object)ClassID   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@sid", SenderID);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool Delete(int id)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("UPDATE Notification SET IsActive=0 WHERE NotifID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool MarkRead(int notifID, int accountID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "IF NOT EXISTS(SELECT 1 FROM NotificationRead WHERE NotifID=@nid AND AccountID=@aid) " +
                    "INSERT INTO NotificationRead(NotifID,AccountID,ReadAt) VALUES(@nid,@aid,GETDATE())", db.conn);
                cmd.Parameters.AddWithValue("@nid", notifID);
                cmd.Parameters.AddWithValue("@aid", accountID);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static int GetUnreadCount(int accountID, string role)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Notification n " +
                    "WHERE n.IsActive=1 " +
                    "AND (n.TargetRole IS NULL OR n.TargetRole=@role) " +
                    "AND NOT EXISTS(SELECT 1 FROM NotificationRead nr WHERE nr.NotifID=n.NotifID AND nr.AccountID=@aid)", db.conn);
                cmd.Parameters.AddWithValue("@aid",  accountID);
                cmd.Parameters.AddWithValue("@role", role);
                return (int)cmd.ExecuteScalar();
            }
            finally { db.closeConnection(); }
        }

        public static DataTable GetForUser(int accountID, string role, string keyword = "", string type = "")
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT n.NotifID, n.Title, n.Content, n.NotifType, n.CreatedAt, " +
                    "a.Username AS Sender, " +
                    "CASE WHEN nr.ReadAt IS NOT NULL THEN 1 ELSE 0 END AS IsRead " +
                    "FROM Notification n " +
                    "JOIN Account a ON n.SenderID=a.ID " +
                    "LEFT JOIN NotificationRead nr ON nr.NotifID=n.NotifID AND nr.AccountID=@aid " +
                    "WHERE n.IsActive=1 " +
                    "AND (n.TargetRole IS NULL OR n.TargetRole=@role) " +
                    "AND (@kw='' OR n.Title LIKE @kw OR n.Content LIKE @kw) " +
                    "AND (@tp='' OR n.NotifType=@tp) " +
                    "ORDER BY n.CreatedAt DESC", db.conn);
                cmd.Parameters.AddWithValue("@aid",  accountID);
                cmd.Parameters.AddWithValue("@role", role);
                cmd.Parameters.AddWithValue("@kw",   string.IsNullOrWhiteSpace(keyword) ? "" : "%" + keyword + "%");
                cmd.Parameters.AddWithValue("@tp",   type ?? "");
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static void CreateMakeupNotification(int classID, string dayType, DateTime date, string note, int senderID)
        {
            var n = new Notification
            {
                Title     = $"[{dayType}] Thông báo {dayType.ToLower()} lớp",
                Content   = $"Lớp sẽ {dayType.ToLower()} vào ngày {date:dd/MM/yyyy}. Ghi chú: {note}",
                NotifType = dayType == "Nghỉ" ? "Thông báo nghỉ" : "Thông báo bù",
                ClassID   = classID,
                SenderID  = senderID,
                TargetRole = "SinhVien"
            };
            n.Add();
        }
    }

    // ============================================================
    //  AuditHelper – Nhật ký thao tác
    // ============================================================
    public static class AuditHelper
    {
        public static void Log(string action, string tableName, string recordID,
                               string oldVal = null, string newVal = null)
        {
            if (AppSession.CurrentAccountID <= 0) return;
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO AuditLog(AccountID,Action,TableName,RecordID,OldValue,NewValue,ActionTime,IPAddress) " +
                    "VALUES(@aid,@ac,@tb,@rid,@ov,@nv,GETDATE(),@ip)", db.conn);
                cmd.Parameters.AddWithValue("@aid", AppSession.CurrentAccountID);
                cmd.Parameters.AddWithValue("@ac",  action);
                cmd.Parameters.AddWithValue("@tb",  tableName);
                cmd.Parameters.AddWithValue("@rid", (object)recordID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ov",  (object)oldVal   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nv",  (object)newVal   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ip",  GetLocalIP());
                cmd.ExecuteNonQuery();
            }
            catch { }
            finally { db.closeConnection(); }
        }

        private static string GetLocalIP()
        {
            try { return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(); }
            catch { return "unknown"; }
        }

        public static DataTable GetLogs(string tableName = "", int accountID = -1, DateTime? from = null, DateTime? to = null)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT al.AuditID, a.Username, al.Action, al.TableName, al.RecordID, " +
                    "al.OldValue, al.NewValue, al.ActionTime, al.IPAddress " +
                    "FROM AuditLog al JOIN Account a ON al.AccountID=a.ID " +
                    "WHERE (@tb='' OR al.TableName=@tb) " +
                    "AND (@aid=-1 OR al.AccountID=@aid) " +
                    "AND (@fr IS NULL OR al.ActionTime>=@fr) " +
                    "AND (@to IS NULL OR al.ActionTime<=@to) " +
                    "ORDER BY al.ActionTime DESC", db.conn);
                cmd.Parameters.AddWithValue("@tb",  tableName ?? "");
                cmd.Parameters.AddWithValue("@aid", accountID);
                cmd.Parameters.AddWithValue("@fr",  (object)from ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@to",  (object)to   ?? DBNull.Value);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }
    }

    // ============================================================
    //  LoginHelper – Ghi log đăng nhập + bắt đổi MK lần đầu
    // ============================================================
    public static class LoginHelper
    {
        public static void LogLogin(int accountID, bool success, string failReason = null)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO LoginLog(AccountID,LoginTime,IPAddress,MachineName,IsSuccess,FailReason) " +
                    "VALUES(@aid,GETDATE(),@ip,@mc,@ok,@fr)", db.conn);
                cmd.Parameters.AddWithValue("@aid", accountID);
                cmd.Parameters.AddWithValue("@ip",  GetLocalIP());
                cmd.Parameters.AddWithValue("@mc",  Environment.MachineName);
                cmd.Parameters.AddWithValue("@ok",  success);
                cmd.Parameters.AddWithValue("@fr",  (object)failReason ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            catch { }
            finally { db.closeConnection(); }
        }

        public static bool IsFirstLogin(int accountID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT IsFirstLogin FROM Account WHERE ID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", accountID);
                object r = cmd.ExecuteScalar();
                return r != null && (bool)r;
            }
            finally { db.closeConnection(); }
        }

        public static bool ClearFirstLogin(int accountID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "UPDATE Account SET IsFirstLogin=0 WHERE ID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", accountID);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetLoginHistory(int accountID = -1)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT TOP 200 a.Username, ll.LoginTime, ll.IPAddress, ll.MachineName, " +
                    "CASE WHEN ll.IsSuccess=1 THEN N'Thành công' ELSE N'Thất bại' END AS [Kết quả], " +
                    "ll.FailReason AS [Lý do] " +
                    "FROM LoginLog ll JOIN Account a ON ll.AccountID=a.ID " +
                    "WHERE @aid=-1 OR ll.AccountID=@aid " +
                    "ORDER BY ll.LoginTime DESC", db.conn);
                cmd.Parameters.AddWithValue("@aid", accountID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        private static string GetLocalIP()
        {
            try { return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(); }
            catch { return "unknown"; }
        }
    }

    // ============================================================
    //  Book – Thư viện
    // ============================================================
    public class Book
    {
        public int    BookID    { get; set; }
        public string ISBN      { get; set; }
        public string Title     { get; set; }
        public string Author    { get; set; }
        public string Publisher { get; set; }
        public int?   PubYear   { get; set; }
        public string Category  { get; set; }
        public int    TotalQty  { get; set; }
        public int    AvailQty  { get; set; }
        public string Location  { get; set; }

        public bool Add()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO Book(ISBN,Title,Author,Publisher,PubYear,Category,TotalQty,AvailQty,Location) " +
                    "VALUES(@isbn,@t,@au,@pub,@py,@cat,@tq,@aq,@loc)", db.conn);
                Bind(cmd);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public bool Update()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "UPDATE Book SET ISBN=@isbn,Title=@t,Author=@au,Publisher=@pub," +
                    "PubYear=@py,Category=@cat,TotalQty=@tq,Location=@loc WHERE BookID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", BookID);
                Bind(cmd);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        private void Bind(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@isbn", (object)ISBN      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@t",    Title);
            cmd.Parameters.AddWithValue("@au",   Author);
            cmd.Parameters.AddWithValue("@pub",  (object)Publisher ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@py",   (object)PubYear   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cat",  (object)Category  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tq",   TotalQty);
            cmd.Parameters.AddWithValue("@aq",   AvailQty);
            cmd.Parameters.AddWithValue("@loc",  (object)Location  ?? DBNull.Value);
        }

        public static DataTable GetAll(string keyword = "")
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT BookID, ISBN, Title AS [Tên sách], Author AS [Tác giả], " +
                    "Publisher AS [NXB], PubYear AS [Năm], Category AS [Thể loại], " +
                    "TotalQty AS [Tổng], AvailQty AS [Còn lại], Location AS [Vị trí] " +
                    "FROM Book " +
                    "WHERE @kw='' OR Title LIKE @kw OR Author LIKE @kw OR ISBN LIKE @kw OR Category LIKE @kw " +
                    "ORDER BY Title", db.conn);
                cmd.Parameters.AddWithValue("@kw",
                    string.IsNullOrWhiteSpace(keyword) ? "" : "%" + keyword + "%");
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static bool Borrow(int bookID, int mssv, int days = 14)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var chk = new SqlCommand("SELECT AvailQty FROM Book WHERE BookID=@bid", db.conn);
                chk.Parameters.AddWithValue("@bid", bookID);
                int avail = (int)chk.ExecuteScalar();
                if (avail <= 0) return false;

                var ins = new SqlCommand(
                    "INSERT INTO BorrowRecord(BookID,MSSV,BorrowDate,DueDate,Status) " +
                    "VALUES(@bid,@m,CAST(GETDATE() AS DATE),DATEADD(DAY,@d,CAST(GETDATE() AS DATE)),N'Đang mượn')", db.conn);
                ins.Parameters.AddWithValue("@bid", bookID);
                ins.Parameters.AddWithValue("@m",   mssv);
                ins.Parameters.AddWithValue("@d",   days);
                ins.ExecuteNonQuery();

                var upd = new SqlCommand("UPDATE Book SET AvailQty=AvailQty-1 WHERE BookID=@bid", db.conn);
                upd.Parameters.AddWithValue("@bid", bookID);
                upd.ExecuteNonQuery();
                return true;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool Return(int borrowID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var getBook = new SqlCommand("SELECT BookID FROM BorrowRecord WHERE BorrowID=@id", db.conn);
                getBook.Parameters.AddWithValue("@id", borrowID);
                int bookID = (int)getBook.ExecuteScalar();

                var upd = new SqlCommand(
                    "UPDATE BorrowRecord SET ReturnDate=CAST(GETDATE() AS DATE),Status=N'Đã trả' WHERE BorrowID=@id", db.conn);
                upd.Parameters.AddWithValue("@id", borrowID);
                upd.ExecuteNonQuery();

                var updBook = new SqlCommand("UPDATE Book SET AvailQty=AvailQty+1 WHERE BookID=@bid", db.conn);
                updBook.Parameters.AddWithValue("@bid", bookID);
                updBook.ExecuteNonQuery();
                return true;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetBorrowByStudent(int mssv)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT br.BorrowID, b.Title AS [Tên sách], br.BorrowDate AS [Ngày mượn], " +
                    "br.DueDate AS [Hạn trả], br.ReturnDate AS [Ngày trả], br.Status AS [Trạng thái], " +
                    "CASE WHEN br.Status=N'Đang mượn' AND br.DueDate < CAST(GETDATE() AS DATE) " +
                    "     THEN N'⚠️ Quá hạn' ELSE '' END AS [Cảnh báo] " +
                    "FROM BorrowRecord br JOIN Book b ON br.BookID=b.BookID " +
                    "WHERE br.MSSV=@m ORDER BY br.BorrowDate DESC", db.conn);
                cmd.Parameters.AddWithValue("@m", mssv);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }
    }

    // ============================================================
    //  Request – Phúc khảo điểm
    // ============================================================
    public class Request
    {
        public int    ReqID       { get; set; }
        public int    MSSV        { get; set; }
        public int    ClassID     { get; set; }
        public string ReqType     { get; set; } = "Phúc khảo";
        public string Reason      { get; set; }
        public string Status      { get; set; } = "Chờ duyệt";
        public int?   ProcessedBy { get; set; }
        public string Result      { get; set; }

        public bool Add()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO Request(MSSV,ClassID,ReqType,Reason,Status,CreatedAt) " +
                    "VALUES(@m,@cid,@rt,@r,N'Chờ duyệt',GETDATE())", db.conn);
                cmd.Parameters.AddWithValue("@m",   MSSV);
                cmd.Parameters.AddWithValue("@cid", ClassID);
                cmd.Parameters.AddWithValue("@rt",  ReqType);
                cmd.Parameters.AddWithValue("@r",   Reason);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool Process(int reqID, int processorID, string status, string result)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "UPDATE Request SET Status=@st,ProcessedBy=@pb,ProcessedAt=GETDATE(),Result=@res " +
                    "WHERE ReqID=@id", db.conn);
                cmd.Parameters.AddWithValue("@st",  status);
                cmd.Parameters.AddWithValue("@pb",  processorID);
                cmd.Parameters.AddWithValue("@res", result);
                cmd.Parameters.AddWithValue("@id",  reqID);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetAll(string status = "")
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT r.ReqID, s.MSSV, s.Lname+' '+s.Fname AS [Sinh viên], " +
                    "co.CourseName AS [Môn học], cr.ClassCode AS [Mã lớp], " +
                    "r.ReqType AS [Loại], r.Reason AS [Lý do], r.Status AS [Trạng thái], " +
                    "r.Result AS [Kết quả], r.CreatedAt AS [Ngày gửi], " +
                    "a.Username AS [Người duyệt] " +
                    "FROM Request r " +
                    "JOIN Student  s  ON r.MSSV=s.MSSV " +
                    "JOIN ClassRoom cr ON r.ClassID=cr.ClassID " +
                    "JOIN Course   co ON cr.CourseID=co.CourseID " +
                    "LEFT JOIN Account a ON r.ProcessedBy=a.ID " +
                    "WHERE @st='' OR r.Status=@st " +
                    "ORDER BY r.CreatedAt DESC", db.conn);
                cmd.Parameters.AddWithValue("@st", status ?? "");
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static DataTable GetByStudent(int mssv)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT r.ReqID, co.CourseName AS [Môn học], cr.ClassCode, " +
                    "r.Reason AS [Lý do], r.Status AS [Trạng thái], " +
                    "r.Result AS [Kết quả], r.CreatedAt AS [Ngày gửi] " +
                    "FROM Request r " +
                    "JOIN ClassRoom cr ON r.ClassID=cr.ClassID " +
                    "JOIN Course   co ON cr.CourseID=co.CourseID " +
                    "WHERE r.MSSV=@m ORDER BY r.CreatedAt DESC", db.conn);
                cmd.Parameters.AddWithValue("@m", mssv);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }
    }

    // ============================================================
    //  GpaCalculator – Tính GPA & xếp loại học lực
    // ============================================================
    public static class GpaCalculator
    {
        public static double CalcGPA10(int mssv, int semesterID = -1)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(vs.TotalScore * co.Credits) / NULLIF(SUM(co.Credits),0), 0) " +
                    "FROM v_ScoreSummary vs " +
                    "JOIN ClassRoom cr ON vs.ClassID=cr.ClassID " +
                    "JOIN Course    co ON cr.CourseID=co.CourseID " +
                    "WHERE vs.MSSV=@m AND (@sid=-1 OR cr.SemesterID=@sid)", db.conn);
                cmd.Parameters.AddWithValue("@m",   mssv);
                cmd.Parameters.AddWithValue("@sid", semesterID);
                return Math.Round(Convert.ToDouble(cmd.ExecuteScalar()), 2);
            }
            finally { db.closeConnection(); }
        }

        public static double ToScale4(double gpa10)
        {
            if (gpa10 >= 9.0) return 4.0;
            if (gpa10 >= 8.5) return 3.7;
            if (gpa10 >= 8.0) return 3.5;
            if (gpa10 >= 7.5) return 3.0;
            if (gpa10 >= 7.0) return 2.5;
            if (gpa10 >= 6.5) return 2.0;
            if (gpa10 >= 6.0) return 1.5;
            if (gpa10 >= 5.0) return 1.0;
            return 0.0;
        }

        public static string GetRank(double gpa10)
        {
            if (gpa10 >= 9.0) return "Xuất sắc";
            if (gpa10 >= 8.0) return "Giỏi";
            if (gpa10 >= 6.5) return "Khá";
            if (gpa10 >= 5.0) return "Trung bình";
            return "Yếu";
        }

        public static DataTable GetSummaryByStudent(int mssv)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT se.SemName AS [Học kỳ], COUNT(vs.ClassID) AS [Số môn], " +
                    "SUM(co.Credits) AS [Tổng TC], " +
                    "ROUND(SUM(vs.TotalScore*co.Credits)/NULLIF(SUM(co.Credits),0),2) AS [GPA thang 10] " +
                    "FROM v_ScoreSummary vs " +
                    "JOIN ClassRoom cr ON vs.ClassID=cr.ClassID " +
                    "JOIN Course    co ON cr.CourseID=co.CourseID " +
                    "JOIN Semester  se ON cr.SemesterID=se.SemesterID " +
                    "WHERE vs.MSSV=@m " +
                    "GROUP BY se.SemesterID, se.SemName, se.StartDate " +
                    "ORDER BY se.StartDate", db.conn);
                cmd.Parameters.AddWithValue("@m", mssv);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static DataTable GetDashboardStats()
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                new SqlDataAdapter(
                    "SELECT d.DeptName AS [Khoa], " +
                    "COUNT(DISTINCT s.MSSV) AS [Số SV], " +
                    "ROUND(AVG(vs.TotalScore),2) AS [GPA TB] " +
                    "FROM Department d " +
                    "LEFT JOIN Student s ON s.DeptID=d.DeptID " +
                    "LEFT JOIN v_ScoreSummary vs ON vs.MSSV=s.MSSV " +
                    "GROUP BY d.DeptID, d.DeptName " +
                    "ORDER BY d.DeptName", db.conn).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }
    }
}
