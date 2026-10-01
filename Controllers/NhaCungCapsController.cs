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

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class NhaCungCapsController : Controller
    {
        private readonly AppDbContext _context;

        public NhaCungCapsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: NhaCungCaps
        public async Task<IActionResult> Index(string? searchString, bool? trangThai)
        {
            var query = _context.NhaCungCap.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(n =>
                    n.TenNhaCungCap.Contains(searchString) ||
                    n.MaNhaCungCap.ToString().Contains(searchString) ||
                    (n.SoDienThoai != null && n.SoDienThoai.Contains(searchString)) ||
                    (n.Email != null && n.Email.Contains(searchString)));
            }

            if (trangThai.HasValue)
            {
                query = query.Where(n => n.TrangThai == trangThai.Value);
            }

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;

            return View(await query.ToListAsync());
        }

        // GET: NhaCungCaps/Details/5
        public async Task<IActionResult> Details(int? id, int? manhacungcap)
        {
            var targetId = id ?? manhacungcap;
            if (targetId == null)
            {
                return NotFound();
            }

            var nhaCungCap = await _context.NhaCungCap
                .Include(n => n.PhieuNhaps)
                .FirstOrDefaultAsync(m => m.MaNhaCungCap == targetId);

            if (nhaCungCap == null)
            {
                return NotFound();
            }

            return View(nhaCungCap);
        }

        // GET: NhaCungCaps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NhaCungCaps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNhaCungCap,TenNhaCungCap,SoDienThoai,Email,DiaChi,TrangThai")] NhaCungCap nhaCungCap)
        {
            if (!string.IsNullOrEmpty(nhaCungCap.Email) && await _context.NhaCungCap.AnyAsync(n => n.Email == nhaCungCap.Email))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại trong hệ thống!");
            }

            if (!string.IsNullOrEmpty(nhaCungCap.SoDienThoai) && await _context.NhaCungCap.AnyAsync(n => n.SoDienThoai == nhaCungCap.SoDienThoai))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã tồn tại trong hệ thống!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(nhaCungCap);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm mới nhà cung cấp thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Vui lòng kiểm tra lại thông tin nhập!";
            return View(nhaCungCap);
        }

        // GET: NhaCungCaps/Edit/5
        public async Task<IActionResult> Edit(int? id, int? manhacungcap)
        {
            var targetId = id ?? manhacungcap;
            if (targetId == null)
            {
                return NotFound();
            }

            var nhaCungCap = await _context.NhaCungCap.FindAsync(targetId);
            if (nhaCungCap == null)
            {
                return NotFound();
            }
            return View(nhaCungCap);
        }

        // POST: NhaCungCaps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? manhacungcap, [Bind("MaNhaCungCap,TenNhaCungCap,SoDienThoai,Email,DiaChi,TrangThai")] NhaCungCap nhaCungCap)
        {
            var targetId = id ?? manhacungcap ?? nhaCungCap.MaNhaCungCap;
            if (targetId != nhaCungCap.MaNhaCungCap)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhaCungCap);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật nhà cung cấp thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhaCungCapExists(nhaCungCap.MaNhaCungCap))
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
            return View(nhaCungCap);
        }

        // POST: NhaCungCaps/DoiTrangThai/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var nhaCungCap = await _context.NhaCungCap.FindAsync(id);

            if (nhaCungCap == null)
            {
                return NotFound();
            }

            nhaCungCap.TrangThai = !nhaCungCap.TrangThai;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thay đổi trạng thái nhà cung cấp.";

            return RedirectToAction(nameof(Index));
        }

        // GET: NhaCungCaps/Delete/5
        public async Task<IActionResult> Delete(int? id, int? manhacungcap)
        {
            var targetId = id ?? manhacungcap;
            if (targetId == null)
            {
                return NotFound();
            }

            var nhaCungCap = await _context.NhaCungCap
                .FirstOrDefaultAsync(m => m.MaNhaCungCap == targetId);
            if (nhaCungCap == null)
            {
                return NotFound();
            }

            return View(nhaCungCap);
        }

        // POST: NhaCungCaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? manhacungcap)
        {
            var targetId = id ?? manhacungcap;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaNhaCungCap"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var nhaCungCap = await _context.NhaCungCap.FindAsync(targetId);
            if (nhaCungCap != null)
            {
                _context.NhaCungCap.Remove(nhaCungCap);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa nhà cung cấp thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NhaCungCapExists(int id)
        {
            return _context.NhaCungCap.Any(e => e.MaNhaCungCap == id);
        }
    }
}
