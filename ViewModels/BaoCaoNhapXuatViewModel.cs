using System;
using System.Collections.Generic;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels
{
    // Họ và tên: Nguyễn Văn Cường
    // Mã sinh viên: 23103100132
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.
    public class BaoCaoNhapXuatViewModel
    {
        // Điều kiện lọc báo cáo
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public int? MaKho { get; set; }
        public string? TenKhoChon { get; set; }

        // Khối chỉ số KPI tổng hợp theo kỳ
        public int TongSoLuongNhap { get; set; }
        public decimal TongGiaTriNhap { get; set; }
        public int TongSoPhieuNhap { get; set; }

        public int TongSoLuongXuat { get; set; }
        public decimal TongGiaTriXuat { get; set; }
        public int TongSoPhieuXuat { get; set; }

        public int TongSoLuongTon { get; set; }
        public decimal TongGiaTriTon { get; set; }

        // 1. Tổng nhập theo hàng / kho / tháng (LINQ GroupBy)
        public List<ThongKeHangHoaItem> TongNhapTheoHang { get; set; } = new();
        public List<ThongKeKhoItem> TongNhapTheoKho { get; set; } = new();
        public List<ThongKeThangItem> TongNhapTheoThang { get; set; } = new();

        // 2. Tổng xuất theo hàng / kho / tháng (LINQ GroupBy)
        public List<ThongKeHangHoaItem> TongXuatTheoHang { get; set; } = new();
        public List<ThongKeKhoItem> TongXuatTheoKho { get; set; } = new();
        public List<ThongKeThangItem> TongXuatTheoThang { get; set; } = new();

        // 3. Tồn hiện tại (LINQ Where, Include, OrderBy)
        public List<ThongKeTonHienTaiItem> DanhSachTonHienTai { get; set; } = new();
        public List<ThongKeTonTheoKhoItem> DanhSachTonTheoKho { get; set; } = new();

        // 4. Hàng nhập nhiều nhất (LINQ GroupBy, OrderByDescending, Take)
        public List<TopHangHoaItem> TopHangNhap { get; set; } = new();

        // 5. Hàng xuất nhiều nhất (LINQ GroupBy, OrderByDescending, Take)
        public List<TopHangHoaItem> TopHangXuat { get; set; } = new();

        // 6. Nhà cung cấp có giá trị nhập lớn (LINQ GroupBy, Sum, OrderByDescending)
        public List<ThongKeNhaCungCapItem> TopNhaCungCap { get; set; } = new();

        // 7. Bộ phận nhận nhiều hàng (LINQ GroupBy, Sum, OrderByDescending)
        public List<ThongKeBoPhanNhanItem> TopBoPhanNhan { get; set; } = new();

        // 8. Số phiếu theo trạng thái (LINQ GroupBy theo TrangThai)
        public List<ThongKeTrangThaiPhieuItem> SoPhieuNhapTheoTrangThai { get; set; } = new();
        public List<ThongKeTrangThaiPhieuItem> SoPhieuXuatTheoTrangThai { get; set; } = new();
    }

    // Thống kê theo Hàng hóa (Nhập / Xuất)
    public class ThongKeHangHoaItem
    {
        public int MaHang { get; set; }
        public string TenHang { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public string LoaiHang { get; set; } = string.Empty;
        public int TongSoLuong { get; set; }
        public decimal TongGiaTri { get; set; }
        public int SoLanGiaoDich { get; set; }
    }

    // Thống kê theo Kho (Nhập / Xuất)
    public class ThongKeKhoItem
    {
        public int MaKho { get; set; }
        public string TenKho { get; set; } = string.Empty;
        public string DiaDiem { get; set; } = string.Empty;
        public int TongSoLuong { get; set; }
        public decimal TongGiaTri { get; set; }
        public int SoPhieu { get; set; }
    }

    // Thống kê theo Tháng (Nhập / Xuất)
    public class ThongKeThangItem
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public string ThangNam => $"Tháng {Thang:D2}/{Nam}";
        public int SoPhieu { get; set; }
        public int TongSoLuong { get; set; }
        public decimal TongGiaTri { get; set; }
    }

    // Thống kê Tồn hiện tại chi tiết từng mặt hàng tại từng kho
    public class ThongKeTonHienTaiItem
    {
        public int MaKho { get; set; }
        public string TenKho { get; set; } = string.Empty;
        public int MaHang { get; set; }
        public string TenHang { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public string LoaiHang { get; set; } = string.Empty;
        public int SoLuongTon { get; set; }
        public int MucTonToiThieu { get; set; }
        public decimal DonGiaThamKhao { get; set; }
        public decimal GiaTriTonKho => (decimal)SoLuongTon * DonGiaThamKhao;
        public DateTime NgayCapNhat { get; set; }
        public string TrangThaiTon => SoLuongTon == 0 ? "Hết hàng" : (SoLuongTon <= MucTonToiThieu ? "Sắp hết" : "Đủ hàng");
        public string BadgeClass => SoLuongTon == 0 ? "bg-danger" : (SoLuongTon <= MucTonToiThieu ? "bg-warning text-dark" : "bg-success");
    }

    // Thống kê Tồn hiện tại tổng hợp theo từng Kho
    public class ThongKeTonTheoKhoItem
    {
        public int MaKho { get; set; }
        public string TenKho { get; set; } = string.Empty;
        public string DiaDiem { get; set; } = string.Empty;
        public int SoMatHang { get; set; }
        public int TongSoLuongTon { get; set; }
        public decimal TongGiaTriTon { get; set; }
        public int SoHangSapHet { get; set; }
        public int SoHangHet { get; set; }
    }

    // Top Hàng hóa nhập / xuất nhiều nhất
    public class TopHangHoaItem
    {
        public int MaHang { get; set; }
        public string TenHang { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public string LoaiHang { get; set; } = string.Empty;
        public int TongSoLuong { get; set; }
        public decimal TongGiaTri { get; set; }
        public int SoPhieu { get; set; }
    }

    // Top Nhà cung cấp có giá trị nhập lớn
    public class ThongKeNhaCungCapItem
    {
        public int MaNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public int SoPhieuNhap { get; set; }
        public int TongSoLuongNhap { get; set; }
        public decimal TongGiaTriNhap { get; set; }
    }

    // Top Bộ phận nhận nhiều hàng
    public class ThongKeBoPhanNhanItem
    {
        public int MaBoPhan { get; set; }
        public string TenBoPhan { get; set; } = string.Empty;
        public string NguoiDaiDien { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public int SoPhieuXuat { get; set; }
        public int TongSoLuongNhan { get; set; }
        public decimal TongGiaTriXuat { get; set; }
    }

    // Thống kê phân loại trạng thái phiếu
    public class ThongKeTrangThaiPhieuItem
    {
        public int TrangThaiId { get; set; }
        public string TenTrangThai { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public double TiLePhanTram { get; set; }
        public string BadgeClass { get; set; } = "bg-secondary";
    }
}

