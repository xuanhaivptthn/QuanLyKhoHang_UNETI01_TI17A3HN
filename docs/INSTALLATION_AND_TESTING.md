# HƯỚNG DẪN CÀI ĐẶT, KHỞI CHẠY & BỘ KIỂM THỬ TỰ ĐỘNG (xUnit TEST SUITE)

> **Dự án:** Hệ thống Quản Lý Kho Hàng UNETI (ASP.NET Core 10 MVC)  
> **Kỹ sư Phụ trách Kiểm thử:** Nguyễn Việt Dũng (Phụ trách Module 4 & Test Suite)  
> **Trở về:** [Xem README Tổng quan](../README.md)

---

## 1. YÊU CẦU MÔI TRƯỜNG PHÁT TRIỂN & CHẠY ỨNG DỤNG

| Thành phần | Yêu cầu tối thiểu | Phiên bản khuyến nghị |
| :--- | :--- | :--- |
| **Hệ điều hành** | Windows 10/11, Ubuntu 22.04 LTS+, macOS Sonoma+ | Windows 11 hoặc Ubuntu 24.04 LTS |
| **.NET SDK** | `.NET 10.0 Preview / GA` (Khai báo trong [`global.json`](../global.json)) | `.NET 10.0` |
| **Hệ quản trị CSDL** | Microsoft SQL Server (LocalDB / Express / Azure SQL) | Azure SQL Database (Remote) |
| **CSDL Kiểm thử** | SQLite In-Memory (Tự động nạp khi chạy kiểm thử xUnit) | Tích hợp sẵn |
| **IDE / Editor** | Visual Studio 2022 (v17.12+) hoặc Visual Studio Code | VS Code kèm C# Dev Kit |

---

## 2. CÁC GÓI THƯ VIỆN KIỂM THỬ ĐÃ TÍCH HỢP

Dự án kiểm thử [`Tests/QuanLyKhoHang.Tests`](../Tests/QuanLyKhoHang.Tests) được cấu hình các thư viện hiện đại nhất:
- `xunit` (v2.9.3+) - Framework kiểm thử đơn vị & tích hợp chuẩn công nghiệp.
- `xunit.runner.visualstudio` - Trình điều phối chạy test trên Visual Studio Test Explorer và CLI.
- `Microsoft.NET.Test.Sdk` - Bộ SDK thực thi kiểm thử của .NET.
- `Microsoft.AspNetCore.Mvc.Testing` - Khởi tạo Web Host ảo `WebApplicationFactory` phục vụ kiểm thử end-to-end không cần mở port mạng vật lý.
- `Microsoft.EntityFrameworkCore.Sqlite` - Engine CSDL SQLite In-Memory độc lập, cô lập dữ liệu thử nghiệm giữa các test run.
- `FluentAssertions` - Thư viện assertion mượt mà, dễ đọc.

---

## 3. KHỞI CHẠY ỨNG DỤNG WEB (QUICK START)

Mở cửa sổ Terminal tại thư mục gốc của dự án (`QuanLyKhoHang_UNETI01_TI17A3HN`):

```bash
# 1. Khôi phục toàn bộ các gói thư viện NuGet
dotnet restore

# 2. Biên dịch giải pháp
dotnet build

# 3. Khởi chạy ứng dụng Web
dotnet run
```

