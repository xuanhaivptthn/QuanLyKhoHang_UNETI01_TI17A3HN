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
    public class PhieuXuatsController : Controller
    {
        private readonly AppDbContext _context;

        public PhieuXuatsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PhieuXuats
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PhieuXuats
                .Include(p => p.BoPhanNhan)
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuXuats);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PhieuXuats/Details/5
        public async Task<IActionResult> Details(int? id, int? maphieuxuat)
        {
            var targetId = id ?? maphieuxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.BoPhanNhan)
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuXuats)
                    .ThenInclude(c => c.HangHoa)
                .FirstOrDefaultAsync(m => m.MaPhieuXuat == targetId);
            if (phieuXuat == null)
            {
                return NotFound();
            }

            return View(phieuXuat);
        }

        // GET: PhieuXuats/Create
        public IActionResult Create()
        {
            ViewData["MaBoPhan"] = new SelectList(_context.BoPhanNhans, "MaBoPhan", "TenBoPhan");
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho");
            return View();
        }

        // POST: PhieuXuats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuXuat,MaBoPhan,MaKho,NgayXuat,NguoiLap,TrangThai,GhiChu")] PhieuXuat phieuXuat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phieuXuat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaBoPhan"] = new SelectList(_context.BoPhanNhans, "MaBoPhan", "TenBoPhan", phieuXuat.MaBoPhan);
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho", phieuXuat.MaKho);
            return View(phieuXuat);
        }

        // GET: PhieuXuats/Edit/5
        public async Task<IActionResult> Edit(int? id, int? maphieuxuat)
        {
            var targetId = id ?? maphieuxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats.FindAsync(targetId);
            if (phieuXuat == null)
            {
                return NotFound();
            }
            ViewData["MaBoPhan"] = new SelectList(_context.BoPhanNhans, "MaBoPhan", "TenBoPhan", phieuXuat.MaBoPhan);
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho", phieuXuat.MaKho);
            return View(phieuXuat);
        }

        // POST: PhieuXuats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? maphieuxuat, [Bind("MaPhieuXuat,MaBoPhan,MaKho,NgayXuat,NguoiLap,TrangThai,GhiChu")] PhieuXuat phieuXuat)
        {
            var targetId = id ?? maphieuxuat ?? phieuXuat.MaPhieuXuat;
            if (targetId != phieuXuat.MaPhieuXuat)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuXuat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuXuatExists(phieuXuat.MaPhieuXuat))
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
            ViewData["MaBoPhan"] = new SelectList(_context.BoPhanNhans, "MaBoPhan", "TenBoPhan", phieuXuat.MaBoPhan);
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho", phieuXuat.MaKho);
            return View(phieuXuat);
        }

        // GET: PhieuXuats/Delete/5
        public async Task<IActionResult> Delete(int? id, int? maphieuxuat)
        {
            var targetId = id ?? maphieuxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.BoPhanNhan)
                .Include(p => p.Kho)
                .FirstOrDefaultAsync(m => m.MaPhieuXuat == targetId);
            if (phieuXuat == null)
            {
                return NotFound();
            }

            return View(phieuXuat);
        }

        // POST: PhieuXuats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? maphieuxuat)
        {
            var targetId = id ?? maphieuxuat;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaPhieuXuat"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats.FindAsync(targetId);
            if (phieuXuat != null)
            {
                _context.PhieuXuats.Remove(phieuXuat);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PhieuXuatExists(int id)
        {
            return _context.PhieuXuats.Any(e => e.MaPhieuXuat == id);
        }
    }
}
