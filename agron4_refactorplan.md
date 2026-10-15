# KẾ HOẠCH PHÁT TRIỂN, REFACTOR & LÀM GỌN DỰ ÁN
## ĐỀ TÀI: HỆ THỐNG QUẢN LÝ KHO HÀNG VÀ NHẬP – XUẤT – TỒN (ASP.NET CORE 10 MVC)

- **Repository mục tiêu:** `https://github.com/xuanhaivptthn/QuanLyKhoHang_UNETI01_TI17A3HN` (Nhánh: `main`)
- **Tên Project / Solution / Namespace chuẩn:** `QuanLyKhoHang_UNETI01_TI17A3HN`
- **Nền tảng bắt buộc:** .NET 10 SDK (`net10.0`), Entity Framework Core 10, SQL Server, ASP.NET Core 10 MVC.
- **Quy mô nhóm:** 05 Sinh viên (Phân chia 05 Module độc lập trách nhiệm nhưng tích hợp chung 01 CSDL & 01 DbContext).

---

## PHẦN 1: MỤC TIÊU REFACTOR & LÀM GỌN (CLEANUP) TRÊN NHÁNH `MAIN`

Để đồ án đạt điểm tối đa khi chấm code, kiểm tra lịch sử GitHub và bảo vệ trực tiếp, dự án trên nhánh `main` cần được rà soát và chuẩn hóa theo 5 tiêu chí cốt lõi:

1. **Đồng bộ Phiên bản & Cấu trúc chuẩn .NET 10:**
   - Kiểm tra file `.csproj`: Đảm bảo `<TargetFramework>net10.0</TargetFramework>` và toàn bộ gói NuGet (`Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`) đều ở phiên bản `10.0.x`.
   - Loại bỏ các thư mục rác bị đẩy nhầm lên Git (`bin/`, `obj/`, `.vs/`, `.idea/`, các file `.user`) thông qua việc cập nhật `.gitignore` chuẩn của Visual Studio.
2. **Làm gọn Kiến trúc MVC (Tinh giản, Không dư thừa):**
   - **Không lạm dụng Repository Pattern / Clean Architecture phức tạp** nếu không cần thiết: Giảng viên yêu cầu sinh viên hiểu rõ và giải thích trực tiếp LINQ (`Where`, `OrderBy`, `Skip`, `Take`, `GroupBy`), `DbContext`, và `Model Binding` ngay tại Controller/Service mỏng.
   - Gom nhóm rõ ràng: `Models/Entities`, `Models/ViewModels`, `Data`, `Controllers`, `Filters` (Phân quyền), `Services` (Xử lý cộng/trừ tồn kho an toàn), `Views`.
   - Xóa bỏ các Controller/View do Scaffolding sinh ra nhưng giữ nguyên mặc định hoặc không dùng đến.
3. **Chuẩn hóa Quy tắc Nghiệp vụ Lõi (Zero-Bug Logic):**
   - Khóa chặt việc sửa số lượng tồn trực tiếp trên UI/Controller.
   - Đảm bảo tính nhất quán dữ liệu bằng Transaction khi **Hoàn tất phiếu nhập** (chỉ cộng tồn 1 lần) và **Hoàn tất phiếu xuất** (kiểm tra $SoLuongXuat \le SoLuongTon$, không xuất âm, chỉ trừ tồn 1 lần).
4. **Chuẩn hóa Dấu ấn Cá nhân (Bắt buộc khi Bảo vệ):**
   - 100% các file (`Entity`, `Controller`, `ViewModel`, `View`, `Service`) phải gắn **Header Comment** ghi rõ Họ tên, MSSV, Nội dung thực hiện ở đầu file.
   - Lịch sử Commit trên nhánh `main` phải minh bạch theo cú pháp `[Mã SV] [Module] Nội dung công việc`.

---

## PHẦN 2: CẤU TRÚC THƯC MỤC CHUẨN HÓA (TARGET ARCHITECTURE)

