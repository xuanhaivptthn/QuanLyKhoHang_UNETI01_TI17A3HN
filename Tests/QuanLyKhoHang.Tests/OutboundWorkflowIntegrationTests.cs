using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Tests.Infrastructure;
using Xunit;

namespace QuanLyKhoHang.Tests;

public sealed class OutboundWorkflowIntegrationTests : WarehouseIntegrationTest
{
    [Fact]
    [Trait("TC", "TC_M4_006")]
    [Trait("TC", "TC_M4_019")]
    [Trait("TC", "TC_M4_020")]
    [Trait("TC", "TC_M5_016")]
    public async Task Completing_dispatch_updates_stock_once_and_records_history()
    {
        await LoginAsAdminAsync();

        var createToken = await GetAntiForgeryTokenAsync("/PhieuXuat/Create");
        using var createResponse = await PostFormAsync(
            "/PhieuXuat/Create",
            createToken,
            new Dictionary<string, string>
            {
                ["MaBoPhan"] = "1",
                ["MaKho"] = "1",
                ["NgayXuat"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["GhiChu"] = "Automated test"
            });

        Assert.Equal(HttpStatusCode.Redirect, createResponse.StatusCode);
        var dispatchId = await WithDbAsync(async db =>
        {
            var dispatch = await db.PhieuXuats.SingleAsync();
            Assert.Equal(0, (int)dispatch.TrangThai);
            Assert.Equal("Administrator", dispatch.NguoiLap);
            return dispatch.MaPhieuXuat;
        });

        var detailPath = $"/ChiTietPhieuXuat/Create?maPhieuXuat={dispatchId}";
        var detailToken = await GetAntiForgeryTokenAsync(detailPath);
        using var detailResponse = await PostFormAsync(
            detailPath,
            detailToken,
            new Dictionary<string, string>
            {
                ["MaPhieuXuat"] = dispatchId.ToString(CultureInfo.InvariantCulture),
                ["MaHang"] = "1",
                ["SoLuongXuat"] = "30",
                ["DonGiaXuatThamChieu"] = "100"
            });
        Assert.Equal(HttpStatusCode.Redirect, detailResponse.StatusCode);

        var draftToken = await GetAntiForgeryTokenAsync($"/PhieuXuat/Edit/{dispatchId}");
        using var submitResponse = await PostFormAsync(
            $"/PhieuXuat/Edit/{dispatchId}",
            draftToken,
            new Dictionary<string, string>
            {
                ["MaPhieuXuat"] = dispatchId.ToString(CultureInfo.InvariantCulture),
                ["MaBoPhan"] = "1",
                ["MaKho"] = "1",
                ["NgayXuat"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["TrangThai"] = "1"
            });
        Assert.Equal(HttpStatusCode.Redirect, submitResponse.StatusCode);

        var approvalToken = await GetAntiForgeryTokenAsync($"/PhieuXuat/Edit/{dispatchId}");
        using var completeResponse = await PostFormAsync(
            $"/PhieuXuat/Edit/{dispatchId}",
            approvalToken,
            new Dictionary<string, string>
            {
                ["MaPhieuXuat"] = dispatchId.ToString(CultureInfo.InvariantCulture),
                ["MaBoPhan"] = "1",
                ["MaKho"] = "1",
                ["NgayXuat"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["TrangThai"] = "2"
            });
        Assert.Equal(HttpStatusCode.Redirect, completeResponse.StatusCode);

        await WithDbAsync(async db =>
        {
            var dispatch = await db.PhieuXuats.SingleAsync(item => item.MaPhieuXuat == dispatchId);
            var stock = await db.TonKhoes.SingleAsync(item => item.MaKho == 1 && item.MaHang == 1);
            var history = await db.LichSuTonKhoes
                .Where(item => item.MaPhieu == $"PX{dispatchId:D4}" && item.LoaiGiaoDich == "Xuất kho")
                .ToListAsync();

            Assert.Equal(2, (int)dispatch.TrangThai);
            Assert.Equal(70, stock.SoLuongTon);
            var transaction = Assert.Single(history);
            Assert.Equal(30, transaction.SoLuong);
            Assert.Equal(70, transaction.TonSauGiaoDich);
            return true;
        });
    }
}
