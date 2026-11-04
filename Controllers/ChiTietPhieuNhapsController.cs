using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Filters;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

// Họ và tên: Lê Văn Hùng
// Mã sinh viên: 23103100177
// Phụ trách Module 3: Nhà cung cấp, Phiếu nhập, Chi tiết phiếu nhập

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Controllers
{
    [AuthorizeRole("Admin", "NhanVienKho")]
    public class ChiTietPhieuNhapsController : Controller
    {
        private readonly AppDbContext _context;

        public ChiTietPhieuNhapsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ChiTietPhieuNhaps
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ChiTietPhieuNhap.Include(c => c.HangHoa).Include(c => c.PhieuNhap);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ChiTietPhieuNhaps/Details/5
        public async Task<IActionResult> Details(int? id, int? machitietnhap)
        {
            var targetId = id ?? machitietnhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap
                .Include(c => c.HangHoa)
                .Include(c => c.PhieuNhap)
                .FirstOrDefaultAsync(m => m.MaChiTietNhap == targetId);
            if (chiTietPhieuNhap == null)
            {
                return NotFound();
            }

            return View(chiTietPhieuNhap);
        }

        // GET: ChiTietPhieuNhaps/Create
        // optional phieuId or maPhieuNhap to preselect parent PhieuNhap when creating from PhieuNhap details
        public IActionResult Create(int? maPhieuNhap, int? phieuId)
        {
            var targetPhieuId = maPhieuNhap ?? phieuId;
            // Only active products and editable PhieuNhap (Nháp/Chờ xác nhận) can be selected
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang");
            if (targetPhieuId.HasValue)
            {
                // only allow creating for editable phieu (Nháp/Chờ xác nhận)
                var ph = _context.PhieuNhap.Find(targetPhieuId.Value);
                if (ph == null || !(ph.TrangThai == TrangThaiPhieuNhap.Nhap || ph.TrangThai == TrangThaiPhieuNhap.ChoXacNhan))
                {
                    return Forbid();
                }
                ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.MaPhieuNhap == targetPhieuId.Value), "MaPhieuNhap", "MaPhieuNhap", targetPhieuId.Value);
                ViewBag.PreselectedPhieuId = targetPhieuId.Value;
            }
            else
            {
                ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == TrangThaiPhieuNhap.Nhap || p.TrangThai == TrangThaiPhieuNhap.ChoXacNhan), "MaPhieuNhap", "MaPhieuNhap");
            }
            return View(new ChiTietPhieuNhap { MaPhieuNhap = targetPhieuId ?? 0 });
        }

        [HttpGet]
        public async Task<IActionResult> Upload(int? maPhieuNhap, int? phieuId)
        {
            var targetPhieuId = maPhieuNhap ?? phieuId;
            var phieuNhap = targetPhieuId.HasValue
                ? await _context.PhieuNhap.FindAsync(targetPhieuId.Value)
                : null;

            if (phieuNhap == null || !IsEditable(phieuNhap))
            {
                return Forbid();
            }

            ViewBag.PhieuNhap = phieuNhap;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int maPhieuNhap, IFormFile? file)
        {
            var phieuNhap = await _context.PhieuNhap.FindAsync(maPhieuNhap);
            if (phieuNhap == null)
            {
                return NotFound();
            }

            if (!IsEditable(phieuNhap))
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Vui lòng chọn file Excel hoặc CSV.");
            }
            else if (file.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("file", "Dung lượng file không được vượt quá 10 MB.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PhieuNhap = phieuNhap;
                return View();
            }

            try
            {
                var rows = await ReadImportRowsAsync(file!);
                if (rows.Count == 0)
                {
                    throw new InvalidDataException("File không có dòng dữ liệu hợp lệ.");
                }

                var productIds = rows.Select(r => r.MaHang).Distinct().ToList();
                var products = await _context.HangHoa
                    .Where(h => productIds.Contains(h.MaHang) && h.TrangThai)
                    .Select(h => h.MaHang)
                    .ToListAsync();
                var missingProducts = productIds.Except(products).ToList();
                if (missingProducts.Count > 0)
                {
                    throw new InvalidDataException($"Mã hàng không tồn tại hoặc đã ngừng hoạt động: {string.Join(", ", missingProducts)}.");
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();
                var existing = await _context.ChiTietPhieuNhap
                    .Where(c => c.MaPhieuNhap == maPhieuNhap && productIds.Contains(c.MaHang))
                    .ToDictionaryAsync(c => c.MaHang);

                foreach (var row in rows)
                {
                    if (existing.TryGetValue(row.MaHang, out var detail))
                    {
                        detail.SoLuongNhap += row.SoLuongNhap;
                        detail.DonGiaNhap = row.DonGiaNhap;
                    }
                    else
                    {
                        var newDetail = new ChiTietPhieuNhap
                        {
                            MaPhieuNhap = maPhieuNhap,
                            MaHang = row.MaHang,
                            SoLuongNhap = row.SoLuongNhap,
                            DonGiaNhap = row.DonGiaNhap
                        };
                        _context.ChiTietPhieuNhap.Add(newDetail);
                        existing[row.MaHang] = newDetail;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] = $"Đã import {rows.Count} dòng hàng vào phiếu #{maPhieuNhap}.";
                return RedirectToAction("Details", "PhieuNhaps", new { id = maPhieuNhap });
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is FormatException)
            {
                ModelState.AddModelError("file", ex.Message);
                ViewBag.PhieuNhap = phieuNhap;
                return View();
            }
        }

        // POST: ChiTietPhieuNhaps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChiTietNhap,MaPhieuNhap,MaHang,SoLuongNhap,DonGiaNhap")] ChiTietPhieuNhap chiTietPhieuNhap)
        {
            // Ensure the selected PhieuNhap is editable
            var ph = await _context.PhieuNhap.FindAsync(chiTietPhieuNhap.MaPhieuNhap);
            if (ph == null || !(ph.TrangThai == TrangThaiPhieuNhap.Nhap || ph.TrangThai == TrangThaiPhieuNhap.ChoXacNhan))
            {
                ModelState.AddModelError(string.Empty, "Chi tiết chỉ được thêm vào phiếu ở trạng thái Nháp hoặc Chờ xác nhận.");
            }

            var existingDetail = await _context.ChiTietPhieuNhap
                .FirstOrDefaultAsync(c => c.MaPhieuNhap == chiTietPhieuNhap.MaPhieuNhap && c.MaHang == chiTietPhieuNhap.MaHang);

            if (existingDetail != null)
            {
                existingDetail.SoLuongNhap += chiTietPhieuNhap.SoLuongNhap;
                existingDetail.DonGiaNhap = chiTietPhieuNhap.DonGiaNhap;
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "PhieuNhaps", new { id = chiTietPhieuNhap.MaPhieuNhap });
            }

            if (ModelState.IsValid)
            {
                _context.Add(chiTietPhieuNhap);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "PhieuNhaps", new { id = chiTietPhieuNhap.MaPhieuNhap });
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == TrangThaiPhieuNhap.Nhap || p.TrangThai == TrangThaiPhieuNhap.ChoXacNhan), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
            return View(chiTietPhieuNhap);
        }

        // GET: ChiTietPhieuNhaps/Edit/5
        public async Task<IActionResult> Edit(int? id, int? machitietnhap)
        {
            var targetId = id ?? machitietnhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap.FindAsync(targetId);
            if (chiTietPhieuNhap == null)
            {
                return NotFound();
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == TrangThaiPhieuNhap.Nhap || p.TrangThai == TrangThaiPhieuNhap.ChoXacNhan), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
            return View(chiTietPhieuNhap);
        }

        // POST: ChiTietPhieuNhaps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? machitietnhap, [Bind("MaChiTietNhap,MaPhieuNhap,MaHang,SoLuongNhap,DonGiaNhap")] ChiTietPhieuNhap chiTietPhieuNhap)
        {
            var targetId = id ?? machitietnhap ?? chiTietPhieuNhap.MaChiTietNhap;
            if (targetId != chiTietPhieuNhap.MaChiTietNhap)
            {
                return NotFound();
            }

            // Ensure parent PhieuNhap is editable before saving changes
            var ph = await _context.PhieuNhap.FindAsync(chiTietPhieuNhap.MaPhieuNhap);
            if (ph == null || !(ph.TrangThai == TrangThaiPhieuNhap.Nhap || ph.TrangThai == TrangThaiPhieuNhap.ChoXacNhan))
            {
                ModelState.AddModelError(string.Empty, "Chi tiết chỉ được sửa khi phiếu ở trạng thái Nháp hoặc Chờ xác nhận.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTietPhieuNhap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTietPhieuNhapExists(chiTietPhieuNhap.MaChiTietNhap))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "PhieuNhaps", new { id = chiTietPhieuNhap.MaPhieuNhap });
            }
            ViewData["MaHang"] = new SelectList(_context.HangHoa.Where(h => h.TrangThai), "MaHang", "TenHang", chiTietPhieuNhap.MaHang);
            ViewData["MaPhieuNhap"] = new SelectList(_context.PhieuNhap.Where(p => p.TrangThai == TrangThaiPhieuNhap.Nhap || p.TrangThai == TrangThaiPhieuNhap.ChoXacNhan), "MaPhieuNhap", "MaPhieuNhap", chiTietPhieuNhap.MaPhieuNhap);
            return View(chiTietPhieuNhap);
        }

        // GET: ChiTietPhieuNhaps/Delete/5
        public async Task<IActionResult> Delete(int? id, int? machitietnhap)
        {
            var targetId = id ?? machitietnhap;
            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap
                .Include(c => c.HangHoa)
                .Include(c => c.PhieuNhap)
                .FirstOrDefaultAsync(m => m.MaChiTietNhap == targetId);
            if (chiTietPhieuNhap == null)
            {
                return NotFound();
            }

            return View(chiTietPhieuNhap);
        }

        // POST: ChiTietPhieuNhaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? machitietnhap)
        {
            var targetId = id ?? machitietnhap;
            if (targetId == null && Request.HasFormContentType && int.TryParse(Request.Form["MaChiTietNhap"], out int formId))
            {
                targetId = formId;
            }

            if (targetId == null)
            {
                return NotFound();
            }

            var chiTietPhieuNhap = await _context.ChiTietPhieuNhap
                .Include(c => c.PhieuNhap)
                .FirstOrDefaultAsync(c => c.MaChiTietNhap == targetId);

            int maPhieuNhap = chiTietPhieuNhap?.MaPhieuNhap ?? 0;
            if (chiTietPhieuNhap != null)
            {
                if (chiTietPhieuNhap.PhieuNhap != null && chiTietPhieuNhap.PhieuNhap.TrangThai == TrangThaiPhieuNhap.DaHoanTat)
                {
                    TempData["Error"] = "Không thể xóa dòng chi tiết của phiếu đã hoàn tất.";
                    return RedirectToAction("Details", "PhieuNhaps", new { id = maPhieuNhap });
                }

                _context.ChiTietPhieuNhap.Remove(chiTietPhieuNhap);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "PhieuNhaps", new { id = maPhieuNhap });
        }

        private bool ChiTietPhieuNhapExists(int id)
        {
            return _context.ChiTietPhieuNhap.Any(e => e.MaChiTietNhap == id);
        }

        private static bool IsEditable(PhieuNhap phieuNhap) =>
            phieuNhap.TrangThai == TrangThaiPhieuNhap.Nhap ||
            phieuNhap.TrangThai == TrangThaiPhieuNhap.ChoXacNhan;

        private static async Task<List<ImportRow>> ReadImportRowsAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return extension switch
            {
                ".xlsx" or ".xlsm" => await ReadExcelRowsAsync(file),
                ".csv" => await ReadCsvRowsAsync(file),
                _ => throw new InvalidDataException("Chỉ hỗ trợ file .xlsx, .xlsm hoặc .csv.")
            };
        }

        private static async Task<List<ImportRow>> ReadExcelRowsAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                throw new InvalidDataException("File Excel không có worksheet.");
            }

            var range = worksheet.RangeUsed();
            if (range == null)
            {
                return [];
            }

            var headers = range.FirstRow().Cells().Select(c => NormalizeHeader(c.GetString())).ToList();
            var indexes = GetColumnIndexes(headers);
            var rows = new List<ImportRow>();
            foreach (var row in range.RowsUsed().Skip(1))
            {
                var values = row.Cells().Select(c => c.GetString()).ToList();
                if (values.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                rows.Add(ParseRow(GetValue(values, indexes.MaHang), GetValue(values, indexes.SoLuongNhap), GetValue(values, indexes.DonGiaNhap), rows.Count + 2));
            }
            return rows;
        }

        private static async Task<List<ImportRow>> ReadCsvRowsAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return [];
            }

            var separator = headerLine.Count(c => c == ';') > headerLine.Count(c => c == ',') ? ';' : ',';
            var headers = ParseCsvLine(headerLine, separator).Select(NormalizeHeader).ToList();
            var indexes = GetColumnIndexes(headers);
            var rows = new List<ImportRow>();
            var lineNumber = 1;
            while (await reader.ReadLineAsync() is { } line)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var values = ParseCsvLine(line, separator);
                rows.Add(ParseRow(GetValue(values, indexes.MaHang), GetValue(values, indexes.SoLuongNhap), GetValue(values, indexes.DonGiaNhap), lineNumber));
            }
            return rows;
        }

        private static (int MaHang, int SoLuongNhap, int DonGiaNhap) GetColumnIndexes(IReadOnlyList<string> headers)
        {
            var maHang = FindColumn(headers, "mahang", "masanpham", "productcode");
            var soLuongNhap = FindColumn(headers, "soluongnhap", "soluong", "quantity");
            var donGiaNhap = FindColumn(headers, "dongianhap", "dongia", "unitprice", "price");
            if (maHang < 0 || soLuongNhap < 0 || donGiaNhap < 0)
            {
                throw new InvalidDataException("Header phải có các cột MaHang, SoLuongNhap và DonGiaNhap.");
            }
            return (maHang, soLuongNhap, donGiaNhap);
        }

        private static int FindColumn(IReadOnlyList<string> headers, params string[] names)
        {
            var result = headers.Select((value, index) => (value, index)).FirstOrDefault(x => names.Contains(x.value));
            return result == default ? -1 : result.index;
        }

        private static string GetValue(IReadOnlyList<string> values, int index) =>
            index < values.Count ? values[index].Trim() : string.Empty;

        private static ImportRow ParseRow(string maHangText, string soLuongText, string donGiaText, int lineNumber)
        {
            if (!int.TryParse(maHangText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var maHang) || maHang <= 0 ||
                !int.TryParse(soLuongText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var soLuong) || soLuong <= 0 ||
                !decimal.TryParse(donGiaText, NumberStyles.Number, CultureInfo.InvariantCulture, out var donGia) || donGia < 0)
            {
                throw new InvalidDataException($"Dòng {lineNumber} có mã hàng, số lượng hoặc đơn giá không hợp lệ.");
            }
            return new ImportRow(maHang, soLuong, donGia);
        }

        private static string NormalizeHeader(string value) =>
            value.Trim().ToLowerInvariant().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("đ", "d").Replace("ô", "o").Replace("ư", "u").Replace("ơ", "o").Replace("ă", "a").Replace("â", "a").Replace("ê", "e").Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a");

        private static List<string> ParseCsvLine(string line, char separator)
        {
            var values = new List<string>();
            var current = new System.Text.StringBuilder();
            var quoted = false;
            for (var i = 0; i < line.Length; i++)
            {
                var character = line[i];
                if (character == '"')
                {
                    if (quoted && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (character == separator && !quoted)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(character);
                }
            }
            values.Add(current.ToString());
            return values;
        }

        private sealed record ImportRow(int MaHang, int SoLuongNhap, decimal DonGiaNhap);
    }
}
