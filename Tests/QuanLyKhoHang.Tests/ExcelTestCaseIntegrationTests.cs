using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Tests.Infrastructure;
using Xunit;

namespace QuanLyKhoHang.Tests;

public sealed class ExcelTestCaseIntegrationTests : WarehouseIntegrationTest
{
    private static readonly HashSet<string> SupportedTestCaseIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "TC_M1_001",
        "TC_M1_003",
        "TC_M1_008",
        "TC_M1_010",
        "TC_M1_012",
        "TC_M1_020",
        "TC_M3_008",
        "TC_M3_012",
        "TC_M3_018",
        "TC_M3_019",
        "TC_M4_006",
        "TC_M4_019",
        "TC_M4_020",
        "TC_M5_016"
    };

    public static IEnumerable<object[]> ExcelTestCases =>
        ExcelTestCaseLoader.LoadAll()
            .Where(testCase => SupportedTestCaseIds.Contains(testCase.Id))
            .Select(testCase => new object[] { testCase.Id, testCase.Title });

    [Theory]
    [MemberData(nameof(ExcelTestCases))]
    public async Task Execute_testcase_from_workbook(string testcaseId, string title)
    {
        var testCase = ExcelTestCaseLoader.GetById(testcaseId);
        Assert.Equal(title, testCase.Title);

        switch (testcaseId)
        {
            case "TC_M1_001":
                await VerifyAdminLoginAsync();
                break;
            case "TC_M1_003":
            case "TC_M1_008":
                await VerifyInvalidLoginAsync(testCase.TestData);
                break;
            case "TC_M1_010":
                await VerifyLogoutAsync();
                break;
            case "TC_M1_012":
                await VerifyAnonymousAccessAsync();
                break;
            case "TC_M1_020":
                await VerifyWarehouseUserAuthorizationAsync();
                break;
            case "TC_M3_008":
                await VerifyDraftReceiptAsync(testCase.TestData);
                break;
            case "TC_M3_012":
            case "TC_M3_018":
                await VerifyReceiptDetailAsync(testCase.TestData);
                break;
            case "TC_M3_019":
                await VerifyReceiptTotalAsync(testCase.TestData);
                break;
            case "TC_M4_006":
                await VerifyDraftDispatchAsync(testCase.TestData);
                break;
            case "TC_M4_019":
            case "TC_M4_020":
                await VerifyCompletedDispatchAsync(testcaseId, testCase.TestData);
                break;
            case "TC_M5_016":
                await VerifyReceiptHistoryAsync(testCase.TestData);
                break;
            default:
                throw new InvalidOperationException(
                    $"Testcase '{testcaseId}' is listed as supported but has no executor.");
        }
    }

    private async Task VerifyAdminLoginAsync()
    {
        await LoginAsync("admin", "Admin@123");

        using var response = await Client.GetAsync("/TaiKhoan/Index");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task VerifyInvalidLoginAsync(string testData)
    {
        var credentials = ParseCredentials(testData);
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var response = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = credentials.Username,
                ["MatKhau"] = credentials.Password
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var protectedResponse = await Client.GetAsync("/TaiKhoan/Index");
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
    }

    private async Task VerifyLogoutAsync()
    {
        await LoginAsync("admin", "Admin@123");

        using var logout = await Client.GetAsync("/TaiKhoan/DangXuat");
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        using var protectedResponse = await Client.GetAsync("/PhieuNhap");
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
    }

    private async Task VerifyAnonymousAccessAsync()
    {
        using var response = await Client.GetAsync("/PhieuNhap");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("login", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task VerifyWarehouseUserAuthorizationAsync()
    {
        await LoginAsync("nvkho", "Kho@123");

        using var response = await Client.GetAsync("/TaiKhoan/Index");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("AccessDenied", response.Headers.Location?.ToString());
    }

    private async Task VerifyDraftReceiptAsync(string testData)
    {
        var (supplierId, warehouseId) = ParseReceiptReferences(testData);
        var receiptId = await CreateReceiptAsync(supplierId, warehouseId);
        var receipt = await WithDbAsync(db => db.PhieuNhap.SingleAsync(item => item.MaPhieuNhap == receiptId));

        Assert.Equal(0, (int)receipt.TrangThai);
        Assert.Equal("Administrator", receipt.NguoiLap);
    }

    private async Task VerifyReceiptDetailAsync(string testData)
    {
        var receiptId = await CreateReceiptAsync(1, 1);
        var (quantity, unitPrice) = ParseQuantityAndPrice(testData);
        await AddReceiptDetailAsync(receiptId, 1, quantity, unitPrice);

        await WithDbAsync(async db =>
        {
            var receipt = await db.PhieuNhap
                .Include(item => item.ChiTietPhieuNhaps)
                .SingleAsync(item => item.MaPhieuNhap == receiptId);
            var detail = Assert.Single(receipt.ChiTietPhieuNhaps);
            Assert.Equal(quantity, detail.SoLuongNhap);
            Assert.Equal(unitPrice, detail.DonGiaNhap);
            Assert.Equal(quantity * unitPrice, detail.ThanhTien);
            Assert.Equal(quantity * unitPrice, receipt.TongTienNhap);
            return true;
        });
    }

    private async Task VerifyReceiptTotalAsync(string testData)
    {
        var receiptId = await CreateReceiptAsync(1, 1);
        var lines = Regex.Matches(testData, @"\((?<quantity>\d+)\s*x\s*(?<price>\d+)\)", RegexOptions.IgnoreCase)
            .Select(match => (
                Quantity: int.Parse(match.Groups["quantity"].Value, CultureInfo.InvariantCulture),
                UnitPrice: decimal.Parse(match.Groups["price"].Value, CultureInfo.InvariantCulture)))
            .ToArray();
        Assert.Equal(3, lines.Length);

        for (var index = 0; index < lines.Length; index++)
        {
            await AddReceiptDetailAsync(receiptId, index + 1, lines[index].Quantity, lines[index].UnitPrice);
        }

        await WithDbAsync(async db =>
        {
            var receipt = await db.PhieuNhap
                .Include(item => item.ChiTietPhieuNhaps)
                .SingleAsync(item => item.MaPhieuNhap == receiptId);

            Assert.Equal(lines.Length, receipt.ChiTietPhieuNhaps.Count);
            Assert.Equal(lines.Sum(line => line.Quantity * line.UnitPrice), receipt.TongTienNhap);
            return true;
        });
    }

    private async Task VerifyDraftDispatchAsync(string testData)
    {
        var (recipientId, warehouseId) = ParseDispatchReferences(testData);
        var dispatchId = await CreateDispatchAsync(recipientId, warehouseId);
        var dispatch = await WithDbAsync(db => db.PhieuXuats.SingleAsync(item => item.MaPhieuXuat == dispatchId));

        Assert.Equal(0, (int)dispatch.TrangThai);
        Assert.Equal("Administrator", dispatch.NguoiLap);
    }

    private async Task VerifyCompletedDispatchAsync(string testcaseId, string testData)
    {
        var quantity = ParseInteger(testData, @"xuất\s*=?\s*(?<value>\d+)");
        var dispatchId = await CreateDispatchAsync(1, 1);
        await AddDispatchDetailAsync(dispatchId, quantity);
        await ChangeDispatchStatusAsync(dispatchId, 1);
        await ChangeDispatchStatusAsync(dispatchId, 2);

        await WithDbAsync(async db =>
        {
            var dispatch = await db.PhieuXuats.SingleAsync(item => item.MaPhieuXuat == dispatchId);
            var stock = await db.TonKhoes.SingleAsync(item => item.MaKho == 1 && item.MaHang == 1);
            var history = await db.LichSuTonKhoes
                .Where(item => item.MaPhieu == $"PX{dispatchId:D4}" && item.LoaiGiaoDich == "Xuất kho")
                .ToListAsync();

            Assert.Equal(2, (int)dispatch.TrangThai);
            Assert.Equal(100 - quantity, stock.SoLuongTon);
            var transaction = Assert.Single(history);
            Assert.Equal(quantity, transaction.SoLuong);
            Assert.Equal(100 - quantity, transaction.TonSauGiaoDich);
            return true;
        });

        if (testcaseId == "TC_M4_020")
        {
            var token = await GetAntiForgeryTokenAsync($"/PhieuXuat/Details/{dispatchId}");
            using var repeatedCompletion = await PostFormAsync(
                $"/PhieuXuat/HoanTat/{dispatchId}",
                token,
                new Dictionary<string, string>());
            Assert.Equal(HttpStatusCode.Redirect, repeatedCompletion.StatusCode);

            await WithDbAsync(async db =>
            {
                var stock = await db.TonKhoes.SingleAsync(item => item.MaKho == 1 && item.MaHang == 1);
                var exportHistoryCount = await db.LichSuTonKhoes.CountAsync(
                    item => item.MaPhieu == $"PX{dispatchId:D4}" && item.LoaiGiaoDich == "Xuất kho");
                Assert.Equal(100 - quantity, stock.SoLuongTon);
                Assert.Equal(1, exportHistoryCount);
                return true;
            });
        }
    }

    private async Task VerifyReceiptHistoryAsync(string testData)
    {
        var quantity = ParseInteger(testData, @"(?<value>\d+)");
        var receiptId = await CreateReceiptAsync(1, 1);
        await AddReceiptDetailAsync(receiptId, 1, quantity, 1m);

        var token = await GetAntiForgeryTokenAsync($"/PhieuNhap/Details/{receiptId}");
        using var completion = await PostFormAsync(
            $"/PhieuNhap/HoanTat/{receiptId}",
            token,
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, completion.StatusCode);

        await WithDbAsync(async db =>
        {
            var stock = await db.TonKhoes.SingleAsync(item => item.MaKho == 1 && item.MaHang == 1);
            var history = await db.LichSuTonKhoes
                .Where(item => item.MaPhieu == $"PN{receiptId:D4}" && item.LoaiGiaoDich == "Nhập kho")
                .ToListAsync();

            Assert.Equal(100 + quantity, stock.SoLuongTon);
            var transaction = Assert.Single(history);
            Assert.Equal(quantity, transaction.SoLuong);
            Assert.Equal(100 + quantity, transaction.TonSauGiaoDich);
            return true;
        });
    }

    private async Task<int> CreateReceiptAsync(int supplierId, int warehouseId)
    {
        await LoginAsync("admin", "Admin@123");
        var token = await GetAntiForgeryTokenAsync("/PhieuNhap/Create");
        using var response = await PostFormAsync(
            "/PhieuNhap/Create",
            token,
            new Dictionary<string, string>
            {
                ["MaNhaCungCap"] = supplierId.ToString(CultureInfo.InvariantCulture),
                ["MaKho"] = warehouseId.ToString(CultureInfo.InvariantCulture),
                ["NgayNhap"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["TrangThai"] = "0",
                ["GhiChu"] = "Excel data-driven test"
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        return await WithDbAsync(async db =>
            (await db.PhieuNhap.OrderByDescending(item => item.MaPhieuNhap).FirstAsync()).MaPhieuNhap);
    }

    private async Task AddReceiptDetailAsync(int receiptId, int itemId, int quantity, decimal unitPrice)
    {
        var path = $"/ChiTietPhieuNhap/Create?phieuId={receiptId}";
        var token = await GetAntiForgeryTokenAsync(path);
        using var response = await PostFormAsync(
            path,
            token,
            new Dictionary<string, string>
            {
                ["MaPhieuNhap"] = receiptId.ToString(CultureInfo.InvariantCulture),
                ["MaHang"] = itemId.ToString(CultureInfo.InvariantCulture),
                ["SoLuongNhap"] = quantity.ToString(CultureInfo.InvariantCulture),
                ["DonGiaNhap"] = unitPrice.ToString(CultureInfo.InvariantCulture)
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private async Task<int> CreateDispatchAsync(int recipientId, int warehouseId)
    {
        await LoginAsync("admin", "Admin@123");
        var token = await GetAntiForgeryTokenAsync("/PhieuXuat/Create");
        using var response = await PostFormAsync(
            "/PhieuXuat/Create",
            token,
            new Dictionary<string, string>
            {
                ["MaBoPhan"] = recipientId.ToString(CultureInfo.InvariantCulture),
                ["MaKho"] = warehouseId.ToString(CultureInfo.InvariantCulture),
                ["NgayXuat"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["GhiChu"] = "Excel data-driven test"
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        return await WithDbAsync(async db =>
            (await db.PhieuXuats.OrderByDescending(item => item.MaPhieuXuat).FirstAsync()).MaPhieuXuat);
    }

    private async Task AddDispatchDetailAsync(int dispatchId, int quantity)
    {
        var path = $"/ChiTietPhieuXuat/Create?maPhieuXuat={dispatchId}";
        var token = await GetAntiForgeryTokenAsync(path);
        using var response = await PostFormAsync(
            path,
            token,
            new Dictionary<string, string>
            {
                ["MaPhieuXuat"] = dispatchId.ToString(CultureInfo.InvariantCulture),
                ["MaHang"] = "1",
                ["SoLuongXuat"] = quantity.ToString(CultureInfo.InvariantCulture),
                ["DonGiaXuatThamChieu"] = "100"
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private async Task ChangeDispatchStatusAsync(int dispatchId, int status)
    {
        var path = $"/PhieuXuat/Edit/{dispatchId}";
        var token = await GetAntiForgeryTokenAsync(path);
        using var response = await PostFormAsync(
            path,
            token,
            new Dictionary<string, string>
            {
                ["MaPhieuXuat"] = dispatchId.ToString(CultureInfo.InvariantCulture),
                ["MaBoPhan"] = "1",
                ["MaKho"] = "1",
                ["NgayXuat"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["TrangThai"] = status.ToString(CultureInfo.InvariantCulture)
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private async Task LoginAsync(string username, string password)
    {
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var response = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = username,
                ["MatKhau"] = password
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static (string Username, string Password) ParseCredentials(string testData)
    {
        var separator = testData.IndexOf('/');
        Assert.True(separator > 0, $"Could not parse credentials from testcase data: '{testData}'.");
        var username = testData[..separator].Trim();
        var password = testData[(separator + 1)..].Trim();
        if (username.Length >= 2 && username[0] == '\'' && username[^1] == '\'')
        {
            username = username[1..^1];
        }
        return (username, password);
    }

    private static (int SupplierId, int WarehouseId) ParseReceiptReferences(string testData)
    {
        var supplierId = ParseInteger(testData, @"NCC\s*(?<value>\d+)");
        var warehouseId = ParseInteger(testData, @"K\s*(?<value>\d+)");
        return (supplierId, warehouseId);
    }

    private static (int RecipientId, int WarehouseId) ParseDispatchReferences(string testData)
    {
        var recipientId = ParseInteger(testData, @"BP\s*(?<value>\d+)");
        var warehouseId = ParseInteger(testData, @"K\s*(?<value>\d+)");
        return (recipientId, warehouseId);
    }

    private static (int Quantity, decimal UnitPrice) ParseQuantityAndPrice(string testData)
    {
        var quantity = ParseInteger(testData, @"SL\s*=\s*(?<value>\d+)");
        var unitPrice = decimal.Parse(
            Regex.Match(testData, @"DonGia\s*=\s*(?<value>\d+(?:[.,]\d+)?)", RegexOptions.IgnoreCase)
                .Groups["value"].Value.Replace(',', '.'),
            CultureInfo.InvariantCulture);
        return (quantity, unitPrice);
    }

    private static int ParseInteger(string input, string pattern)
    {
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        Assert.True(match.Success, $"Could not find an integer matching '{pattern}' in testcase data: '{input}'.");
        return int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
    }
}
