using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    // Họ và tên: Nguyễn Văn Cường
    // Mã sinh viên: 23103100132
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.
    public class TonKhoController : Controller
    {
        private readonly AppDbContext _context;

        public TonKhoController(AppDbContext context)
        {
            _context = context;
        }

        // Chức năng Cảnh báo tồn: Lấy danh sách những hàng hóa = 0 hoặc <= Mức tồn tối thiểu
        public async Task<IActionResult> CanhBao()
        {
            var danhSachCanhBao = await _context.TonKhoes
                .Include(t => t.HangHoa)
                .Include(t => t.Kho) // Join để lấy tên kho và tên hàng hóa ra hiển thị
                .Where(t => t.SoLuongTon <= t.HangHoa.MucTonToiThieu)
                .OrderBy(t => t.SoLuongTon) // Sắp xếp tăng dần: những hàng bằng 0 lên đầu
                .ToListAsync();

            return View(danhSachCanhBao);
        }
    }
}