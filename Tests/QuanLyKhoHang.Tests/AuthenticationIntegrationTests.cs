// Họ và tên: Trần Xuân Hải // MSSV: 23103100135
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Bộ kiểm thử tích hợp xác thực Cookie, Lockout và phân quyền RBAC

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

    [Fact]
    [Trait("TC", "TC_M1_UPDATE_PROFILE")]
    public async Task Authenticated_user_can_update_their_own_profile()
    {
        await LoginAsAdminAsync();

        using var getEditResp = await Client.GetAsync("/TaiKhoan/CapNhatProfile");
        Assert.Equal(HttpStatusCode.OK, getEditResp.StatusCode);

        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/CapNhatProfile");
        using var postResp = await PostFormAsync(
            "/TaiKhoan/CapNhatProfile",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "admin",
                ["VaiTro"] = "Admin",
                ["HoTen"] = "Administrator Cập Nhật",
                ["Email"] = "admin_new@example.test"
            });

        Assert.Equal(HttpStatusCode.Redirect, postResp.StatusCode);
        Assert.Contains("/TaiKhoan/Profile", postResp.Headers.Location?.ToString());

        using var profileResp = await Client.GetAsync("/TaiKhoan/Profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);
        var html = await profileResp.Content.ReadAsStringAsync();
        var decodedHtml = WebUtility.HtmlDecode(html);
        Assert.Contains("Administrator Cập Nhật", decodedHtml);
        Assert.Contains("admin_new@example.test", html);
    }

    [Fact]
    [Trait("TC", "TC_M1_CHANGE_PASSWORD")]
    public async Task Authenticated_user_can_access_and_change_password_successfully()
    {
        await LoginAsAdminAsync();

        using var getDoiMatKhauResp = await Client.GetAsync("/TaiKhoan/DoiMatKhau");
        Assert.Equal(HttpStatusCode.OK, getDoiMatKhauResp.StatusCode);
        var getHtml = await getDoiMatKhauResp.Content.ReadAsStringAsync();
        var decodedGetHtml = WebUtility.HtmlDecode(getHtml);
        Assert.Contains("Đổi mật khẩu", decodedGetHtml);

        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DoiMatKhau");
        using var postResp = await PostFormAsync(
            "/TaiKhoan/DoiMatKhau",
            token,
            new Dictionary<string, string>
            {
                ["MatKhauHienTai"] = "Admin@123",
                ["MatKhauMoi"] = "Admin@NewPass456",
                ["XacNhanMatKhau"] = "Admin@NewPass456"
            });

        Assert.Equal(HttpStatusCode.Redirect, postResp.StatusCode);
        Assert.Contains("/TaiKhoan/Profile", postResp.Headers.Location?.ToString());

        using var profileResp = await Client.GetAsync("/TaiKhoan/Profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);
        var html = await profileResp.Content.ReadAsStringAsync();
        var decodedHtml = WebUtility.HtmlDecode(html);
        Assert.Contains("Đổi mật khẩu thành công!", decodedHtml);
    }

    [Fact]
    [Trait("TC", "TC_M1_CHANGE_PASSWORD_FAIL")]
    public async Task Change_password_with_incorrect_current_password_fails()
    {
        await LoginAsAdminAsync();

        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DoiMatKhau");
        using var postResp = await PostFormAsync(
            "/TaiKhoan/DoiMatKhau",
            token,
            new Dictionary<string, string>
            {
                ["MatKhauHienTai"] = "WrongPass123",
                ["MatKhauMoi"] = "Admin@NewPass456",
                ["XacNhanMatKhau"] = "Admin@NewPass456"
            });

        Assert.Equal(HttpStatusCode.OK, postResp.StatusCode);
        var html = await postResp.Content.ReadAsStringAsync();
        var decodedHtml = WebUtility.HtmlDecode(html);
        Assert.Contains("Mật khẩu hiện tại không chính xác.", decodedHtml);
    }

    [Fact]
    [Trait("CV", "CV1.1")]
    public async Task Role_based_menu_and_header_badge_are_synchronized_correctly()
    {
        // 1. Kiểm tra với tài khoản Admin
        await LoginAsAdminAsync();
        using var adminResp = await Client.GetAsync("/TaiKhoan/Index");
        Assert.Equal(HttpStatusCode.OK, adminResp.StatusCode);
        var adminHtml = await adminResp.Content.ReadAsStringAsync();

        // Admin: Header hiển thị họ tên và badge Admin màu đỏ
        Assert.Contains("Administrator", adminHtml);
        Assert.Contains("bg-danger", adminHtml);
        Assert.Contains("Admin", adminHtml);

        // Admin: Menu hiển thị đầy đủ Quản trị tài khoản và các phân hệ
        Assert.Contains("Quản trị Tài khoản", adminHtml);
        Assert.Contains("Bộ phận nhận", adminHtml);
        Assert.Contains("Phiếu xuất", adminHtml);
        Assert.Contains("Phiếu nhập", adminHtml);

        // 2. Kiểm tra với tài khoản NhanVienKho
        using var logout1 = await Client.GetAsync("/TaiKhoan/DangXuat");
        Assert.Equal(HttpStatusCode.Redirect, logout1.StatusCode);

        var tokenNvkho = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var loginNvkho = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            tokenNvkho,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "nvkho",
                ["MatKhau"] = "Kho@123"
            });
        Assert.Equal(HttpStatusCode.Redirect, loginNvkho.StatusCode);

        using var nvkhoResp = await Client.GetAsync("/PhieuNhap");
        Assert.Equal(HttpStatusCode.OK, nvkhoResp.StatusCode);
        var nvkhoHtml = await nvkhoResp.Content.ReadAsStringAsync();

        // NhanVienKho: Header hiển thị họ tên và badge NhanVienKho
        Assert.Contains("Nhan vien kho", nvkhoHtml);
        Assert.Contains("bg-primary", nvkhoHtml);
        Assert.Contains("NhanVienKho", nvkhoHtml);

        // NhanVienKho: Ẩn menu Quản trị Tài khoản (chỉ dành cho Admin)
        Assert.DoesNotContain("Quản trị Tài khoản", nvkhoHtml);

        // NhanVienKho: Hiển thị các chức năng kho
        Assert.Contains("Phiếu nhập", nvkhoHtml);
        Assert.Contains("Phiếu xuất", nvkhoHtml);
        Assert.Contains("Hàng hóa", nvkhoHtml);
        Assert.Contains("Kho", nvkhoHtml);

        // 3. Kiểm tra với tài khoản BoPhanNhan
        using var logout2 = await Client.GetAsync("/TaiKhoan/DangXuat");
        Assert.Equal(HttpStatusCode.Redirect, logout2.StatusCode);

        var tokenBoPhan = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var loginBoPhan = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            tokenBoPhan,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "bophan",
                ["MatKhau"] = "BoPhan@123"
            });
        Assert.Equal(HttpStatusCode.Redirect, loginBoPhan.StatusCode);

        using var boPhanResp = await Client.GetAsync("/PhieuXuat");
        Assert.Equal(HttpStatusCode.OK, boPhanResp.StatusCode);
        var boPhanHtml = await boPhanResp.Content.ReadAsStringAsync();

        // BoPhanNhan: Header hiển thị họ tên và badge BoPhanNhan
        Assert.Contains("Dai dien Bo phan nhan", boPhanHtml);
        Assert.Contains("BoPhanNhan", boPhanHtml);

        // BoPhanNhan: Ẩn menu Quản trị Tài khoản
        Assert.DoesNotContain("Quản trị Tài khoản", boPhanHtml);
        Assert.Contains("Phiếu xuất", boPhanHtml);
    }

    [Fact]
    [Trait("CV", "CV1.2")]
    public async Task Password_is_hashed_with_identity_password_hasher_on_create_and_verifies_login()
    {
        await LoginAsAdminAsync();

        // 1. Tạo mới tài khoản qua giao diện Admin
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/Create");
        using var createResp = await PostFormAsync(
            "/TaiKhoan/Create",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "test_hasher",
                ["MatKhau"] = "SecretPass@123",
                ["HoTen"] = "Nguoi Dung Hash",
                ["Email"] = "hasher@example.test",
                ["VaiTro"] = "NhanVienKho",
                ["TrangThai"] = "true"
            });
        Assert.Equal(HttpStatusCode.Redirect, createResp.StatusCode);

        // 2. Kiểm tra trong CSDL: Mật khẩu KHÔNG được lưu dạng text rõ mà phải là mã băm Identity
        var createdUser = await WithDbAsync(async db =>
            await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                db.TaiKhoans, u => u.TenDangNhap == "test_hasher"));

        Assert.NotNull(createdUser);
        Assert.NotEqual("SecretPass@123", createdUser.MatKhau);
        Assert.True(createdUser.MatKhau.Length >= 50, "Mật khẩu băm Identity phải có độ dài lớn hơn 50 ký tự.");

        // Xác thực băm chuẩn ASP.NET Core Identity PasswordHasher
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<QuanLyKhoHang_UNETI01_TI17A3HN.Models.TaiKhoan>();
        var verifyResult = hasher.VerifyHashedPassword(createdUser, createdUser.MatKhau, "SecretPass@123");
        Assert.Equal(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success, verifyResult);

        // 3. Đăng xuất Admin và thử đăng nhập bằng tài khoản vừa tạo
        using var logoutResp = await Client.GetAsync("/TaiKhoan/DangXuat");
        Assert.Equal(HttpStatusCode.Redirect, logoutResp.StatusCode);

        // Đăng nhập sai mật khẩu -> thất bại
        var loginToken1 = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var failLogin = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            loginToken1,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "test_hasher",
                ["MatKhau"] = "WrongPass@123"
            });
        Assert.Equal(HttpStatusCode.OK, failLogin.StatusCode);

        // Đăng nhập đúng mật khẩu -> thành công chuyển hướng
        var loginToken2 = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var successLogin = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            loginToken2,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "test_hasher",
                ["MatKhau"] = "SecretPass@123"
            });
        Assert.Equal(HttpStatusCode.Redirect, successLogin.StatusCode);
    }
}


