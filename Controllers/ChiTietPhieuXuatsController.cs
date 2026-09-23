
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

public class ChiTietPhieuXuatsController : Controller
{
    private readonly QuanLyKhoHang_UNETI01_TI17A3HNContext _context;

    public ChiTietPhieuXuatsController(QuanLyKhoHang_UNETI01_TI17A3HNContext context)
    {
        _context = context;
    }

    // GET: CHITIETPHIEUXUATS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ChiTietPhieuXuats.ToListAsync());
    }

    // GET: CHITIETPHIEUXUATS/Details/5
    public async Task<IActionResult> Details(int? machitietxuat)
    {
        if (machitietxuat == null)
        {
            return NotFound();
        }

        var chitietphieuxuat = await _context.ChiTietPhieuXuats
            .FirstOrDefaultAsync(m => m.MaChiTietXuat == machitietxuat);
        if (chitietphieuxuat == null)
        {
            return NotFound();
        }

        return View(chitietphieuxuat);
    }

    // GET: CHITIETPHIEUXUATS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CHITIETPHIEUXUATS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaChiTietXuat,MaPhieuXuat,MaHang,SoLuongXuat,DonGiaXuatThamChieu,GhiChu,PhieuXuat")] ChiTietPhieuXuat chitietphieuxuat)
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
    public async Task<IActionResult> Edit(int? machitietxuat)
    {
        if (machitietxuat == null)
        {
            return NotFound();
        }

        var chitietphieuxuat = await _context.ChiTietPhieuXuats.FindAsync(machitietxuat);
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
    public async Task<IActionResult> Edit(int? machitietxuat, [Bind("MaChiTietXuat,MaPhieuXuat,MaHang,SoLuongXuat,DonGiaXuatThamChieu,GhiChu,PhieuXuat")] ChiTietPhieuXuat chitietphieuxuat)
    {
        if (machitietxuat != chitietphieuxuat.MaChiTietXuat)
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
    public async Task<IActionResult> Delete(int? machitietxuat)
    {
        if (machitietxuat == null)
        {
            return NotFound();
        }

        var chitietphieuxuat = await _context.ChiTietPhieuXuats
            .FirstOrDefaultAsync(m => m.MaChiTietXuat == machitietxuat);
        if (chitietphieuxuat == null)
        {
            return NotFound();
        }

        return View(chitietphieuxuat);
    }

    // POST: CHITIETPHIEUXUATS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? machitietxuat)
    {
        var chitietphieuxuat = await _context.ChiTietPhieuXuats.FindAsync(machitietxuat);
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
