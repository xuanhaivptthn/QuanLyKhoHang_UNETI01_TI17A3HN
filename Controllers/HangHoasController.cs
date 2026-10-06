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
// Nội dung: Danh sách + Tìm kiếm + Lọc + CRUD Hàng hóa

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class HangHoasController : Controller
    {
        private readonly AppDbContext _context;

        public HangHoasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: HangHoas
        public async Task<IActionResult> Index(
            string? searchString,
            int? maLoaiHang,
            int? maDonViTinh,
            bool? trangThai,
            decimal? giaTu,
            decimal? giaDen,
            string? sortOrder,
            int page = 1)
        {
            int pageSize = 10;
            if (page < 1) page = 1;

            var query = _context.HangHoa
                .Include(h => h.LoaiHang)
                .Include(h => h.DonViTinh)
                .AsQueryable();

            // 1. TÌM KIẾM THEO MÃ HÀNG / TÊN HÀNG / TÊN LOẠI HÀNG / TÊN KHO
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(h =>
                    h.TenHang.Contains(searchString) ||
                    h.MaHang.ToString().Contains(searchString) ||
                    (h.LoaiHang != null && h.LoaiHang.TenLoaiHang.Contains(searchString)) ||
                    h.DanhSachTonKho!.Any(t => t.Kho != null && t.Kho.TenKho.Contains(searchString)));
            }

            // 2. LỌC THEO LOẠI HÀNG
            if (maLoaiHang.HasValue)
            {
                query = query.Where(h => h.MaLoaiHang == maLoaiHang.Value);
            }

            // 3. LỌC THEO ĐƠN VỊ TÍNH
            if (maDonViTinh.HasValue)
            {
                query = query.Where(h => h.MaDonViTinh == maDonViTinh.Value);
            }

            // 4. LỌC THEO TRẠNG THÁI
            if (trangThai.HasValue)
            {
                query = query.Where(h => h.TrangThai == trangThai.Value);
            }

            // 5. LỌC THEO KHOẢNG GIÁ
            if (giaTu.HasValue)
            {
                query = query.Where(h => h.GiaNhapThamKhao >= giaTu.Value);
            }

            if (giaDen.HasValue)
            {
                query = query.Where(h => h.GiaNhapThamKhao <= giaDen.Value);
            }

            // 6. SẮP XẾP
            switch (sortOrder)
            {
                case "ten_tang_dan":
                    query = query.OrderBy(h => h.TenHang);
                    break;
                case "ten_giam_dan":
                    query = query.OrderByDescending(h => h.TenHang);
                    break;
                case "gia_tang_dan":
                    query = query.OrderBy(h => h.GiaNhapThamKhao);
                    break;
                case "gia_giam_dan":
                    query = query.OrderByDescending(h => h.GiaNhapThamKhao);
                    break;
                case "ton_tang":
                    query = query.OrderBy(h => h.MucTonToiThieu);
                    break;
                case "ton_giam":
                    query = query.OrderByDescending(h => h.MucTonToiThieu);
                    break;
                default:
                    query = query.OrderBy(h => h.MaHang);
                    break;
            }

            // 7. ĐẾM VÀ PHÂN TRANG
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var hangHoas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.LoaiHangs = new SelectList(await _context.LoaiHangs.ToListAsync(), "MaLoaiHang", "TenLoaiHang", maLoaiHang);
            ViewBag.DonViTinhs = new SelectList(await _context.DonViTinhs.ToListAsync(), "MaDonViTinh", "TenDonViTinh", maDonViTinh);

            ViewBag.SearchString = searchString;
            ViewBag.MaLoaiHang = maLoaiHang;
            ViewBag.MaDonViTinh = maDonViTinh;
            ViewBag.TrangThai = trangThai;
            ViewBag.GiaTu = giaTu;
            ViewBag.GiaDen = giaDen;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(hangHoas);
        }

        // GET: HangHoas/Details/5
        public async Task<IActionResult> Details(int? id, int? mahang)
        {
            var targetId = id ?? mahang;
            if (targetId == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoa
                .Include(h => h.LoaiHang)
                .Include(h => h.DonViTinh)
                .Include(h => h.DanhSachTonKho!)
                    .ThenInclude(t => t.Kho)
                .FirstOrDefaultAsync(m => m.MaHang == targetId);

            if (hangHoa == null)
            {
                return NotFound();
            }

            return View(hangHoa);
        }

        // GET: HangHoas/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        // POST: HangHoas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaHang,TenHang,MaLoaiHang,MaDonViTinh,GiaNhapThamKhao,MucTonToiThieu,MoTa,TrangThai")] HangHoa hangHoa)
        {
            if (string.IsNullOrWhiteSpace(hangHoa.TenHang))
            {
                ModelState.AddModelError("TenHang", "Tên hàng không được để trống.");
            }

            if (hangHoa.GiaNhapThamKhao < 0)
            {
                ModelState.AddModelError("GiaNhapThamKhao", "Giá nhập tham khảo không được âm.");
            }

            if (hangHoa.MucTonToiThieu < 0)
            {
                ModelState.AddModelError("MucTonToiThieu", "Mức tồn tối thiểu không được âm.");
            }

            var loaiHangExists = await _context.LoaiHangs.AnyAsync(x => x.MaLoaiHang == hangHoa.MaLoaiHang);
            if (!loaiHangExists)
            {
                ModelState.AddModelError("MaLoaiHang", "Loại hàng không tồn tại.");
            }

            var donViTinhExists = await _context.DonViTinhs.AnyAsync(x => x.MaDonViTinh == hangHoa.MaDonViTinh);
            if (!donViTinhExists)
            {
                ModelState.AddModelError("MaDonViTinh", "Đơn vị tính không tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(hangHoa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns(hangHoa.MaLoaiHang, hangHoa.MaDonViTinh);
            return View(hangHoa);
        }

        // GET: HangHoas/Edit/5
        public async Task<IActionResult> Edit(int? id, int? mahang)
        {
            var targetId = id ?? mahang;
            if (targetId == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoa.FindAsync(targetId);
            if (hangHoa == null)
            {
                return NotFound();
            }

            await LoadDropdowns(hangHoa.MaLoaiHang, hangHoa.MaDonViTinh);
            return View(hangHoa);
        }

        // POST: HangHoas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int? id,
            int? mahang,
            [Bind("MaHang,TenHang,MaLoaiHang,MaDonViTinh,GiaNhapThamKhao,MucTonToiThieu,MoTa,TrangThai")] HangHoa hangHoa)
        {
            var targetId = id ?? mahang ?? hangHoa.MaHang;
            if (targetId != hangHoa.MaHang)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(hangHoa.TenHang))
            {
                ModelState.AddModelError("TenHang", "Tên hàng không được để trống.");
            }

            if (hangHoa.GiaNhapThamKhao < 0)
            {
                ModelState.AddModelError("GiaNhapThamKhao", "Giá nhập tham khảo không được âm.");
            }

            if (hangHoa.MucTonToiThieu < 0)
            {
                ModelState.AddModelError("MucTonToiThieu", "Mức tồn tối thiểu không được âm.");
            }

            var loaiHangExists = await _context.LoaiHangs.AnyAsync(x => x.MaLoaiHang == hangHoa.MaLoaiHang);
            if (!loaiHangExists)
            {
                ModelState.AddModelError("MaLoaiHang", "Loại hàng không tồn tại.");
            }

            var donViTinhExists = await _context.DonViTinhs.AnyAsync(x => x.MaDonViTinh == hangHoa.MaDonViTinh);
            if (!donViTinhExists)
            {
                ModelState.AddModelError("MaDonViTinh", "Đơn vị tính không tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hangHoa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HangHoaExists(hangHoa.MaHang))
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

            await LoadDropdowns(hangHoa.MaLoaiHang, hangHoa.MaDonViTinh);
            return View(hangHoa);
        }

        // GET: HangHoas/Delete/5
        public async Task<IActionResult> Delete(int? id, int? mahang)
        {
            var targetId = id ?? mahang;
            if (targetId == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoa
                .Include(h => h.LoaiHang)
                .Include(h => h.DonViTinh)
                .FirstOrDefaultAsync(m => m.MaHang == targetId);

            if (hangHoa == null)
            {
                return NotFound();
            }

            return View(hangHoa);
        }

        // POST: HangHoas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? mahang)
        {
            var targetId = id ?? mahang;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaHang"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoa.FindAsync(targetId);
            if (hangHoa != null)
            {
                _context.HangHoa.Remove(hangHoa);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: HangHoas/DoiTrangThai/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var hangHoa = await _context.HangHoa.FindAsync(id);
            if (hangHoa == null)
            {
                return NotFound();
            }

            hangHoa.TrangThai = !hangHoa.TrangThai;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? selectedLoaiHang = null, int? selectedDonViTinh = null)
        {
            ViewBag.LoaiHangs = new SelectList(
                await _context.LoaiHangs.ToListAsync(),
                "MaLoaiHang",
                "TenLoaiHang",
                selectedLoaiHang
            );

            ViewBag.DonViTinhs = new SelectList(
                await _context.DonViTinhs.ToListAsync(),
                "MaDonViTinh",
                "TenDonViTinh",
                selectedDonViTinh
            );
        }

        private bool HangHoaExists(int id)
        {
            return _context.HangHoa.Any(e => e.MaHang == id);
        }
    }
}
