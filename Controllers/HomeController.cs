using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(AppDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            List<TaiKhoan> taiKhoans = new();
            try
            {
                taiKhoans = await _context.TaiKhoans.AsNoTracking().ToListAsync();
            }
            catch (Exception)
            {
                // Nếu kết nối vừa gặp sự cố (Failover Interceptor đã tự động chuyển sang CSDL Local),
                // thử đọc lại từ DbContext mới để trang hiển thị ngay mà không bị trống dữ liệu
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var fallbackContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    taiKhoans = await fallbackContext.TaiKhoans.AsNoTracking().ToListAsync();
                }
                catch { }
            }
            return View(taiKhoans);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
