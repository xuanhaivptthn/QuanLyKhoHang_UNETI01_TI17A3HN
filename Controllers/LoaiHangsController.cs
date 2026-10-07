using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

// Họ và tên: Trần Xuân Hải
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class LoaiHangsController : Controller
    {
        private readonly AppDbContext _context;

        public LoaiHangsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LoaiHangs
        public async Task<IActionResult> Index()
        {
            return View(await _context.LoaiHangs.ToListAsync());
        }

        // GET: LoaiHangs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiHang = await _context.LoaiHangs
                .FirstOrDefaultAsync(m => m.MaLoaiHang == id);
            if (loaiHang == null)
            {
                return NotFound();
            }

            return View(loaiHang);
        }

        // GET: LoaiHangs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoaiHangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLoaiHang,TenLoaiHang,MoTa,TrangThai")] LoaiHang loaiHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaiHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loaiHang);
        }

        // GET: LoaiHangs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiHang = await _context.LoaiHangs.FindAsync(id);
            if (loaiHang == null)
            {
                return NotFound();
            }
            return View(loaiHang);
        }

        // POST: LoaiHangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaLoaiHang,TenLoaiHang,MoTa,TrangThai")] LoaiHang loaiHang)
        {
            if (id != loaiHang.MaLoaiHang)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiHangExists(loaiHang.MaLoaiHang))
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
            return View(loaiHang);
        }

        // GET: LoaiHangs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiHang = await _context.LoaiHangs
                .FirstOrDefaultAsync(m => m.MaLoaiHang == id);
            if (loaiHang == null)
            {
                return NotFound();
            }

            return View(loaiHang);
        }

        // POST: LoaiHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaiHang = await _context.LoaiHangs.FindAsync(id);
            if (loaiHang != null)
            {
                var hasHangHoa = await _context.HangHoa.AnyAsync(h => h.MaLoaiHang == id);
                if (hasHangHoa)
                {
                    TempData["Error"] = "Không thể xóa loại hàng này vì vẫn còn hàng hóa thuộc loại này. Vui lòng chuyển hoặc xóa các hàng hóa liên quan trước.";
                    return RedirectToAction(nameof(Index));
                }

                _context.LoaiHangs.Remove(loaiHang);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa loại hàng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LoaiHangExists(int id)
        {
            return _context.LoaiHangs.Any(e => e.MaLoaiHang == id);
        }
    }
}
