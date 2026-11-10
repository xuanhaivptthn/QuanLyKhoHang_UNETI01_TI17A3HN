using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Filters;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    // Họ và tên: Nguyễn Văn Cường
    // Mã sinh viên: 23103100132
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.
    [AuthorizeRole("Admin", "NhanVienKho")]
    public class TonKhoController : Controller
    {
        private readonly AppDbContext _context;

        public TonKhoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /TonKho
        public IActionResult Index()
        {
            return RedirectToAction(nameof(CanhBao));
        }

        // Chức năng Cảnh báo tồn: Lấy danh sách những hàng hóa = 0 hoặc <= Mức tồn tối thiểu
        // Hỗ trợ lọc theo:
        // - loaiCanhBao: "hethang" (Tồn = 0), "saphet" (0 < Tồn <= Mức tối thiểu), "tatca" (tất cả cảnh báo)
        // - maKho: Lọc theo điểm kho cụ thể
        // - timKiem: Tìm kiếm theo tên hoặc mã hàng hóa
        public async Task<IActionResult> CanhBao(string? loaiCanhBao, int? maKho, string? timKiem)
        {
            var baseQuery = _context.TonKhoes
                .Include(t => t.HangHoa)
                    .ThenInclude(h => h!.LoaiHang)
                .Include(t => t.HangHoa)
                    .ThenInclude(h => h!.DonViTinh)
                .Include(t => t.Kho)
                .Where(t => t.HangHoa != null && t.SoLuongTon <= t.HangHoa.MucTonToiThieu);

            // 1. Lọc theo kho (LINQ Where)
            if (maKho.HasValue && maKho.Value > 0)
            {
                baseQuery = baseQuery.Where(t => t.MaKho == maKho.Value);
            }

            // 2. Lọc theo từ khóa tìm kiếm (Tên hàng hoặc Mã hàng)
            if (!string.IsNullOrWhiteSpace(timKiem))
            {
                var keyword = timKiem.Trim().ToLower();
                baseQuery = baseQuery.Where(t => t.HangHoa!.TenHang.ToLower().Contains(keyword) 
                                              || t.HangHoa.MaHang.ToString().Contains(keyword));
            }

            // Tính số lượng tổng quan theo điều kiện lọc kho & từ khóa
            var hetHangCount = await baseQuery.Where(t => t.SoLuongTon == 0).CountAsync();
            var sapHetCount = await baseQuery.Where(t => t.SoLuongTon > 0 && t.SoLuongTon <= t.HangHoa!.MucTonToiThieu).CountAsync();
            var tatCaCount = hetHangCount + sapHetCount;

            ViewBag.HetHangCount = hetHangCount;
            ViewBag.SapHetCount = sapHetCount;
            ViewBag.TatCaCount = tatCaCount;
            ViewBag.LoaiCanhBao = loaiCanhBao?.ToLower();
            ViewBag.MaKho = maKho;
            ViewBag.TimKiem = timKiem;

            // Danh sách kho bãi đang hoạt động phục vụ dropdown lọc
            ViewBag.DanhSachKho = await _context.Khoes
                .Where(k => k.TrangThai == true)
                .OrderBy(k => k.TenKho)
                .ToListAsync();

            // 3. Lọc theo phân loại: Hàng hết (Tồn = 0) hoặc Hàng sắp hết (0 < Tồn <= Mức tối thiểu)
            var query = baseQuery;
            if (string.Equals(loaiCanhBao, "hethang", StringComparison.OrdinalIgnoreCase) || string.Equals(loaiCanhBao, "het", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.SoLuongTon == 0);
            }
            else if (string.Equals(loaiCanhBao, "saphet", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.SoLuongTon > 0 && t.SoLuongTon <= t.HangHoa!.MucTonToiThieu);
            }

            // Sắp xếp: Ưu tiên hàng hết (tồn = 0) lên đầu tiên, sau đó đến hàng tồn ít nhất
            var danhSachCanhBao = await query
                .OrderBy(t => t.SoLuongTon)
                .ThenBy(t => t.HangHoa!.TenHang)
                .ToListAsync();

            return View(danhSachCanhBao);
        }

        // Chức năng lịch sử tồn kho 
        public async Task<IActionResult> LichSu(string timKiem, int? pageNumber)
        {
            int pageSize = 10;
            int currentPage = pageNumber ?? 1;

            var query = _context.LichSuTonKhoes
                .Include(l => l.HangHoa)
                .Include(l => l.Kho)
                .AsQueryable();

            if (!string.IsNullOrEmpty(timKiem))
            {
                query = query.Where(l => (l.HangHoa != null && l.HangHoa.TenHang.Contains(timKiem)) || (l.Kho != null && l.Kho.TenKho.Contains(timKiem)));
                ViewBag.TimKiem = timKiem;
            }

            query = query.OrderByDescending(l => l.NgayPhatSinh);

            int totalRecords = await query.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = currentPage;

            var lichSuList = await query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(lichSuList);
        }
    }
}
