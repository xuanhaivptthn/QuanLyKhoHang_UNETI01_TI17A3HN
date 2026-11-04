# BẢN TÓM TẮT THAM CHIẾU NHANH MODULE 1 (QUICK REFERENCE CHEATSHEET)
## Tài Khoản, Xác Thực Cookie/Session, Lockout, RBAC & Danh Mục Cơ Sở

> **Tài liệu tham chiếu dành cho AI Agent & Lập trình viên**  
> **Phụ trách:** Trần Xuân Hải (MSSV: 23103100135 - Nhóm trưởng / Architect)  
> **Phiên bản:** ASP.NET Core 10 MVC (C# 14 / EF Core 10)  
> **Cập nhật:** Tháng 11/2026

---

## 1. PHẠM VI NGHIỆP VỤ & QUYỀN HẠN

Module 1 là khối **xác thực, bảo mật và danh mục gốc** của hệ thống Quản lý Kho hàng UNETI:
- **Xác thực danh tính:** Cookie Authentication (`.AspNetCore.Cookies`) kết hợp `ISession`.
- **Chống dò mật khẩu (Brute-force):** Khóa tài khoản tạm thời 5 phút sau 5 lần nhập sai liên tiếp.
- **Mã hóa chuẩn:** `IPasswordHasher<TaiKhoan>` (PBKDF2 HMAC-SHA256, 128-bit salt, 10.000 vòng lặp).
- **Phân quyền (RBAC):** 3 vai trò chính:
  - `Admin`: Toàn quyền cấu hình, tài khoản, danh mục, kho, báo cáo.
  - `NhanVienKho`: Quản lý danh mục hàng hóa, nhập kho, cảnh báo tồn, lịch sử.
  - `BoPhanNhan`: Xem hàng hóa, xem phiếu xuất của đơn vị mình.
- **Hồ sơ & Đổi mật khẩu:** Cá nhân tự quản lý họ tên/email/mật khẩu, chặn đổi `TenDangNhap` và `VaiTro`.
- **Thao tác nhanh AJAX cho Admin:**
  - `ResetPasswordQuick`: Đặt lại mật khẩu về mặc định (`123456@`), xóa lockout.
  - `DoiTrangThai`: Bật/Tắt khóa tài khoản bằng Bootstrap Switch trực tiếp, có bảo vệ chống tự khóa chính mình.
- **Danh mục cơ sở:** CRUD `LoaiHang` và `DonViTinh`, có ràng buộc toàn vẹn dữ liệu (chặn xóa nếu có `HangHoa`).
- **CSDL High-Availability (HA):** Kết nối tự động Failover giữa Azure SQL Remote và LocalDB.

---

## 2. KIẾN TRÚC DỮ LIỆU & THỰC THỂ (ENTITIES)

### 2.1. `TaiKhoan` (`Data/AppDbContext.cs` / `Models/TaiKhoan.cs`)
```csharp
public class TaiKhoan
{
    [Key]
    public int MaTaiKhoan { get; set; }

    [Required, StringLength(50)]
    public string TenDangNhap { get; set; } = string.Empty; // Unique index

    [Required, StringLength(255)]
    public string MatKhau { get; set; } = string.Empty; // PBKDF2 Identity hash

    [Required, StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }

    [Required, StringLength(20)]
    public string VaiTro { get; set; } = "NhanVienKho"; // Admin | NhanVienKho | BoPhanNhan

    public bool TrangThai { get; set; } = true; // true: Hoạt động, false: Khóa

    public int SoLanDangNhapSai { get; set; } = 0;

    public DateTime? ThoiGianKhoaDen { get; set; } // Khóa tạm đến thời điểm này

    public DateTime NgayTao { get; set; } = DateTime.Now;
}
```

### 2.2. `LoaiHang` (`Models/LoaiHang.cs`)
- `MaLoaiHang` (PK, int): Mã phân loại.
- `TenLoaiHang` (Required, Unique, nvarchar(100)): Tên ngành hàng.
- `MoTa` (nvarchar(255)): Mô tả.
- `TrangThai` (bool, default true): Sử dụng / Tạm ngừng.
- Quan hệ: `1 - N` với `HangHoa` (Chặn xóa vật lý khi `HangHoas.Any()`).

### 2.3. `DonViTinh` (`Models/DonViTinh.cs`)
- `MaDonViTinh` / `MaDVT` (PK, int): Mã đơn vị tính.
- `TenDonViTinh` (Required, Unique, nvarchar(50)): Cái, Thùng, Hộp, Kg, Mét...
- `MoTa` (nvarchar(255)): Ghi chú quy cách.
- `TrangThai` (bool, default true): Sử dụng / Tạm ngừng.
- Quan hệ: `1 - N` với `HangHoa` (Chặn xóa vật lý khi `HangHoas.Any()`).

---

## 3. DANH SÁCH CONTROLLERS & ENDPOINTS

### 3.1. `TaiKhoansController` (`Controllers/TaiKhoansController.cs`)
| Endpoint | Method | Phân quyền | Mục đích |
| :--- | :---: | :---: | :--- |
| `/login`, `/TaiKhoans/DangNhap` | `GET`, `POST` | Public | Đăng nhập hệ thống, kiểm tra lockout 5 lần, cấp Cookie & Session |
| `/logout`, `/TaiKhoans/DangXuat` | `GET`, `POST` | Logged In | Đăng xuất an toàn: `SignOutAsync`, `Session.Clear()`, xóa Session Cookie |
| `/TaiKhoans/Profile` | `GET` | Logged In | Xem thông tin cá nhân hiện tại |
| `/TaiKhoans/CapNhatProfile` | `GET`, `POST` | Logged In | Sửa Họ tên, Email (Khóa cứng `TenDangNhap`, `VaiTro`) |
| `/TaiKhoans/DoiMatKhau` | `GET`, `POST` | Logged In | Đổi mật khẩu có xác thực mật khẩu cũ và băm PBKDF2 mới |
| `/TaiKhoans/Index` | `GET` | `Admin` | Quản lý danh sách tài khoản, switch AJAX trạng thái, reset MK |
| `/TaiKhoans/Create` | `GET`, `POST` | `Admin` | Tạo tài khoản mới, mã hóa mật khẩu, phân vai trò |
| `/TaiKhoans/Edit/{id}` | `GET`, `POST` | `Admin` | Sửa thông tin tài khoản, đổi vai trò, đổi trạng thái |
| `/TaiKhoans/Details/{id}` | `GET` | `Admin` | Xem chi tiết thông tin tài khoản |
| `/TaiKhoans/Delete/{id}` | `GET`, `POST` | `Admin` | Xóa tài khoản (Chặn tự xóa chính tài khoản đang đăng nhập) |
| `/TaiKhoans/ResetPasswordQuick` | `POST` (AJAX) | `Admin` | Đặt lại mật khẩu về `123456@`, reset lockout, trả về JSON |
| `/TaiKhoans/DoiTrangThai` | `POST` (AJAX) | `Admin` | Bật/tắt trạng thái qua Bootstrap Switch, bảo vệ Admin duy nhất |

### 3.2. `LoaiHangsController` (`Controllers/LoaiHangsController.cs`)
| Endpoint | Method | Phân quyền | Mục đích |
| :--- | :---: | :---: | :--- |
| `/LoaiHangs/Index` | `GET` | `Admin`, `NhanVienKho` | Danh sách loại hàng |
| `/LoaiHangs/Create` | `GET`, `POST` | `Admin`, `NhanVienKho` | Thêm mới loại hàng (Check trùng tên) |
| `/LoaiHangs/Edit/{id}` | `GET`, `POST` | `Admin`, `NhanVienKho` | Chỉnh sửa tên / mô tả loại hàng |
| `/LoaiHangs/Details/{id}` | `GET` | `Admin`, `NhanVienKho` | Xem chi tiết loại hàng |
| `/LoaiHangs/Delete/{id}` | `GET`, `POST` | `Admin`, `NhanVienKho` | Xóa loại hàng (Chặn xóa nếu đã có Hàng hóa liên kết) |

### 3.3. `DonViTinhsController` (`Controllers/DonViTinhsController.cs`)
| Endpoint | Method | Phân quyền | Mục đích |
| :--- | :---: | :---: | :--- |
| `/DonViTinhs/Index` | `GET` | `Admin`, `NhanVienKho` | Danh sách đơn vị tính |
| `/DonViTinhs/Create` | `GET`, `POST` | `Admin`, `NhanVienKho` | Thêm mới ĐVT (Check trùng tên) |
| `/DonViTinhs/Edit/{id}` | `GET`, `POST` | `Admin`, `NhanVienKho` | Chỉnh sửa tên / mô tả ĐVT |
| `/DonViTinhs/Details/{id}` | `GET` | `Admin`, `NhanVienKho` | Xem chi tiết ĐVT |
| `/DonViTinhs/Delete/{id}` | `GET`, `POST` | `Admin`, `NhanVienKho` | Xóa ĐVT (Chặn xóa nếu đã có Hàng hóa liên kết) |

---

## 4. QUY TẮC BẢO MẬT & BẤT BIẾN NGHIỆP VỤ (CRITICAL INVARIANTS)

1. **Quy tắc Băm Mật khẩu:**
   - Tuyệt đối KHÔNG lưu mật khẩu dạng Plain Text hoặc SHA-256 thuần túy.
   - Luôn sử dụng `IPasswordHasher<TaiKhoan>`:
     ```csharp
     // Băm mới:
     user.MatKhau = hasher.HashPassword(user, plainPassword);

     // Kiểm tra:
     var result = hasher.VerifyHashedPassword(user, user.MatKhau, enteredPassword);
     if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded) { ... }
     ```
2. **Quy tắc Lockout 5 Lần:**
   - Nhập sai: `user.SoLanDangNhapSai++`.
   - Nếu `user.SoLanDangNhapSai >= 5`: `user.ThoiGianKhoaDen = DateTime.Now.AddMinutes(5)`.
   - Đăng nhập đúng: `user.SoLanDangNhapSai = 0; user.ThoiGianKhoaDen = null;`.
3. **Quy tắc Tự Bảo Vệ Quản Trị (Admin Self-Defense):**
   - Admin không thể tự xóa tài khoản của chính mình (`user.MaTaiKhoan == currentAdminId`).
   - Admin không thể tự vô hiệu hóa tài khoản của chính mình qua switch AJAX.
   - Không thể vô hiệu hóa tài khoản Admin hoạt động duy nhất còn lại trong hệ thống.
4. **Quy tắc Ràng Buộc Khóa Ngoại:**
   - Không được xóa `LoaiHang` hoặc `DonViTinh` nếu bảng `HangHoa` có dữ liệu tham chiếu đến khóa ngoại tương ứng.
5. **Đồng Bộ Cookie & Session:**
   - Khi đăng nhập thành công, bắt buộc ghi cả Claim Cookie:
     `NameIdentifier` (ID), `Name` (TenDangNhap), `Role` (VaiTro), `Email`, `HoTen`.
   - Đồng thời lưu `ISession`: `MaTaiKhoan`, `TenDangNhap`, `HoTen`, `VaiTro`.
6. **Bảo vệ CSRF:**
   - Mọi Action `POST` (kể cả AJAX endpoints) đều phải có `[ValidateAntiForgeryToken]` và đính kèm token vào AJAX headers:
     ```javascript
     headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() }
     ```

---

## 5. TÀI KHOẢN MẪU SEED DỮ LIỆU (`Data/AppDbContext.Seed.cs`)

| Tên đăng nhập | Mật khẩu mặc định | Vai trò | Người phụ trách |
| :--- | :---: | :---: | :--- |
| `admin` | `Admin@123` | **Admin** | Trần Xuân Hải (Nhóm trưởng) |
| `nvkho_hung` | `123456` | **NhanVienKho** | Lê Văn Hùng |
| `nvkho_cuc` | `123456` | **NhanVienKho** | Nguyễn Thị Cúc |
| `nvkho_cuong` | `123456` | **NhanVienKho** | Nguyễn Văn Cường |
| `bophan_dung` | `123456` | **BoPhanNhan** | Nguyễn Việt Dũng |
| `bophan_lan` | `123456` | **BoPhanNhan** | Khách nhận mẫu |
| `user_khoa` | `123456` | **NhanVienKho** | Tài khoản thử nghiệm bị khóa (`TrangThai=false`) |

---

## 6. DANH SÁCH KIỂM THỬ XÁC MINH (TEST CASES)

Tệp kiểm thử: `Tests/QuanLyKhoHang.Tests/AuthenticationIntegrationTests.cs`
- `TC-AUTH-01`: Đăng nhập thành công với tài khoản chuẩn -> Nhận Cookie `.AspNetCore.Cookies` và chuyển hướng.
- `TC-AUTH-02`: Đăng nhập sai mật khẩu -> Báo lỗi và tăng bộ đếm `SoLanDangNhapSai`.
- `TC-AUTH-03`: Đăng nhập sai liên tiếp 5 lần -> Kích hoạt khóa tạm 5 phút (`ThoiGianKhoaDen`).
- `TC-AUTH-04`: Đăng nhập vào tài khoản có `TrangThai == false` -> Bị từ chối truy cập.
- `TC-AUTH-05`: Truy cập route yêu cầu đăng nhập khi chưa có Cookie -> Chuyển hướng về `/login?ReturnUrl=...`.
- `TC-AUTH-06`: Người dùng `NhanVienKho` cố truy cập `/TaiKhoans/Index` -> Nhận 403 Forbidden `/Home/AccessDenied`.
- `TC-AUTH-07`: Đổi mật khẩu thành công -> Hash mới được lưu và mật khẩu cũ bị vô hiệu hóa.
- `TC-AUTH-08`: Đặt lại mật khẩu nhanh AJAX `ResetPasswordQuick` -> Reset về `123456@` và xóa lockout.
- `TC-AUTH-09`: Đổi trạng thái AJAX `DoiTrangThai` -> Chặn tự khóa tài khoản Admin đang đăng nhập.
