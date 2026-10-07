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
    public class PhieuNhapsController : Controller
    {
        private readonly AppDbContext _context;

        public PhieuNhapsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PhieuNhaps
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PhieuNhap
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .Include(p => p.ChiTietPhieuNhaps);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PhieuNhaps/Details/5
        public async Task<IActionResult> Details(int? id, int? maphieunhap)
        {
            var targetId = id ?? maphieunhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhap
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(ct => ct.HangHoa)
                .FirstOrDefaultAsync(m => m.MaPhieuNhap == targetId);
            if (phieuNhap == null)
            {
                return NotFound();
            }

            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Create
        public IActionResult Create()
        {
            // Only active kho and suppliers may be selected
            ViewData["MaKho"] = new SelectList(_context.Kho.Where(k => k.TrangThai), "MaKho", "TenKho");
            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap");
            return View();
        }

        // POST: PhieuNhaps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuNhap,MaNhaCungCap,MaKho,NgayNhap,NguoiLap,TrangThai,GhiChu")] PhieuNhap phieuNhap)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phieuNhap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho", phieuNhap.MaKho);
            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Edit/5
        public async Task<IActionResult> Edit(int? id, int? maphieunhap)
        {
            var targetId = id ?? maphieunhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhap.FindAsync(targetId);
            if (phieuNhap == null)
            {
                return NotFound();
            }
            // Only allow edit when status is Nháp (0) or Chờ xác nhận (1)
            if (!(phieuNhap.TrangThai == 0 || phieuNhap.TrangThai == 1))
            {
                return Forbid();
            }
            ViewData["MaKho"] = new SelectList(_context.Kho.Where(k => k.TrangThai), "MaKho", "TenKho", phieuNhap.MaKho);
            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
            return View(phieuNhap);
        }

        // POST: PhieuNhaps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? maphieunhap, [Bind("MaPhieuNhap,MaNhaCungCap,MaKho,NgayNhap,NguoiLap,TrangThai,GhiChu")] PhieuNhap phieuNhap)
        {
            var targetId = id ?? maphieunhap ?? phieuNhap.MaPhieuNhap;
            if (targetId != phieuNhap.MaPhieuNhap)
            {
                return NotFound();
            }

            // Verify the existing phieuNhap status before allowing update
            var existing = await _context.PhieuNhap.AsNoTracking().FirstOrDefaultAsync(p => p.MaPhieuNhap == phieuNhap.MaPhieuNhap);
            if (existing == null)
            {
                return NotFound();
            }
            if (!(existing.TrangThai == 0 || existing.TrangThai == 1))
            {
                ModelState.AddModelError(string.Empty, "Phiếu chỉ được sửa khi ở trạng thái Nháp hoặc Chờ xác nhận.");
                ViewData["MaKho"] = new SelectList(_context.Kho.Where(k => k.TrangThai), "MaKho", "TenKho", phieuNhap.MaKho);
                ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
                return View(phieuNhap);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // If changing status to 'Đã hoàn tất' (2), apply stock updates atomically
                    if (phieuNhap.TrangThai == 2 && existing.TrangThai != 2)
                    {
                        // Load full phieu with details and product info
                        var phFull = await _context.PhieuNhap
                            .Include(p => p.ChiTietPhieuNhaps)
                                .ThenInclude(ct => ct.HangHoa)
                            .FirstOrDefaultAsync(p => p.MaPhieuNhap == phieuNhap.MaPhieuNhap);

                        if (phFull == null)
                        {
                            ModelState.AddModelError(string.Empty, "Phiếu không tồn tại.");
                            ViewData["MaKho"] = new SelectList(_context.Kho.Where(k => k.TrangThai), "MaKho", "TenKho", phieuNhap.MaKho);
                            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
                            return View(phieuNhap);
                        }

                        // Validate supplier and kho active
                        var supplier = await _context.NhaCungCap.FindAsync(phFull.MaNhaCungCap);
                        var kho = await _context.Kho.FindAsync(phFull.MaKho);
                        if (supplier == null || !supplier.TrangThai)
                        {
                            ModelState.AddModelError(string.Empty, "Nhà cung cấp không hoạt động hoặc không tồn tại.");
                        }
                        if (kho == null || !kho.TrangThai)
                        {
                            ModelState.AddModelError(string.Empty, "Kho không hoạt động hoặc không tồn tại.");
                        }

                        // Validate all products active
                        var inactiveProducts = phFull.ChiTietPhieuNhaps.Where(ct => ct.HangHoa == null || !ct.HangHoa.TrangThai).ToList();
                        if (inactiveProducts.Any())
                        {
                            ModelState.AddModelError(string.Empty, "Một hoặc nhiều hàng hóa trong chi tiết đã ngưng hoạt động. Không thể hoàn tất phiếu.");
                        }

                        if (!ModelState.IsValid)
                        {
                            ViewData["MaKho"] = new SelectList(_context.Kho.Where(k => k.TrangThai), "MaKho", "TenKho", phieuNhap.MaKho);
                            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
                            return View(phieuNhap);
                        }

                        // Begin transaction to update stock and history
                        using (var tx = await _context.Database.BeginTransactionAsync())
                        {
                            foreach (var ct in phFull.ChiTietPhieuNhaps)
                            {
                                // Find or create TonKho entry
                                var ton = await _context.TonKhoes.FindAsync(phFull.MaKho, ct.MaHang);
                                if (ton == null)
                                {
                                    ton = new TonKho
                                    {
                                        MaKho = phFull.MaKho,
                                        MaHang = ct.MaHang,
                                        SoLuongTon = ct.SoLuongNhap,
                                        NgayCapNhat = DateTime.Now
                                    };
                                    _context.TonKhoes.Add(ton);
                                }
                                else
                                {
                                    ton.SoLuongTon += ct.SoLuongNhap;
                                    ton.NgayCapNhat = DateTime.Now;
                                    _context.TonKhoes.Update(ton);
                                }

                                // Add history record
                                var lichSu = new LichSuTonKho
                                {
                                    MaHang = ct.MaHang,
                                    MaKho = phFull.MaKho,
                                    NgayPhatSinh = DateTime.Now,
                                    LoaiGiaoDich = "Nhập",
                                    MaPhieu = phFull.MaPhieuNhap.ToString(),
                                    SoLuong = ct.SoLuongNhap,
                                    TonSauGiaoDich = ton.SoLuongTon,
                                    NguoiThucHien = phFull.NguoiLap,
                                    GhiChu = phFull.GhiChu
                                };
                                _context.LichSuTonKhoes.Add(lichSu);
                            }

                            // Update phieu status and save all
                            _context.Update(phieuNhap);
                            await _context.SaveChangesAsync();
                            await tx.CommitAsync();
                        }
                    }
                    else
                    {
                        _context.Update(phieuNhap);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuNhapExists(phieuNhap.MaPhieuNhap))
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
            ViewData["MaKho"] = new SelectList(_context.Kho, "MaKho", "TenKho", phieuNhap.MaKho);
            ViewData["MaNhaCungCap"] = new SelectList(_context.NhaCungCap.Where(n => n.TrangThai), "MaNhaCungCap", "TenNhaCungCap", phieuNhap.MaNhaCungCap);
            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Delete/5
        public async Task<IActionResult> Delete(int? id, int? maphieunhap)
        {
            var targetId = id ?? maphieunhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhap
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .FirstOrDefaultAsync(m => m.MaPhieuNhap == targetId);
            if (phieuNhap == null)
            {
                return NotFound();
            }

            return View(phieuNhap);
        }

        // POST: PhieuNhaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? maphieunhap)
        {
            var targetId = id ?? maphieunhap;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaPhieuNhap"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhap.FindAsync(targetId);
            if (phieuNhap != null)
            {
                _context.PhieuNhap.Remove(phieuNhap);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PhieuNhapExists(int id)
        {
            return _context.PhieuNhap.Any(e => e.MaPhieuNhap == id);
        }
    }
}
