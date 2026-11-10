namespace QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels
{
    // Họ và tên: Nguyễn Văn Cường
    // Mã sinh viên: 23103100132
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.
    public class DashboardViewModel
    {
        // 4 KPI chính hôm nay & hệ thống
        public int TongSoHangHoa { get; set; }
        public int TongSoKho { get; set; }
        public int SoPhieuNhapTrongNgay { get; set; }
        public int SoPhieuXuatTrongNgay { get; set; }
        public int SoHangHet { get; set; }
        public int SoHangSapHet { get; set; }

        // Báo cáo thống kê toàn diện bằng LINQ tích hợp trực tiếp vào Dashboard
        public BaoCaoNhapXuatViewModel BaoCao { get; set; } = new();
    }
}