```text
QuanLyKhoHang_UNETI01_TI17A3HN/
├── .gitignore
├── QuanLyKhoHang_UNETI01_TI17A3HN.sln
└── QuanLyKhoHang_UNETI01_TI17A3HN/
    ├── QuanLyKhoHang_UNETI01_TI17A3HN.csproj
    ├── Program.cs                          # Cấu hình DbContext, Session, Routing
    ├── appsettings.json                    # Chuỗi kết nối SQL Server dùng chung
    ├── Data/
    │   ├── AppDbContext.cs                 # Khai báo DbSet & FluentAPI (PK, FK, Unique, Decimal)
    │   ├── DbInitializer.cs                # Seed Data đầy đủ theo mục 16 của đề bài
    │   └── Migrations/                     # Chứa lịch sử Migration EF Core 10
    ├── Filters/
    │   └── AuthorizeRoleAttribute.cs       # ActionFilter kiểm tra Session & Phân quyền tại Controller (SV1)
    ├── Models/
    │   ├── Entities/                       # 13 Entities chuẩn hóa Data Annotations
    │   │   ├── TaiKhoan.cs                 # [SV1]
    │   │   ├── LoaiHang.cs                 # [SV1]
    │   │   ├── DonViTinh.cs                # [SV1]
    │   │   ├── Kho.cs                      # [SV2]
    │   │   ├── HangHoa.cs                  # [SV2]
    │   │   ├── NhaCungCap.cs               # [SV3]
    │   │   ├── PhieuNhap.cs                # [SV3]
    │   │   ├── ChiTietPhieuNhap.cs         # [SV3]
    │   │   ├── BoPhanNhan.cs               # [SV4]
    │   │   ├── PhieuXuat.cs                # [SV4]
    │   │   ├── ChiTietPhieuXuat.cs         # [SV4]
    │   │   ├── TonKho.cs                   # [SV5] (Bảng lưu trữ tồn thực tế theo Kho - Hàng)
    │   │   └── LichSuTonKho.cs             # [SV5] (Bảng lưu vết biến động Nhập/Xuất)
    │   ├── Enums/                          # Chuẩn hóa trạng thái tránh gõ sai chuỗi (Magic Strings)
    │   │   ├── VaiTroHeThong.cs            # Admin, NhanVienKho, BoPhanNhan
    │   │   └── TrangThaiPhieu.cs           # Nhap (0), ChoXacNhan (1), DaHoanTat (2), DaHuy (3)
    │   └── ViewModels/                     # Các ViewModel phục vụ Search/Filter/Sort/Paging & Thống kê
    │       ├── LoginViewModel.cs           # [SV1]
    │       ├── HangHoaFilterViewModel.cs   # [SV2] Kết hợp Search + Filter + Sort + Pagination
    │       ├── PhieuNhapViewModel.cs       # [SV3] Master-Detail Phiếu nhập
    │       ├── PhieuXuatViewModel.cs       # [SV4] Master-Detail Phiếu xuất & Check tồn
    │       └── DashboardThongKeViewModel.cs# [SV5] Tổng hợp thẻ KPI, Cảnh báo & Báo cáo LINQ
    ├── Controllers/
    │   ├── TaiKhoanController.cs           # [SV1] Đăng nhập, Đăng xuất, CRUD Tài khoản
    │   ├── LoaiHangController.cs           # [SV1] CRUD Loại hàng
    │   ├── DonViTinhController.cs          # [SV1] CRUD Đơn vị tính
    │   ├── KhoController.cs                # [SV2] CRUD Kho hàng
    │   ├── HangHoaController.cs            # [SV2] CRUD Hàng hóa + Search/Filter/Sort/Paging + Tồn theo kho
    │   ├── NhaCungCapController.cs         # [SV3] CRUD Nhà cung cấp
    │   ├── PhieuNhapController.cs          # [SV3] Lập phiếu, thêm chi tiết, tính tiền, Hoàn tất nhập
    │   ├── BoPhanNhanController.cs         # [SV4] CRUD Bộ phận nhận
    │   ├── PhieuXuatController.cs          # [SV4] Lập phiếu, check tồn, Hoàn tất xuất, Hủy phiếu
    │   ├── TonKhoController.cs             # [SV5] Xem tồn kho, Cảnh báo sắp hết/hết hàng, Lịch sử
    │   └── ThongKeController.cs            # [SV5] Dashboard & Báo cáo LINQ theo kỳ
    └── Views/                              # Chia thư mục tương ứng với 11 Controllers + Shared/_Layout.cshtml
```

---

## PHẦN 3: THIẾT KẾ CƠ SỞ DỮ LIỆU & RÀNG BUỘC (EF CORE 10 CODE FIRST)

Để tránh xung đột Migration giữa 5 thành viên, nhóm cần chốt và refactor toàn bộ lớp `Models/Entities` và `AppDbContext.cs` ngay ở Bước 1.

### 3.1. Chi tiết 13 Bảng (Entities) & Ràng buộc
1. **`TaiKhoan` (SV1):**
   - `MaTaiKhoan` (PK, int, Identity), `TenDangNhap` (Required, StringLength(50), **Unique Index**), `MatKhau` (Required, StringLength(255)), `HoTen` (Required, StringLength(100)), `Email` (Required, EmailAddress), `VaiTro` (Required: `"Admin"`, `"NhanVienKho"`, `"BoPhanNhan"`), `TrangThai` (bool: `true` = Hoạt động, `false` = Khóa).
2. **`LoaiHang` (SV1):**
   - `MaLoaiHang` (PK), `TenLoaiHang` (Required, StringLength(100), **Unique Index**), `MoTa` (StringLength(500)), `TrangThai` (bool).
3. **`DonViTinh` (SV1):**
   - `MaDonViTinh` (PK), `TenDonViTinh` (Required, StringLength(50), **Unique Index**), `MoTa` (StringLength(255)), `TrangThai` (bool).
