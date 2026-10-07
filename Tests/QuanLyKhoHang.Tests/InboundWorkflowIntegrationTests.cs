using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Tests.Infrastructure;
using Xunit;

namespace QuanLyKhoHang.Tests;

public sealed class InboundWorkflowIntegrationTests : WarehouseIntegrationTest
{
    [Fact]
    [Trait("TC", "TC_M3_008")]
    [Trait("TC", "TC_M3_012")]
    [Trait("TC", "TC_M3_018")]
    [Trait("TC", "TC_M3_019")]
    public async Task Draft_receipt_and_its_details_are_persisted_and_totalled()
    {
        await LoginAsAdminAsync();

        var createToken = await GetAntiForgeryTokenAsync("/PhieuNhap/Create");
        using var createResponse = await PostFormAsync(
            "/PhieuNhap/Create",
            createToken,
            new Dictionary<string, string>
            {
                ["MaNhaCungCap"] = "1",
                ["MaKho"] = "1",
                ["NgayNhap"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["TrangThai"] = "0",
                ["GhiChu"] = "Automated test"
            });

        Assert.Equal(HttpStatusCode.Redirect, createResponse.StatusCode);
        var receiptId = await WithDbAsync(async db =>
        {
            var receipt = await db.PhieuNhap.SingleAsync();
            Assert.Equal(0, receipt.TrangThai);
            Assert.Equal("Administrator", receipt.NguoiLap);
            return receipt.MaPhieuNhap;
        });

        var detailPath = $"/ChiTietPhieuNhap/Create?phieuId={receiptId}";
        var detailToken = await GetAntiForgeryTokenAsync(detailPath);
        using var detailResponse = await PostFormAsync(
            detailPath,
            detailToken,
            new Dictionary<string, string>
            {
                ["MaPhieuNhap"] = receiptId.ToString(CultureInfo.InvariantCulture),
                ["MaHang"] = "1",
                ["SoLuongNhap"] = "7",
                ["DonGiaNhap"] = "12500"
            });
        Assert.Equal(HttpStatusCode.Redirect, detailResponse.StatusCode);

        await WithDbAsync(async db =>
        {
            var receipt = await db.PhieuNhap
                .Include(item => item.ChiTietPhieuNhaps)
                .SingleAsync(item => item.MaPhieuNhap == receiptId);

            var detail = Assert.Single(receipt.ChiTietPhieuNhaps);
            Assert.Equal(7, detail.SoLuongNhap);
            Assert.Equal(12_500m, detail.DonGiaNhap);
            Assert.Equal(87_500m, detail.ThanhTien);
            Assert.Equal(87_500m, receipt.TongTienNhap);
            return true;
        });
    }
}
