// Họ và tên: Trần Xuân Hải // MSSV: 23103100135
// Mã sinh viên: 23103100135
// Phụ trách: Kiểm thử tích hợp các chức năng AJAX, jQuery và đồng bộ dữ liệu toàn hệ thống

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Tests.Infrastructure;
using Xunit;

namespace QuanLyKhoHang.Tests;

public sealed class AjaxFeaturesIntegrationTests : WarehouseIntegrationTest
{
    [Fact]
    public async Task Ajax_CheckTenDangNhap_detects_existing_and_new_usernames()
    {
        await LoginAsAdminAsync();

        // 1. Check existing username "admin"
        using var checkAdminReq = new HttpRequestMessage(HttpMethod.Get, "/TaiKhoans/CheckTenDangNhap?username=admin");
        checkAdminReq.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var checkAdminResp = await Client.SendAsync(checkAdminReq);
        Assert.Equal(HttpStatusCode.OK, checkAdminResp.StatusCode);
        var jsonAdmin = await checkAdminResp.Content.ReadAsStringAsync();
        using var docAdmin = JsonDocument.Parse(jsonAdmin);
        Assert.True(docAdmin.RootElement.GetProperty("exists").GetBoolean());

        // 2. Check brand new username "user_non_existing_999"
        using var checkNewReq = new HttpRequestMessage(HttpMethod.Get, "/TaiKhoans/CheckTenDangNhap?username=user_non_existing_999");
        checkNewReq.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var checkNewResp = await Client.SendAsync(checkNewReq);
        Assert.Equal(HttpStatusCode.OK, checkNewResp.StatusCode);
        var jsonNew = await checkNewResp.Content.ReadAsStringAsync();
        using var docNew = JsonDocument.Parse(jsonNew);
        Assert.False(docNew.RootElement.GetProperty("exists").GetBoolean());
    }

    [Fact]
    public async Task Ajax_GetDashboardData_returns_valid_kpi()
    {
        await LoginAsAdminAsync();

        using var req = new HttpRequestMessage(HttpMethod.Get, "/ThongKe/GetDashboardData");
        req.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var resp = await Client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.True(root.TryGetProperty("tongSoHangHoa", out var p1) && p1.GetInt32() >= 0);
        Assert.True(root.TryGetProperty("tongSoKho", out var p2) && p2.GetInt32() >= 0);
        Assert.True(root.TryGetProperty("soHangHet", out var p3) && p3.GetInt32() >= 0);
    }

    [Fact]
    public async Task Ajax_GetHangHoaInfo_returns_product_details()
    {
        await LoginAsAdminAsync();

        var firstHangHoa = await WithDbAsync(async db => await db.HangHoa.FirstOrDefaultAsync());
        Assert.NotNull(firstHangHoa);

        using var req = new HttpRequestMessage(HttpMethod.Get, $"/HangHoas/GetHangHoaInfo?id={firstHangHoa.MaHang}");
        req.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var resp = await Client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(firstHangHoa.MaHang, root.GetProperty("maHang").GetInt32());
        Assert.Equal(firstHangHoa.TenHang, root.GetProperty("tenHang").GetString());
    }

    [Fact]
    public async Task Ajax_GetTonKhoHienTai_returns_stock_availability()
    {
        await LoginAsAdminAsync();

        var (khoId, hangId) = await WithDbAsync(async db =>
        {
            var kho = await db.Kho.FirstOrDefaultAsync();
            var hang = await db.HangHoa.FirstOrDefaultAsync();
            return (kho!.MaKho, hang!.MaHang);
        });

        using var req = new HttpRequestMessage(HttpMethod.Get, $"/ChiTietPhieuXuats/GetTonKhoHienTai?maKho={khoId}&maHang={hangId}");
        req.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var resp = await Client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.True(root.TryGetProperty("tonKho", out var tk) && tk.GetInt32() >= 0);
    }

    [Fact]
    public async Task Ajax_Toggle_Status_HangHoa_returns_json_and_toggles_state()
    {
        await LoginAsAdminAsync();

        var hangHoa = await WithDbAsync(async db => await db.HangHoa.FirstAsync());
        var initialStatus = hangHoa.TrangThai;

        var token = await GetAntiForgeryTokenAsync("/HangHoas/Index");
        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, $"/HangHoas/DoiTrangThai/{hangHoa.MaHang}")
        {
            Content = new FormUrlEncodedContent(form)
        };
        req.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var resp = await Client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(!initialStatus, root.GetProperty("trangThai").GetBoolean());

        // Verify in database
        await WithDbAsync(async db =>
        {
            var updated = await db.HangHoa.FindAsync(hangHoa.MaHang);
            Assert.NotNull(updated);
            Assert.Equal(!initialStatus, updated.TrangThai);
            return 0;
        });
    }

    [Fact]
    public async Task Ajax_DangNhap_returns_json_on_ajax_post()
    {
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["TenDangNhap"] = "admin",
            ["MatKhau"] = "Admin@123"
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "/TaiKhoan/DangNhap")
        {
            Content = new FormUrlEncodedContent(form)
        };
        req.Headers.Add("X-Requested-With", "XMLHttpRequest");
        using var resp = await Client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.True(root.TryGetProperty("redirectUrl", out var r) && !string.IsNullOrEmpty(r.GetString()));
    }
}
