// ============================================================
//  UC_Department.cs – Quản lý Khoa & Hệ đào tạo
// ============================================================
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLSV
{
    public partial class UC_Department : UserControl
    {
        private int  _selDeptID = -1;
        private int  _selSysID  = -1;
        private bool _tabDept   = true;

        public UC_Department() { InitializeComponent(); }

        public void LoadData() { LoadDepts(); LoadSystems(); }

        private void LoadDepts(string kw = "")
        {
            DataTable dt = Department.GetAll(kw);
            dgvDept.DataSource = dt;
            dgvDept.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDept.RowHeadersVisible   = false;
            dgvDept.ReadOnly            = true;
            dgvDept.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            if (dgvDept.Columns.Contains("DeptID")) dgvDept.Columns["DeptID"].Visible = false;
            if (dgvDept.Columns.Contains("DeptCode")) dgvDept.Columns["DeptCode"].HeaderText = "Mã khoa";
            if (dgvDept.Columns.Contains("DeptName")) dgvDept.Columns["DeptName"].HeaderText = "Tên khoa";
            if (dgvDept.Columns.Contains("Note"))     dgvDept.Columns["Note"].HeaderText     = "Ghi chú";
            if (dgvDept.Columns.Contains("SoSinhVien")) dgvDept.Columns["SoSinhVien"].HeaderText = "Số SV";
            lblDeptTotal.Text = $"Tổng: {dt.Rows.Count} khoa";
        }

        private void LoadSystems()
        {
            DataTable dt = Department.GetAllSystems();
            dgvSys.DataSource = dt;
            dgvSys.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSys.RowHeadersVisible   = false;
            dgvSys.ReadOnly            = true;
            dgvSys.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            if (dgvSys.Columns.Contains("SysID"))   dgvSys.Columns["SysID"].Visible    = false;
            if (dgvSys.Columns.Contains("SysCode")) dgvSys.Columns["SysCode"].HeaderText = "Mã hệ";
            if (dgvSys.Columns.Contains("SysName")) dgvSys.Columns["SysName"].HeaderText = "Tên hệ đào tạo";
            if (dgvSys.Columns.Contains("SoSinhVien")) dgvSys.Columns["SoSinhVien"].HeaderText = "Số SV";
        }

        // ── KHOA ─────────────────────────────────────────────────
        private void dgvDept_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvDept.Rows[e.RowIndex];
            _selDeptID = Convert.ToInt32(row.Cells["DeptID"].Value);
            txbDeptCode.Text = row.Cells["DeptCode"].Value?.ToString();
            txbDeptName.Text = row.Cells["DeptName"].Value?.ToString();
            txbDeptNote.Text = row.Cells["Note"].Value?.ToString();
            btnDeptDelete.Enabled = true;
        }

        private void btnDeptNew_Click(object sender, EventArgs e)
        {
            _selDeptID = -1;
            txbDeptCode.Clear(); txbDeptName.Clear(); txbDeptNote.Clear();
            txbDeptCode.Focus();
            btnDeptDelete.Enabled = false;
        }

        private void btnDeptSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbDeptCode.Text) || string.IsNullOrWhiteSpace(txbDeptName.Text))
            { MessageBox.Show("Vui lòng nhập mã và tên khoa!", "⚠️"); return; }

            var dept = new Department
            {
                DeptID   = _selDeptID,
                DeptCode = txbDeptCode.Text.Trim().ToUpper(),
                DeptName = txbDeptName.Text.Trim(),
                Note     = txbDeptNote.Text.Trim()
            };

            bool ok = _selDeptID == -1 ? dept.Add() : dept.Update();
            if (ok)
            {
                AuditHelper.Log(_selDeptID == -1 ? "INSERT" : "UPDATE", "Department",
                                dept.DeptCode, null, dept.DeptName);
                MessageBox.Show("Lưu thành công!", "✅", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnDeptNew_Click(null, null);
                LoadDepts();
            }
            else MessageBox.Show("Lưu thất bại!", "❌", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnDeptDelete_Click(object sender, EventArgs e)
        {
            if (_selDeptID == -1) return;
            if (MessageBox.Show("Xóa khoa này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            if (Department.Delete(_selDeptID))
            {
                AuditHelper.Log("DELETE", "Department", _selDeptID.ToString());
                MessageBox.Show("Đã xóa!", "✅", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnDeptNew_Click(null, null);
                LoadDepts();
            }
            else MessageBox.Show("Không thể xóa (còn sinh viên liên kết)!", "❌", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void txbDeptSearch_TextChanged(object sender, EventArgs e)
            => LoadDepts(txbDeptSearch.Text.Trim());

        // ── HỆ ĐÀO TẠO ───────────────────────────────────────────
        private void dgvSys_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvSys.Rows[e.RowIndex];
            _selSysID      = Convert.ToInt32(row.Cells["SysID"].Value);
            txbSysCode.Text = row.Cells["SysCode"].Value?.ToString();
            txbSysName.Text = row.Cells["SysName"].Value?.ToString();
            btnSysDelete.Enabled = true;
        }

        private void btnSysNew_Click(object sender, EventArgs e)
        {
            _selSysID = -1;
            txbSysCode.Clear(); txbSysName.Clear();
            btnSysDelete.Enabled = false;
        }

        private void btnSysSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbSysCode.Text) || string.IsNullOrWhiteSpace(txbSysName.Text))
            { MessageBox.Show("Vui lòng nhập mã và tên hệ!", "⚠️"); return; }

            bool ok = _selSysID == -1
                ? Department.AddSystem(txbSysCode.Text.Trim(), txbSysName.Text.Trim())
                : Department.UpdateSystem(_selSysID, txbSysCode.Text.Trim(), txbSysName.Text.Trim());

            if (ok)
            {
                MessageBox.Show("Lưu thành công!", "✅", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSysNew_Click(null, null);
                LoadSystems();
            }
            else MessageBox.Show("Lưu thất bại!", "❌", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnSysDelete_Click(object sender, EventArgs e)
        {
            if (_selSysID == -1) return;
            if (MessageBox.Show("Xóa hệ đào tạo này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            if (Department.DeleteSystem(_selSysID))
            { MessageBox.Show("Đã xóa!", "✅"); btnSysNew_Click(null, null); LoadSystems(); }
            else MessageBox.Show("Không thể xóa!", "❌", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
