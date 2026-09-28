using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;

        public TaiKhoanController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /TaiKhoan/DangNhap
        [HttpGet]
        public IActionResult DangNhap()
        {
            // Nếu đã đăng nhập thì điều hướng về trang chủ
            if (HttpContext.Session.GetInt32("MaTaiKhoan") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /TaiKhoan/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Sử dụng Entity Framework Core và LINQ để kiểm tra tài khoản
            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap 
                                       && t.MatKhau == model.MatKhau 
                                       && t.TrangThai == true);

            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác, hoặc tài khoản đã bị khóa.");
                return View(model);
            }

            // Đăng nhập thành công -> Lưu tối thiểu Mã tài khoản, Họ tên, Vai trò vào Session
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap);

            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoan/DangXuat
        [HttpGet]
        public IActionResult DangXuat()
        {
            // Xóa toàn bộ dữ liệu Session khi đăng xuất
            HttpContext.Session.Clear();

            return RedirectToAction("DangNhap", "TaiKhoan");
        }
    }
}
