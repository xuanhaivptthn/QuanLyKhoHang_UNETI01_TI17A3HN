using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Filters;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    // Họ và tên: Nguyễn Văn Cường
    // Mã sinh viên: 23103100132
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.
    [AuthorizeRole("Admin", "NhanVienKho")]
    public class ThongKeController : Controller
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /ThongKe
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // Yêu cầu 9.5 & 9.6: Dashboard tích hợp Báo cáo Thống kê Toàn diện bằng LINQ
        // • 4 KPI tổng quan hệ thống & cảnh báo khẩn cấp
        // • Tổng nhập theo hàng/kho/tháng
        // • Tổng xuất theo hàng/kho/tháng
        // • Tồn hiện tại
        // • Hàng nhập nhiều nhất
        // • Hàng xuất nhiều nhất
        // • Nhà cung cấp có giá trị nhập lớn
        // • Bộ phận nhận nhiều hàng
        // • Số phiếu theo trạng thái
        public async Task<IActionResult> Dashboard(DateTime? tuNgay, DateTime? denNgay, int? maKho)
        {
            var today = DateTime.Today;

            // Mặc định từ đầu năm đến ngày hiện tại để bao quát toàn bộ dữ liệu mẫu trong kỳ
            if (!tuNgay.HasValue) tuNgay = new DateTime(today.Year, 1, 1);
            if (!denNgay.HasValue) denNgay = today;

            var fromDate = tuNgay.Value.Date;
            var toDateExclusive = denNgay.Value.Date.AddDays(1);

            ViewBag.TuNgay = tuNgay.Value.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay.Value.ToString("yyyy-MM-dd");
            ViewBag.MaKho = maKho;

            // Danh sách kho để hiển thị bộ lọc
            var danhSachKho = await _context.Khoes
                .Where(k => k.TrangThai == true)
                .OrderBy(k => k.TenKho)
                .ToListAsync();
            ViewBag.DanhSachKho = danhSachKho;

            string? tenKhoChon = null;
            if (maKho.HasValue && maKho.Value > 0)
            {
                tenKhoChon = danhSachKho.FirstOrDefault(k => k.MaKho == maKho.Value)?.TenKho;
            }

            // =========================================================================
            // KHỐI 1: 4 CHỈ SỐ KPI CHÍNH & 2 CẢNH BÁO TỒN KHO HÔM NAY (LINQ)
            // =========================================================================
            var tongSoKho = await _context.Khoes.Where(k => k.TrangThai == true).CountAsync();
            var soPhieuXuatTrongNgay = await _context.PhieuXuats.Where(p => p.NgayXuat.Date == today).CountAsync();
            var soHangHet = await _context.TonKhoes.Where(t => t.SoLuongTon == 0).CountAsync();
            var tongSoHangHoa = await _context.HangHoa.CountAsync();
            var soPhieuNhapTrongNgay = await _context.PhieuNhap.Where(p => p.NgayNhap.Date == today).CountAsync();
            var soHangSapHet = await _context.TonKhoes
                .Include(t => t.HangHoa)
                .Where(t => t.SoLuongTon > 0 && t.HangHoa != null && t.SoLuongTon <= t.HangHoa.MucTonToiThieu)
                .CountAsync();

            // =========================================================================
            // KHỐI 2: TRUY VẤN LINQ THỐNG KÊ TOÀN DIỆN
            // =========================================================================

            // A. Phiếu Nhập & Chi tiết Nhập hoàn tất trong kỳ
            var queryPhieuNhap = _context.PhieuNhap
                .Where(p => p.NgayNhap >= fromDate && p.NgayNhap < toDateExclusive);

            if (maKho.HasValue && maKho.Value > 0)
            {
                queryPhieuNhap = queryPhieuNhap.Where(p => p.MaKho == maKho.Value);
            }

            var queryChiTietNhapHoanTat = _context.ChiTietPhieuNhap
                .Where(c => c.PhieuNhap != null
                         && c.PhieuNhap.TrangThai == TrangThaiPhieuNhap.DaHoanTat
                         && c.PhieuNhap.NgayNhap >= fromDate
                         && c.PhieuNhap.NgayNhap < toDateExclusive);

            if (maKho.HasValue && maKho.Value > 0)
            {
                queryChiTietNhapHoanTat = queryChiTietNhapHoanTat.Where(c => c.PhieuNhap!.MaKho == maKho.Value);
            }

            // B. Phiếu Xuất & Chi tiết Xuất hoàn tất trong kỳ
            var queryPhieuXuat = _context.PhieuXuats
                .Where(p => p.NgayXuat >= fromDate && p.NgayXuat < toDateExclusive);

            if (maKho.HasValue && maKho.Value > 0)
            {
                queryPhieuXuat = queryPhieuXuat.Where(p => p.MaKho == maKho.Value);
            }

            var queryChiTietXuatHoanTat = _context.ChiTietPhieuXuats
                .Where(c => c.PhieuXuat != null
                         && c.PhieuXuat.TrangThai == TrangThaiPhieuXuat.DaHoanTat
                         && c.PhieuXuat.NgayXuat >= fromDate
                         && c.PhieuXuat.NgayXuat < toDateExclusive);

            if (maKho.HasValue && maKho.Value > 0)
            {
                queryChiTietXuatHoanTat = queryChiTietXuatHoanTat.Where(c => c.PhieuXuat!.MaKho == maKho.Value);
            }

            // 1. LINQ: Tổng nhập theo hàng / kho / tháng
            // 1.1. Theo Hàng hóa
            var tongNhapTheoHang = await queryChiTietNhapHoanTat
                .GroupBy(c => new {
                    c.MaHang,
                    TenHang = c.HangHoa != null ? c.HangHoa.TenHang : "N/A",
                    DonViTinh = (c.HangHoa != null && c.HangHoa.DonViTinh != null) ? c.HangHoa.DonViTinh.TenDonViTinh : "",
                    LoaiHang = (c.HangHoa != null && c.HangHoa.LoaiHang != null) ? c.HangHoa.LoaiHang.TenLoaiHang : ""
                })
                .Select(g => new ThongKeHangHoaItem
                {
                    MaHang = g.Key.MaHang,
                    TenHang = g.Key.TenHang,
                    DonViTinh = g.Key.DonViTinh,
                    LoaiHang = g.Key.LoaiHang,
                    TongSoLuong = g.Sum(c => c.SoLuongNhap),
                    TongGiaTri = g.Sum(c => (decimal)c.SoLuongNhap * c.DonGiaNhap),
                    SoLanGiaoDich = g.Count()
                })
                .OrderByDescending(x => x.TongSoLuong)
                .ToListAsync();

            // 1.2. Theo Kho
            var tongNhapTheoKho = await queryChiTietNhapHoanTat
                .GroupBy(c => new {
                    c.PhieuNhap!.MaKho,
                    TenKho = c.PhieuNhap.Kho != null ? c.PhieuNhap.Kho.TenKho : "N/A",
                    DiaDiem = c.PhieuNhap.Kho != null ? c.PhieuNhap.Kho.DiaDiem : ""
                })
                .Select(g => new ThongKeKhoItem
                {
                    MaKho = g.Key.MaKho,
                    TenKho = g.Key.TenKho,
                    DiaDiem = g.Key.DiaDiem ?? "",
                    TongSoLuong = g.Sum(c => c.SoLuongNhap),
                    TongGiaTri = g.Sum(c => (decimal)c.SoLuongNhap * c.DonGiaNhap),
                    SoPhieu = g.Select(c => c.MaPhieuNhap).Distinct().Count()
                })
                .OrderByDescending(x => x.TongGiaTri)
                .ToListAsync();

            // 1.3. Theo Tháng
            var rawNhapTheoThang = await queryChiTietNhapHoanTat
                .Select(c => new {
                    NgayNhap = c.PhieuNhap!.NgayNhap,
                    c.MaPhieuNhap,
                    c.SoLuongNhap,
                    ThanhTien = (decimal)c.SoLuongNhap * c.DonGiaNhap
                })
                .ToListAsync();

            var tongNhapTheoThang = rawNhapTheoThang
                .GroupBy(x => new { x.NgayNhap.Year, x.NgayNhap.Month })
                .Select(g => new ThongKeThangItem
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    TongSoLuong = g.Sum(x => x.SoLuongNhap),
                    TongGiaTri = g.Sum(x => x.ThanhTien),
                    SoPhieu = g.Select(x => x.MaPhieuNhap).Distinct().Count()
                })
                .OrderBy(x => x.Nam).ThenBy(x => x.Thang)
                .ToList();

            // 2. LINQ: Tổng xuất theo hàng / kho / tháng
            // 2.1. Theo Hàng hóa
            var tongXuatTheoHang = await queryChiTietXuatHoanTat
                .GroupBy(c => new {
                    c.MaHang,
                    TenHang = c.HangHoa != null ? c.HangHoa.TenHang : "N/A",
                    DonViTinh = (c.HangHoa != null && c.HangHoa.DonViTinh != null) ? c.HangHoa.DonViTinh.TenDonViTinh : "",
                    LoaiHang = (c.HangHoa != null && c.HangHoa.LoaiHang != null) ? c.HangHoa.LoaiHang.TenLoaiHang : ""
                })
                .Select(g => new ThongKeHangHoaItem
                {
                    MaHang = g.Key.MaHang,
                    TenHang = g.Key.TenHang,
                    DonViTinh = g.Key.DonViTinh,
                    LoaiHang = g.Key.LoaiHang,
                    TongSoLuong = g.Sum(c => c.SoLuongXuat),
                    TongGiaTri = g.Sum(c => (decimal)c.SoLuongXuat * c.DonGiaXuatThamChieu),
                    SoLanGiaoDich = g.Count()
                })
                .OrderByDescending(x => x.TongSoLuong)
                .ToListAsync();

            // 2.2. Theo Kho
            var tongXuatTheoKho = await queryChiTietXuatHoanTat
                .GroupBy(c => new {
                    c.PhieuXuat!.MaKho,
                    TenKho = c.PhieuXuat.Kho != null ? c.PhieuXuat.Kho.TenKho : "N/A",
                    DiaDiem = c.PhieuXuat.Kho != null ? c.PhieuXuat.Kho.DiaDiem : ""
                })
                .Select(g => new ThongKeKhoItem
                {
                    MaKho = g.Key.MaKho,
                    TenKho = g.Key.TenKho,
                    DiaDiem = g.Key.DiaDiem ?? "",
                    TongSoLuong = g.Sum(c => c.SoLuongXuat),
                    TongGiaTri = g.Sum(c => (decimal)c.SoLuongXuat * c.DonGiaXuatThamChieu),
                    SoPhieu = g.Select(c => c.MaPhieuXuat).Distinct().Count()
                })
                .OrderByDescending(x => x.TongGiaTri)
                .ToListAsync();

            // 2.3. Theo Tháng
            var rawXuatTheoThang = await queryChiTietXuatHoanTat
                .Select(c => new {
                    NgayXuat = c.PhieuXuat!.NgayXuat,
                    c.MaPhieuXuat,
                    c.SoLuongXuat,
                    ThanhTien = (decimal)c.SoLuongXuat * c.DonGiaXuatThamChieu
                })
                .ToListAsync();

            var tongXuatTheoThang = rawXuatTheoThang
                .GroupBy(x => new { x.NgayXuat.Year, x.NgayXuat.Month })
                .Select(g => new ThongKeThangItem
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    TongSoLuong = g.Sum(x => x.SoLuongXuat),
                    TongGiaTri = g.Sum(x => x.ThanhTien),
                    SoPhieu = g.Select(x => x.MaPhieuXuat).Distinct().Count()
                })
                .OrderBy(x => x.Nam).ThenBy(x => x.Thang)
                .ToList();

            // 3. LINQ: Tồn hiện tại
            var queryTonKho = _context.TonKhoes
                .Include(t => t.Kho)
                .Include(t => t.HangHoa)
                    .ThenInclude(h => h!.LoaiHang)
                .Include(t => t.HangHoa)
                    .ThenInclude(h => h!.DonViTinh)
                .AsQueryable();

            if (maKho.HasValue && maKho.Value > 0)
            {
                queryTonKho = queryTonKho.Where(t => t.MaKho == maKho.Value);
            }

            var rawTonKho = await queryTonKho.ToListAsync();

            var danhSachTonHienTai = rawTonKho
                .Select(t => new ThongKeTonHienTaiItem
                {
                    MaKho = t.MaKho,
                    TenKho = t.Kho?.TenKho ?? "N/A",
                    MaHang = t.MaHang,
                    TenHang = t.HangHoa?.TenHang ?? "N/A",
                    DonViTinh = t.HangHoa?.DonViTinh?.TenDonViTinh ?? "",
                    LoaiHang = t.HangHoa?.LoaiHang?.TenLoaiHang ?? "",
                    SoLuongTon = t.SoLuongTon,
                    MucTonToiThieu = t.HangHoa?.MucTonToiThieu ?? 0,
                    DonGiaThamKhao = t.HangHoa?.GiaNhapThamKhao ?? 0,
                    NgayCapNhat = t.NgayCapNhat
                })
                .OrderBy(x => x.TenKho).ThenBy(x => x.TenHang)
                .ToList();

            var danhSachTonTheoKho = rawTonKho
                .GroupBy(t => new {
                    t.MaKho,
                    TenKho = t.Kho?.TenKho ?? "N/A",
                    DiaDiem = t.Kho?.DiaDiem ?? ""
                })
                .Select(g => new ThongKeTonTheoKhoItem
                {
                    MaKho = g.Key.MaKho,
                    TenKho = g.Key.TenKho,
                    DiaDiem = g.Key.DiaDiem,
                    SoMatHang = g.Count(),
                    TongSoLuongTon = g.Sum(x => x.SoLuongTon),
                    TongGiaTriTon = g.Sum(x => (decimal)x.SoLuongTon * (x.HangHoa?.GiaNhapThamKhao ?? 0)),
                    SoHangSapHet = g.Count(x => x.SoLuongTon > 0 && x.HangHoa != null && x.SoLuongTon <= x.HangHoa.MucTonToiThieu),
                    SoHangHet = g.Count(x => x.SoLuongTon == 0)
                })
                .OrderBy(x => x.TenKho)
                .ToList();

            // 4. LINQ: Hàng nhập nhiều nhất (Top 10)
            var topHangNhap = tongNhapTheoHang
                .Take(10)
                .Select(x => new TopHangHoaItem
                {
                    MaHang = x.MaHang,
                    TenHang = x.TenHang,
                    DonViTinh = x.DonViTinh,
                    LoaiHang = x.LoaiHang,
                    TongSoLuong = x.TongSoLuong,
                    TongGiaTri = x.TongGiaTri,
                    SoPhieu = x.SoLanGiaoDich
                })
                .ToList();

            // 5. LINQ: Hàng xuất nhiều nhất (Top 10)
            var topHangXuat = tongXuatTheoHang
                .Take(10)
                .Select(x => new TopHangHoaItem
                {
                    MaHang = x.MaHang,
                    TenHang = x.TenHang,
                    DonViTinh = x.DonViTinh,
                    LoaiHang = x.LoaiHang,
                    TongSoLuong = x.TongSoLuong,
                    TongGiaTri = x.TongGiaTri,
                    SoPhieu = x.SoLanGiaoDich
                })
                .ToList();

            // 6. LINQ: Nhà cung cấp có giá trị nhập lớn
            var topNhaCungCap = await queryChiTietNhapHoanTat
                .GroupBy(c => new {
                    c.PhieuNhap!.MaNhaCungCap,
                    TenNhaCungCap = c.PhieuNhap.NhaCungCap != null ? c.PhieuNhap.NhaCungCap.TenNhaCungCap : "N/A",
                    SoDienThoai = c.PhieuNhap.NhaCungCap != null ? c.PhieuNhap.NhaCungCap.SoDienThoai : "",
                    DiaChi = c.PhieuNhap.NhaCungCap != null ? c.PhieuNhap.NhaCungCap.DiaChi : ""
                })
                .Select(g => new ThongKeNhaCungCapItem
                {
                    MaNhaCungCap = g.Key.MaNhaCungCap,
                    TenNhaCungCap = g.Key.TenNhaCungCap,
                    SoDienThoai = g.Key.SoDienThoai ?? "",
                    DiaChi = g.Key.DiaChi ?? "",
                    SoPhieuNhap = g.Select(c => c.MaPhieuNhap).Distinct().Count(),
                    TongSoLuongNhap = g.Sum(c => c.SoLuongNhap),
                    TongGiaTriNhap = g.Sum(c => (decimal)c.SoLuongNhap * c.DonGiaNhap)
                })
                .OrderByDescending(x => x.TongGiaTriNhap)
                .Take(10)
                .ToListAsync();

            // 7. LINQ: Bộ phận nhận nhiều hàng
            var topBoPhanNhan = await queryChiTietXuatHoanTat
                .GroupBy(c => new {
                    c.PhieuXuat!.MaBoPhan,
                    TenBoPhan = c.PhieuXuat.BoPhanNhan != null ? c.PhieuXuat.BoPhanNhan.TenBoPhan : "N/A",
                    NguoiDaiDien = c.PhieuXuat.BoPhanNhan != null ? c.PhieuXuat.BoPhanNhan.NguoiDaiDien : "",
                    SoDienThoai = c.PhieuXuat.BoPhanNhan != null ? c.PhieuXuat.BoPhanNhan.SoDienThoai : ""
                })
                .Select(g => new ThongKeBoPhanNhanItem
                {
                    MaBoPhan = g.Key.MaBoPhan,
                    TenBoPhan = g.Key.TenBoPhan,
                    NguoiDaiDien = g.Key.NguoiDaiDien ?? "",
                    SoDienThoai = g.Key.SoDienThoai ?? "",
                    SoPhieuXuat = g.Select(c => c.MaPhieuXuat).Distinct().Count(),
                    TongSoLuongNhan = g.Sum(c => c.SoLuongXuat),
                    TongGiaTriXuat = g.Sum(c => (decimal)c.SoLuongXuat * c.DonGiaXuatThamChieu)
                })
                .OrderByDescending(x => x.TongSoLuongNhan)
                .Take(10)
                .ToListAsync();

            // 8. LINQ: Số phiếu theo trạng thái (Phiếu Nhập & Phiếu Xuất)
            var rawTrangThaiNhap = await queryPhieuNhap
                .GroupBy(p => p.TrangThai)
                .Select(g => new { TrangThai = g.Key, Count = g.Count() })
                .ToListAsync();

            int tongPhieuNhapKy = rawTrangThaiNhap.Sum(x => x.Count);
            var soPhieuNhapTheoTrangThai = Enum.GetValues<TrangThaiPhieuNhap>().Select(tt => {
                int cnt = rawTrangThaiNhap.FirstOrDefault(x => x.TrangThai == tt)?.Count ?? 0;
                double pct = tongPhieuNhapKy > 0 ? Math.Round((double)cnt * 100 / tongPhieuNhapKy, 1) : 0;
                string badge = tt switch {
                    TrangThaiPhieuNhap.Nhap => "bg-secondary",
                    TrangThaiPhieuNhap.ChoXacNhan => "bg-warning text-dark",
                    TrangThaiPhieuNhap.DaHoanTat => "bg-success",
                    TrangThaiPhieuNhap.DaHuy => "bg-danger",
                    _ => "bg-light text-dark"
                };
                string ten = tt switch {
                    TrangThaiPhieuNhap.Nhap => "Nháp",
                    TrangThaiPhieuNhap.ChoXacNhan => "Chờ xác nhận",
                    TrangThaiPhieuNhap.DaHoanTat => "Đã hoàn tất",
                    TrangThaiPhieuNhap.DaHuy => "Đã hủy",
                    _ => tt.ToString()
                };
                return new ThongKeTrangThaiPhieuItem
                {
                    TrangThaiId = (int)tt,
                    TenTrangThai = ten,
                    SoLuong = cnt,
                    TiLePhanTram = pct,
                    BadgeClass = badge
                };
            }).ToList();

            var rawTrangThaiXuat = await queryPhieuXuat
                .GroupBy(p => p.TrangThai)
                .Select(g => new { TrangThai = g.Key, Count = g.Count() })
                .ToListAsync();

            int tongPhieuXuatKy = rawTrangThaiXuat.Sum(x => x.Count);
            var soPhieuXuatTheoTrangThai = Enum.GetValues<TrangThaiPhieuXuat>().Select(tt => {
                int cnt = rawTrangThaiXuat.FirstOrDefault(x => x.TrangThai == tt)?.Count ?? 0;
                double pct = tongPhieuXuatKy > 0 ? Math.Round((double)cnt * 100 / tongPhieuXuatKy, 1) : 0;
                string badge = tt switch {
                    TrangThaiPhieuXuat.Nhap => "bg-secondary",
                    TrangThaiPhieuXuat.ChoXacNhan => "bg-warning text-dark",
                    TrangThaiPhieuXuat.DaHoanTat => "bg-success",
                    TrangThaiPhieuXuat.DaHuy => "bg-danger",
                    _ => "bg-light text-dark"
                };
                string ten = tt switch {
                    TrangThaiPhieuXuat.Nhap => "Nháp",
                    TrangThaiPhieuXuat.ChoXacNhan => "Chờ xác nhận",
                    TrangThaiPhieuXuat.DaHoanTat => "Đã hoàn tất",
                    TrangThaiPhieuXuat.DaHuy => "Đã hủy",
                    _ => tt.ToString()
                };
                return new ThongKeTrangThaiPhieuItem
                {
                    TrangThaiId = (int)tt,
                    TenTrangThai = ten,
                    SoLuong = cnt,
                    TiLePhanTram = pct,
                    BadgeClass = badge
                };
            }).ToList();

            // Tổng hợp ViewModel hoàn chỉnh
            var baoCao = new BaoCaoNhapXuatViewModel
            {
                TuNgay = fromDate,
                DenNgay = denNgay.Value.Date,
                MaKho = maKho,
                TenKhoChon = tenKhoChon,

                // KPI Nhập
                TongSoLuongNhap = tongNhapTheoHang.Sum(x => x.TongSoLuong),
                TongGiaTriNhap = tongNhapTheoHang.Sum(x => x.TongGiaTri),
                TongSoPhieuNhap = tongPhieuNhapKy,

                // KPI Xuất
                TongSoLuongXuat = tongXuatTheoHang.Sum(x => x.TongSoLuong),
                TongGiaTriXuat = tongXuatTheoHang.Sum(x => x.TongGiaTri),
                TongSoPhieuXuat = tongPhieuXuatKy,

                // Tồn kho
                TongSoLuongTon = danhSachTonHienTai.Sum(x => x.SoLuongTon),
                TongGiaTriTon = danhSachTonHienTai.Sum(x => x.GiaTriTonKho),

                // Danh sách thống kê LINQ
                TongNhapTheoHang = tongNhapTheoHang,
                TongNhapTheoKho = tongNhapTheoKho,
                TongNhapTheoThang = tongNhapTheoThang,

                TongXuatTheoHang = tongXuatTheoHang,
                TongXuatTheoKho = tongXuatTheoKho,
                TongXuatTheoThang = tongXuatTheoThang,

                DanhSachTonHienTai = danhSachTonHienTai,
                DanhSachTonTheoKho = danhSachTonTheoKho,

                TopHangNhap = topHangNhap,
                TopHangXuat = topHangXuat,
                TopNhaCungCap = topNhaCungCap,
                TopBoPhanNhan = topBoPhanNhan,

                SoPhieuNhapTheoTrangThai = soPhieuNhapTheoTrangThai,
                SoPhieuXuatTheoTrangThai = soPhieuXuatTheoTrangThai
            };

            var viewModel = new DashboardViewModel
            {
                TongSoKho = tongSoKho,
                SoPhieuXuatTrongNgay = soPhieuXuatTrongNgay,
                SoHangHet = soHangHet,
                TongSoHangHoa = tongSoHangHoa,
                SoPhieuNhapTrongNgay = soPhieuNhapTrongNgay,
                SoHangSapHet = soHangSapHet,
                BaoCao = baoCao
            };

            // Gán ViewBag tương thích ngược
            ViewBag.TopHangNhap = topHangNhap.Select(x => new { TenHang = x.TenHang, TongSo = x.TongSoLuong }).ToList();
            ViewBag.TopHangXuat = topHangXuat.Select(x => new { TenHang = x.TenHang, TongSo = x.TongSoLuong }).ToList();
            ViewBag.TrangThaiPhieu = rawTrangThaiNhap.Select(x => new { TenTrangThai = x.TrangThai, SoLuong = x.Count }).ToList();

            return View("Dashboard", viewModel);
        }

        // Điều hướng tương thích ngược nếu người dùng hoặc liên kết cũ truy cập /ThongKe/BaoCaoNhapXuat
        public async Task<IActionResult> BaoCaoNhapXuat(DateTime? tuNgay, DateTime? denNgay, int? maKho)
        {
            return await Dashboard(tuNgay, denNgay, maKho);
        }
    }
}
