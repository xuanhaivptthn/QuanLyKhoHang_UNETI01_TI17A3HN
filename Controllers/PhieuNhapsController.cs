using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

// Họ và tên: Lê Văn Hùng
// Mã sinh viên: 23103100177
// Phụ trách Module 3: Nhà cung cấp, Phiếu nhập, Chi tiết phiếu nhập

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class PhieuNhapsController : Controller
    {
        private readonly AppDbContext _context;

        public PhieuNhapsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PhieuNhaps
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PhieuNhap
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .Include(p => p.ChiTietPhieuNhaps);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PhieuNhaps/Details/5
        public async Task<IActionResult> Details(int? id, int? maphieunhap)
        {
            var targetId = id ?? maphieunhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhap
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(ct => ct.HangHoa)
                .FirstOrDefaultAsync(m => m.MaPhieuNhap == targetId);
            if (phieuNhap == null)
            {
                return NotFound();
            }

            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Create
        public IActionResult Create()
        {
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho");
            // Only active suppliers can be selected for new PhieuNhap
            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap");
            return View();
        }

        // POST: PhieuNhaps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuNhap,MaNhaCungCap,MaKho,NgayNhap,NguoiLap,TrangThai,GhiChu")] PhieuNhap phieuNhap)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phieuNhap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho", phieuNhap.MaKho);
            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Edit/5
        public async Task<IActionResult> Edit(int? id, int? maphieunhap)
        {
            var targetId = id ?? maphieunhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhap.FindAsync(targetId);
            if (phieuNhap == null)
            {
                return NotFound();
            }
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho", phieuNhap.MaKho);
            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
            return View(phieuNhap);
        }

        // POST: PhieuNhaps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? maphieunhap, [Bind("MaPhieuNhap,MaNhaCungCap,MaKho,NgayNhap,NguoiLap,TrangThai,GhiChu")] PhieuNhap phieuNhap)
        {
            var targetId = id ?? maphieunhap ?? phieuNhap.MaPhieuNhap;
            if (targetId != phieuNhap.MaPhieuNhap)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuNhap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuNhapExists(phieuNhap.MaPhieuNhap))
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
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho", phieuNhap.MaKho);
            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Delete/5
        public async Task<IActionResult> Delete(int? id, int? maphieunhap)
        {
            var targetId = id ?? maphieunhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhap
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .FirstOrDefaultAsync(m => m.MaPhieuNhap == targetId);
            if (phieuNhap == null)
            {
                return NotFound();
            }

            return View(phieuNhap);
        }

        // POST: PhieuNhaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? maphieunhap)
        {
            var targetId = id ?? maphieunhap;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaPhieuNhap"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhap.FindAsync(targetId);
            if (phieuNhap != null)
            {
                _context.PhieuNhap.Remove(phieuNhap);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PhieuNhapExists(int id)
        {
            return _context.PhieuNhap.Any(e => e.MaPhieuNhap == id);
        }
    }
}
