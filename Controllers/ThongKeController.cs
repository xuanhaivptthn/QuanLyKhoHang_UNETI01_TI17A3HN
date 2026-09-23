using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_DHTI17A3HN.Models; // Đổi Namespace
using QuanLyKhoHang_UNETI01_DHTI17A3HN.ViewModels; // Đổi Namespace
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhoHang_UNETI01_DHTI17A3HN.Controllers
{
    // Họ và tên: Nguyễn Văn Cường[cite: 1]
    // Mã sinh viên: 23103100132[cite: 1]
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.[cite: 1]
    public class ThongKeController : Controller
    {
        private readonly QuanLyKhoHang_UNETI01_TI17A3HNContext _context;

        public ThongKeController(QuanLyKhoHang_UNETI01_TI17A3HNContext context)
        {
            _context = context;
        }

        // Yêu cầu 9.5 & 9.6: Dashboard và Thống kê tổng quan (Sử dụng LINQ)
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;

            var viewModel = new DashboardViewModel
            {
                // Tổng số hàng hóa và kho
                TongSoHangHoa = await _context.HangHoas.CountAsync(),
                TongSoKho = await _context.Khoes.CountAsync(),

                // Số phiếu nhập/xuất trong ngày (LINQ: Where)
                SoPhieuNhapTrongNgay = await _context.PhieuNhaps
                    .Where(p => p.NgayNhap.Date == today)
                    .CountAsync(),
                SoPhieuXuatTrongNgay = await _context.PhieuXuats
                    .Where(p => p.NgayXuat.Date == today)
                    .CountAsync(),

                // Cảnh báo tồn: Hàng hết và sắp hết (LINQ: Where)
                // Lưu ý: Tồn kho <= MucTonToiThieu của chính Hàng hóa đó
                SoHangHet = await _context.TonKhoes
                    .Where(t => t.SoLuongTon == 0)
                    .CountAsync(),
                SoHangSapHet = await _context.TonKhoes
                    .Include(t => t.HangHoa) // Join với bảng HangHoa để lấy MucTonToiThieu
                    .Where(t => t.SoLuongTon > 0 && t.SoLuongTon <= t.HangHoa.MucTonToiThieu)
                    .CountAsync(),

                // Tính tổng nhập theo kỳ (Ví dụ: tính trong tháng hiện tại) (LINQ: Where, Sum)
                TongSoLuongNhapTheoKy = await _context.ChiTietPhieuNhaps
                    .Include(c => c.PhieuNhap)
                    .Where(c => c.PhieuNhap.NgayNhap.Month == today.Month && c.PhieuNhap.NgayNhap.Year == today.Year && c.PhieuNhap.TrangThai == "Đã hoàn tất") // Chỉ tính phiếu đã hoàn tất
                    .SumAsync(c => c.SoLuongNhap) // Chú ý: Nếu không có dữ liệu, Sum có thể lỗi null nếu không bắt nullable
            };

            return View(viewModel);
        }

        // Yêu cầu 9.6: Báo cáo Thống kê linh động theo ngày (Sử dụng Date picker trên View)
        public async Task<IActionResult> BaoCaoNhapXuat(DateTime? tuNgay, DateTime? denNgay)
        {
            // Nếu người dùng chưa chọn ngày, mặc định lấy trong tháng này
            if (!tuNgay.HasValue || !denNgay.HasValue)
            {
                tuNgay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                denNgay = DateTime.Today;
            }

            ViewBag.TuNgay = tuNgay.Value.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay.Value.ToString("yyyy-MM-dd");

            // LINQ: Thống kê Hàng nhập nhiều nhất trong khoảng thời gian (GroupBy, OrderByDescending, Take)
            var hangNhapNhieuNhat = await _context.ChiTietPhieuNhaps
                .Include(c => c.PhieuNhap)
                .Include(c => c.HangHoa)
                .Where(c => c.PhieuNhap.NgayNhap >= tuNgay && c.PhieuNhap.NgayNhap <= denNgay && c.PhieuNhap.TrangThai == "Đã hoàn tất")
                .GroupBy(c => new { c.MaHang, c.HangHoa.TenHang })
                .Select(g => new
                {
                    TenHang = g.Key.TenHang,
                    TongNhap = g.Sum(c => c.SoLuongNhap)
                })
                .OrderByDescending(x => x.TongNhap)
                .Take(5) // Lấy top 5
                .ToListAsync();

            ViewBag.TopHangNhap = hangNhapNhieuNhat;

            // TODO: Bạn có thể viết thêm truy vấn tương tự cho Top hàng xuất nhiều nhất

            return View(); // Tạo một file View BaoCaoNhapXuat.cshtml tương ứng
        }
    }
}