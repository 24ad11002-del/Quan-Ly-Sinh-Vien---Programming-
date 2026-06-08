// ============================================================
//  ClassRoom.cs – Model Lớp môn học
// ============================================================
using System;
using System.Data;
using System.Data.SqlClient;

namespace QLSV
{
    public class ClassRoom
    {
        public int    ClassID      { get; set; }
        public string ClassCode    { get; set; }
        public int    SemesterID   { get; set; }
        public string CourseID     { get; set; }
        public string TeacherName  { get; set; }
        public string TeacherEmail { get; set; }
        public int    MaxSlot      { get; set; }
        public string Room         { get; set; }
        public int    DayOfWeek    { get; set; }
        public int    StartPeriod  { get; set; }
        public int    NumPeriod    { get; set; }
        public string Status       { get; set; }
        public string Note         { get; set; }

        public static readonly string[] DAY_NAMES =
            { "", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };

        public string DayName =>
            (DayOfWeek >= 2 && DayOfWeek <= 8) ? DAY_NAMES[DayOfWeek - 1] : "?";

        public string Schedule =>
            $"{DayName}, Tiết {StartPeriod}–{StartPeriod + NumPeriod - 1}";

        public bool Add()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO ClassRoom(ClassCode,SemesterID,CourseID,TeacherName,TeacherEmail," +
                    "MaxSlot,Room,DayOfWeek,StartPeriod,NumPeriod,Status,Note) " +
                    "VALUES(@cc,@sid,@cid,@tn,@te,@ms,@rm,@dow,@sp,@np,@st,@no)", db.conn);
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
                    "UPDATE ClassRoom SET ClassCode=@cc,SemesterID=@sid,CourseID=@cid," +
                    "TeacherName=@tn,TeacherEmail=@te,MaxSlot=@ms,Room=@rm," +
                    "DayOfWeek=@dow,StartPeriod=@sp,NumPeriod=@np,Status=@st,Note=@no " +
                    "WHERE ClassID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", ClassID);
                Bind(cmd);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        private void Bind(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@cc",  ClassCode);
            cmd.Parameters.AddWithValue("@sid", SemesterID);
            cmd.Parameters.AddWithValue("@cid", CourseID);
            cmd.Parameters.AddWithValue("@tn",  TeacherName);
            cmd.Parameters.AddWithValue("@te",  (object)TeacherEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ms",  MaxSlot);
            cmd.Parameters.AddWithValue("@rm",  (object)Room ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dow", DayOfWeek);
            cmd.Parameters.AddWithValue("@sp",  StartPeriod);
            cmd.Parameters.AddWithValue("@np",  NumPeriod);
            cmd.Parameters.AddWithValue("@st",  Status ?? "Đang mở");
            cmd.Parameters.AddWithValue("@no",  (object)Note ?? DBNull.Value);
        }

        public static bool Delete(int classID)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("DELETE FROM ClassRoom WHERE ClassID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", classID);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetAll(string keyword = "", int semesterID = -1)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT cr.ClassID, cr.ClassCode, se.SemName AS [Học kỳ], " +
                    "co.CourseName AS [Môn học], co.CourseID AS [Mã môn], " +
                    "cr.TeacherName AS [Giảng viên], cr.Room AS [Phòng], " +
                    "cr.MaxSlot AS [Sĩ số tối đa], " +
                    "(SELECT COUNT(*) FROM Enrollment e WHERE e.ClassID=cr.ClassID AND e.Status=N'Đã đăng ký') AS [Đã đăng ký], " +
                    "cr.DayOfWeek, cr.StartPeriod, cr.NumPeriod, cr.Status AS [Trạng thái] " +
                    "FROM ClassRoom cr " +
                    "JOIN Semester se ON cr.SemesterID = se.SemesterID " +
                    "JOIN Course   co ON cr.CourseID   = co.CourseID " +
                    "WHERE (@sid=-1 OR cr.SemesterID=@sid) " +
                    "AND (@kw='' OR cr.ClassCode LIKE @kw OR cr.TeacherName LIKE @kw OR co.CourseName LIKE @kw) " +
                    "ORDER BY se.StartDate DESC, cr.ClassCode", db.conn);
                cmd.Parameters.AddWithValue("@sid", semesterID);
                cmd.Parameters.AddWithValue("@kw",
                    string.IsNullOrWhiteSpace(keyword) ? "" : "%" + keyword + "%");
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static DataTable GetForTeacher(string teacherEmail, int semesterID = -1)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT cr.ClassID, cr.ClassCode, co.CourseName AS [Môn học], " +
                    "se.SemName AS [Học kỳ], cr.Room AS [Phòng], cr.MaxSlot AS [Sĩ số], " +
                    "cr.DayOfWeek, cr.StartPeriod, cr.NumPeriod, cr.Status " +
                    "FROM ClassRoom cr " +
                    "JOIN Semester se ON cr.SemesterID = se.SemesterID " +
                    "JOIN Course   co ON cr.CourseID   = co.CourseID " +
                    "WHERE cr.TeacherEmail=@te " +
                    "AND (@sid=-1 OR cr.SemesterID=@sid) " +
                    "ORDER BY cr.DayOfWeek, cr.StartPeriod", db.conn);
                cmd.Parameters.AddWithValue("@te",  teacherEmail ?? "");
                cmd.Parameters.AddWithValue("@sid", semesterID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static DataTable GetForStudent(int mssv, int semesterID = -1)
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT cr.ClassID, cr.ClassCode, co.CourseName AS [Môn học], " +
                    "cr.TeacherName AS [Giảng viên], se.SemName AS [Học kỳ], " +
                    "cr.Room AS [Phòng], cr.DayOfWeek, cr.StartPeriod, cr.NumPeriod " +
                    "FROM Enrollment en " +
                    "JOIN ClassRoom cr ON en.ClassID   = cr.ClassID " +
                    "JOIN Course    co ON cr.CourseID   = co.CourseID " +
                    "JOIN Semester  se ON cr.SemesterID = se.SemesterID " +
                    "WHERE en.MSSV=@m AND en.Status=N'Đã đăng ký' " +
                    "AND (@sid=-1 OR cr.SemesterID=@sid) " +
                    "ORDER BY cr.DayOfWeek, cr.StartPeriod", db.conn);
                cmd.Parameters.AddWithValue("@m",   mssv);
                cmd.Parameters.AddWithValue("@sid", semesterID);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static bool HasTimeConflict(int mssv, int dayOfWeek, int startPeriod, int numPeriod, int semesterID, int excludeClassID = -1)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Enrollment en " +
                    "JOIN ClassRoom cr ON en.ClassID = cr.ClassID " +
                    "WHERE en.MSSV=@m AND en.Status=N'Đã đăng ký' " +
                    "AND cr.SemesterID=@sid " +
                    "AND cr.DayOfWeek=@dow " +
                    "AND cr.ClassID<>@excl " +
                    "AND cr.StartPeriod < @sp+@np " +
                    "AND @sp < cr.StartPeriod+cr.NumPeriod", db.conn);
                cmd.Parameters.AddWithValue("@m",    mssv);
                cmd.Parameters.AddWithValue("@sid",  semesterID);
                cmd.Parameters.AddWithValue("@dow",  dayOfWeek);
                cmd.Parameters.AddWithValue("@sp",   startPeriod);
                cmd.Parameters.AddWithValue("@np",   numPeriod);
                cmd.Parameters.AddWithValue("@excl", excludeClassID);
                return (int)cmd.ExecuteScalar() > 0;
            }
            finally { db.closeConnection(); }
        }

        public static bool ImportFromExcel(System.Collections.Generic.List<ClassRoom> list, out string errors)
        {
            errors = "";
            int ok = 0, fail = 0;
            foreach (var cr in list)
            {
                if (cr.Add()) ok++;
                else { fail++; errors += $"Lỗi: {cr.ClassCode}\n"; }
            }
            return fail == 0;
        }
    }
}
