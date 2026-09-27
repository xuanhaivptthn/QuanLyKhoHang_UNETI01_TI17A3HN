
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;

public class NhaCungCapsController : Controller
{
    private readonly AppDbContext _context;

    public NhaCungCapsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: NHACUNGCAPS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.NhaCungCap.ToListAsync());
    }

    // GET: NHACUNGCAPS/Details/5
    public async Task<IActionResult> Details(int? manhacungcap)
    {
        if (manhacungcap == null)
        {
            return NotFound();
        }

        var nhacungcap = await _context.NhaCungCap
            .FirstOrDefaultAsync(m => m.MaNhaCungCap == manhacungcap);
        if (nhacungcap == null)
        {
            return NotFound();
        }

        return View(nhacungcap);
    }

    // GET: NHACUNGCAPS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: NHACUNGCAPS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaNhaCungCap,TenNhaCungCap,SoDienThoai,Email,DiaChi,TrangThai")] NhaCungCap nhacungcap)
    {
        if (ModelState.IsValid)
        {
            _context.Add(nhacungcap);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(nhacungcap);
    }

    // GET: NHACUNGCAPS/Edit/5
    public async Task<IActionResult> Edit(int? manhacungcap)
    {
        if (manhacungcap == null)
        {
            return NotFound();
        }

        var nhacungcap = await _context.NhaCungCap.FindAsync(manhacungcap);
        if (nhacungcap == null)
        {
            return NotFound();
        }
        return View(nhacungcap);
    }

    // POST: NHACUNGCAPS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? manhacungcap, [Bind("MaNhaCungCap,TenNhaCungCap,SoDienThoai,Email,DiaChi,TrangThai")] NhaCungCap nhacungcap)
    {
        if (manhacungcap != nhacungcap.MaNhaCungCap)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(nhacungcap);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhaCungCapExists(nhacungcap.MaNhaCungCap))
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
        return View(nhacungcap);
    }

    // GET: NHACUNGCAPS/Delete/5
    public async Task<IActionResult> Delete(int? manhacungcap)
    {
        if (manhacungcap == null)
        {
            return NotFound();
        }

        var nhacungcap = await _context.NhaCungCap
            .FirstOrDefaultAsync(m => m.MaNhaCungCap == manhacungcap);
        if (nhacungcap == null)
        {
            return NotFound();
        }

        return View(nhacungcap);
    }

    // POST: NHACUNGCAPS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? manhacungcap)
    {
        var nhacungcap = await _context.NhaCungCap.FindAsync(manhacungcap);
        if (nhacungcap != null)
        {
            _context.NhaCungCap.Remove(nhacungcap);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool NhaCungCapExists(int? manhacungcap)
    {
        return _context.NhaCungCap.Any(e => e.MaNhaCungCap == manhacungcap);
    }
}