4. **`Kho` (SV2):**
   - `MaKho` (PK), `TenKho` (Required, StringLength(100), **Unique Index**), `DiaDiem` (Required, StringLength(255)), `MoTa` (StringLength(500)), `TrangThai` (bool).
5. **`HangHoa` (SV2):**
   - `MaHang` (PK, int hoặc mã chuỗi tự định nghĩa như `HH001`), `TenHang` (Required, StringLength(150)), `MaLoaiHang` (FK), `MaDonViTinh` (FK), `GiaNhapThamKhao` (`decimal(18,2)`, `[Range(0, double.MaxValue)]`), `MucTonToiThieu` (`int`, `[Range(0, int.MaxValue)]`), `MoTa`, `TrangThai` (bool).
6. **`NhaCungCap` (SV3):**
   - `MaNhaCungCap` (PK), `TenNhaCungCap` (Required, StringLength(150)), `SoDienThoai` (Required, `[Phone]`, StringLength(15)), `Email` (`[EmailAddress]`), `DiaChi` (StringLength(255)), `TrangThai` (bool).
7. **`PhieuNhap` (SV3):**
   - `MaPhieuNhap` (PK), `MaNhaCungCap` (FK), `MaKho` (FK), `NgayNhap` (DateTime), `NguoiLap` (FK -> `TaiKhoan` hoặc lưu `TenDangNhap`/`HoTen`), `TrangThai` (int/string: `Nháp`, `Chờ xác nhận`, `Đã hoàn tất`, `Đã hủy`), `GhiChu` (StringLength(500)).
   - *Thuộc tính tính toán (`[NotMapped]` hoặc lưu DB do hệ thống tự cập nhật):* `TongTienNhap`.
8. **`ChiTietPhieuNhap` (SV3):**
   - `MaChiTietNhap` (PK), `MaPhieuNhap` (FK), `MaHang` (FK), `SoLuongNhap` (`int`, `[Range(1, int.MaxValue)]`), `DonGiaNhap` (`decimal(18,2)`, `[Range(0, double.MaxValue)]`), `ThanhTien` (`decimal(18,2)`).
9. **`BoPhanNhan` (SV4):**
   - `MaBoPhan` (PK), `TenBoPhan` (Required, StringLength(150)), `NguoiDaiDien` (Required, StringLength(100)), `SoDienThoai` (`[Phone]`), `MoTa`, `TrangThai` (bool).
10. **`PhieuXuat` (SV4):**
    - `MaPhieuXuat` (PK), `MaBoPhan` (FK), `MaKho` (FK), `NgayXuat` (DateTime), `NguoiLap`, `TrangThai` (`Nháp`, `Chờ xác nhận`, `Đã hoàn tất`, `Đã hủy`), `GhiChu`.
11. **`ChiTietPhieuXuat` (SV4):**
    - `MaChiTietXuat` (PK), `MaPhieuXuat` (FK), `MaHang` (FK), `SoLuongXuat` (`int`, `[Range(1, int.MaxValue)]`), `DonGiaXuatThamChieu` (`decimal(18,2)`, `[Range(0, double.MaxValue)]`), `GhiChu`.
12. **`TonKho` (SV5 - Bảng tối ưu truy vấn & khóa dòng giao dịch):**
    - Khóa chính phức hợp `(MaKho, MaHang)` hoặc `MaTonKho` (PK) + **Unique Index `(MaKho, MaHang)`**.
    - `SoLuongTon` (`int`, mặc định = 0, ràng buộc không âm), `NgayCapNhat` (DateTime).
13. **`LichSuTonKho` (SV5 - Bảng truy vết lịch sử Nhập/Xuất):**
    - `MaLichSu` (PK), `NgayPhatSinh` (DateTime), `MaKho` (FK), `MaHang` (FK), `LoaiGiaoDich` (`"NHAP"` / `"XUAT"`), `MaPhieuThamChieu` (string, VD: `"PN001"`, `"PX005"`), `SoLuongThayDoi` (`int`, dương khi nhập, âm khi xuất), `TonSauGiaoDich` (`int`), `NguoiThucHien` (string), `GhiChu` (string).

### 3.2. Cấu hình Fluent API trong `AppDbContext.cs`
- Thiết lập `DeleteBehavior.Restrict` cho toàn bộ khóa ngoại danh mục (`LoaiHang`, `DonViTinh`, `Kho`, `HangHoa`, `NhaCungCap`, `BoPhanNhan`) để **ngăn chặn xóa vật lý (Physical Delete)** khi đã có phát sinh phiếu nhập/xuất.
- Cấu hình kiểu dữ liệu tiền tệ `.HasColumnType("decimal(18,2)")` cho `GiaNhapThamKhao`, `DonGiaNhap`, `ThanhTien`, `DonGiaXuatThamChieu` để không bị cảnh báo mất độ chính xác của EF Core 10.

---

## PHẦN 4: KẾ HOẠCH REFACTOR CHI TIẾT THEO 05 MODULE (PHÂN CÔNG 5 SINH VIÊN)

