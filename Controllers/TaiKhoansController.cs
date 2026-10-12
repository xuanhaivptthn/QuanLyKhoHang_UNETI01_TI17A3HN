using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Filters;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels;

// Họ và tên: Trần Xuân Hải
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Cookie & Session

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    [AuthorizeRole("Admin")]
    public class TaiKhoansController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public TaiKhoansController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        #region Xác thực & Quản lý Cookie / Session (Authentication & State Management)

        // GET: /TaiKhoans/DangNhap hoặc /TaiKhoan/DangNhap
        [AllowAnonymous]
        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            // Nếu đã xác thực qua Cookie hoặc Session -> điều hướng về returnUrl hoặc trang chủ
            if (User.Identity?.IsAuthenticated == true || HttpContext.Session.GetInt32("MaTaiKhoan") != null)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;

            // Đọc Cookie "RememberedUser" nếu đã được lưu từ phiên trước
            var model = new LoginViewModel();
            if (Request.Cookies.TryGetValue("RememberedUser", out var rememberedUser) && !string.IsNullOrWhiteSpace(rememberedUser))
            {
                model.TenDangNhap = rememberedUser;
                model.GhiNhoDangNhap = true;
            }

            return View(model);
        }

        // POST: /TaiKhoans/DangNhap hoặc /TaiKhoan/DangNhap
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // Tìm tài khoản theo tên đăng nhập
            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap);

            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // 1. Kiểm tra trạng thái tài khoản bị khóa bởi Quản trị viên
            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản này hiện đang bị khóa bởi Quản trị viên.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // 2. Cơ chế khóa tài khoản chống dò mật khẩu (Lockout lưu trực tiếp trong CSDL)
            if (taiKhoan.KhoaDenUtc.HasValue)
            {
                if (taiKhoan.KhoaDenUtc.Value > DateTime.UtcNow)
                {
                    var remainingMinutes = (int)Math.Ceiling((taiKhoan.KhoaDenUtc.Value - DateTime.UtcNow).TotalMinutes);
                    if (remainingMinutes < 1) remainingMinutes = 1;
                    ModelState.AddModelError(string.Empty, $"Tài khoản tạm thời bị khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau {remainingMinutes} phút.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }
                else
                {
                    // Đã hết thời hạn khóa tạm -> mở lại tài khoản
                    taiKhoan.KhoaDenUtc = null;
                    taiKhoan.SoLanDangNhapSai = 0;
                    await _context.SaveChangesAsync();
                }
            }

            // 3. Kiểm tra mật khẩu
            if (taiKhoan.MatKhau != model.MatKhau)
            {
                taiKhoan.SoLanDangNhapSai++;
                if (taiKhoan.SoLanDangNhapSai >= 5)
                {
                    taiKhoan.KhoaDenUtc = DateTime.UtcNow.AddMinutes(5);
                    taiKhoan.SoLanDangNhapSai = 0;
                    await _context.SaveChangesAsync();
                    ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa 5 phút do nhập sai mật khẩu quá 5 lần liên tiếp.");
                }
                else
                {
                    await _context.SaveChangesAsync();
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                }

                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // 4. Đăng nhập thành công -> Reset bộ đếm số lần sai và trạng thái khóa trong CSDL
            taiKhoan.SoLanDangNhapSai = 0;
            taiKhoan.KhoaDenUtc = null;
            await _context.SaveChangesAsync();

            // 1. Quản lý Cookie "RememberedUser"
            if (model.GhiNhoDangNhap)
            {
                var cookieOption = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7), // Lưu 7 ngày
                    HttpOnly = true,                  // Chống XSS
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true
                };
                Response.Cookies.Append("RememberedUser", taiKhoan.TenDangNhap, cookieOption);
            }
            else
            {
                Response.Cookies.Delete("RememberedUser");
            }

            // 2. Thiết lập Cookie Authentication & Đóng gói Claims-based
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, taiKhoan.VaiTro),
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.MaTaiKhoan.ToString()),
                new Claim(ClaimTypes.Email, taiKhoan.Email ?? string.Empty),
                new Claim("HoTen", taiKhoan.HoTen ?? string.Empty),
                new Claim("Department", taiKhoan.VaiTro ?? string.Empty) // Custom Claim Department
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNhoDangNhap,
                ExpiresUtc = model.GhiNhoDangNhap ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // Sinh Cookie Authentication gửi về Browser
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            // 3. Đồng bộ vào Session
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen ?? string.Empty);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro ?? string.Empty);
            HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap ?? string.Empty);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoans/DangXuat hoặc /TaiKhoan/DangXuat
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> DangXuat()
        {
            // 1. Hủy Cookie Authentication khỏi Browser
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Xóa toàn bộ dữ liệu Session
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(DangNhap));
        }

        // GET: /TaiKhoans/Profile
        [AuthorizeRole("Admin", "NhanVienKho", "BoPhanNhan")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userName = User.Identity?.Name ?? HttpContext.Session.GetString("TenDangNhap");
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction(nameof(DangNhap));
            }

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == userName);
            if (taiKhoan == null)
            {
                return NotFound();
            }

            return View(taiKhoan);
        }

        #endregion

        #region Quản lý tài khoản (CRUD Admin)

        // GET: TaiKhoans
        public async Task<IActionResult> Index()
        {
            return View(await _context.TaiKhoans.ToListAsync());
        }

        // GET: TaiKhoans/Details/5
        public async Task<IActionResult> Details(int? id, int? mataikhoan)
        {
            var targetId = id ?? mataikhoan;
            if (targetId == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(m => m.MaTaiKhoan == targetId);
            if (taiKhoan == null)
            {
                return NotFound();
            }

            return View(taiKhoan);
        }

        // GET: TaiKhoans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TaiKhoans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,Email,VaiTro,TrangThai")] TaiKhoan taiKhoan)
        {
            bool duplicateUser = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == taiKhoan.TenDangNhap);
            if (duplicateUser)
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã tạo mới tài khoản '{taiKhoan.TenDangNhap}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // GET: TaiKhoans/Edit/5
        public async Task<IActionResult> Edit(int? id, int? mataikhoan)
        {
            var targetId = id ?? mataikhoan;
            if (targetId == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(targetId);
            if (taiKhoan == null)
            {
                return NotFound();
            }
            return View(taiKhoan);
        }

        // POST: TaiKhoans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? mataikhoan, [Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,Email,VaiTro,TrangThai")] TaiKhoan taiKhoan)
        {
            var targetId = id ?? mataikhoan ?? taiKhoan.MaTaiKhoan;
            if (targetId != taiKhoan.MaTaiKhoan)
            {
                return NotFound();
            }

            bool duplicateUser = await _context.TaiKhoans
                .AnyAsync(t => t.TenDangNhap == taiKhoan.TenDangNhap && t.MaTaiKhoan != taiKhoan.MaTaiKhoan);
            if (duplicateUser)
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taiKhoan);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã cập nhật thông tin tài khoản '{taiKhoan.TenDangNhap}' thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaiKhoanExists(taiKhoan.MaTaiKhoan))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // GET: TaiKhoans/Delete/5
        public async Task<IActionResult> Delete(int? id, int? mataikhoan)
        {
            var targetId = id ?? mataikhoan;
            if (targetId == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(m => m.MaTaiKhoan == targetId);
            if (taiKhoan == null)
            {
                return NotFound();
            }

            return View(taiKhoan);
        }

        // POST: TaiKhoans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? mataikhoan)
        {
            var targetId = id ?? mataikhoan;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaTaiKhoan"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(targetId);
            if (taiKhoan != null)
            {
                int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int claimId);
                var currentUserId = HttpContext.Session.GetInt32("MaTaiKhoan") ?? (claimId > 0 ? claimId : (int?)null);

                if (currentUserId == targetId)
                {
                    TempData["Error"] = "Bạn không thể tự xóa tài khoản đang đăng nhập.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.Equals(taiKhoan.VaiTro, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminCount = await _context.TaiKhoans.CountAsync(t => t.VaiTro == "Admin" && t.TrangThai);
                    if (adminCount <= 1)
                    {
                        TempData["Error"] = "Hệ thống phải có ít nhất 1 tài khoản Admin đang hoạt động. Không thể xóa.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.TaiKhoans.Remove(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa tài khoản thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoans.Any(e => e.MaTaiKhoan == id);
        }

        #endregion
    }
}
