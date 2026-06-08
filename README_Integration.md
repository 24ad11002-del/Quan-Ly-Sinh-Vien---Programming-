# QLSV – Hướng dẫn tích hợp tất cả Phases
> Đọc kỹ trước khi chạy. Thứ tự QUAN TRỌNG.

---

## 🗄 BƯỚC 1 – Chạy SQL (theo thứ tự)

```
1. Phase1/SQL/Phase1_Setup.sql        ← Khoa, HệĐT, HọcKỳ, LớpMH, LoginLog
2. Phase2/SQL/Phase2_Setup.sql        ← Post, Comment, Tài liệu, Điểm TP, Notification
3. Phase3/SQL/Phase3_Phase4_Setup.sql ← Request, Book, AuditLog, TrainingScore, ProfileFieldConfig
```

**Chạy trong SSMS:** File → Open → chọn file → F5

---

## 📦 BƯỚC 2 – Cài NuGet Packages

Mở Package Manager Console (Tools → NuGet → PMC):

```powershell
Install-Package ClosedXML          # Export/Import Excel
Install-Package iTextSharp         # Xuất PDF (nếu cần)
```

---

## 🗂 BƯỚC 3 – Thêm file vào project Visual Studio

Kéo thả vào project theo cấu trúc:

```
QLSV_Final/
├── Infrastructure/
│   ├── AppSession_Upgrade.cs    ← Merge vào AppSession.cs hiện có
│   └── Phase4_Helpers.cs        ← BackupHelper, ExcelHelper, AuditHelper, LoginHelper
├── Models/
│   ├── Department.cs            ← Phase 1
│   ├── Semester.cs              ← Phase 1
│   ├── ClassRoom.cs             ← Phase 1
│   ├── Phase2_Models.cs         ← Enrollment, Post, ScoreComponent
│   └── Phase3_Phase4_Models.cs  ← Notification, Book, Request, GpaCalculator
└── Forms/
    ├── UC_Department.cs         ← Phase 1
    ├── UC_Semester.cs           ← Phase 1
    ├── UC_ClassRoom.cs          ← Phase 1
    ├── UC_Enrollment.cs         ← Phase 2
    ├── UC_ClassDetail.cs        ← Phase 2
    ├── Phase3_Phase4_Forms.cs   ← UC_Notification, UC_Library, UC_Request, UC_Dashboard
    └── f_ChangePasswordFirst.cs ← Phase 4 (trong Phase4_Helpers.cs)
```

---

## 🔧 BƯỚC 4 – Merge AppSession

Mở `Infrastructure/AppSession.cs` hiện có, thêm vào class:

```csharp
public static bool IsAdmin      => CurrentRole == "Admin";
public static bool IsGiaoVien   => CurrentRole == "GiaoVien";
public static bool IsSinhVien   => CurrentRole == "SinhVien";
public static string CurrentEmail     { get; set; }
public static int    CurrentAccountID { get; set; }
public static int    CurrentMSSV      { get; set; }
public static int    CurrentSemesterID { get; set; } = -1;
```

---

## 🔧 BƯỚC 5 – Thêm vào f_Login.cs (sau khi login thành công)

```csharp
// Sau khi xác thực thành công, thêm:
AppSession.CurrentAccountID = accountID;
AppSession.CurrentEmail     = email;
AppSession.CurrentMSSV      = mssv; // nếu là SinhVien
AppSession.CurrentSemesterID = Semester.GetActiveSemesterID();

// Ghi log đăng nhập
LoginHelper.LogLogin(accountID, true);

// Kiểm tra lần đầu đăng nhập
if (LoginHelper.IsFirstLogin(accountID))
{
    new f_ChangePasswordFirst(accountID).ShowDialog();
    return;
}
```

---

## 🔧 BƯỚC 6 – Thêm menu vào f_Main.cs

