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
        // optional phieuId or maPhieuNhap to preselect parent PhieuNhap when creating from PhieuNhap details
        public IActionResult Create(int? maPhieuNhap, int? phieuId)
        {
            var targetPhieuId = maPhieuNhap ?? phieuId;
            // Only active products and editable PhieuNhap (Nháp/Chờ xác nhận) can be selected
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang");
            if (targetPhieuId.HasValue)
            {
                // only allow creating for editable phieu (Nháp/Chờ xác nhận)
                var ph = _context.PhieuNhap.Find(targetPhieuId.Value);
                if (ph == null || !(ph.TrangThai == TrangThaiPhieuNhap.Nhap || ph.TrangThai == TrangThaiPhieuNhap.ChoXacNhan))
                {
                    return Forbid();
                }
                ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.MaPhieuNhap == targetPhieuId.Value), "MaPhieuNhap", "MaPhieuNhap", targetPhieuId.Value);
                ViewBag.PreselectedPhieuId = targetPhieuId.Value;
            }
            else
            {
                ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == TrangThaiPhieuNhap.Nhap || p.TrangThai == TrangThaiPhieuNhap.ChoXacNhan), "MaPhieuNhap", "MaPhieuNhap");
            }
            return View(new ChiTietPhieuNhap { MaPhieuNhap = targetPhieuId ?? 0 });
        }

        // POST: ChiTietPhieuNhaps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChiTietNhap,MaPhieuNhap,MaHang,SoLuongNhap,DonGiaNhap")] ChiTietPhieuNhap chiTietPhieuNhap)
        {
            // Ensure the selected PhieuNhap is editable
            var ph = await _context.PhieuNhap.FindAsync(chiTietPhieuNhap.MaPhieuNhap);
            if (ph == null || !(ph.TrangThai == TrangThaiPhieuNhap.Nhap || ph.TrangThai == TrangThaiPhieuNhap.ChoXacNhan))
            {
                ModelState.AddModelError(string.Empty, "Chi tiết chỉ được thêm vào phiếu ở trạng thái Nháp hoặc Chờ xác nhận.");
            }

            bool daTonTai = await _context.ChiTietPhieuNhap.AnyAsync(c => c.MaPhieuNhap == chiTietPhieuNhap.MaPhieuNhap && c.MaHang == chiTietPhieuNhap.MaHang);
            if (daTonTai)
            {
                ModelState.AddModelError("MaHang", "Mặt hàng này đã có trong phiếu nhập. Vui lòng chỉnh sửa số lượng tại dòng đã có thay vì thêm mới.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(chiTietPhieuNhap);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "PhieuNhaps", new { id = chiTietPhieuNhap.MaPhieuNhap });
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == TrangThaiPhieuNhap.Nhap || p.TrangThai == TrangThaiPhieuNhap.ChoXacNhan), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
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
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == TrangThaiPhieuNhap.Nhap || p.TrangThai == TrangThaiPhieuNhap.ChoXacNhan), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
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

            // Ensure parent PhieuNhap is editable before saving changes
            var ph = await _context.PhieuNhap.FindAsync(chiTietPhieuNhap.MaPhieuNhap);
            if (ph == null || !(ph.TrangThai == TrangThaiPhieuNhap.Nhap || ph.TrangThai == TrangThaiPhieuNhap.ChoXacNhan))
            {
                ModelState.AddModelError(string.Empty, "Chi tiết chỉ được sửa khi phiếu ở trạng thái Nháp hoặc Chờ xác nhận.");
            }

            if (ModelState.IsValid)
            {
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
                return RedirectToAction("Details", "PhieuNhaps", new { id = chiTietPhieuNhap.MaPhieuNhap });
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

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap
                .Include(c => c.PhieuNhap)
                .FirstOrDefaultAsync(c => c.MaChiTietNhap == targetId);

            int maPhieuNhap = chiTietPhieuNhap?.MaPhieuNhap ?? 0;
            if (chiTietPhieuNhap != null)
            {
                if (chiTietPhieuNhap.PhieuNhap != null && chiTietPhieuNhap.PhieuNhap.TrangThai == TrangThaiPhieuNhap.DaHoanTat)
                {
                    TempData["Error"] = "Không thể xóa dòng chi tiết của phiếu đã hoàn tất.";
                    return RedirectToAction("Details", "PhieuNhaps", new { id = maPhieuNhap });
                }

                _context.ChiTietPhieuNhap.Remove(chiTietPhieuNhap);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "PhieuNhaps", new { id = maPhieuNhap });
        }

        private bool ChiTietPhieuNhapExists(int id)
        {
            return _context.ChiTietPhieuNhap.Any(e => e.MaChiTietNhap == id);
        }
    }
}
