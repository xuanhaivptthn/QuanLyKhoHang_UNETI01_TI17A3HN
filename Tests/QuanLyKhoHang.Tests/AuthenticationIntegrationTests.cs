using System.Net;
using QuanLyKhoHang.Tests.Infrastructure;
using Xunit;

namespace QuanLyKhoHang.Tests;

public sealed class AuthenticationIntegrationTests : WarehouseIntegrationTest
{
    [Fact]
    [Trait("TC", "TC_M1_001")]
    [Trait("TC", "TC_M1_010")]
    public async Task Admin_can_login_reach_protected_page_and_logout()
    {
        await LoginAsAdminAsync();

        using var protectedResponse = await Client.GetAsync("/TaiKhoan/Index");
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);

        using var logoutResponse = await Client.GetAsync("/TaiKhoan/DangXuat");
        Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);

        using var afterLogout = await Client.GetAsync("/PhieuNhap");
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
        Assert.True(afterLogout.Headers.Location?.ToString()
            .Contains("login", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("TC", "TC_M1_003")]
    [Trait("TC", "TC_M1_008")]
    public async Task Invalid_and_injection_credentials_do_not_authenticate()
    {
        foreach (var (username, password) in new[]
                 {
                     ("admin", "Sai@123"),
                     ("' OR 1=1 --", "anything")
                 })
        {
            var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
            using var loginResponse = await PostFormAsync(
                "/TaiKhoan/DangNhap",
                token,
                new Dictionary<string, string>
                {
                    ["TenDangNhap"] = username,
                    ["MatKhau"] = password
                });

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            using var protectedResponse = await Client.GetAsync("/TaiKhoan/Index");
            Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
        }
    }

    [Fact]
    [Trait("TC", "TC_M1_012")]
    [Trait("TC", "TC_M1_020")]
    public async Task Anonymous_and_warehouse_users_cannot_open_admin_pages()
    {
        using var anonymousResponse = await Client.GetAsync("/PhieuNhap");
        Assert.Equal(HttpStatusCode.Redirect, anonymousResponse.StatusCode);

        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var loginResponse = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "nvkho",
                ["MatKhau"] = "Kho@123"
            });
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        using var adminPageResponse = await Client.GetAsync("/TaiKhoan/Index");
        Assert.Equal(HttpStatusCode.Redirect, adminPageResponse.StatusCode);
        Assert.Contains("AccessDenied", adminPageResponse.Headers.Location?.ToString());
    }

    [Fact]
    [Trait("TC", "TC_M1_COOKIE_REMEMBER")]
    public async Task Remember_me_cookie_is_saved_and_prefills_login_form()
    {
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var loginResp = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "admin",
                ["MatKhau"] = "Admin@123",
                ["GhiNhoDangNhap"] = "true"
            });
        Assert.Equal(HttpStatusCode.Redirect, loginResp.StatusCode);

        // Đăng xuất
        using var logoutResp = await Client.GetAsync("/TaiKhoan/DangXuat");
        Assert.Equal(HttpStatusCode.Redirect, logoutResp.StatusCode);

        // Truy cập lại /TaiKhoan/DangNhap - Cookie RememberedUser tự động điền sẵn tên đăng nhập
        using var getLoginResp = await Client.GetAsync("/TaiKhoan/DangNhap");
        Assert.Equal(HttpStatusCode.OK, getLoginResp.StatusCode);
        var html = await getLoginResp.Content.ReadAsStringAsync();
        Assert.Contains("value=\"admin\"", html);
    }

    [Fact]
    [Trait("TC", "TC_M1_AUTH_PROFILE")]
    public async Task Profile_endpoint_reflects_authenticated_user_and_claims()
    {
        // Khi chưa đăng nhập -> Chuyển hướng về đăng nhập
        using var anonProfileResp = await Client.GetAsync("/TaiKhoan/Profile");
        Assert.Equal(HttpStatusCode.Redirect, anonProfileResp.StatusCode);

        // Đăng nhập Admin
        await LoginAsAdminAsync();

        // Truy cập Profile của User
        using var profileResp = await Client.GetAsync("/TaiKhoan/Profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);
        var profileHtml = await profileResp.Content.ReadAsStringAsync();
        Assert.Contains("Thông tin Hồ sơ Cá nhân", profileHtml);
        Assert.Contains("admin", profileHtml);
    }
}
