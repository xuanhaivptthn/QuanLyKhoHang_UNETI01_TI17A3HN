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
    [AuthorizeRole("Admin")]
    public class BoPhanNhansController : Controller
    {
        private readonly AppDbContext _context;

        public BoPhanNhansController(AppDbContext context)
        {
            _context = context;
        }

        // GET: BoPhanNhans
        public async Task<IActionResult> Index(string? searchString, bool? trangThai)
        {
            var boPhanNhans = _context.BoPhanNhans.AsNoTracking();
            var searchTerm = searchString?.Trim();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                boPhanNhans = boPhanNhans.Where(b =>
                    b.TenBoPhan.Contains(searchTerm)
                    || (b.NguoiDaiDien != null && b.NguoiDaiDien.Contains(searchTerm))
                    || (b.SoDienThoai != null && b.SoDienThoai.Contains(searchTerm)));
            }

            if (trangThai.HasValue)
            {
                boPhanNhans = boPhanNhans.Where(b => b.TrangThai == trangThai.Value);
            }

            ViewData["SearchString"] = searchTerm;
            ViewData["TrangThaiFilter"] = new List<SelectListItem>
            {
                new() { Value = "", Text = "Tất cả trạng thái", Selected = !trangThai.HasValue },
                new() { Value = "true", Text = "Đang hoạt động", Selected = trangThai == true },
                new() { Value = "false", Text = "Ngừng hoạt động", Selected = trangThai == false }
            };

            return View(await boPhanNhans.ToListAsync());
        }

        // GET: BoPhanNhans/Details/5
        public async Task<IActionResult> Details(int? id, int? mabophan)
        {
            var targetId = id ?? mabophan;
            if (targetId == null)
            {
                return NotFound();
            }

            var boPhanNhan = await _context.BoPhanNhans
                .FirstOrDefaultAsync(m => m.MaBoPhan == targetId);
            if (boPhanNhan == null)
            {
                return NotFound();
            }

            ViewData["DraftExportCount"] = await _context.PhieuXuats
                .CountAsync(p => p.MaBoPhan == targetId && p.TrangThai == TrangThaiPhieuXuat.Nhap);
            return View(boPhanNhan);
        }

        // GET: BoPhanNhans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BoPhanNhans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaBoPhan,TenBoPhan,NguoiDaiDien,SoDienThoai,MoTa,TrangThai")] BoPhanNhan boPhanNhan)
        {
            boPhanNhan.TenBoPhan = (boPhanNhan.TenBoPhan ?? string.Empty).Trim();
            if (await _context.BoPhanNhans.AnyAsync(b => b.TenBoPhan == boPhanNhan.TenBoPhan))
            {
                ModelState.AddModelError(nameof(boPhanNhan.TenBoPhan), "Tên bộ phận đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(boPhanNhan);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _context.Entry(boPhanNhan).State = EntityState.Detached;
                    if (!await _context.BoPhanNhans.AnyAsync(b => b.TenBoPhan == boPhanNhan.TenBoPhan))
                    {
                        throw;
                    }

                    ModelState.AddModelError(nameof(boPhanNhan.TenBoPhan), "Tên bộ phận đã tồn tại.");
                    return View(boPhanNhan);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(boPhanNhan);
        }

        // GET: BoPhanNhans/Edit/5
        public async Task<IActionResult> Edit(int? id, int? mabophan)
        {
            var targetId = id ?? mabophan;
            if (targetId == null)
            {
                return NotFound();
            }

            var boPhanNhan = await _context.BoPhanNhans.FindAsync(targetId);
            if (boPhanNhan == null)
            {
                return NotFound();
            }
            return View(boPhanNhan);
        }

        // POST: BoPhanNhans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? mabophan, [Bind("MaBoPhan,TenBoPhan,NguoiDaiDien,SoDienThoai,MoTa,TrangThai")] BoPhanNhan boPhanNhan)
        {
            var targetId = id ?? mabophan ?? boPhanNhan.MaBoPhan;
            if (targetId != boPhanNhan.MaBoPhan)
            {
                return NotFound();
            }

            boPhanNhan.TenBoPhan = (boPhanNhan.TenBoPhan ?? string.Empty).Trim();
            if (await _context.BoPhanNhans.AnyAsync(b => b.MaBoPhan != targetId && b.TenBoPhan == boPhanNhan.TenBoPhan))
            {
                ModelState.AddModelError(nameof(boPhanNhan.TenBoPhan), "Tên bộ phận đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.BoPhanNhans.FindAsync(targetId);
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    existing.TenBoPhan = boPhanNhan.TenBoPhan;
                    existing.NguoiDaiDien = boPhanNhan.NguoiDaiDien;
                    existing.SoDienThoai = boPhanNhan.SoDienThoai;
                    existing.MoTa = boPhanNhan.MoTa;
                    existing.TrangThai = boPhanNhan.TrangThai;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoPhanNhanExists(boPhanNhan.MaBoPhan))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (DbUpdateException)
                {
                    if (!await _context.BoPhanNhans.AnyAsync(b => b.MaBoPhan != targetId && b.TenBoPhan == boPhanNhan.TenBoPhan))
                    {
                        throw;
                    }

                    ModelState.AddModelError(nameof(boPhanNhan.TenBoPhan), "Tên bộ phận đã tồn tại.");
                    return View(boPhanNhan);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(boPhanNhan);
        }

        // GET: BoPhanNhans/Delete/5
        public async Task<IActionResult> Delete(int? id, int? mabophan)
        {
            var targetId = id ?? mabophan;
            if (targetId == null)
            {
                return NotFound();
            }

            var boPhanNhan = await _context.BoPhanNhans
                .FirstOrDefaultAsync(m => m.MaBoPhan == targetId);
            if (boPhanNhan == null)
            {
                return NotFound();
            }

            return View(boPhanNhan);
        }

        // POST: BoPhanNhans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? mabophan)
        {
            var targetId = id ?? mabophan;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaBoPhan"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var boPhanNhan = await _context.BoPhanNhans.FindAsync(targetId);
            if (boPhanNhan != null)
            {
                var draftExportCount = await _context.PhieuXuats
                    .CountAsync(p => p.MaBoPhan == targetId && p.TrangThai == TrangThaiPhieuXuat.Nhap);
                if (draftExportCount > 0)
                {
                    TempData["ErrorMessage"] =
                        $"Không thể ngừng hoạt động bộ phận này vì còn {draftExportCount} phiếu xuất Nháp. " +
                        "Hãy xóa hoặc chuyển các phiếu Nháp sang bộ phận khác trước.";
                    return RedirectToAction(nameof(Index));
                }

                boPhanNhan.TrangThai = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bộ phận đã được ngừng hoạt động. Các phiếu xuất cũ vẫn được giữ lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BoPhanNhanExists(int id)
        {
            return _context.BoPhanNhans.Any(e => e.MaBoPhan == id);
        }
    }
}
