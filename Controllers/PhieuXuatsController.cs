using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Filters;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using System.Data;

// Họ và tên: Nguyễn Việt Dũng
// Mã sinh viên: 23103100127
// Phụ trách Module 4: Bộ phận nhận, Phiếu xuất, Chi tiết phiếu xuất

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    public class PhieuXuatsController : Controller
    {
        private readonly AppDbContext _context;

        public PhieuXuatsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PhieuXuats
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PhieuXuats
                .Include(p => p.BoPhanNhan)
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuXuats);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PhieuXuats/Details/5
        public async Task<IActionResult> Details(int? id, int? maphieuxuat)
        {
            var targetId = id ?? maphieuxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.BoPhanNhan)
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuXuats)
                    .ThenInclude(c => c.HangHoa)
                .FirstOrDefaultAsync(m => m.MaPhieuXuat == targetId);
            if (phieuXuat == null)
            {
                return NotFound();
            }

            return View(phieuXuat);
        }

        // GET: PhieuXuats/Create
        [AuthorizeRole("Admin", "NhanVienKho")]
        public IActionResult Create()
        {
            PopulateCreateLists();
            return View();
        }

        // POST: PhieuXuats/Create
        [AuthorizeRole("Admin", "NhanVienKho")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuXuat,MaBoPhan,MaKho,NgayXuat,NguoiLap,GhiChu")] PhieuXuat phieuXuat)
        {
            if (!await IsActiveRecipientAndWarehouseAsync(phieuXuat.MaBoPhan, phieuXuat.MaKho))
            {
                ModelState.AddModelError(string.Empty, "Chỉ được lập phiếu với bộ phận nhận và kho đang hoạt động.");
            }

            if (ModelState.IsValid)
            {
                phieuXuat.TrangThai = TrangThaiPhieuXuat.Nhap;
                phieuXuat.NguoiLap = HttpContext.Session.GetString("HoTen") ?? phieuXuat.NguoiLap;
                _context.Add(phieuXuat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await PopulateCreateListsAsync(phieuXuat.MaBoPhan, phieuXuat.MaKho);
            return View(phieuXuat);
        }

        // GET: PhieuXuats/Edit/5
        [AuthorizeRole("Admin", "NhanVienKho")]
        public async Task<IActionResult> Edit(int? id, int? maphieuxuat)
        {
            var targetId = id ?? maphieuxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.BoPhanNhan)
                .Include(p => p.Kho)
                .FirstOrDefaultAsync(p => p.MaPhieuXuat == targetId);
            if (phieuXuat == null)
            {
                return NotFound();
            }
            if (phieuXuat.TrangThai != TrangThaiPhieuXuat.Nhap
                && !IsAdmin())
            {
                TempData["ErrorMessage"] = "Chỉ Admin được xử lý phiếu đang chờ xác nhận hoặc đã hoàn tất.";
                return RedirectToAction(nameof(Index));
            }
            PopulateEditView(phieuXuat);
            return View(phieuXuat);
        }

        // POST: PhieuXuats/Edit/5
        [AuthorizeRole("Admin", "NhanVienKho")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? maphieuxuat, [Bind("MaPhieuXuat,MaBoPhan,MaKho,NgayXuat,NguoiLap,TrangThai,GhiChu")] PhieuXuat phieuXuat)
        {
            var targetId = id ?? maphieuxuat ?? phieuXuat.MaPhieuXuat;
            if (targetId != phieuXuat.MaPhieuXuat)
            {
                return NotFound();
            }

            var currentStatus = await _context.PhieuXuats
                .Where(p => p.MaPhieuXuat == targetId)
                .Select(p => (TrangThaiPhieuXuat?)p.TrangThai)
                .FirstOrDefaultAsync();
            if (!currentStatus.HasValue)
            {
                return NotFound();
            }

            if (!Enum.IsDefined(phieuXuat.TrangThai))
            {
                ModelState.AddModelError(nameof(phieuXuat.TrangThai), "Trạng thái phiếu không hợp lệ.");
            }
            if (!ModelState.IsValid)
            {
                return ReturnEditView(phieuXuat, currentStatus.Value);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var phieuHienTai = await _context.PhieuXuats
                .Include(p => p.ChiTietPhieuXuats)
                .FirstOrDefaultAsync(p => p.MaPhieuXuat == targetId);
            if (phieuHienTai == null)
            {
                return NotFound();
            }

            if (phieuHienTai.TrangThai == TrangThaiPhieuXuat.DaHuy)
            {
                ModelState.AddModelError(nameof(phieuXuat.TrangThai), "Phiếu đã hủy không thể chuyển sang trạng thái khác.");
                return ReturnEditView(phieuXuat, phieuHienTai.TrangThai);
            }

            if (!IsAllowedTransition(phieuHienTai.TrangThai, phieuXuat.TrangThai, IsAdmin()))
            {
                ModelState.AddModelError(string.Empty, "Trạng thái chuyển không hợp lệ hoặc bạn không có quyền thực hiện thao tác này.");
                return ReturnEditView(phieuXuat, phieuHienTai.TrangThai);
            }

            if (phieuHienTai.TrangThai == TrangThaiPhieuXuat.Nhap
                || phieuXuat.TrangThai == TrangThaiPhieuXuat.ChoXacNhan
                || phieuXuat.TrangThai == TrangThaiPhieuXuat.DaHoanTat)
            {
                var recipientId = phieuHienTai.TrangThai == TrangThaiPhieuXuat.Nhap
                    ? phieuXuat.MaBoPhan
                    : phieuHienTai.MaBoPhan;
                var warehouseId = phieuHienTai.TrangThai == TrangThaiPhieuXuat.Nhap
                    ? phieuXuat.MaKho
                    : phieuHienTai.MaKho;
                if (!await IsActiveRecipientAndWarehouseAsync(recipientId, warehouseId))
                {
                    ModelState.AddModelError(string.Empty, "Không thể gửi hoặc duyệt phiếu có bộ phận nhận hoặc kho đã ngừng hoạt động.");
                    return ReturnEditView(phieuXuat, phieuHienTai.TrangThai);
                }
            }

            string? inventoryError = null;

            if (phieuHienTai.TrangThai == TrangThaiPhieuXuat.ChoXacNhan
                && phieuXuat.TrangThai == TrangThaiPhieuXuat.DaHoanTat)
            {
                inventoryError = await ApplyCompletedExportAsync(phieuHienTai, phieuHienTai.MaKho);
            }
            else if (phieuHienTai.TrangThai == TrangThaiPhieuXuat.DaHoanTat
                && phieuXuat.TrangThai == TrangThaiPhieuXuat.DaHuy)
            {
                inventoryError = await ReverseCompletedExportAsync(phieuHienTai);
            }

            if (inventoryError != null)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, inventoryError);
                return ReturnEditView(phieuXuat, phieuHienTai.TrangThai);
            }

            if (phieuHienTai.TrangThai == TrangThaiPhieuXuat.Nhap)
            {
                phieuHienTai.MaBoPhan = phieuXuat.MaBoPhan;
                phieuHienTai.MaKho = phieuXuat.MaKho;
                phieuHienTai.NgayXuat = phieuXuat.NgayXuat;
                phieuHienTai.NguoiLap = phieuXuat.NguoiLap;
                phieuHienTai.GhiChu = phieuXuat.GhiChu;
            }

            phieuHienTai.TrangThai = phieuXuat.TrangThai;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: PhieuXuats/Delete/5
        [AuthorizeRole("Admin", "NhanVienKho")]
        public async Task<IActionResult> Delete(int? id, int? maphieuxuat)
        {
            var targetId = id ?? maphieuxuat;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.BoPhanNhan)
                .Include(p => p.Kho)
                .FirstOrDefaultAsync(m => m.MaPhieuXuat == targetId);
            if (phieuXuat == null)
            {
                return NotFound();
            }

            return View(phieuXuat);
        }

        // POST: PhieuXuats/Delete/5
        [AuthorizeRole("Admin", "NhanVienKho")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? maphieuxuat)
        {
            var targetId = id ?? maphieuxuat;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaPhieuXuat"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats.FindAsync(targetId);
            if (phieuXuat?.TrangThai == TrangThaiPhieuXuat.DaHoanTat)
            {
                TempData["ErrorMessage"] = "Không thể xóa phiếu đã hoàn tất. Hãy chuyển trạng thái sang Đã hủy để hoàn trả tồn kho và lưu lịch sử.";
                return RedirectToAction(nameof(Index));
            }
            if (phieuXuat?.TrangThai == TrangThaiPhieuXuat.ChoXacNhan)
            {
                TempData["ErrorMessage"] = "Phiếu đã gửi xác nhận không được xóa. Admin cần chuyển trạng thái sang Nháp hoặc Đã hủy.";
                return RedirectToAction(nameof(Index));
            }
            if (phieuXuat != null && !IsAdmin()
                && phieuXuat.TrangThai != TrangThaiPhieuXuat.Nhap)
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể xóa phiếu ở trạng thái Nháp.";
                return RedirectToAction(nameof(Index));
            }
            if (phieuXuat != null && await _context.LichSuTonKhoes.AnyAsync(l => l.MaPhieu == GetInventoryReference(phieuXuat.MaPhieuXuat)))
            {
                TempData["ErrorMessage"] = "Không thể xóa phiếu đã phát sinh giao dịch tồn kho để bảo toàn lịch sử. Hãy hủy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            if (phieuXuat != null)
            {
                _context.PhieuXuats.Remove(phieuXuat);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private IActionResult ReturnEditView(PhieuXuat phieuXuat, TrangThaiPhieuXuat currentStatus)
        {
            PopulateEditView(phieuXuat, currentStatus);
            return View("Edit", phieuXuat);
        }

        private void PopulateCreateLists()
        {
            ViewData["MaBoPhan"] = new SelectList(
                _context.BoPhanNhans.Where(b => b.TrangThai),
                "MaBoPhan",
                "TenBoPhan");
            ViewData["MaKho"] = new SelectList(
                _context.Kho.Where(k => k.TrangThai),
                "MaKho",
                "TenKho");
        }

        private async Task PopulateCreateListsAsync(int selectedRecipientId, int selectedWarehouseId)
        {
            ViewData["MaBoPhan"] = new SelectList(
                await _context.BoPhanNhans.Where(b => b.TrangThai || b.MaBoPhan == selectedRecipientId).ToListAsync(),
                "MaBoPhan",
                "TenBoPhan",
                selectedRecipientId);
            ViewData["MaKho"] = new SelectList(
                await _context.Kho.Where(k => k.TrangThai || k.MaKho == selectedWarehouseId).ToListAsync(),
                "MaKho",
                "TenKho",
                selectedWarehouseId);
        }

        private void PopulateEditView(PhieuXuat phieuXuat, TrangThaiPhieuXuat? currentStatus = null)
        {
            currentStatus ??= phieuXuat.TrangThai;
            ViewData["CurrentStatus"] = currentStatus.Value;
            ViewData["TenBoPhan"] = _context.BoPhanNhans
                .Where(b => b.MaBoPhan == phieuXuat.MaBoPhan)
                .Select(b => b.TenBoPhan)
                .FirstOrDefault() ?? phieuXuat.MaBoPhan.ToString();
            ViewData["TenKho"] = _context.Kho
                .Where(k => k.MaKho == phieuXuat.MaKho)
                .Select(k => k.TenKho)
                .FirstOrDefault() ?? phieuXuat.MaKho.ToString();
            ViewData["MaBoPhan"] = new SelectList(
                _context.BoPhanNhans.Where(b => b.TrangThai || b.MaBoPhan == phieuXuat.MaBoPhan),
                "MaBoPhan",
                "TenBoPhan",
                phieuXuat.MaBoPhan);
            ViewData["MaKho"] = new SelectList(
                _context.Kho.Where(k => k.TrangThai || k.MaKho == phieuXuat.MaKho),
                "MaKho",
                "TenKho",
                phieuXuat.MaKho);

            var statuses = GetAvailableTransitions(currentStatus.Value, IsAdmin());
            ViewData["TrangThai"] = new SelectList(statuses, "Value", "Text", (int)phieuXuat.TrangThai);
        }

        private bool IsAdmin()
        {
            return string.Equals(HttpContext.Session.GetString("VaiTro"), "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> IsActiveRecipientAndWarehouseAsync(int maBoPhan, int maKho)
        {
            return await _context.BoPhanNhans.AnyAsync(b => b.MaBoPhan == maBoPhan && b.TrangThai)
                && await _context.Kho.AnyAsync(k => k.MaKho == maKho && k.TrangThai);
        }

        private static bool IsAllowedTransition(
            TrangThaiPhieuXuat current,
            TrangThaiPhieuXuat requested,
            bool isAdmin)
        {
            if (!isAdmin)
            {
                return current == TrangThaiPhieuXuat.Nhap
                    && (requested == TrangThaiPhieuXuat.Nhap
                        || requested == TrangThaiPhieuXuat.ChoXacNhan);
            }

            if (current == requested)
            {
                return current != TrangThaiPhieuXuat.DaHuy;
            }

            return (current, requested) switch
            {
                (TrangThaiPhieuXuat.Nhap, TrangThaiPhieuXuat.ChoXacNhan) => true,
                (TrangThaiPhieuXuat.Nhap, TrangThaiPhieuXuat.DaHuy) => true,
                (TrangThaiPhieuXuat.ChoXacNhan, TrangThaiPhieuXuat.Nhap) => true,
                (TrangThaiPhieuXuat.ChoXacNhan, TrangThaiPhieuXuat.DaHoanTat) => true,
                (TrangThaiPhieuXuat.ChoXacNhan, TrangThaiPhieuXuat.DaHuy) => true,
                (TrangThaiPhieuXuat.DaHoanTat, TrangThaiPhieuXuat.DaHuy) => true,
                _ => false
            };
        }

        private static List<SelectListItem> GetAvailableTransitions(TrangThaiPhieuXuat current, bool isAdmin)
        {
            var statuses = new List<TrangThaiPhieuXuat> { current };
            if (isAdmin)
            {
                statuses.AddRange(current switch
                {
                    TrangThaiPhieuXuat.Nhap => new[] { TrangThaiPhieuXuat.ChoXacNhan, TrangThaiPhieuXuat.DaHuy },
                    TrangThaiPhieuXuat.ChoXacNhan => new[] { TrangThaiPhieuXuat.Nhap, TrangThaiPhieuXuat.DaHoanTat, TrangThaiPhieuXuat.DaHuy },
                    TrangThaiPhieuXuat.DaHoanTat => new[] { TrangThaiPhieuXuat.DaHuy },
                    _ => Array.Empty<TrangThaiPhieuXuat>()
                });
            }
            else if (current == TrangThaiPhieuXuat.Nhap)
            {
                statuses.Add(TrangThaiPhieuXuat.ChoXacNhan);
            }

            return statuses
                .Distinct()
                .Select(status => new SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = status switch
                    {
                        TrangThaiPhieuXuat.Nhap => "Nháp",
                        TrangThaiPhieuXuat.ChoXacNhan => "Chờ xác nhận",
                        TrangThaiPhieuXuat.DaHoanTat => "Đã hoàn tất",
                        TrangThaiPhieuXuat.DaHuy => "Đã hủy",
                        _ => status.ToString()
                    }
                })
                .ToList();
        }

        private async Task<string?> ApplyCompletedExportAsync(PhieuXuat phieuXuat, int maKho)
        {
            if (phieuXuat.ChiTietPhieuXuats.Count == 0)
            {
                return "Không thể hoàn tất phiếu xuất chưa có chi tiết hàng hóa.";
            }
            if (phieuXuat.ChiTietPhieuXuats.Any(c => c.SoLuongXuat <= 0))
            {
                return "Số lượng của mọi chi tiết phiếu xuất phải lớn hơn 0.";
            }

            var maPhieu = GetInventoryReference(phieuXuat.MaPhieuXuat);
            if (await _context.LichSuTonKhoes.AnyAsync(l => l.MaPhieu == maPhieu && l.LoaiGiaoDich == "Xuất kho"))
            {
                return "Lịch sử tồn kho đã có giao dịch xuất cho phiếu này. Không thể ghi nhận xuất lần thứ hai.";
            }

            var quantities = phieuXuat.ChiTietPhieuXuats
                .GroupBy(c => c.MaHang)
                .ToDictionary(g => g.Key, g => g.Sum(c => (long)c.SoLuongXuat));
            if (quantities.Values.Any(q => q <= 0 || q > int.MaxValue))
            {
                return "Tổng số lượng xuất của một mặt hàng không hợp lệ.";
            }

            var maHangs = quantities.Keys.ToList();
            var tonKhoes = await _context.TonKhoes
                .Where(t => t.MaKho == maKho && maHangs.Contains(t.MaHang))
                .ToDictionaryAsync(t => t.MaHang);

            foreach (var (maHang, soLuongXuat) in quantities)
            {
                if (!tonKhoes.TryGetValue(maHang, out var tonKho))
                {
                    return $"Mặt hàng mã {maHang} chưa có bản ghi tồn kho tại kho đã chọn.";
                }

                if (tonKho.SoLuongTon < soLuongXuat)
                {
                    return $"Mặt hàng mã {maHang} không đủ tồn. Tồn hiện tại: {tonKho.SoLuongTon}, cần xuất: {soLuongXuat}.";
                }
            }

            var now = DateTime.Now;
            foreach (var (maHang, soLuongXuat) in quantities)
            {
                var tonKho = tonKhoes[maHang];
                tonKho.SoLuongTon -= (int)soLuongXuat;
                tonKho.NgayCapNhat = now;

                _context.LichSuTonKhoes.Add(new LichSuTonKho
                {
                    MaKho = maKho,
                    MaHang = maHang,
                    NgayPhatSinh = now,
                    LoaiGiaoDich = "Xuất kho",
                    MaPhieu = maPhieu,
                    SoLuong = (int)soLuongXuat,
                    TonSauGiaoDich = tonKho.SoLuongTon,
                    NguoiThucHien = phieuXuat.NguoiLap,
                    GhiChu = $"Xuất kho theo phiếu {maPhieu}"
                });
            }

            return null;
        }

        private async Task<string?> ReverseCompletedExportAsync(PhieuXuat phieuXuat)
        {
            var maPhieu = GetInventoryReference(phieuXuat.MaPhieuXuat);
            var history = await _context.LichSuTonKhoes
                .Where(l => l.MaPhieu == maPhieu && l.LoaiGiaoDich == "Xuất kho")
                .ToListAsync();
            var quantities = phieuXuat.ChiTietPhieuXuats
                .GroupBy(c => c.MaHang)
                .ToDictionary(g => g.Key, g => g.Sum(c => (long)c.SoLuongXuat));
            var recordedQuantities = history
                .GroupBy(l => l.MaHang)
                .ToDictionary(g => g.Key, g => g.Sum(l => (long)l.SoLuong));

            if (quantities.Count == 0
                || phieuXuat.ChiTietPhieuXuats.Any(c => c.SoLuongXuat <= 0)
                || history.Any(l => l.MaKho != phieuXuat.MaKho)
                || quantities.Count != recordedQuantities.Count
                || quantities.Any(q => !recordedQuantities.TryGetValue(q.Key, out var recorded) || recorded != q.Value))
            {
                return "Không thể hủy phiếu: lịch sử xuất không khớp với chi tiết phiếu. Cần đối soát dữ liệu trước khi hoàn trả tồn kho.";
            }

            var maHangs = quantities.Keys.ToList();
            var tonKhoes = await _context.TonKhoes
                .Where(t => t.MaKho == phieuXuat.MaKho && maHangs.Contains(t.MaHang))
                .ToDictionaryAsync(t => t.MaHang);

            foreach (var (maHang, soLuongHoan) in quantities)
            {
                if (!tonKhoes.TryGetValue(maHang, out var tonKho))
                {
                    return $"Không thể hủy phiếu: mặt hàng mã {maHang} chưa có bản ghi tồn kho tại kho xuất.";
                }

                if ((long)tonKho.SoLuongTon + soLuongHoan > int.MaxValue)
                {
                    return $"Không thể hủy phiếu: số lượng tồn của mặt hàng mã {maHang} vượt giới hạn cho phép.";
                }
            }

            var now = DateTime.Now;
            foreach (var (maHang, soLuongHoan) in quantities)
            {
                var tonKho = tonKhoes[maHang];
                tonKho.SoLuongTon += (int)soLuongHoan;
                tonKho.NgayCapNhat = now;

                _context.LichSuTonKhoes.Add(new LichSuTonKho
                {
                    MaKho = phieuXuat.MaKho,
                    MaHang = maHang,
                    NgayPhatSinh = now,
                    LoaiGiaoDich = "Hoàn trả xuất",
                    MaPhieu = maPhieu,
                    SoLuong = (int)soLuongHoan,
                    TonSauGiaoDich = tonKho.SoLuongTon,
                    NguoiThucHien = phieuXuat.NguoiLap,
                    GhiChu = $"Hoàn trả tồn kho do hủy phiếu {maPhieu}"
                });
            }

            return null;
        }

        private static string GetInventoryReference(int maPhieuXuat)
        {
            return $"PX{maPhieuXuat:D4}";
        }
    }
}
