using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

// Họ và tên: Nguyễn Thị Cúc
// Mã sinh viên: 23103100178
// Nội dung: chi tiết, thêm, sửa, xóa và thay đổi trạng thái, tìm kiếm tên kho

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class KhoesController : Controller
    {
        private readonly AppDbContext _context;

        public KhoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Khoes
        public async Task<IActionResult> Index(string? searchString)
        {
            var query = _context.Kho.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(k => k.TenKho.Contains(searchString));
            }

            query = query.OrderBy(k => k.MaKho);

            ViewBag.SearchString = searchString;

            return View(await query.ToListAsync());
        }

        // GET: Khoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kho = await _context.Kho
                .FirstOrDefaultAsync(m => m.MaKho == id);
            if (kho == null)
            {
                return NotFound();
            }

            return View(kho);
        }

        // GET: Khoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Khoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKho,TenKho,DiaDiem,MoTa,TrangThai")] Kho kho)
        {
            bool trungTen = await _context.Kho
                .AnyAsync(k => k.TenKho == kho.TenKho);

            if (trungTen)
            {
                ModelState.AddModelError("TenKho", "Tên kho đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(kho);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm kho thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(kho);
        }

        // GET: Khoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kho = await _context.Kho.FindAsync(id);
            if (kho == null)
            {
                return NotFound();
            }
            return View(kho);
        }

        // POST: Khoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKho,TenKho,DiaDiem,MoTa,TrangThai")] Kho kho)
        {
            if (id != kho.MaKho)
            {
                return NotFound();
            }

            bool trungTen = await _context.Kho
                .AnyAsync(k => k.TenKho == kho.TenKho && k.MaKho != kho.MaKho);

            if (trungTen)
            {
                ModelState.AddModelError("TenKho", "Tên kho đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingKho = await _context.Kho.FirstOrDefaultAsync(k => k.MaKho == id);
                    if (existingKho == null)
                    {
                        return NotFound();
                    }

                    existingKho.TenKho = kho.TenKho;
                    existingKho.DiaDiem = kho.DiaDiem;
                    existingKho.MoTa = kho.MoTa;
                    existingKho.TrangThai = kho.TrangThai;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật kho thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoExists(kho.MaKho))
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
            return View(kho);
        }

        // POST: Khoes/DoiTrangThai/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var kho = await _context.Kho.FindAsync(id);

            if (kho == null)
            {
                return NotFound();
            }

            kho.TrangThai = !kho.TrangThai;
            await _context.SaveChangesAsync();

            if (kho.TrangThai)
            {
                TempData["Success"] = "Đã kích hoạt kho.";
            }
            else
            {
                TempData["Success"] = "Đã ngừng hoạt động kho.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Khoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kho = await _context.Kho
                .FirstOrDefaultAsync(m => m.MaKho == id);
            if (kho == null)
            {
                return NotFound();
            }

            return View(kho);
        }

        // POST: Khoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kho = await _context.Kho.FindAsync(id);
            if (kho != null)
            {
                _context.Kho.Remove(kho);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa kho thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool KhoExists(int id)
        {
            return _context.Kho.Any(e => e.MaKho == id);
        }
    }
}
