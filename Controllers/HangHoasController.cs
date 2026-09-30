using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

public class HangHoasController : Controller
{
    private readonly AppDbContext _context;

    public HangHoasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: HANGHOAS
    //Họ và tên: Nguyễn Thị Cúc
    //Mã sinh viên: 23103100178
    //Nội dung: Danh sách + Tìm kiếm + Lọc
 
    public async Task<IActionResult> Index(
        string searchString,
        int? maLoaiHang,
        int? maDonViTinh,
        bool? trangThai,
        decimal? giaTu,
        decimal? giaDen,
        string sortOrder,
        int page = 1)
    {
        // Số hàng hóa trên mỗi trang
        int pageSize = 10;

        // Không cho page nhỏ hơn 1
        if (page < 1)
        {
            page = 1;
        }

        // Truy vấn bảng hàng hóa
        var query = _context.HangHoa
            .Include(h => h.LoaiHang)
            .Include(h => h.DonViTinh)
            .AsQueryable();

        // 1. TÌM KIẾM THEO MÃ HÀNG / TÊN HÀNG
        
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim();

            query = query.Where(h =>
                h.TenHang.Contains(searchString) ||
                h.MaHang.ToString().Contains(searchString));
        }

        // 2. LỌC THEO LOẠI HÀNG
        if (maLoaiHang.HasValue)
        {
            query = query.Where(h =>
                h.MaLoaiHang == maLoaiHang.Value);
        }

        // 3. LỌC THEO ĐƠN VỊ TÍNH
        if (maDonViTinh.HasValue)
        {
            query = query.Where(h =>
                h.MaDonViTinh == maDonViTinh.Value);
        }

        // 4. LỌC THEO TRẠNG THÁI
        if (trangThai.HasValue)
        {
            query = query.Where(h =>
                h.TrangThai == trangThai.Value);
        }

        // 5.LỌC THEO KHOẢNG GIÁ
        if (giaTu.HasValue)
        {
            query = query.Where(h =>
                h.GiaNhapThamKhao >= giaTu.Value);
        }

        if (giaDen.HasValue)
        {
            query = query.Where(h =>
                h.GiaNhapThamKhao <= giaDen.Value);
        }

        // 6. SẮP XẾP
        switch (sortOrder)
        {
            // Tên A -> Z
            case "ten_asc":
                query = query.OrderBy(h => h.TenHang);
                break;

            // Tên Z -> A
            case "ten_desc":
                query = query.OrderByDescending(h => h.TenHang);
                break;

            // Giá tăng dần
            case "gia_asc":
                query = query.OrderBy(h => h.GiaNhapThamKhao);
                break;

            // Giá giảm dần
            case "gia_desc":
                query = query.OrderByDescending(h => h.GiaNhapThamKhao);
                break;

            // Mức tồn tối thiểu tăng
            case "ton_asc":
                query = query.OrderBy(h => h.MucTonToiThieu);
                break;

            // Mức tồn tối thiểu giảm
            case "ton_desc":
                query = query.OrderByDescending(h => h.MucTonToiThieu);
                break;

            // Mặc định
            default:
                query = query.OrderBy(h => h.MaHang);
                break;
        }

        // 7. ĐẾM TỔNG SỐ HÀNG HÓA
        int totalItems = await query.CountAsync();
        int totalPages =
            (int)Math.Ceiling(totalItems / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        // 8. PHÂN TRANG
        var hangHoas = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 9. LOAD LOẠI HÀNG
        ViewBag.LoaiHangs = new SelectList(
            await _context.LoaiHangs.ToListAsync(),
            "MaLoaiHang",
            "TenLoaiHang",
            maLoaiHang
        );

        // 10. LOAD ĐƠN VỊ TÍNH
        ViewBag.DonViTinhs = new SelectList(
            await _context.DonViTinhs.ToListAsync(),
            "MaDonViTinh",
            "TenDonViTinh",
            maDonViTinh
        );

        // 11. GIỮ LẠI ĐIỀU KIỆN TÌM KIẾM / LỌC / SẮP XẾP
        ViewBag.SearchString = searchString;
        ViewBag.MaLoaiHang = maLoaiHang;
        ViewBag.MaDonViTinh = maDonViTinh;
        ViewBag.TrangThai = trangThai;
        ViewBag.GiaTu = giaTu;
        ViewBag.GiaDen = giaDen;
        ViewBag.SortOrder = sortOrder;

        // Thông tin phân trang
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(hangHoas);
    }

    // GET: HANGHOAS/Details/5
    public async Task<IActionResult> Details(int? mahang)
    {
        if (mahang == null)
        {
            return NotFound();
        }

        var hanghoa = await _context.HangHoa
            .Include(h => h.LoaiHang)
            .Include(h => h.DonViTinh)
            .FirstOrDefaultAsync(h => h.MaHang == mahang);

        if (hanghoa == null)
        {
            return NotFound();
        }

        return View(hanghoa);
    }

    // GET: HANGHOAS/Create
    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();

