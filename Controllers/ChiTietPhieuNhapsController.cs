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
    public class ChiTietPhieuNhapsController : Controller
    {
        private readonly AppDbContext _context;

        public ChiTietPhieuNhapsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ChiTietPhieuNhaps
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ChiTietPhieuNhap.Include(c => c.HangHoa).Include(c => c.PhieuNhap);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ChiTietPhieuNhaps/Details/5
        public async Task<IActionResult> Details(int? id, int? machitietnhap)
        {
            var targetId = id ?? machitietnhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap
                .Include(c => c.HangHoa)
                .Include(c => c.PhieuNhap)
                .FirstOrDefaultAsync(m => m.MaChiTietNhap == targetId);
            if (chiTietPhieuNhap == null)
            {
                return NotFound();
            }

            return View(chiTietPhieuNhap);
        }

        // GET: ChiTietPhieuNhaps/Create
        public IActionResult Create()
        {
            // Only active products and editable PhieuNhap (Nháp/Chờ xác nhận) can be selected
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang");
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == 0 || p.TrangThai == 1), "MaPhieuNhap", "MaPhieuNhap");
            return View();
        }

        // POST: ChiTietPhieuNhaps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChiTietNhap,MaPhieuNhap,MaHang,SoLuongNhap,DonGiaNhap")] ChiTietPhieuNhap chiTietPhieuNhap)
        {
            if (ModelState.IsValid)
            {
                // Ensure the selected PhieuNhap is editable
                var ph = await _context.PhieuNhap.FindAsync(chiTietPhieuNhap.MaPhieuNhap);
                if (ph == null || !(ph.TrangThai == 0 || ph.TrangThai == 1))
                {
                    ModelState.AddModelError(string.Empty, "Chi tiết chỉ được thêm vào phiếu ở trạng thái Nháp hoặc Chờ xác nhận.");
                    ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
                    ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == 0 || p.TrangThai == 1), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
                    return View(chiTietPhieuNhap);
                }
                _context.Add(chiTietPhieuNhap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == 0 || p.TrangThai == 1), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
            return View(chiTietPhieuNhap);
        }

        // GET: ChiTietPhieuNhaps/Edit/5
        public async Task<IActionResult> Edit(int? id, int? machitietnhap)
        {
            var targetId = id ?? machitietnhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap.FindAsync(targetId);
            if (chiTietPhieuNhap == null)
            {
                return NotFound();
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == 0 || p.TrangThai == 1), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
            return View(chiTietPhieuNhap);
        }

        // POST: ChiTietPhieuNhaps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? machitietnhap, [Bind("MaChiTietNhap,MaPhieuNhap,MaHang,SoLuongNhap,DonGiaNhap")] ChiTietPhieuNhap chiTietPhieuNhap)
        {
            var targetId = id ?? machitietnhap ?? chiTietPhieuNhap.MaChiTietNhap;
            if (targetId != chiTietPhieuNhap.MaChiTietNhap)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Ensure parent PhieuNhap is editable before saving changes
                var ph = await _context.PhieuNhap.FindAsync(chiTietPhieuNhap.MaPhieuNhap);
                if (ph == null || !(ph.TrangThai == 0 || ph.TrangThai == 1))
                {
                    ModelState.AddModelError(string.Empty, "Chi tiết chỉ được sửa khi phiếu ở trạng thái Nháp hoặc Chờ xác nhận.");
                    ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
                    ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == 0 || p.TrangThai == 1), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
                    return View(chiTietPhieuNhap);
                }
                try
                {
                    _context.Update(chiTietPhieuNhap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTietPhieuNhapExists(chiTietPhieuNhap.MaChiTietNhap))
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
            ViewData["MaHang"] = new SelectList(_context.HangHoa, "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap, "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
            return View(chiTietPhieuNhap);
        }

        // GET: ChiTietPhieuNhaps/Delete/5
        public async Task<IActionResult> Delete(int? id, int? machitietnhap)
        {
            var targetId = id ?? machitietnhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap
                .Include(c => c.HangHoa)
                .Include(c => c.PhieuNhap)
                .FirstOrDefaultAsync(m => m.MaChiTietNhap == targetId);
            if (chiTietPhieuNhap == null)
            {
                return NotFound();
            }

            return View(chiTietPhieuNhap);
        }

        // POST: ChiTietPhieuNhaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? machitietnhap)
        {
            var targetId = id ?? machitietnhap;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaChiTietNhap"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap.FindAsync(targetId);
            if (chiTietPhieuNhap != null)
            {
                _context.ChiTietPhieuNhap.Remove(chiTietPhieuNhap);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ChiTietPhieuNhapExists(int id)
        {
            return _context.ChiTietPhieuNhap.Any(e => e.MaChiTietNhap == id);
        }
    }
}