```csharp
// Trong f_Main.cs, thêm các nút/tab mới:

// Phase 1
btnDepartment.Click += (s,e) => ShowUC(new UC_Department());
btnSemester.Click   += (s,e) => ShowUC(new UC_Semester());
btnClassRoom.Click  += (s,e) => ShowUC(new UC_ClassRoom());

// Phase 2
btnEnrollment.Click += (s,e) => {
    var uc = new UC_Enrollment();
    uc.LoadData(AppSession.CurrentMSSV);
    ShowUC(uc);
};

// Phase 3
btnNotification.Click += (s,e) => {
    var uc = new UC_Notification();
    uc.LoadData(AppSession.CurrentAccountID, AppSession.CurrentRole);
    ShowUC(uc);
};
btnLibrary.Click += (s,e) => {
    var uc = new UC_Library();
    uc.LoadData(AppSession.CurrentMSSV);
    ShowUC(uc);
};
btnRequest.Click += (s,e) => {
    var uc = new UC_Request();
    uc.LoadData(AppSession.CurrentMSSV, AppSession.CurrentRole);
    ShowUC(uc);
};

// Phase 4 (Admin only)
if (AppSession.IsAdmin)
{
    btnDashboard.Click += (s,e) => {
        var uc = new UC_Dashboard();
        uc.LoadData();
        ShowUC(uc);
    };
    btnBackup.Click += (s,e) => BackupHelper.Backup();
    btnAuditLog.Click += (s,e) => {
        // Hiển thị AuditLog dạng grid
        var dt = AuditHelper.GetLogs();
        // Tạo form đơn giản hoặc hiện trong dgv có sẵn
    };
}
```

---

## 📋 BƯỚC 7 – Tạo Designer cho mỗi UC

Mỗi UserControl cần file `.Designer.cs`. Cách nhanh nhất:
1. Nhấp phải vào tên UC trong Solution Explorer
2. **View Designer**
3. Kéo thả controls từ Toolbox theo tên biến trong code
4. Tên control phải khớp với tên dùng trong code `.cs`

### Controls cần tạo theo từng UC:

| UC | Controls cần có |
|----|----------------|
| UC_Department | dgvDept, dgvSys, txbDeptCode, txbDeptName, txbDeptNote, txbDeptSearch, lblDeptTotal, txbSysCode, txbSysName, btnDeptNew, btnDeptSave, btnDeptDelete, btnSysNew, btnSysSave, btnSysDelete |
| UC_Semester | dgv, txbCode, txbName, txbYear, dtpStart, dtpEnd, chkRegOpen, chkActive, lblTotal, btnNew, btnSave, btnDelete, btnToggleReg, btnSetActive |
| UC_ClassRoom | dgv, txbSearch, cbSemFilter, cbSemForm, cbCourse, cbStatus, txbClassCode, txbTeacher, txbTeacherEmail, txbRoom, txbNote, nudMaxSlot, nudStartPeriod, nudNumPeriod, nudDay, lblTotal, pnlForm, btnNew, btnSave, btnDelete, btnDetail |
| UC_Enrollment | dgvMy, dgvAvail, txbSearch, cbSem, pnlTimetable, toolTip, lblMyCount, lblCredits, lblAvail, btnRegister, btnCancel, btnRefresh |
| UC_Notification | dgv, rtbContent, rtbNew, txbTitle, txbSearch, cbType, cbTargetRole, cbFilterType, lblUnread, lblDetailTitle, lblDetailType, pnlForm, btnSend, btnDelete |

---

## ⚠️ Lưu ý quan trọng

1. **ClosedXML** cần .NET Framework 4.6+ hoặc .NET 5+
2. **MakeupDay** tên thư mục `Folder` trùng với keyword C# — đặt tên bảng hoặc namespace cẩn thận
3. **UC_ClassDetail** dùng `Microsoft.VisualBasic.Interaction.InputBox` — cần thêm reference `Microsoft.VisualBasic`
4. **BackupHelper.Restore** yêu cầu SQL Server instance cho phép đường dẫn file — test trên local trước
5. **ClassDocs** folder lưu file tài liệu tạo tự động trong `Application.StartupPath`

---

## 🗺 Tóm tắt files đã tạo

| Phase | SQL | Models | Forms |
|-------|-----|--------|-------|
| Phase 1 | Phase1_Setup.sql | Department.cs, Semester.cs, ClassRoom.cs | UC_Department.cs, UC_Semester.cs, UC_ClassRoom.cs |
| Phase 2 | Phase2_Setup.sql | Phase2_Models.cs | UC_Enrollment.cs, UC_ClassDetail.cs |
| Phase 3+4 | Phase3_Phase4_Setup.sql | Phase3_Phase4_Models.cs | Phase3_Phase4_Forms.cs |
| Infra | — | — | Phase4_Helpers.cs (ExcelHelper, BackupHelper, AuditHelper, LoginHelper) |