        return View();
    }

    // POST: HANGHOAS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MaHang,TenHang,MaLoaiHang,MaDonViTinh,GiaNhapThamKhao,MucTonToiThieu,MoTa,TrangThai")]
        HangHoa hanghoa)
    {
        if (string.IsNullOrWhiteSpace(hanghoa.TenHang))
        {
            ModelState.AddModelError(
                "TenHang",
                "Tên hàng không được để trống.");
        }

        if (hanghoa.GiaNhapThamKhao < 0)
        {
            ModelState.AddModelError(
                "GiaNhapThamKhao",
                "Giá nhập tham khảo không được âm.");
        }

        if (hanghoa.MucTonToiThieu < 0)
        {
            ModelState.AddModelError(
                "MucTonToiThieu",
                "Mức tồn tối thiểu không được âm.");
        }

        var loaiHangExists = await _context.LoaiHangs
            .AnyAsync(x => x.MaLoaiHang == hanghoa.MaLoaiHang);

        if (!loaiHangExists)
        {
            ModelState.AddModelError(
                "MaLoaiHang",
                "Loại hàng không tồn tại.");
        }

        var donViTinhExists = await _context.DonViTinhs
            .AnyAsync(x => x.MaDonViTinh == hanghoa.MaDonViTinh);

        if (!donViTinhExists)
        {
            ModelState.AddModelError(
                "MaDonViTinh",
                "Đơn vị tính không tồn tại.");
        }

        if (ModelState.IsValid)
        {
            _context.HangHoa.Add(hanghoa);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await LoadDropdowns(
            hanghoa.MaLoaiHang,
            hanghoa.MaDonViTinh);

        return View(hanghoa);
    }

    // GET: HANGHOAS/Edit/5
    public async Task<IActionResult> Edit(int? mahang)
    {
        if (mahang == null)
        {
            return NotFound();
        }

        var hanghoa = await _context.HangHoa
            .FindAsync(mahang);

        if (hanghoa == null)
        {
            return NotFound();
        }

        await LoadDropdowns(
            hanghoa.MaLoaiHang,
            hanghoa.MaDonViTinh);

        return View(hanghoa);
    }

    // POST: HANGHOAS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? mahang,
        [Bind("MaHang,TenHang,MaLoaiHang,MaDonViTinh,GiaNhapThamKhao,MucTonToiThieu,MoTa,TrangThai")]
        HangHoa hanghoa)
    {
        if (mahang != hanghoa.MaHang)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(hanghoa.TenHang))
        {
            ModelState.AddModelError(
                "TenHang",
                "Tên hàng không được để trống.");
        }

        if (hanghoa.GiaNhapThamKhao < 0)
        {
            ModelState.AddModelError(
                "GiaNhapThamKhao",
                "Giá nhập tham khảo không được âm.");
        }

        if (hanghoa.MucTonToiThieu < 0)
        {
            ModelState.AddModelError(
                "MucTonToiThieu",
                "Mức tồn tối thiểu không được âm.");
        }

        var loaiHangExists = await _context.LoaiHangs
            .AnyAsync(x => x.MaLoaiHang == hanghoa.MaLoaiHang);

        if (!loaiHangExists)
        {
            ModelState.AddModelError(
                "MaLoaiHang",
                "Loại hàng không tồn tại.");
        }

        var donViTinhExists = await _context.DonViTinhs
            .AnyAsync(x => x.MaDonViTinh == hanghoa.MaDonViTinh);

        if (!donViTinhExists)
        {
            ModelState.AddModelError(
                "MaDonViTinh",
                "Đơn vị tính không tồn tại.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.HangHoa.Update(hanghoa);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HangHoaExists(hanghoa.MaHang))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await LoadDropdowns(
            hanghoa.MaLoaiHang,
            hanghoa.MaDonViTinh);

        return View(hanghoa);
    }

    // GET: HANGHOAS/Delete/5
    public async Task<IActionResult> Delete(int? mahang)
    {
        if (mahang == null)
        {
            return NotFound();
        }

        var hanghoa = await _context.HangHoa
            .Include(h => h.LoaiHang)
            .Include(h => h.DonViTinh)
            .FirstOrDefaultAsync(h => h.MaHang == mahang);

        if (hanghoa == null)
        {
            return NotFound();
        }

        return View(hanghoa);
    }

    // POST: HANGHOAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? mahang)
    {
        if (mahang == null)
        {
            return NotFound();
        }

        var hanghoa = await _context.HangHoa
            .FindAsync(mahang);

        if (hanghoa == null)
        {
            return NotFound();
        }

        _context.HangHoa.Remove(hanghoa);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ĐỔI TRẠNG THÁI HÀNG HÓA
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var hanghoa = await _context.HangHoa
            .FindAsync(id);

        if (hanghoa == null)
        {
            return NotFound();
        }

        hanghoa.TrangThai = !hanghoa.TrangThai;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // LOAD DROPDOWN LOẠI HÀNG + ĐƠN VỊ TÍNH
    private async Task LoadDropdowns(
        int? selectedLoaiHang = null,
        int? selectedDonViTinh = null)
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

    // KIỂM TRA HÀNG HÓA TỒN TẠI
    private bool HangHoaExists(int mahang)
    {
        return _context.HangHoa
            .Any(e => e.MaHang == mahang);
    }
}