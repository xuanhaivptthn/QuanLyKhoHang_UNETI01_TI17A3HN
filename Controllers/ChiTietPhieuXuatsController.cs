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
        public async Task<IActionResult> Create(int? maPhieuXuat)
        {
            if (maPhieuXuat.HasValue && !await IsEditableExportAsync(maPhieuXuat.Value))
            {
                TempData["ErrorMessage"] = "Chỉ có thể thêm chi tiết cho phiếu xuất đang ở trạng thái Nháp hoặc Chờ xác nhận.";
                return RedirectToAction(nameof(Index), new { maPhieuXuat });
            }

            PopulateCreateEditLists(maPhieuXuat);
            return View(new ChiTietPhieuXuat { MaPhieuXuat = maPhieuXuat ?? 0 });
        }

        // POST: ChiTietPhieuXuats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChiTietXuat,MaPhieuXuat,MaHang,SoLuongXuat,DonGiaXuatThamChieu,GhiChu")] ChiTietPhieuXuat chiTietPhieuXuat)
        {
            if (!await IsEditableExportAsync(chiTietPhieuXuat.MaPhieuXuat))
            {
                ModelState.AddModelError(nameof(chiTietPhieuXuat.MaPhieuXuat), "Chỉ có thể thêm chi tiết cho phiếu xuất đang ở trạng thái Nháp hoặc Chờ xác nhận.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(chiTietPhieuXuat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }
            PopulateCreateEditLists(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang);
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

            var chiTietPhieuXuat = await _context.ChiTietPhieuXuats
                .Include(c => c.PhieuXuat)
                .FirstOrDefaultAsync(c => c.MaChiTietXuat == targetId);
            if (chiTietPhieuXuat == null)
            {
                return NotFound();
            }
            if (chiTietPhieuXuat.PhieuXuat == null || !IsEditableExport(chiTietPhieuXuat.PhieuXuat.TrangThai))
            {
                TempData["ErrorMessage"] = "Không thể sửa chi tiết của phiếu xuất đã hoàn tất hoặc đã hủy.";
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }
            PopulateCreateEditLists(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang);
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

            var existingDetail = await _context.ChiTietPhieuXuats
                .Include(c => c.PhieuXuat)
                .FirstOrDefaultAsync(c => c.MaChiTietXuat == targetId);
            if (existingDetail == null)
            {
                return NotFound();
            }

            if (existingDetail.PhieuXuat == null || !IsEditableExport(existingDetail.PhieuXuat.TrangThai))
            {
                TempData["ErrorMessage"] = "Không thể sửa chi tiết của phiếu xuất đã hoàn tất hoặc đã hủy.";
                return RedirectToAction(nameof(Index), new { maPhieuXuat = existingDetail.MaPhieuXuat });
            }

            if (!await IsEditableExportAsync(chiTietPhieuXuat.MaPhieuXuat))
            {
                ModelState.AddModelError(nameof(chiTietPhieuXuat.MaPhieuXuat), "Chi tiết chỉ có thể chuyển sang phiếu xuất đang ở trạng thái Nháp hoặc Chờ xác nhận.");
            }

            if (ModelState.IsValid)
            {
                existingDetail.MaPhieuXuat = chiTietPhieuXuat.MaPhieuXuat;
                existingDetail.MaHang = chiTietPhieuXuat.MaHang;
                existingDetail.SoLuongXuat = chiTietPhieuXuat.SoLuongXuat;
                existingDetail.DonGiaXuatThamChieu = chiTietPhieuXuat.DonGiaXuatThamChieu;
                existingDetail.GhiChu = chiTietPhieuXuat.GhiChu;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }
            PopulateCreateEditLists(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang);
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
            if (chiTietPhieuXuat.PhieuXuat == null || !IsEditableExport(chiTietPhieuXuat.PhieuXuat.TrangThai))
            {
                TempData["ErrorMessage"] = "Không thể xóa chi tiết của phiếu xuất đã hoàn tất hoặc đã hủy.";
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
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

            var chiTietPhieuXuat = await _context.ChiTietPhieuXuats
                .Include(c => c.PhieuXuat)
                .FirstOrDefaultAsync(c => c.MaChiTietXuat == targetId);
            int? maPhieuXuat = chiTietPhieuXuat?.MaPhieuXuat;
            if (chiTietPhieuXuat != null)
            {
                if (chiTietPhieuXuat.PhieuXuat == null || !IsEditableExport(chiTietPhieuXuat.PhieuXuat.TrangThai))
                {
                    TempData["ErrorMessage"] = "Không thể xóa chi tiết của phiếu xuất đã hoàn tất hoặc đã hủy.";
                    return RedirectToAction(nameof(Index), new { maPhieuXuat });
                }

                _context.ChiTietPhieuXuats.Remove(chiTietPhieuXuat);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { maPhieuXuat });
        }

        private void PopulateCreateEditLists(int? maPhieuXuat = null, int? maHang = null)
        {
            ViewData["MaHang"] = new SelectList(_context.HangHoa, "MaHang", "TenHang", maHang);
            ViewData["MaPhieuXuat"] = new SelectList(
                _context.PhieuXuats.Where(p => p.TrangThai == TrangThaiPhieuXuat.Nhap
                    || p.TrangThai == TrangThaiPhieuXuat.ChoXacNhan),
                "MaPhieuXuat",
                "MaPhieuXuat",
                maPhieuXuat);
        }

        private async Task<bool> IsEditableExportAsync(int maPhieuXuat)
        {
            var status = await _context.PhieuXuats
                .Where(p => p.MaPhieuXuat == maPhieuXuat)
                .Select(p => (TrangThaiPhieuXuat?)p.TrangThai)
                .FirstOrDefaultAsync();
            return status.HasValue && IsEditableExport(status.Value);
        }

        private static bool IsEditableExport(TrangThaiPhieuXuat status)
        {
            return status == TrangThaiPhieuXuat.Nhap || status == TrangThaiPhieuXuat.ChoXacNhan;
        }
    }
}
