// ============================================================
//  Enrollment.cs – Model Đăng ký học phần
// ============================================================
using System;
using System.Data;
using System.Data.SqlClient;

namespace QLSV
{
    public class Enrollment
    {
        public int    EnrollID   { get; set; }
        public int    MSSV       { get; set; }
        public int    ClassID    { get; set; }
        public DateTime EnrollTime { get; set; }
        public string Status     { get; set; }

        public static bool Register(int mssv, int classID, out string error)
        {
            error = "";
            My_DB db = new My_DB();
            try
            {
                db.openConnection();

                // 1. Học kỳ có mở đăng ký không?
                var chkSem = new SqlCommand(
                    "SELECT COUNT(*) FROM ClassRoom cr JOIN Semester se ON cr.SemesterID=se.SemesterID " +
                    "WHERE cr.ClassID=@cid AND se.IsRegOpen=1", db.conn);
                chkSem.Parameters.AddWithValue("@cid", classID);
                if ((int)chkSem.ExecuteScalar() == 0) { error = "Học kỳ chưa mở đăng ký."; return false; }

                // 2. Còn chỗ không?
                var chkSlot = new SqlCommand(
                    "SELECT cr.MaxSlot - COUNT(en.EnrollID) FROM ClassRoom cr " +
                    "LEFT JOIN Enrollment en ON en.ClassID=cr.ClassID AND en.Status=N'Đã đăng ký' " +
                    "WHERE cr.ClassID=@cid GROUP BY cr.MaxSlot", db.conn);
                chkSlot.Parameters.AddWithValue("@cid", classID);
                object slotObj = chkSlot.ExecuteScalar();
                if (slotObj == null || (int)slotObj <= 0) { error = "Lớp đã đủ sĩ số."; return false; }

                // 3. Đã đăng ký chưa?
                var chkDup = new SqlCommand(
                    "SELECT COUNT(*) FROM Enrollment WHERE MSSV=@m AND ClassID=@cid AND Status=N'Đã đăng ký'", db.conn);
                chkDup.Parameters.AddWithValue("@m", mssv);
                chkDup.Parameters.AddWithValue("@cid", classID);
                if ((int)chkDup.ExecuteScalar() > 0) { error = "Bạn đã đăng ký lớp này rồi."; return false; }

                // 4. Trùng giờ không?
                var chkTime = new SqlCommand(
                    "SELECT COUNT(*) FROM Enrollment en " +
                    "JOIN ClassRoom a ON en.ClassID=a.ClassID " +
                    "JOIN ClassRoom b ON b.ClassID=@cid " +
                    "WHERE en.MSSV=@m AND en.Status=N'Đã đăng ký' " +
                    "AND a.SemesterID=b.SemesterID " +
                    "AND a.DayOfWeek=b.DayOfWeek " +
                    "AND a.StartPeriod < b.StartPeriod+b.NumPeriod " +
                    "AND b.StartPeriod < a.StartPeriod+a.NumPeriod", db.conn);
                chkTime.Parameters.AddWithValue("@m",   mssv);
                chkTime.Parameters.AddWithValue("@cid", classID);
                if ((int)chkTime.ExecuteScalar() > 0) { error = "Trùng giờ với lớp đã đăng ký!"; return false; }

                // 5. Thêm đăng ký
                var ins = new SqlCommand(
                    "INSERT INTO Enrollment(MSSV,ClassID,EnrollTime,Status) VALUES(@m,@cid,GETDATE(),N'Đã đăng ký')", db.conn);
                ins.Parameters.AddWithValue("@m",   mssv);
                ins.Parameters.AddWithValue("@cid", classID);
                return ins.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { error = ex.Message; return false; }
            finally { db.closeConnection(); }
        }

        public static bool Cancel(int mssv, int classID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "UPDATE Enrollment SET Status=N'Đã hủy' WHERE MSSV=@m AND ClassID=@cid", db.conn);
                cmd.Parameters.AddWithValue("@m",   mssv);
                cmd.Parameters.AddWithValue("@cid", classID);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetByStudent(int mssv, int semesterID = -1)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT en.EnrollID, cr.ClassCode, co.CourseName AS [Môn học], " +
                    "co.Credits AS [TC], cr.TeacherName AS [Giảng viên], " +
                    "cr.Room AS [Phòng], cr.DayOfWeek, cr.StartPeriod, cr.NumPeriod, " +
                    "se.SemName AS [Học kỳ], en.Status, en.EnrollTime AS [Ngày ĐK] " +
                    "FROM Enrollment en " +
                    "JOIN ClassRoom cr ON en.ClassID   = cr.ClassID " +
                    "JOIN Course    co ON cr.CourseID   = co.CourseID " +
                    "JOIN Semester  se ON cr.SemesterID = se.SemesterID " +
                    "WHERE en.MSSV=@m AND (@sid=-1 OR cr.SemesterID=@sid) " +
                    "ORDER BY cr.DayOfWeek, cr.StartPeriod", db.conn);
                cmd.Parameters.AddWithValue("@m",   mssv);
                cmd.Parameters.AddWithValue("@sid", semesterID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static DataTable GetAvailableClasses(int mssv, int semesterID, string keyword = "")
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT cr.ClassID, cr.ClassCode, co.CourseName AS [Môn học], co.Credits AS [TC], " +
                    "cr.TeacherName AS [Giảng viên], cr.Room AS [Phòng], " +
                    "cr.MaxSlot AS [Sĩ số tối đa], cr.DayOfWeek, cr.StartPeriod, cr.NumPeriod, " +
                    "(SELECT COUNT(*) FROM Enrollment e WHERE e.ClassID=cr.ClassID AND e.Status=N'Đã đăng ký') AS [Đã đăng ký] " +
                    "FROM ClassRoom cr " +
                    "JOIN Course   co ON cr.CourseID   = co.CourseID " +
                    "JOIN Semester se ON cr.SemesterID = se.SemesterID " +
                    "WHERE cr.SemesterID=@sid AND se.IsRegOpen=1 " +
                    "AND cr.ClassID NOT IN (SELECT ClassID FROM Enrollment WHERE MSSV=@m AND Status=N'Đã đăng ký') " +
                    "AND (@kw='' OR cr.ClassCode LIKE @kw OR co.CourseName LIKE @kw) " +
                    "ORDER BY co.CourseName", db.conn);
                cmd.Parameters.AddWithValue("@sid", semesterID);
                cmd.Parameters.AddWithValue("@m",   mssv);
                cmd.Parameters.AddWithValue("@kw",
                    string.IsNullOrWhiteSpace(keyword) ? "" : "%" + keyword + "%");
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }
    }

    // ============================================================
    //  Post.cs – Model Bảng tin lớp học
    // ============================================================
    public class Post
    {
        public int    PostID    { get; set; }
        public int    ClassID   { get; set; }
        public int    AuthorID  { get; set; }
        public string Title     { get; set; }
        public string Content   { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool   IsPinned  { get; set; }

        public bool Add()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO Post(ClassID,AuthorID,Title,Content,ImageData,IsPinned,CreatedAt) " +
                    "VALUES(@cid,@aid,@t,@con,@img,@pin,GETDATE())", db.conn);
                cmd.Parameters.AddWithValue("@cid", ClassID);
                cmd.Parameters.AddWithValue("@aid", AuthorID);
                cmd.Parameters.AddWithValue("@t",   Title);
                cmd.Parameters.AddWithValue("@con", Content);
                cmd.Parameters.Add("@img", SqlDbType.VarBinary).Value = (object)ImageData ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@pin", IsPinned);
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
                    "UPDATE Post SET Title=@t,Content=@con,ImageData=@img,IsPinned=@pin,UpdatedAt=GETDATE() " +
                    "WHERE PostID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id",  PostID);
                cmd.Parameters.AddWithValue("@t",   Title);
                cmd.Parameters.AddWithValue("@con", Content);
                cmd.Parameters.Add("@img", SqlDbType.VarBinary).Value = (object)ImageData ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@pin", IsPinned);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool Delete(int postID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("DELETE FROM Post WHERE PostID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", postID);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetByClass(int classID)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT p.PostID, p.Title, p.Content, p.ImageData, p.IsPinned, " +
                    "p.CreatedAt, p.UpdatedAt, a.Username AS AuthorName, " +
                    "(SELECT COUNT(*) FROM Comment c WHERE c.PostID=p.PostID) AS CmtCount " +
                    "FROM Post p JOIN Account a ON p.AuthorID=a.ID " +
                    "WHERE p.ClassID=@cid " +
                    "ORDER BY p.IsPinned DESC, p.CreatedAt DESC", db.conn);
                cmd.Parameters.AddWithValue("@cid", classID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static bool AddComment(int postID, int authorID, string content)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO Comment(PostID,AuthorID,Content,CreatedAt) VALUES(@pid,@aid,@con,GETDATE())", db.conn);
                cmd.Parameters.AddWithValue("@pid", postID);
                cmd.Parameters.AddWithValue("@aid", authorID);
                cmd.Parameters.AddWithValue("@con", content);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetComments(int postID)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT c.CmtID, c.Content, c.CreatedAt, a.Username AS Author " +
                    "FROM Comment c JOIN Account a ON c.AuthorID=a.ID " +
                    "WHERE c.PostID=@pid ORDER BY c.CreatedAt", db.conn);
                cmd.Parameters.AddWithValue("@pid", postID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }
    }

    // ============================================================
    //  ScoreComponent.cs – Model Điểm thành phần
    // ============================================================
    public class ScoreComponent
    {
        public int    CompID    { get; set; }
        public int    ClassID   { get; set; }
        public string CompName  { get; set; }
        public double Weight    { get; set; }
        public int    CompOrder { get; set; }

        public bool Add()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO ScoreComponent(ClassID,CompName,Weight,CompOrder) VALUES(@cid,@n,@w,@o)", db.conn);
                cmd.Parameters.AddWithValue("@cid", ClassID);
                cmd.Parameters.AddWithValue("@n",   CompName);
                cmd.Parameters.AddWithValue("@w",   Weight);
                cmd.Parameters.AddWithValue("@o",   CompOrder);
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
                    "UPDATE ScoreComponent SET CompName=@n,Weight=@w,CompOrder=@o WHERE CompID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", CompID);
                cmd.Parameters.AddWithValue("@n",  CompName);
                cmd.Parameters.AddWithValue("@w",  Weight);
                cmd.Parameters.AddWithValue("@o",  CompOrder);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool Delete(int compID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("DELETE FROM ScoreComponent WHERE CompID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", compID);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetByClass(int classID)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT CompID, CompName, Weight, CompOrder FROM ScoreComponent " +
                    "WHERE ClassID=@cid ORDER BY CompOrder", db.conn);
                cmd.Parameters.AddWithValue("@cid", classID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static bool SetScore(int compID, int mssv, double score)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "IF EXISTS(SELECT 1 FROM ScoreDetail WHERE CompID=@cid AND MSSV=@m) " +
                    "    UPDATE ScoreDetail SET Score=@s WHERE CompID=@cid AND MSSV=@m " +
                    "ELSE " +
                    "    INSERT INTO ScoreDetail(CompID,MSSV,Score) VALUES(@cid,@m,@s)", db.conn);
                cmd.Parameters.AddWithValue("@cid", compID);
                cmd.Parameters.AddWithValue("@m",   mssv);
                cmd.Parameters.AddWithValue("@s",   score);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetScoreSheet(int classID)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT s.MSSV, s.Fname+' '+s.Lname AS [Họ tên], " +
                    "sc.CompName, sc.Weight, " +
                    "ISNULL(sd.Score, 0) AS [Điểm], " +
                    "sc.CompID " +
                    "FROM Enrollment en " +
                    "JOIN Student s ON en.MSSV = s.MSSV " +
                    "CROSS JOIN ScoreComponent sc " +
                    "LEFT JOIN ScoreDetail sd ON sd.CompID=sc.CompID AND sd.MSSV=en.MSSV " +
                    "WHERE en.ClassID=@cid AND sc.ClassID=@cid AND en.Status=N'Đã đăng ký' " +
                    "ORDER BY s.Lname, s.Fname, sc.CompOrder", db.conn);
                cmd.Parameters.AddWithValue("@cid", classID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static double GetTotalScore(int classID, int mssv)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(sd.Score * sc.Weight / 100.0), 0) " +
                    "FROM ScoreDetail sd " +
                    "JOIN ScoreComponent sc ON sd.CompID=sc.CompID " +
                    "WHERE sc.ClassID=@cid AND sd.MSSV=@m", db.conn);
                cmd.Parameters.AddWithValue("@cid", classID);
                cmd.Parameters.AddWithValue("@m",   mssv);
                return Convert.ToDouble(cmd.ExecuteScalar());
            }
            finally { db.closeConnection(); }
        }
    }
}
