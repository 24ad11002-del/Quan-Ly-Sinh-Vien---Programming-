// ============================================================
//  Semester.cs – Model Học kỳ
// ============================================================
using System;
using System.Data;
using System.Data.SqlClient;

namespace QLSV
{
    public class Semester
    {
        public int      SemesterID   { get; set; }
        public string   SemCode      { get; set; }
        public string   SemName      { get; set; }
        public string   AcademicYear { get; set; }
        public DateTime StartDate    { get; set; }
        public DateTime EndDate      { get; set; }
        public bool     IsRegOpen    { get; set; }
        public bool     IsActive     { get; set; }

        public bool Add()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO Semester(SemCode,SemName,AcademicYear,StartDate,EndDate,IsRegOpen,IsActive) " +
                    "VALUES(@c,@n,@ay,@sd,@ed,@ro,@ac)", db.conn);
                cmd.Parameters.AddWithValue("@c",  SemCode);
                cmd.Parameters.AddWithValue("@n",  SemName);
                cmd.Parameters.AddWithValue("@ay", AcademicYear);
                cmd.Parameters.AddWithValue("@sd", StartDate);
                cmd.Parameters.AddWithValue("@ed", EndDate);
                cmd.Parameters.AddWithValue("@ro", IsRegOpen);
                cmd.Parameters.AddWithValue("@ac", IsActive);
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
                    "UPDATE Semester SET SemCode=@c,SemName=@n,AcademicYear=@ay," +
                    "StartDate=@sd,EndDate=@ed,IsRegOpen=@ro,IsActive=@ac WHERE SemesterID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", SemesterID);
                cmd.Parameters.AddWithValue("@c",  SemCode);
                cmd.Parameters.AddWithValue("@n",  SemName);
                cmd.Parameters.AddWithValue("@ay", AcademicYear);
                cmd.Parameters.AddWithValue("@sd", StartDate);
                cmd.Parameters.AddWithValue("@ed", EndDate);
                cmd.Parameters.AddWithValue("@ro", IsRegOpen);
                cmd.Parameters.AddWithValue("@ac", IsActive);
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
                var cmd = new SqlCommand("DELETE FROM Semester WHERE SemesterID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool ToggleRegistration(int id, bool open)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "UPDATE Semester SET IsRegOpen=@ro WHERE SemesterID=@id", db.conn);
                cmd.Parameters.AddWithValue("@ro", open);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool SetActive(int id)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                new SqlCommand("UPDATE Semester SET IsActive=0", db.conn).ExecuteNonQuery();
                var cmd = new SqlCommand("UPDATE Semester SET IsActive=1 WHERE SemesterID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetAll()
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                new SqlDataAdapter(
                    "SELECT SemesterID, SemCode, SemName, AcademicYear, " +
                    "StartDate, EndDate, IsRegOpen, IsActive " +
                    "FROM Semester ORDER BY StartDate DESC", db.conn).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static DataTable GetComboSource()
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                new SqlDataAdapter(
                    "SELECT SemesterID AS ID, SemName AS Name FROM Semester ORDER BY StartDate DESC", db.conn).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static int GetActiveSemesterID()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                object r = new SqlCommand(
                    "SELECT TOP 1 SemesterID FROM Semester WHERE IsActive=1", db.conn).ExecuteScalar();
                return r != null ? (int)r : -1;
            }
            finally { db.closeConnection(); }
        }
    }
}