Sau khi màn hình hiển thị log khởi động, mở trình duyệt truy cập:
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:5001` (nếu cấu hình chứng chỉ dev-certs)

---

## 4. BẢNG TÀI KHOẢN ĐĂNG NHẬP MẪU SẴN CÓ (SEED DATA)

Dữ liệu ban đầu được cấu hình qua Fluent API `OnModelCreating` trong [`AppDbContext.cs`](../Data/AppDbContext.cs). Tất cả mật khẩu đã được băm an toàn bằng `PasswordHasher<TaiKhoan>`:

| STT | Tên đăng nhập | Mật khẩu mặc định | Vai trò (Role) | Họ và tên sinh viên | Ghi chú quyền hạn |
| :---: | :--- | :--- | :---: | :--- | :--- |
| **1** | `admin` | `Admin@123` | **Admin** | **Trần Xuân Hải** | Quản trị viên tối cao: Toàn quyền mọi tính năng, quản trị tài khoản, cấu hình kho. |
| **2** | `nvkho_hung` | `123456` | **NhanVienKho** | **Lê Văn Hùng** | Thủ kho: Lập & duyệt phiếu nhập, quản lý nhà cung cấp, xem tồn & cảnh báo. |
| **3** | `nvkho_cuc` | `123456` | **NhanVienKho** | **Nguyễn Thị Cúc** | Thủ kho: Quản lý hàng hóa, tra cứu & lọc LINQ, xem chi tiết phân bổ tồn. |
| **4** | `nvkho_cuong`| `123456` | **NhanVienKho** | **Nguyễn Văn Cường** | Thủ kho: Giám sát tồn kho tức thời, cảnh báo hạn mức, Audit Trail, Dashboard KPIs. |
| **5** | `bophan_dung`| `123456` | **BoPhanNhan** | **Nguyễn Việt Dũng** | Khách nhận hàng: Lập & duyệt phiếu xuất, xem hàng hóa, xem phiếu xuất đơn vị mình. |
| **6** | `kho_hanoi` | `123456` | **NhanVienKho** | Nhân viên Kho Hà Nội | Tài khoản thử nghiệm tác nghiệp tại chi nhánh Hà Nội. |
| **7** | `kho_saigon` | `123456` | **NhanVienKho** | Nhân viên Kho Sài Gòn | Tài khoản thử nghiệm tác nghiệp tại chi nhánh Sài Gòn. |

---

## 5. HƯỚNG DẪN CHẠY BỘ KIỂM THỬ TỰ ĐỘNG (xUnit CLI)

### 5.1. Các câu lệnh chạy test thông dụng
```bash
# Chạy toàn bộ 26 kịch bản kiểm thử tích hợp (Mọi module)
dotnet test

# Hiển thị chi tiết từng test case được phát hiện
dotnet test -t

# Chạy có xuất log kết quả chi tiết từng bài test
dotnet test --logger "console;verbosity=detailed"
```

### 5.2. Chạy kiểm thử theo từng Module hoặc Lớp kiểm thử (Test Class)
```bash
# Kiểm thử Xác thực, Cookie, Lockout & Phân quyền (Module 1)
dotnet test --filter "FullyQualifiedName~AuthenticationIntegrationTests"

# Kiểm thử Quy trình Nhập kho, Transaction cộng dồn tồn kho & chống cộng lặp (Module 3)
dotnet test --filter "FullyQualifiedName~InboundWorkflowIntegrationTests"

# Kiểm thử Quy trình Xuất kho, Kiểm tra tồn khả dụng & chống xuất âm (Module 4)
dotnet test --filter "FullyQualifiedName~OutboundWorkflowIntegrationTests"
```

### 5.3. Chạy kiểm thử theo Mã Test Case / Từ khóa chức năng
```bash
# Lọc theo mã test case trong DisplayName
dotnet test --filter "DisplayName~TC_M1_001"
dotnet test --filter "DisplayName~TC_M3_001"
dotnet test --filter "DisplayName~TC_M4_001"

# Lọc theo chức năng nghiệp vụ
dotnet test --filter "DisplayName~Đăng nhập"
dotnet test --filter "DisplayName~Xuất kho"
dotnet test --filter "DisplayName~Nhập kho"
```

### 5.4. Chạy thông qua Scripts PowerShell tự động hóa (Kèm xuất báo cáo)
```powershell
# Chạy script PowerShell tự động thực thi và định dạng bảng kết quả:
pwsh ./Tests/Run-Tests.ps1

# Chạy script đối soát độ phủ test case với file Excel kịch bản kiểm thử:
pwsh ./Tests/Show-TestcaseCoverage.ps1
```