### MODULE 1: TÀI KHOẢN, ĐĂNG NHẬP, PHÂN QUYỀN, LOẠI HÀNG & ĐƠN VỊ TÍNH (SINH VIÊN 1)
1. **Refactor Đăng nhập & Session (`TaiKhoanController.cs`):**
   - Cấu hình `builder.Services.AddSession()` và `app.UseSession()` trong `Program.cs`.
   - Khi đăng nhập: Truy vấn LINQ kiểm tra `TenDangNhap` và `MatKhau`. Nếu `TrangThai == false` (Bị khóa) -> Trả về lỗi `"Tài khoản của bạn đã bị khóa, không thể đăng nhập!"`.
   - Nếu hợp lệ: Lưu vào `HttpContext.Session`: `"MaTaiKhoan"`, `"HoTen"`, `"VaiTro"`.
   - Khi đăng xuất: Gọi `HttpContext.Session.Clear()` và chuyển hướng về trang `Login`.
2. **Refactor Phân quyền tại Controller (`AuthorizeRoleAttribute.cs`):**
   - **Tuyệt đối không chỉ ẩn nút trên View.** Xây dựng Custom Action Filter kế thừa `ActionFilterAttribute`:
     - Kiểm tra Session `"MaTaiKhoan"` có tồn tại không (nếu không -> Redirect về `/TaiKhoan/DangNhap`).
     - Kiểm tra Session `"VaiTro"` có nằm trong danh sách Role được phép truy cập Action/Controller đó không (nếu không -> Redirect về trang `AccessDenied`).
   - Áp dụng:
     - `Admin`: Toàn quyền.
     - `NhanVienKho`: Được vào xem Hàng hóa, Kho, Tồn kho và thực hiện lập/cập nhật `PhieuNhap`, `PhieuXuat`.
     - `BoPhanNhan`: Chỉ xem danh sách `PhieuXuat` liên quan.
3. **Refactor `LoaiHangController` & `DonViTinhController`:**
   - Khi `Create` / `Edit`: Dùng LINQ `.AnyAsync(x => x.TenLoaiHang.ToLower() == model.TenLoaiHang.ToLower() && x.MaLoaiHang != model.MaLoaiHang)` để bắt lỗi trùng tên và đẩy vào `ModelState.AddModelError`.
   - Khi xóa: Nếu đã có `HangHoa` tham chiếu, chuyển sang **Đổi trạng thái ngừng hoạt động (`TrangThai = false`)** thay vì xóa vật lý.

---

### MODULE 2: KHO, HÀNG HÓA & TÌM KIẾM - LỌC - SẮP XẾP - PHÂN TRANG (SINH VIÊN 2)
1. **Refactor CRUD `KhoController` & `HangHoaController`:**
   - Trong `HangHoaController.Create/Edit`: Dropdown chọn `LoaiHang` và `DonViTinh` chỉ lấy các bản ghi đang hoạt động (`.Where(x => x.TrangThai == true)`).
   - Không cho phép xóa vật lý Kho hoặc Hàng hóa nếu đã tồn tại trong `ChiTietPhieuNhap` hoặc `ChiTietPhieuXuat` (hiển thị thông báo yêu cầu chuyển sang trạng thái "Ngừng hoạt động").
2. **Refactor Luồng Truy vấn Chuẩn (Search -> Filter -> Sort -> Pagination) tại `HangHoaController.Index`:**
   - Khởi tạo truy vấn cơ sở `IQueryable<HangHoa>` kết hợp tính tổng tồn từ bảng `TonKho`:
     ```csharp
     var query = _context.HangHoas
         .Include(h => h.LoaiHang)
         .Include(h => h.DonViTinh)
         .Include(h => h.TonKhos)
         .AsQueryable();
     ```
   - **Bước 1 - Tìm kiếm (`Where`):** Tìm theo `TenHang`, `MaHang`, `TenLoaiHang`, hoặc `TenKho` (thông qua `TonKhos.Any(tk => tk.Kho.TenKho.Contains(keyword))`).
   - **Bước 2 - Lọc đa điều kiện (`Where`):**
     - Theo `MaLoaiHang`, `MaDonViTinh`, `MaKho`, `TrangThai`.
     - Theo khoảng giá: `GiaTu <= GiaNhapThamKhao <= GiaDen`.
     - Theo trạng thái cảnh báo tồn:
       - *Hết hàng:* Tổng tồn (hoặc tồn tại kho đang lọc) $== 0$.
       - *Sắp hết:* $0 < SoLuongTon \le MucTonToiThieu$.
       - *An toàn:* $SoLuongTon > MucTonToiThieu$.
   - **Bước 3 - Sắp xếp (`OrderBy` / `OrderByDescending`):**
     - `ten_asc`, `ten_desc`, `gia_asc`, `gia_desc`, ` toithieu_asc`, `toithieu_desc`, `ton_asc`, `ton_desc`.
   - **Bước 4 - Phân trang (`Skip` & `Take`):**
     - Tính `totalItems = await query.CountAsync();`
     - Lấy dữ liệu trang hiện tại: `var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();`
   - **Trên Razor View (`Views/HangHoa/Index.cshtml`):** Các thẻ `<a>` phân trang bắt buộc truyền lại toàn bộ `asp-route-*` (từ khóa, bộ lọc, cột sắp xếp) để **giữ nguyên điều kiện khi chuyển trang**.
