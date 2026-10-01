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
    public class BoPhanNhansController : Controller
    {
        private readonly AppDbContext _context;

        public BoPhanNhansController(AppDbContext context)
        {
            _context = context;
        }

        // GET: BoPhanNhans
        public async Task<IActionResult> Index()
        {
            return View(await _context.BoPhanNhans.ToListAsync());
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
            if (ModelState.IsValid)
            {
                _context.Add(boPhanNhan);
                await _context.SaveChangesAsync();
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boPhanNhan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoPhanNhanExists(boPhanNhan.MaBoPhan))
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
                _context.BoPhanNhans.Remove(boPhanNhan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BoPhanNhanExists(int id)
        {
            return _context.BoPhanNhans.Any(e => e.MaBoPhan == id);
        }
    }
}
