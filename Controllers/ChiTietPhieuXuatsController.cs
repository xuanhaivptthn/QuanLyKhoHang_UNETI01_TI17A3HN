using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

// Họ và tên: Nguyễn Việt Dũng
// Mã sinh viên: 23103100127
// Phụ trách Module 4: Bộ phận nhận, Phiếu xuất, Chi tiết phiếu xuất

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class ChiTietPhieuXuatsController : Controller
    {
        private readonly AppDbContext _context;

        public ChiTietPhieuXuatsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ChiTietPhieuXuats
        public async Task<IActionResult> Index(int? maPhieuXuat)
        {
            var query = _context.ChiTietPhieuXuats
                .Include(c => c.HangHoa)
                .Include(c => c.PhieuXuat)
                .AsQueryable();

            if (maPhieuXuat.HasValue)
            {
                query = query.Where(c => c.MaPhieuXuat == maPhieuXuat.Value);
                ViewData["MaPhieuXuat"] = maPhieuXuat.Value;
            }

            return View(await query.ToListAsync());
        }

        // GET: ChiTietPhieuXuats/Details/5
        public async Task<IActionResult> Details(int? id, int? machitietxuat)
        {
            var targetId = id ?? machitietxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuXuat = await _context.ChiTietPhieuXuats
                .Include(c => c.HangHoa)
                .Include(c => c.PhieuXuat)
                .FirstOrDefaultAsync(m => m.MaChiTietXuat == targetId);
            if (chiTietPhieuXuat == null)
            {
                return NotFound();
            }

            return View(chiTietPhieuXuat);
        }

        // GET: ChiTietPhieuXuats/Create
        public IActionResult Create(int? maPhieuXuat)
        {
            ViewData["MaHang"] = new SelectList(_context.HangHoa, "MaHang", "TenHang");
            ViewData["MaPhieuXuat"] = new SelectList(_context.PhieuXuats, "MaPhieuXuat", "MaPhieuXuat", maPhieuXuat);
            return View(new ChiTietPhieuXuat { MaPhieuXuat = maPhieuXuat ?? 0 });
        }

        // POST: ChiTietPhieuXuats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChiTietXuat,MaPhieuXuat,MaHang,SoLuongXuat,DonGiaXuatThamChieu,GhiChu")] ChiTietPhieuXuat chiTietPhieuXuat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chiTietPhieuXuat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa, "MaHang", "TenHang", chiTietPhieuXuat.MaHang);
            ViewData["MaPhieuXuat"] = new SelectList(_context.PhieuXuats, "MaPhieuXuat", "MaPhieuXuat", chiTietPhieuXuat.MaPhieuXuat);
            return View(chiTietPhieuXuat);
        }

        // GET: ChiTietPhieuXuats/Edit/5
        public async Task<IActionResult> Edit(int? id, int? machitietxuat)
        {
            var targetId = id ?? machitietxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuXuat = await _context.ChiTietPhieuXuats.FindAsync(targetId);
            if (chiTietPhieuXuat == null)
            {
                return NotFound();
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa, "MaHang", "TenHang", chiTietPhieuXuat.MaHang);
            ViewData["MaPhieuXuat"] = new SelectList(_context.PhieuXuats, "MaPhieuXuat", "MaPhieuXuat", chiTietPhieuXuat.MaPhieuXuat);
            return View(chiTietPhieuXuat);
        }

        // POST: ChiTietPhieuXuats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? machitietxuat, [Bind("MaChiTietXuat,MaPhieuXuat,MaHang,SoLuongXuat,DonGiaXuatThamChieu,GhiChu")] ChiTietPhieuXuat chiTietPhieuXuat)
        {
            var targetId = id ?? machitietxuat ?? chiTietPhieuXuat.MaChiTietXuat;
            if (targetId != chiTietPhieuXuat.MaChiTietXuat)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTietPhieuXuat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTietPhieuXuatExists(chiTietPhieuXuat.MaChiTietXuat))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa, "MaHang", "TenHang", chiTietPhieuXuat.MaHang);
            ViewData["MaPhieuXuat"] = new SelectList(_context.PhieuXuats, "MaPhieuXuat", "MaPhieuXuat", chiTietPhieuXuat.MaPhieuXuat);
            return View(chiTietPhieuXuat);
        }

        // GET: ChiTietPhieuXuats/Delete/5
        public async Task<IActionResult> Delete(int? id, int? machitietxuat)
        {
            var targetId = id ?? machitietxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuXuat = await _context.ChiTietPhieuXuats
                .Include(c => c.HangHoa)
                .Include(c => c.PhieuXuat)
                .FirstOrDefaultAsync(m => m.MaChiTietXuat == targetId);
            if (chiTietPhieuXuat == null)
            {
                return NotFound();
            }

            return View(chiTietPhieuXuat);
        }

        // POST: ChiTietPhieuXuats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? machitietxuat)
        {
            var targetId = id ?? machitietxuat;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaChiTietXuat"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuXuat = await _context.ChiTietPhieuXuats.FindAsync(targetId);
            int? maPhieuXuat = chiTietPhieuXuat?.MaPhieuXuat;
            if (chiTietPhieuXuat != null)
            {
                _context.ChiTietPhieuXuats.Remove(chiTietPhieuXuat);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { maPhieuXuat });
        }

        private bool ChiTietPhieuXuatExists(int id)
        {
            return _context.ChiTietPhieuXuats.Any(e => e.MaChiTietXuat == id);
        }
    }
}