3. **Màn hình Chi tiết Hàng hóa (`HangHoa/Details`):**
   - Hiển thị bảng danh sách các Kho kèm theo `SoLuongTon` thực tế của mặt hàng đó tại từng kho và Lịch sử nhập xuất gần nhất.

---

### MODULE 3: NHÀ CUNG CẤP, PHIẾU NHẬP & HOÀN TẤT NHẬP KHO (SINH VIÊN 3)
1. **Refactor `NhaCungCapController`:**
   - Kiểm tra định dạng `Email`, `SoDienThoai`. Ngăn xóa vật lý nếu đã có `PhieuNhap`.
2. **Refactor Nghiệp vụ Lập & Tính tiền Phiếu nhập (`PhieuNhapController`):**
   - Khi tạo phiếu mới: Chỉ load `NhaCungCap`, `Kho`, `HangHoa` có `TrangThai == true`. Mặc định phiếu ở trạng thái `Nháp` hoặc `Chờ xác nhận`.
   - Khi thêm/sửa `ChiTietPhieuNhap`:
     - Bắt buộc `SoLuongNhap > 0` và `DonGiaNhap >= 0`.
     - Hệ thống tự động tính tại Server:
       $$ThanhTien = SoLuongNhap \times DonGiaNhap$$
       $$TongTienNhap = \sum ThanhTien$$
     - **Tuyệt đối không nhận giá trị `ThanhTien` hay `TongTienNhap` từ form gửi lên.**
   - Chỉ cho phép thêm/sửa/xóa chi tiết khi phiếu đang ở trạng thái `Nháp` hoặc `Chờ xác nhận`.
3. **Refactor Chức năng "Hoàn tất nhập kho" (`HoanTatNhap` - POST):**
   - Kiểm tra điều kiện nghiêm ngặt:
     1. Phiếu phải tồn tại và đang ở trạng thái `Nháp` hoặc `Chờ xác nhận` (Nếu đã `Đã hoàn tất` hoặc `Đã hủy` -> Báo lỗi ngay để **chống cộng tồn 2 lần**).
     2. Phiếu phải có ít nhất 01 dòng `ChiTietPhieuNhap` (`!phieu.ChiTietPhieuNhaps.Any()` -> Báo lỗi không thể hoàn tất phiếu rỗng).
     3. Kho và Nhà cung cấp của phiếu phải đang hoạt động.
   - Thực thi trong `using var transaction = await _context.Database.BeginTransactionAsync();`:
     - Duyệt qua từng dòng `ChiTietPhieuNhap`:
       - Tìm bản ghi `TonKho` theo `(phieu.MaKho, ct.MaHang)`. Nếu chưa có thì khởi tạo mới với `SoLuongTon = ct.SoLuongNhap`, nếu đã có thì cộng dồn `tonKho.SoLuongTon += ct.SoLuongNhap`.
       - Ghi 1 dòng vào bảng `LichSuTonKho` (`LoaiGiaoDich = "NHAP"`, `SoLuongThayDoi = +ct.SoLuongNhap`, `TonSauGiaoDich = tonKho.SoLuongTon`).
     - Cập nhật `phieu.TrangThai = TrangThaiPhieu.DaHoanTat`.
     - Lưu thay đổi và `CommitAsync()`.
4. **Danh sách Phiếu nhập:** Bổ sung tìm kiếm theo mã phiếu/tên NCC, lọc theo Kho/Từ ngày - Đến ngày/Trạng thái, sắp xếp theo Ngày nhập/Tổng tiền và Phân trang.

---

### MODULE 4: BỘ PHẬN NHẬN, PHIẾU XUẤT & KIỂM TRA TỒN KHO (SINH VIÊN 4)
1. **Refactor `BoPhanNhanController`:**
   - Quản lý danh sách bộ phận/phòng ban nhận hàng, kiểm tra SĐT hợp lệ, khóa trạng thái nếu ngừng hoạt động.
