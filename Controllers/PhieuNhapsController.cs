
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;

public class PhieuNhapsController : Controller
{
    private readonly AppDbContext _context;

    public PhieuNhapsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: PHIEUNHAPS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.PhieuNhap.ToListAsync());
    }

    // GET: PHIEUNHAPS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var phieunhap = await _context.PhieuNhap
            .FirstOrDefaultAsync(m => m.MaPhieuNhap == id);
        if (phieunhap == null)
        {
            return NotFound();
        }

        return View(phieunhap);
    }

    // GET: PHIEUNHAPS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PHIEUNHAPS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaPhieuNhap,MaNhaCungCap,MaKho,NgayNhap,NguoiLap,TrangThai,GhiChu,NhaCungCap,Kho,ChiTietPhieuNhaps")] PhieuNhap phieunhap)
    {
        if (ModelState.IsValid)
        {
            _context.Add(phieunhap);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(phieunhap);
    }

    // GET: PHIEUNHAPS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var phieunhap = await _context.PhieuNhap.FindAsync(id);
        if (phieunhap == null)
        {
            return NotFound();
        }
        return View(phieunhap);
    }

    // POST: PHIEUNHAPS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("MaPhieuNhap,MaNhaCungCap,MaKho,NgayNhap,NguoiLap,TrangThai,GhiChu,NhaCungCap,Kho,ChiTietPhieuNhaps")] PhieuNhap phieunhap)
    {
        if (id != phieunhap.MaPhieuNhap)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(phieunhap);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhieuNhapExists(phieunhap.MaPhieuNhap))
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
        return View(phieunhap);
    }

    // GET: PHIEUNHAPS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var phieunhap = await _context.PhieuNhap
            .FirstOrDefaultAsync(m => m.MaPhieuNhap == id);
        if (phieunhap == null)
        {
            return NotFound();
        }

        return View(phieunhap);
    }

    // POST: PHIEUNHAPS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var phieunhap = await _context.PhieuNhap.FindAsync(id);
        if (phieunhap != null)
        {
            _context.PhieuNhap.Remove(phieunhap);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PhieuNhapExists(int? maphieunhap)
    {
        return _context.PhieuNhap.Any(e => e.MaPhieuNhap == maphieunhap);
    }
}
