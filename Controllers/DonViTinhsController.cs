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
    public class DonViTinhsController : Controller
    {
        private readonly AppDbContext _context;

        public DonViTinhsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DonViTinhs
        public async Task<IActionResult> Index()
        {
            return View(await _context.DonViTinhs.ToListAsync());
        }

        // GET: DonViTinhs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donViTinh = await _context.DonViTinhs
                .FirstOrDefaultAsync(m => m.MaDonViTinh == id);
            if (donViTinh == null)
            {
                return NotFound();
            }

            return View(donViTinh);
        }

        // GET: DonViTinhs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DonViTinhs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDonViTinh,TenDonViTinh,MoTa,TrangThai")] DonViTinh donViTinh)
        {
            if (ModelState.IsValid)
            {
                _context.Add(donViTinh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(donViTinh);
        }

        // GET: DonViTinhs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donViTinh = await _context.DonViTinhs.FindAsync(id);
            if (donViTinh == null)
            {
                return NotFound();
            }
            return View(donViTinh);
        }

        // POST: DonViTinhs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDonViTinh,TenDonViTinh,MoTa,TrangThai")] DonViTinh donViTinh)
        {
            if (id != donViTinh.MaDonViTinh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donViTinh);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonViTinhExists(donViTinh.MaDonViTinh))
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
            return View(donViTinh);
        }

        // GET: DonViTinhs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donViTinh = await _context.DonViTinhs
                .FirstOrDefaultAsync(m => m.MaDonViTinh == id);
            if (donViTinh == null)
            {
                return NotFound();
            }

            return View(donViTinh);
        }

        // POST: DonViTinhs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donViTinh = await _context.DonViTinhs.FindAsync(id);
            if (donViTinh != null)
            {
                _context.DonViTinhs.Remove(donViTinh);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DonViTinhExists(int id)
        {
            return _context.DonViTinhs.Any(e => e.MaDonViTinh == id);
        }
    }
}
