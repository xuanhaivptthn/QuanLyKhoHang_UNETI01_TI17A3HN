using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    // Họ và tên: Nguyễn Văn Cường
    // Mã sinh viên: 23103100132
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.
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

        // Yêu cầu 9.5 & 9.6: Dashboard và Thống kê tổng quan (Sử dụng LINQ)
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;

            var viewModel = new DashboardViewModel
            {
                TongSoKho = await _context.Khoes.Where(k => k.TrangThai == true).CountAsync(),

                // Số phiếu xuất trong ngày (LINQ: Where)
                SoPhieuXuatTrongNgay = await _context.PhieuXuats
                    .Where(p => p.NgayXuat.Date == today)
                    .CountAsync(),

                // Cảnh báo tồn: Hàng hết
                SoHangHet = await _context.TonKhoes
                    .Where(t => t.SoLuongTon == 0)
                    .CountAsync(),

                TongSoHangHoa = await _context.HangHoa.CountAsync(),

                SoPhieuNhapTrongNgay = await _context.PhieuNhap
                    .Where(p => p.NgayNhap.Date == today)
                    .CountAsync(),

                SoHangSapHet = await _context.TonKhoes
                    .Include(t => t.HangHoa)
                    // Hàng sắp hết là lớn hơn 0 và nhỏ hơn hoặc bằng mức tối thiểu
                    .Where(t => t.SoLuongTon > 0 && t.SoLuongTon <= t.HangHoa.MucTonToiThieu)
                    .CountAsync(),
            };

            return View(viewModel);
        }

        // Yêu cầu 9.6 & 9.7: Báo cáo Thống kê theo thời gian (Sử dụng LINQ)
        public async Task<IActionResult> BaoCaoNhapXuat(DateTime? tuNgay, DateTime? denNgay)
        {
            // Nếu người dùng chưa chọn ngày, mặc định lấy trong tháng hiện tại
            if (!tuNgay.HasValue) tuNgay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            if (!denNgay.HasValue) denNgay = DateTime.Today;

            ViewBag.TuNgay = tuNgay.Value.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay.Value.ToString("yyyy-MM-dd");

            // 1. LINQ: Top 5 Hàng nhập nhiều nhất
            ViewBag.TopHangNhap = await _context.ChiTietPhieuNhap
                .Include(c => c.PhieuNhap).Include(c => c.HangHoa)
                .Where(c => c.PhieuNhap.NgayNhap.Date >= tuNgay.Value.Date && c.PhieuNhap.NgayNhap.Date <= denNgay.Value.Date && c.PhieuNhap.TrangThai == 2)
                .GroupBy(c => new { c.MaHang, c.HangHoa.TenHang })
                .Select(g => new { TenHang = g.Key.TenHang, TongSo = g.Sum(c => c.SoLuongNhap) })
                .OrderByDescending(x => x.TongSo).Take(5).ToListAsync();

            // 2. LINQ: Top 5 Hàng xuất nhiều nhất
            ViewBag.TopHangXuat = await _context.ChiTietPhieuXuats
                .Include(c => c.PhieuXuat).Include(c => c.HangHoa)
                .Where(c => c.PhieuXuat.NgayXuat.Date >= tuNgay.Value.Date && c.PhieuXuat.NgayXuat.Date <= denNgay.Value.Date && c.PhieuXuat.TrangThai == TrangThaiPhieuXuat.DaHoanTat)
                .GroupBy(c => new { c.MaHang, c.HangHoa.TenHang })
                .Select(g => new { TenHang = g.Key.TenHang, TongSo = g.Sum(c => c.SoLuongXuat) })
                .OrderByDescending(x => x.TongSo).Take(5).ToListAsync();

            // 3. LINQ: Thống kê số lượng phiếu theo trạng thái
            ViewBag.TrangThaiPhieu = await _context.PhieuNhap
                .Where(p => p.NgayNhap.Date >= tuNgay.Value.Date && p.NgayNhap.Date <= denNgay.Value.Date)
                .GroupBy(p => p.TrangThai)
                .Select(g => new { TenTrangThai = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            return View();
        }
    }
}