2. **Refactor Kiểm tra tồn kho kép (Khi thêm chi tiết & Khi hoàn tất xuất):**
   - **Lần 1 - Khi thêm/sửa `ChiTietPhieuXuat`:**
     - Truy vấn số lượng tồn hiện tại của `MaHang` tại đúng `MaKho` của phiếu xuất:
       ```csharp
       int soLuongTon = await _context.TonKhos
           .Where(t => t.MaKho == phieuXuat.MaKho && t.MaHang == model.MaHang)
           .Select(t => t.SoLuongTon)
           .FirstOrDefaultAsync();
       ```
     - Tính tổng số lượng của mặt hàng đó đã có trong chính phiếu xuất này (nếu thêm trùng mã hàng) + `SoLuongXuat` mới.
     - Kiểm tra điều kiện:
       $$SoLuongXuat \le SoLuongTon$$
     - Nếu vượt quá tồn kho hoặc $SoLuongTon == 0$: Trả về lỗi `ModelState` rõ ràng: *"Số lượng xuất ({SoLuongXuat}) vượt quá số lượng tồn thực tế tại kho ({soLuongTon})!"*.
   - **Lần 2 - Khi bấm "Hoàn tất xuất kho" (`HoanTatXuat` - POST):**
     - Chỉ tài khoản có quyền (`Admin`/`QuanLyKho` hoặc theo phân quyền) mới được bấm hoàn tất.
     - Kiểm tra trạng thái phiếu: Nếu đã `Đã hoàn tất` hoặc `Đã hủy` -> Chặn ngay để **chống trừ tồn nhiều lần**.
     - Kiểm tra lại tồn kho thực tế của tất cả mặt hàng trong phiếu một lần nữa trong Database Transaction (đề phòng có phiếu xuất khác vừa hoàn tất trước đó làm hụt tồn):
       - Nếu bất kỳ mặt hàng nào có $SoLuongXuat > SoLuongTon$: Rollback và báo lỗi mặt hàng bị thiếu tồn.
       - Nếu hợp lệ: Trừ tồn `tonKho.SoLuongTon -= ct.SoLuongXuat` (đảm bảo $SoLuongTon \ge 0$), ghi nhận vào `LichSuTonKho` (`LoaiGiaoDich = "XUAT"`, `SoLuongThayDoi = -ct.SoLuongXuat`, `TonSauGiaoDich = tonKho.SoLuongTon`), đổi trạng thái phiếu sang `Đã hoàn tất`.
3. **Refactor Hủy phiếu (`HuyPhieu` - POST):**
   - Chỉ cho phép hủy khi phiếu đang ở trạng thái `Nháp` hoặc `Chờ xác nhận`. Tuyệt đối chặn hủy trực tiếp phiếu `Đã hoàn tất`.

---

### MODULE 5: TỒN KHO, CẢNH BÁO, LỊCH SỬ & DASHBOARD THỐNG KÊ LINQ (SINH VIÊN 5)
1. **Refactor Quản lý Tồn kho & Đối chiếu công thức (`TonKhoController`):**
   - Đảm bảo không có bất kỳ Action `Create`/`Edit`/`Delete` nào cho phép người dùng sửa tay `SoLuongTon`.
   - Xây dựng hàm kiểm tra/đồng bộ hoặc hiển thị minh bạch trên giao diện tồn kho công thức:
     $$TonCuoi = TonDau (0) + TongNhap (Đã\ hoàn\ tất) - TongXuat (Đã\ hoàn\ tất)$$
   - Hỗ trợ lọc tồn kho theo `MaKho`, `MaLoaiHang`, từ khóa tên hàng và trạng thái cảnh báo.
2. **Refactor Cảnh báo Tồn kho:**
   - Gắn nhãn màu (Badge Bootstrap) trực quan:
     - **Hết hàng (Đỏ - Danger):** $SoLuongTon == 0$.
     - **Sắp hết hàng (Vàng - Warning):** $0 < SoLuongTon \le MucTonToiThieu$.
     - **Định mức an toàn (Xanh - Success):** $SoLuongTon > MucTonToiThieu$.
3. **Refactor Màn hình Lịch sử Nhập – Xuất:**
   - Truy vấn từ bảng `LichSuTonKho` (hoặc `Union` từ `ChiTietPhieuNhap` và `ChiTietPhieuXuat` đã hoàn tất), cho phép lọc theo Kho, Hàng hóa, Loại giao dịch (`Nhập`/`Xuất`), Khoảng thời gian.
4. **Refactor Dashboard & Thống kê LINQ (`ThongKeController`):**
   - **Các thẻ KPI tổng quan (Cards):**
     - Tổng số mặt hàng đang kinh doanh; Tổng số kho hoạt động.
     - Số phiếu nhập / phiếu xuất phát sinh trong ngày (`NgayNhap.Date == DateTime.Today`).
     - Số mặt hàng đang **Hết hàng** ($SoLuongTon == 0$) và **Sắp hết hàng** ($SoLuongTon \le MucTonToiThieu$).
   - **Bộ lọc Báo cáo theo Khoảng thời gian (`tuNgay`, `denNgay`):**
     - Sử dụng `LINQ GroupBy` truy vấn trực tiếp từ Database (chỉ tính các phiếu `Đã hoàn tất`):
       1. *Tổng nhập & Tổng xuất theo Hàng hóa / theo Kho / theo Tháng.*
       2. *Top 5 Hàng hóa nhập nhiều nhất & xuất nhiều nhất trong kỳ.*
       3. *Top Nhà cung cấp có tổng giá trị nhập (`Sum(ThanhTien)`) lớn nhất.*
       4. *Top Bộ phận nhận nhiều hàng hóa nhất (`Sum(SoLuongXuat)`).*
       5. *Thống kê số lượng Phiếu nhập / Phiếu xuất theo từng Trạng thái (`GroupBy(p => p.TrangThai)`).*

