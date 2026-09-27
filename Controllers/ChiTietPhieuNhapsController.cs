
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;

public class ChiTietPhieuNhapsController : Controller
{
    private readonly AppDbContext _context;

    public ChiTietPhieuNhapsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: CHITIETPHIEUNHAPS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ChiTietPhieuNhap.ToListAsync());
    }

    // GET: CHITIETPHIEUNHAPS/Details/5
    public async Task<IActionResult> Details(int? machitietnhap)
    {
        if (machitietnhap == null)
        {
            return NotFound();
        }

        var chitietphieunhap = await _context.ChiTietPhieuNhap
            .FirstOrDefaultAsync(m => m.MaChiTietNhap == machitietnhap);
        if (chitietphieunhap == null)
        {
            return NotFound();
        }

        return View(chitietphieunhap);
    }

    // GET: CHITIETPHIEUNHAPS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CHITIETPHIEUNHAPS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaChiTietNhap,MaPhieuNhap,MaHang,SoLuongNhap,DonGiaNhap,ThanhTien,PhieuNhap,HangHoa")] ChiTietPhieuNhap chitietphieunhap)
    {
        if (ModelState.IsValid)
        {
            _context.Add(chitietphieunhap);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(chitietphieunhap);
    }

    // GET: CHITIETPHIEUNHAPS/Edit/5
    public async Task<IActionResult> Edit(int? machitietnhap)
    {
        if (machitietnhap == null)
        {
            return NotFound();
        }

        var chitietphieunhap = await _context.ChiTietPhieuNhap.FindAsync(machitietnhap);
        if (chitietphieunhap == null)
        {
            return NotFound();
        }
        return View(chitietphieunhap);
    }

    // POST: CHITIETPHIEUNHAPS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? machitietnhap, [Bind("MaChiTietNhap,MaPhieuNhap,MaHang,SoLuongNhap,DonGiaNhap,ThanhTien,PhieuNhap,HangHoa")] ChiTietPhieuNhap chitietphieunhap)
    {
        if (machitietnhap != chitietphieunhap.MaChiTietNhap)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(chitietphieunhap);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChiTietPhieuNhapExists(chitietphieunhap.MaChiTietNhap))
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
        return View(chitietphieunhap);
    }

    // GET: CHITIETPHIEUNHAPS/Delete/5
    public async Task<IActionResult> Delete(int? machitietnhap)
    {
        if (machitietnhap == null)
        {
            return NotFound();
        }

        var chitietphieunhap = await _context.ChiTietPhieuNhap
            .FirstOrDefaultAsync(m => m.MaChiTietNhap == machitietnhap);
        if (chitietphieunhap == null)
        {
            return NotFound();
        }

        return View(chitietphieunhap);
    }

    // POST: CHITIETPHIEUNHAPS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? machitietnhap)
    {
        var chitietphieunhap = await _context.ChiTietPhieuNhap.FindAsync(machitietnhap);
        if (chitietphieunhap != null)
        {
            _context.ChiTietPhieuNhap.Remove(chitietphieunhap);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ChiTietPhieuNhapExists(int? machitietnhap)
    {
        return _context.ChiTietPhieuNhap.Any(e => e.MaChiTietNhap == machitietnhap);
    }
}
