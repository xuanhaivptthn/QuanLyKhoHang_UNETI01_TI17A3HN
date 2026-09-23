
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

public class KhoesController : Controller
{
    private readonly QuanLyKhoHang_UNETI01_TI17A3HNContext _context;

    public KhoesController(QuanLyKhoHang_UNETI01_TI17A3HNContext context)
    {
        _context = context;
    }

    // GET: KHOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Kho.ToListAsync());
    }

    // GET: KHOS/Details/5
    public async Task<IActionResult> Details(int? makho)
    {
        if (makho == null)
        {
            return NotFound();
        }

        var kho = await _context.Kho
            .FirstOrDefaultAsync(m => m.MaKho == makho);
        if (kho == null)
        {
            return NotFound();
        }

        return View(kho);
    }

    // GET: KHOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: KHOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaKho,TenKho,DiaDiem,MoTa,TrangThai")] Kho kho)
    {
        if (ModelState.IsValid)
        {
            _context.Add(kho);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(kho);
    }

    // GET: KHOS/Edit/5
    public async Task<IActionResult> Edit(int? makho)
    {
        if (makho == null)
        {
            return NotFound();
        }

        var kho = await _context.Kho.FindAsync(makho);
        if (kho == null)
        {
            return NotFound();
        }
        return View(kho);
    }

    // POST: KHOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? makho, [Bind("MaKho,TenKho,DiaDiem,MoTa,TrangThai")] Kho kho)
    {
        if (makho != kho.MaKho)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(kho);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhoExists(kho.MaKho))
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
        return View(kho);
    }

    // GET: KHOS/Delete/5
    public async Task<IActionResult> Delete(int? makho)
    {
        if (makho == null)
        {
            return NotFound();
        }

        var kho = await _context.Kho
            .FirstOrDefaultAsync(m => m.MaKho == makho);
        if (kho == null)
        {
            return NotFound();
        }

        return View(kho);
    }

    // POST: KHOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? makho)
    {
        var kho = await _context.Kho.FindAsync(makho);
        if (kho != null)
        {
            _context.Kho.Remove(kho);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool KhoExists(int? makho)
    {
        return _context.Kho.Any(e => e.MaKho == makho);
    }
}