---

## PHẦN 5: KẾ HOẠCH CHUẨN BỊ DỮ LIỆU MẪU (SEED DATA) ĐẠT CHUẨN MỤC 16

Tạo class `Data/DbInitializer.cs` tự động kiểm tra và nạp dữ liệu ngay khi khởi chạy ứng dụng (`Program.cs`) nếu DB trống, đảm bảo đủ số lượng cho buổi bảo vệ:

| Thực thể | Số lượng tối thiểu theo đề | Kịch bản dữ liệu bắt buộc phải có trong Seed Data |
| :--- | :---: | :--- |
| **Tài khoản (`TaiKhoan`)** | 04 - 05 tài khoản | `admin` (Admin), `nvkho1`, `nvkho2` (Nhân viên kho), `bophan1` (Bộ phận nhận), `locked_user` (Tài khoản bị khóa `TrangThai = false` để test chặn đăng nhập). |
| **Loại hàng (`LoaiHang`)** | $\ge 05$ loại | Điện tử, Gia dụng, Văn phòng phẩm, Linh kiện máy tính, Thiết bị mạng, Hàng ngừng KD (`TrangThai = false`). |
| **Đơn vị tính (`DonViTinh`)** | $\ge 05$ ĐVT | Cái, Chiếc, Bộ, Thùng, Hộp, Cuộn. |
| **Kho (`Kho`)** | $\ge 03$ kho | Kho Tổng Hà Nội, Kho Chi Nhánh Cầu Giấy, Kho Linh Kiện Hoàng Mai (+ 01 Kho bảo trì `TrangThai = false`). |
| **Nhà cung cấp (`NhaCungCap`)** | 08 - 10 NCC | Đầy đủ SĐT, Email, Địa chỉ; có 1-2 NCC ngừng hợp tác (`TrangThai = false`). |
| **Bộ phận nhận (`BoPhanNhan`)** | 06 - 08 bộ phận | Phòng Kỹ thuật, Phòng Kinh doanh, Phòng Hành chính, Bộ phận Bảo hành, Showroom 1, Showroom 2... |
| **Hàng hóa (`HangHoa`)** | **35 - 40 mặt hàng** | Đảm bảo phân trang có ít nhất 3-4 trang (với `pageSize = 10`). Có mặt hàng giá cao/thấp, mức tồn tối thiểu đa dạng (5, 10, 20). |
| **Phiếu nhập (`PhieuNhap`)** | **30 - 40 phiếu** | Trải đều trong 3-6 tháng gần nhất; đủ 4 trạng thái (`Nháp`, `Chờ xác nhận`, `Đã hoàn tất`, `Đã hủy`), mỗi phiếu có 2-4 dòng chi tiết. |
| **Phiếu xuất (`PhieuXuat`)** | **30 - 40 phiếu** | Trải đều theo các bộ phận và kho; đủ 4 trạng thái. |
| **Tồn kho (`TonKho`)** | Khớp 100% lịch sử | Sau khi chạy Seed Nhập/Xuất, dữ liệu tồn kho phải tự động tạo ra đủ 3 nhóm: **Còn nhiều** ($Ton > MucTonToiThieu$), **Sắp hết** ($0 < Ton \le MucTonToiThieu$), và **Hết hàng** ($Ton == 0$). |

---

## PHẦN 6: QUY TẮC GHI COMMENT ĐẦU FILE & QUY TRÌNH COMMIT GITHUB

Đây là yêu cầu **bắt buộc (Mục 18 & 21)** để giảng viên đối chiếu điểm cá nhân:

### 6.1. Mẫu Comment bắt buộc ở đầu MỌI file code (`.cs`, `.cshtml`)
```csharp
// Họ và tên: [Họ và tên Sinh viên phụ trách]
// Mã sinh viên: [Mã số SV]
// Nội dung thực hiện: [Mô tả ngắn gọn chức năng của file thuộc Module mấy]
```
*(Nếu 1 file dùng chung như `Program.cs`, `AppDbContext.cs`, `_Layout.cshtml` có nhiều người cùng đóng góp, liệt kê đầy đủ từng sinh viên và phần việc tương ứng ở đầu file).*

### 6.2. Lộ trình Refactor & Commit lên nhánh `main` (Chia làm 6 Đợt - Sprints)
Để lịch sử Git trên nhánh `main` đẹp, tự nhiên và thể hiện đúng tiến độ làm việc của cả 5 thành viên (không dồn 1 commit cuối):

- **Đợt 1 (Chuẩn hóa nền tảng & Database):**
  - Cập nhật `.gitignore`, xóa file rác `bin/obj`, đồng bộ tên Solution/Project/Namespace `QuanLyKhoHang_UNETI01_TI17A3HN` trên `.NET 10`.
  - Từng sinh viên commit các `Entity` thuộc Module của mình.
  - Tạo `AppDbContext`, chạy Migration `InitialCreate` trên EF Core 10.
  - *Mẫu commit:* `[22103100xxx] [Database] Khoi tao Entity Kho, HangHoa va cau hinh FluentAPI`
