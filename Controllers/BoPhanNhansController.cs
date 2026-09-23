using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

public class BoPhanNhansController : Controller
{
    private readonly QuanLyKhoHang_UNETI01_TI17A3HNContext _context;

    public BoPhanNhansController(QuanLyKhoHang_UNETI01_TI17A3HNContext context)
    {
        _context = context;
    }

    // GET: BoPhanNhans
    public async Task<IActionResult> Index()    
    {
        return View(await _context.BoPhanNhans.ToListAsync());
    }

    // GET: BoPhanNhans/Details/5
    public async Task<IActionResult> Details(int? id, int? mabophan)
    {
        var targetId = id ?? mabophan;
        if (targetId == null)
        {
            return NotFound();
        }

        var bophannhan = await _context.BoPhanNhans
            .FirstOrDefaultAsync(m => m.MaBoPhan == targetId);
        if (bophannhan == null)
        {
            return NotFound();
        }

        return View(bophannhan);
    }

    // GET: BoPhanNhans/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: BoPhanNhans/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaBoPhan,TenBoPhan,NguoiDaiDien,SoDienThoai,MoTa,TrangThai")] BoPhanNhan bophannhan)
    {
        if (ModelState.IsValid)
        {
            _context.Add(bophannhan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(bophannhan);
    }

    // GET: BoPhanNhans/Edit/5
    public async Task<IActionResult> Edit(int? id, int? mabophan)
    {
        var targetId = id ?? mabophan;
        if (targetId == null)
        {
            return NotFound();
        }

        var bophannhan = await _context.BoPhanNhans.FindAsync(targetId);
        if (bophannhan == null)
        {
            return NotFound();
        }
        return View(bophannhan);
    }

    // POST: BoPhanNhans/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("MaBoPhan,TenBoPhan,NguoiDaiDien,SoDienThoai,MoTa,TrangThai")] BoPhanNhan bophannhan)
    {
        var targetId = id ?? bophannhan.MaBoPhan;
        if (targetId != bophannhan.MaBoPhan)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(bophannhan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BoPhanNhanExists(bophannhan.MaBoPhan))
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
        return View(bophannhan);
    }

    // GET: BoPhanNhans/Delete/5
    public async Task<IActionResult> Delete(int? id, int? mabophan)
    {
        var targetId = id ?? mabophan;
        if (targetId == null)
        {
            return NotFound();
        }

        var bophannhan = await _context.BoPhanNhans
            .FirstOrDefaultAsync(m => m.MaBoPhan == targetId);
        if (bophannhan == null)
        {
            return NotFound();
        }

        return View(bophannhan);
    }

    // POST: BoPhanNhans/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id, int? mabophan)
    {
        var targetId = id ?? mabophan;
        if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaBoPhan"], out int formId))
        {
            targetId = formId;
        }

        if (targetId == null)
        {
            return NotFound();
        }

        var bophannhan = await _context.BoPhanNhans.FindAsync(targetId);
        if (bophannhan != null)
        {
            _context.BoPhanNhans.Remove(bophannhan);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool BoPhanNhanExists(int id)
    {
        return _context.BoPhanNhans.Any(e => e.MaBoPhan == id);
    }
}
