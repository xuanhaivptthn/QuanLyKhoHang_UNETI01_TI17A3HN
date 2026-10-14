// Họ và tên: Nguyễn Văn Cường
// Mã sinh viên: 23103100132
// Phụ trách Module 5: ViewModel Tổng hợp thẻ KPI, Cảnh báo & Báo cáo LINQ

using System;
using System.Collections.Generic;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels
{
    public class DashboardThongKeViewModel : DashboardViewModel
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public decimal TongGiaTriNhap { get; set; }
        public decimal TongGiaTriXuat { get; set; }
        public List<ThongKeHangHoaItem> TopHangNhap { get; set; } = new();
        public List<ThongKeHangHoaItem> TopHangXuat { get; set; } = new();
        public List<ThongKeNhaCungCapItem> TopNhaCungCap { get; set; } = new();
        public List<ThongKeBoPhanItem> TopBoPhanNhan { get; set; } = new();
    }

    public class ThongKeHangHoaItem
    {
        public string TenHang { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class ThongKeNhaCungCapItem
    {
        public string TenNhaCungCap { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
    }

    public class ThongKeBoPhanItem
    {
        public string TenBoPhan { get; set; } = string.Empty;
        public int TongSoLuong { get; set; }
    }
}