- **Đợt 2 (Xây dựng Module 1 & Module 2):**
  - SV1 commit `TaiKhoanController`, Session Login, `AuthorizeRoleAttribute`, CRUD `LoaiHang`, `DonViTinh`.
  - SV2 commit CRUD `Kho`, `HangHoa` và bộ máy `Search + Filter + Sort + Pagination` bằng LINQ.
  - *Mẫu commit:* `[22103100xxx] [HangHoa] Hoan thien tim kiem, loc, sap xep va phan trang LINQ`
- **Đợt 3 (Xây dựng Module 3 - Nhập kho):**
  - SV3 commit CRUD `NhaCungCap`, Master-Detail `PhieuNhap`, tự động tính `ThanhTien`/`TongTienNhap` và Transaction `HoanTatNhap` cộng tồn đúng 1 lần.
  - *Mẫu commit:* `[22103100xxx] [PhieuNhap] Xu ly nghiep vu hoan tat phieu nhap va cong ton kho`
- **Đợt 4 (Xây dựng Module 4 - Xuất kho):**
  - SV4 commit CRUD `BoPhanNhan`, Master-Detail `PhieuXuat`, truy vấn kiểm tra $SoLuongXuat \le SoLuongTon$, chặn xuất âm và Transaction `HoanTatXuat`.
  - *Mẫu commit:* `[22103100xxx] [PhieuXuat] Kiem tra ton kho truoc khi xuat va chan xuat vuot ton`
- **Đợt 5 (Xây dựng Module 5 - Tồn kho, Cảnh báo & Thống kê):**
  - SV5 commit `TonKhoController`, `LichSuTonKho`, cảnh báo sắp hết/hết hàng và `ThongKeController` (Dashboard + Báo cáo `GroupBy` LINQ theo thời gian).
  - *Mẫu commit:* `[22103100xxx] [ThongKe] Xay dung Dashboard, canh bao ton va thong ke LINQ theo ky`
- **Đợt 6 (Nạp Seed Data, Chuẩn hóa UI Razor View & Kiểm thử chéo):**
  - Tích hợp `DbInitializer.cs` (35+ hàng hóa, 35+ phiếu nhập, 35+ phiếu xuất).
  - Việt hóa và làm đẹp toàn bộ Razor Views (Bootstrap 5, Badge trạng thái, định dạng tiền tệ `VNĐ`, ẩn/hiện nút theo đúng trạng thái phiếu và phân quyền).
  - Rà soát Header Comment Họ tên/MSSV trên toàn bộ các file và cập nhật `README.md` hướng dẫn cài đặt.

---

## PHẦN 7: DANH SÁCH KIỂM TRA (CHECKLIST) TRƯỚC KHI NỘP BÀI & BẢO VỆ

| STT | Hạng mục kiểm tra | Yêu cầu đạt được |
| :-: | :--- | :--- |
| 1 | **Phiên bản .NET & EF Core** | `<TargetFramework>net10.0</TargetFramework>`, EF Core `10.0.x`, dùng Code First & có thư mục `Migrations`. |
| 2 | **Bảo mật & Phân quyền** | Chưa đăng nhập gõ thẳng URL `/HangHoa` hay `/ThongKe` phải bị đẩy về `/TaiKhoan/DangNhap`. Nhân viên kho không vào được trang quản lý Tài khoản của Admin. |
| 3 | **Tìm kiếm, Lọc, Sắp xếp, Phân trang** | Chạy hoàn toàn bằng LINQ (`Where` -> `OrderBy` -> `Skip` -> `Take`). Khi bấm sang Trang 2, 3 không bị mất từ khóa tìm kiếm hay tiêu chí lọc. |
| 4 | **Nghiệp vụ Nhập kho** | `ThanhTien` hệ thống tự tính. Phiếu `Nháp` chưa cộng tồn. Bấm `Hoàn tất` cộng tồn đúng 1 lần. Bấm hoàn tất lần 2 bị chặn. |
| 5 | **Nghiệp vụ Xuất kho** | Chọn số lượng xuất > tồn thực tế tại kho đó -> Báo lỗi ngay, không cho lưu/hoàn tất. Không bao giờ xảy ra tồn âm. |
| 6 | **Công thức Tồn kho** | $TonCuoi = TonDau + TongNhap - TongXuat$ khớp 100% với bảng `TonKho` và `LichSuTonKho`. Không có ô input nào cho sửa tay số lượng tồn. |
| 7 | **Quy chuẩn Cá nhân** | 100% file có Comment `Họ và tên`, `Mã sinh viên`, `Nội dung thực hiện`. Cả 5 thành viên đều có lịch sử Commit chuẩn cú pháp trên nhánh `main`. |