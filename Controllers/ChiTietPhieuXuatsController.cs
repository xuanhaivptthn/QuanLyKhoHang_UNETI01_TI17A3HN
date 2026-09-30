
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
public class ChiTietPhieuXuatsController : Controller
{
    private readonly AppDbContext _context;

    public ChiTietPhieuXuatsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: CHITIETPHIEUXUATS
    public async Task<IActionResult> Index(int? maPhieuXuat)
    {
        var query = _context.ChiTietPhieuXuats.AsQueryable();
        if (maPhieuXuat.HasValue)
        {
            query = query.Where(c => c.MaPhieuXuat == maPhieuXuat.Value);
            ViewData["MaPhieuXuat"] = maPhieuXuat.Value;
        }
        return View(await query.ToListAsync());
    }

    // GET: CHITIETPHIEUXUATS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var chitietphieuxuat = await _context.ChiTietPhieuXuats
            .FirstOrDefaultAsync(m => m.MaChiTietXuat == id);
        if (chitietphieuxuat == null)
        {
            return NotFound();
        }

        return View(chitietphieuxuat);
    }

    // GET: CHITIETPHIEUXUATS/Create
    public IActionResult Create(int? maPhieuXuat)
    {
        return View(new ChiTietPhieuXuat { MaPhieuXuat = maPhieuXuat ?? 0 });
    }

    // POST: CHITIETPHIEUXUATS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaPhieuXuat,MaHang,SoLuongXuat,DonGiaXuatThamChieu,GhiChu")] ChiTietPhieuXuat chitietphieuxuat)
    {
        if (ModelState.IsValid)
        {
            _context.Add(chitietphieuxuat);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(chitietphieuxuat);
    }

    // GET: CHITIETPHIEUXUATS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var chitietphieuxuat = await _context.ChiTietPhieuXuats.FindAsync(id);
        if (chitietphieuxuat == null)
        {
            return NotFound();
        }
        return View(chitietphieuxuat);
    }

    // POST: CHITIETPHIEUXUATS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("MaChiTietXuat,MaPhieuXuat,MaHang,SoLuongXuat,DonGiaXuatThamChieu,GhiChu")] ChiTietPhieuXuat chitietphieuxuat)
    {
        if (id != chitietphieuxuat.MaChiTietXuat)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(chitietphieuxuat);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChiTietPhieuXuatExists(chitietphieuxuat.MaChiTietXuat))
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
        return View(chitietphieuxuat);
    }

    // GET: CHITIETPHIEUXUATS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var chitietphieuxuat = await _context.ChiTietPhieuXuats
            .FirstOrDefaultAsync(m => m.MaChiTietXuat == id);
        if (chitietphieuxuat == null)
        {
            return NotFound();
        }

        return View(chitietphieuxuat);
    }

    // POST: CHITIETPHIEUXUATS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var chitietphieuxuat = await _context.ChiTietPhieuXuats.FindAsync(id);
        if (chitietphieuxuat != null)
        {
            _context.ChiTietPhieuXuats.Remove(chitietphieuxuat);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ChiTietPhieuXuatExists(int? machitietxuat)
    {
        return _context.ChiTietPhieuXuats.Any(e => e.MaChiTietXuat == machitietxuat);
    }
}
