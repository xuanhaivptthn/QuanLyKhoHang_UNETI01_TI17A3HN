using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Filters;
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
        [AuthorizeRole("Admin", "NhanVienKho")]
        public async Task<IActionResult> Create(int? maPhieuXuat)
        {
            if (maPhieuXuat.HasValue && !await IsEditableExportAsync(maPhieuXuat.Value))
            {
                TempData["ErrorMessage"] = "Chỉ có thể thêm chi tiết cho phiếu xuất đang ở trạng thái Nháp.";
                return RedirectToAction(nameof(Index), new { maPhieuXuat });
            }

            await PopulateCreateEditListsAsync(maPhieuXuat);
            return View(new ChiTietPhieuXuat { MaPhieuXuat = maPhieuXuat ?? 0 });
        }

        // POST: ChiTietPhieuXuats/Create
        [AuthorizeRole("Admin", "NhanVienKho")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChiTietXuat,MaPhieuXuat,MaHang,SoLuongXuat,DonGiaXuatThamChieu,GhiChu")] ChiTietPhieuXuat chiTietPhieuXuat)
        {
            if (!await IsEditableExportAsync(chiTietPhieuXuat.MaPhieuXuat))
            {
                ModelState.AddModelError(nameof(chiTietPhieuXuat.MaPhieuXuat), "Chỉ có thể thêm chi tiết cho phiếu xuất đang ở trạng thái Nháp.");
            }
            if (!await IsActiveProductAsync(chiTietPhieuXuat.MaHang))
            {
                ModelState.AddModelError(nameof(chiTietPhieuXuat.MaHang), "Chỉ được chọn mặt hàng đang hoạt động.");
            }
            if (await HasDuplicateItemAsync(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang))
            {
                ModelState.AddModelError(nameof(chiTietPhieuXuat.MaHang), "Mặt hàng này đã có trong phiếu xuất.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(chiTietPhieuXuat);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _context.Entry(chiTietPhieuXuat).State = EntityState.Detached;
                    if (!await HasDuplicateItemAsync(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang))
                    {
                        throw;
                    }

                    ModelState.AddModelError(nameof(chiTietPhieuXuat.MaHang), "Mặt hàng này đã có trong phiếu xuất.");
                    await PopulateCreateEditListsAsync(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang);
                    return View(chiTietPhieuXuat);
                }
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }
            await PopulateCreateEditListsAsync(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang);
            return View(chiTietPhieuXuat);
        }

        // GET: ChiTietPhieuXuats/Edit/5
        [AuthorizeRole("Admin", "NhanVienKho")]
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
                TempData["ErrorMessage"] = "Chỉ có thể sửa chi tiết của phiếu xuất đang ở trạng thái Nháp.";
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }
            await PopulateCreateEditListsAsync(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang);
            return View(chiTietPhieuXuat);
        }

        // POST: ChiTietPhieuXuats/Edit/5
        [AuthorizeRole("Admin", "NhanVienKho")]
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
                TempData["ErrorMessage"] = "Chỉ có thể sửa chi tiết của phiếu xuất đang ở trạng thái Nháp.";
                return RedirectToAction(nameof(Index), new { maPhieuXuat = existingDetail.MaPhieuXuat });
            }

            if (!await IsEditableExportAsync(chiTietPhieuXuat.MaPhieuXuat))
            {
                ModelState.AddModelError(nameof(chiTietPhieuXuat.MaPhieuXuat), "Chi tiết chỉ có thể chuyển sang phiếu xuất đang ở trạng thái Nháp.");
            }
            if (!await IsActiveProductAsync(chiTietPhieuXuat.MaHang))
            {
                ModelState.AddModelError(nameof(chiTietPhieuXuat.MaHang), "Chỉ được chọn mặt hàng đang hoạt động.");
            }
            if (await HasDuplicateItemAsync(
                chiTietPhieuXuat.MaPhieuXuat,
                chiTietPhieuXuat.MaHang,
                chiTietPhieuXuat.MaChiTietXuat))
            {
                ModelState.AddModelError(nameof(chiTietPhieuXuat.MaHang), "Mặt hàng này đã có trong phiếu xuất.");
            }

            if (ModelState.IsValid)
            {
                existingDetail.MaPhieuXuat = chiTietPhieuXuat.MaPhieuXuat;
                existingDetail.MaHang = chiTietPhieuXuat.MaHang;
                existingDetail.SoLuongXuat = chiTietPhieuXuat.SoLuongXuat;
                existingDetail.DonGiaXuatThamChieu = chiTietPhieuXuat.DonGiaXuatThamChieu;
                existingDetail.GhiChu = chiTietPhieuXuat.GhiChu;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (!await HasDuplicateItemAsync(
                        chiTietPhieuXuat.MaPhieuXuat,
                        chiTietPhieuXuat.MaHang,
                        chiTietPhieuXuat.MaChiTietXuat))
                    {
                        throw;
                    }

                    ModelState.AddModelError(nameof(chiTietPhieuXuat.MaHang), "Mặt hàng này đã có trong phiếu xuất.");
                    await PopulateCreateEditListsAsync(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang);
                    return View(chiTietPhieuXuat);
                }
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }
            await PopulateCreateEditListsAsync(chiTietPhieuXuat.MaPhieuXuat, chiTietPhieuXuat.MaHang);
            return View(chiTietPhieuXuat);
        }

        // GET: ChiTietPhieuXuats/Delete/5
        [AuthorizeRole("Admin", "NhanVienKho")]
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
                TempData["ErrorMessage"] = "Chỉ có thể xóa chi tiết của phiếu xuất đang ở trạng thái Nháp.";
                return RedirectToAction(nameof(Index), new { maPhieuXuat = chiTietPhieuXuat.MaPhieuXuat });
            }

            return View(chiTietPhieuXuat);
        }

        // POST: ChiTietPhieuXuats/Delete/5
        [AuthorizeRole("Admin", "NhanVienKho")]
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
                    TempData["ErrorMessage"] = "Chỉ có thể xóa chi tiết của phiếu xuất đang ở trạng thái Nháp.";
                    return RedirectToAction(nameof(Index), new { maPhieuXuat });
                }

                _context.ChiTietPhieuXuats.Remove(chiTietPhieuXuat);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { maPhieuXuat });
        }

        private async Task PopulateCreateEditListsAsync(int? maPhieuXuat = null, int? maHang = null)
        {
            ViewData["MaHang"] = new SelectList(
                await _context.HangHoa
                    .Where(h => h.TrangThai || h.MaHang == maHang)
                    .ToListAsync(),
                "MaHang",
                "TenHang",
                maHang);
            ViewData["MaPhieuXuat"] = new SelectList(
                await _context.PhieuXuats
                    .Where(p => p.TrangThai == TrangThaiPhieuXuat.Nhap || p.MaPhieuXuat == maPhieuXuat)
                    .ToListAsync(),
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
            return status == TrangThaiPhieuXuat.Nhap;
        }

        private Task<bool> IsActiveProductAsync(int maHang)
        {
            return _context.HangHoa.AnyAsync(h => h.MaHang == maHang && h.TrangThai);
        }

        private Task<bool> HasDuplicateItemAsync(int maPhieuXuat, int maHang, int? excludeId = null)
        {
            return _context.ChiTietPhieuXuats.AnyAsync(c =>
                c.MaPhieuXuat == maPhieuXuat
                && c.MaHang == maHang
                && (!excludeId.HasValue || c.MaChiTietXuat != excludeId.Value));
        }
    }
}
