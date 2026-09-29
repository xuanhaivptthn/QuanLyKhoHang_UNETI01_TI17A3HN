
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

public class KhoesController : Controller
{
    private readonly AppDbContext _context;

    public KhoesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: KHOS
    public async Task<IActionResult> Index(string searchString)
    {
        var query = _context.Khoes.AsQueryable();

        // Tìm kiếm theo tên kho
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(k =>
                k.TenKho.Contains(searchString));
        }

        // Sắp xếp theo mã kho
        query = query.OrderBy(k => k.MaKho);

        var khoes = await query.ToListAsync();

        ViewBag.SearchString = searchString;

        return View(khoes);
    }

    // =========================================================
    // GET: Kho/Details/5
    // Xem chi tiết kho
    // =========================================================
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var kho = await _context.Khoes
            .FirstOrDefaultAsync(k => k.MaKho == id);

        if (kho == null)
        {
            return NotFound();
        }

        return View(kho);
    }

    // =========================================================
    // GET: Kho/Create
    // Hiển thị form thêm kho
    // =========================================================
    public IActionResult Create()
    {
        return View();
    }

    // =========================================================
    // POST: Kho/Create
    // Thêm kho
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Kho model)
    {
        // Kiểm tra tên kho có bị trùng không
        bool trungTen = await _context.Khoes
            .AnyAsync(k => k.TenKho == model.TenKho);

        if (trungTen)
        {
            ModelState.AddModelError(
                "TenKho",
                "Tên kho đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Khoes.Add(model);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm kho thành công.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // GET: Kho/Edit/5
    // Hiển thị form sửa kho
    // =========================================================
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var kho = await _context.Khoes.FindAsync(id);

        if (kho == null)
        {
            return NotFound();
        }

        return View(kho);
    }

    // =========================================================
    // POST: Kho/Edit/5
    // Sửa kho
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Kho model)
    {
        if (id != model.MaKho)
        {
            return NotFound();
        }

        // Kiểm tra tên kho trùng với kho khác
        bool trungTen = await _context.Khoes
            .AnyAsync(k =>
                k.TenKho == model.TenKho &&
                k.MaKho != model.MaKho);

        if (trungTen)
        {
            ModelState.AddModelError(
                "TenKho",
                "Tên kho đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Lấy kho hiện tại trong Database
            var kho = await _context.Khoes
                .FirstOrDefaultAsync(k => k.MaKho == id);

            if (kho == null)
            {
                return NotFound();
            }

            // Chỉ cập nhật các trường cần thiết
            kho.TenKho = model.TenKho;
            kho.DiaDiem = model.DiaDiem;
            kho.MoTa = model.MoTa;
            kho.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật kho thành công.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!KhoExists(model.MaKho))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // POST: Kho/DoiTrangThai/5
    // Bật / tắt trạng thái kho
    // Không xóa vật lý
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var kho = await _context.Khoes.FindAsync(id);

        if (kho == null)
        {
            return NotFound();
        }

        // Đảo trạng thái
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

    // =========================================================
    // Kiểm tra kho có tồn tại không
    // =========================================================
    private bool KhoExists(int id)
    {
        return _context.Khoes.Any(e => e.MaKho == id);
    }
}

