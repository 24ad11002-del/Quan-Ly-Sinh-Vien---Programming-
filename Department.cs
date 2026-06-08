// ============================================================
//  Department.cs – Model Khoa & Hệ đào tạo
// ============================================================
using System.Data;
using System.Data.SqlClient;

namespace QLSV
{
    public class Department
    {
        public int    DeptID   { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string Note     { get; set; }

        public bool Add()
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO Department(DeptCode,DeptName,Note) VALUES(@c,@n,@no)", db.conn);
                cmd.Parameters.AddWithValue("@c",  DeptCode);
                cmd.Parameters.AddWithValue("@n",  DeptName);
                cmd.Parameters.AddWithValue("@no", (object)Note ?? System.DBNull.Value);
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
                    "UPDATE Department SET DeptCode=@c,DeptName=@n,Note=@no WHERE DeptID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", DeptID);
                cmd.Parameters.AddWithValue("@c",  DeptCode);
                cmd.Parameters.AddWithValue("@n",  DeptName);
                cmd.Parameters.AddWithValue("@no", (object)Note ?? System.DBNull.Value);
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
                var cmd = new SqlCommand("DELETE FROM Department WHERE DeptID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetAll(string keyword = "")
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT d.DeptID, d.DeptCode, d.DeptName, d.Note, " +
                    "COUNT(s.MSSV) AS SoSinhVien " +
                    "FROM Department d " +
                    "LEFT JOIN Student s ON s.DeptID = d.DeptID " +
                    "WHERE @kw='' OR d.DeptName LIKE @kw OR d.DeptCode LIKE @kw " +
                    "GROUP BY d.DeptID, d.DeptCode, d.DeptName, d.Note " +
                    "ORDER BY d.DeptCode", db.conn);
                cmd.Parameters.AddWithValue("@kw",
                    string.IsNullOrWhiteSpace(keyword) ? "" : "%" + keyword + "%");
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static DataTable GetAllSystems()
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "SELECT ts.SysID, ts.SysCode, ts.SysName, COUNT(s.MSSV) AS SoSinhVien " +
                    "FROM TrainingSystem ts " +
                    "LEFT JOIN Student s ON s.SysID = ts.SysID " +
                    "GROUP BY ts.SysID, ts.SysCode, ts.SysName " +
                    "ORDER BY ts.SysCode", db.conn);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }

        public static bool AddSystem(string code, string name)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "INSERT INTO TrainingSystem(SysCode,SysName) VALUES(@c,@n)", db.conn);
                cmd.Parameters.AddWithValue("@c", code);
                cmd.Parameters.AddWithValue("@n", name);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool UpdateSystem(int id, string code, string name)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand(
                    "UPDATE TrainingSystem SET SysCode=@c,SysName=@n WHERE SysID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@c",  code);
                cmd.Parameters.AddWithValue("@n",  name);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static bool DeleteSystem(int id)
        {
            My_DB db = new My_DB();
            try
            {
                db.openConnection();
                var cmd = new SqlCommand("DELETE FROM TrainingSystem WHERE SysID=@id", db.conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
            finally { db.closeConnection(); }
        }

        public static DataTable GetComboSource(string table = "Department")
        {
            My_DB db = new My_DB();
            DataTable dt = new DataTable();
            try
            {
                db.openConnection();
                string q = table == "Department"
                    ? "SELECT DeptID AS ID, DeptName AS Name FROM Department ORDER BY DeptName"
                    : "SELECT SysID  AS ID, SysName  AS Name FROM TrainingSystem ORDER BY SysName";
                new SqlDataAdapter(new SqlCommand(q, db.conn)).Fill(dt);
            }
            finally { db.closeConnection(); }
            return dt;
        }
    }
}
