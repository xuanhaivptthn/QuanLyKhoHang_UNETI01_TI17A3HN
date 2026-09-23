
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

public class PhieuXuatsController : Controller
{
    private readonly QuanLyKhoHang_UNETI01_TI17A3HNContext _context;

    public PhieuXuatsController(QuanLyKhoHang_UNETI01_TI17A3HNContext context)
    {
        _context = context;
    }

    // GET: PHIEUXUATS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.PhieuXuats.ToListAsync());
    }

    // GET: PHIEUXUATS/Details/5
    public async Task<IActionResult> Details(int? maphieuxuat)
    {
        if (maphieuxuat == null)
        {
            return NotFound();
        }

        var phieuxuat = await _context.PhieuXuats
            .FirstOrDefaultAsync(m => m.MaPhieuXuat == maphieuxuat);
        if (phieuxuat == null)
        {
            return NotFound();
        }

        return View(phieuxuat);
    }

    // GET: PHIEUXUATS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PHIEUXUATS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaPhieuXuat,MaBoPhan,MaKho,NgayXuat,NguoiLap,TrangThai,GhiChu,BoPhanNhan,Kho,ChiTietPhieuXuats")] PhieuXuat phieuxuat)
    {
        if (ModelState.IsValid)
        {
            _context.Add(phieuxuat);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(phieuxuat);
    }

    // GET: PHIEUXUATS/Edit/5
    public async Task<IActionResult> Edit(int? maphieuxuat)
    {
        if (maphieuxuat == null)
        {
            return NotFound();
        }

        var phieuxuat = await _context.PhieuXuats.FindAsync(maphieuxuat);
        if (phieuxuat == null)
        {
            return NotFound();
        }
        return View(phieuxuat);
    }

    // POST: PHIEUXUATS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? maphieuxuat, [Bind("MaPhieuXuat,MaBoPhan,MaKho,NgayXuat,NguoiLap,TrangThai,GhiChu,BoPhanNhan,Kho,ChiTietPhieuXuats")] PhieuXuat phieuxuat)
    {
        if (maphieuxuat != phieuxuat.MaPhieuXuat)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(phieuxuat);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhieuXuatExists(phieuxuat.MaPhieuXuat))
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
        return View(phieuxuat);
    }

    // GET: PHIEUXUATS/Delete/5
    public async Task<IActionResult> Delete(int? maphieuxuat)
    {
        if (maphieuxuat == null)
        {
            return NotFound();
        }

        var phieuxuat = await _context.PhieuXuats
            .FirstOrDefaultAsync(m => m.MaPhieuXuat == maphieuxuat);
        if (phieuxuat == null)
        {
            return NotFound();
        }

        return View(phieuxuat);
    }

    // POST: PHIEUXUATS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? maphieuxuat)
    {
        var phieuxuat = await _context.PhieuXuats.FindAsync(maphieuxuat);
        if (phieuxuat != null)
        {
            _context.PhieuXuats.Remove(phieuxuat);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PhieuXuatExists(int? maphieuxuat)
    {
        return _context.PhieuXuats.Any(e => e.MaPhieuXuat == maphieuxuat);
    }
}
