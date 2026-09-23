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

        // Yêu cầu 9.5 & 9.6: Dashboard và Thống kê tổng quan (Sử dụng LINQ)
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;

            var viewModel = new DashboardViewModel
            {
                // Tổng số kho
                TongSoKho = await _context.Khoes.CountAsync(),

                // Số phiếu xuất trong ngày (LINQ: Where)
                SoPhieuXuatTrongNgay = await _context.PhieuXuats
                    .Where(p => p.NgayXuat.Date == today)
                    .CountAsync(),

                // Cảnh báo tồn: Hàng hết
                SoHangHet = await _context.TonKhoes
                    .Where(t => t.SoLuongTon == 0)
                    .CountAsync(),

                // Các chỉ số liên quan đến Module 2 (HangHoa) và Module 3 (PhieuNhap) sẽ mở lại khi 2 module này hoàn thành:
                TongSoHangHoa = 0, // await _context.HangHoas.CountAsync(),
                SoPhieuNhapTrongNgay = 0, // await _context.PhieuNhaps.Where(p => p.NgayNhap.Date == today).CountAsync(),
                SoHangSapHet = 0,
                TongSoLuongNhapTheoKy = 0
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

            // TODO (Module 3 & 2): Mở lại thống kê hàng nhập khi có ChiTietPhieuNhap và HangHoa
            /*
            var hangNhapNhieuNhat = await _context.ChiTietPhieuNhaps
                .Include(c => c.PhieuNhap)
                .Include(c => c.HangHoa)
                .Where(c => c.PhieuNhap != null && c.HangHoa != null && c.PhieuNhap.NgayNhap >= tuNgay && c.PhieuNhap.NgayNhap <= denNgay && c.PhieuNhap.TrangThai == "Đã hoàn tất")
                .GroupBy(c => new { c.MaHang, TenHang = c.HangHoa!.TenHang })
                .Select(g => new
                {
                    TenHang = g.Key.TenHang,
                    TongNhap = g.Sum(c => c.SoLuongNhap)
                })
                .OrderByDescending(x => x.TongNhap)
                .Take(5)
                .ToListAsync();

            ViewBag.TopHangNhap = hangNhapNhieuNhat;
            */

            return View(); // Tạo một file View BaoCaoNhapXuat.cshtml tương ứng
        }
    }
}