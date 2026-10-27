// Họ và tên: Trần Xuân Hải // MSSV: 23103100135
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Bộ kiểm thử tích hợp xác thực Cookie, Lockout và phân quyền RBAC

using System.Net;
using Microsoft.EntityFrameworkCore;
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
        Assert.Contains("bg-danger text-white ms-1", adminHtml);
        Assert.Contains("Admin", adminHtml);

        // Admin: Menu hiển thị đầy đủ Quản lý tài khoản và Bộ phận nhận
        Assert.Contains("Tài khoản", adminHtml);
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

        // NhanVienKho: Header hiển thị họ tên và badge NhanVienKho màu xanh
        Assert.Contains("Nhan vien kho", nvkhoHtml);
        Assert.Contains("bg-primary text-white ms-1", nvkhoHtml);
        Assert.Contains("NhanVienKho", nvkhoHtml);

        // NhanVienKho: Ẩn menu Tài khoản và Ẩn menu Bộ phận nhận
        Assert.DoesNotContain("Tài khoản", nvkhoHtml);
        Assert.DoesNotContain("Bộ phận nhận", nvkhoHtml);

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
        Assert.Contains("bg-success text-white ms-1", boPhanHtml);
        Assert.Contains("BoPhanNhan", boPhanHtml);

        // BoPhanNhan: Chỉ hiển thị Phiếu xuất, ẩn các mục quản lý nội bộ kho
        Assert.Contains("Phiếu xuất", boPhanHtml);
        Assert.DoesNotContain("Tài khoản", boPhanHtml);
        Assert.DoesNotContain("Bộ phận nhận", boPhanHtml);
        Assert.DoesNotContain("Phiếu nhập", boPhanHtml);
        Assert.DoesNotContain("Loại hàng", boPhanHtml);
        Assert.DoesNotContain("Đơn vị tính", boPhanHtml);
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

    [Fact]
    [Trait("RBAC", "BoPhanNhan")]
    public async Task BoPhanNhan_permission_matrix_is_strictly_enforced()
    {
        // 1. Đăng nhập tài khoản vai trò BoPhanNhan
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var loginResp = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "bophan",
                ["MatKhau"] = "BoPhan@123"
            });
        Assert.Equal(HttpStatusCode.Redirect, loginResp.StatusCode);

        // 2. Chức năng được phép: Phiếu xuất, Hàng hóa (tra cứu), Hồ sơ cá nhân
        using var phieuXuatResp = await Client.GetAsync("/PhieuXuat");
        Assert.Equal(HttpStatusCode.OK, phieuXuatResp.StatusCode);

        using var hangHoaResp = await Client.GetAsync("/HangHoa");
        Assert.Equal(HttpStatusCode.OK, hangHoaResp.StatusCode);

        using var profileResp = await Client.GetAsync("/TaiKhoan/Profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);

        // 3. Các chức năng BỊ CẤM (chuyển hướng sang AccessDenied):
        var forbiddenUrls = new[]
        {
            "/Khoes",
            "/Khoes/Create",
            "/Khoes/Details/1",
            "/PhieuNhap",
            "/PhieuNhap/Create",
            "/LoaiHangs",
            "/DonViTinhs",
            "/NhaCungCaps",
            "/BoPhanNhans",
            "/TonKho/CanhBao",
            "/TonKho/LichSu",
            "/ThongKe/Dashboard"
        };

        foreach (var url in forbiddenUrls)
        {
            using var forbiddenResp = await Client.GetAsync(url);
            Assert.Equal(HttpStatusCode.Redirect, forbiddenResp.StatusCode);
            Assert.Contains("AccessDenied", forbiddenResp.Headers.Location?.ToString());
        }

        // 4. Thử POST tạo phiếu xuất khi không đủ quyền -> bị chặn AccessDenied
        var pxToken = await GetAntiForgeryTokenAsync("/TaiKhoan/CapNhatProfile");
        using var postPxResp = await PostFormAsync(
            "/PhieuXuat/Create",
            pxToken,
            new Dictionary<string, string>
            {
                ["MaBoPhan"] = "1",
                ["MaKho"] = "1",
                ["NgayXuat"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["NguoiLap"] = "Dai dien Bo phan nhan"
            });
        Assert.Equal(HttpStatusCode.Redirect, postPxResp.StatusCode);
        Assert.Contains("AccessDenied", postPxResp.Headers.Location?.ToString());
    }

    [Fact]
    [Trait("RBAC", "NhanVienKho")]
    public async Task NhanVienKho_cannot_create_or_delete_warehouses()
    {
        // 1. Đăng nhập tài khoản vai trò NhanVienKho
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var loginResp = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "nvkho",
                ["MatKhau"] = "Kho@123"
            });
        Assert.Equal(HttpStatusCode.Redirect, loginResp.StatusCode);

        // 2. Nhân viên kho chỉ có quyền xem danh sách và chi tiết kho
        using var indexKhoResp = await Client.GetAsync("/Khoes");
        Assert.Equal(HttpStatusCode.OK, indexKhoResp.StatusCode);

        using var detailsKhoResp = await Client.GetAsync("/Khoes/Details/1");
        Assert.Equal(HttpStatusCode.OK, detailsKhoResp.StatusCode);

        // 3. Nhân viên kho xem được hồ sơ cá nhân
        using var profileResp = await Client.GetAsync("/TaiKhoan/Profile");
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);

        // 4. Các thao tác ghi trên Kho bị chặn (chỉ dành cho Admin):
        using var getCreateKho = await Client.GetAsync("/Khoes/Create");
        Assert.Equal(HttpStatusCode.Redirect, getCreateKho.StatusCode);
        Assert.Contains("AccessDenied", getCreateKho.Headers.Location?.ToString());

        var khoToken = await GetAntiForgeryTokenAsync("/TaiKhoan/CapNhatProfile");
        using var postCreateKho = await PostFormAsync(
            "/Khoes/Create",
            khoToken,
            new Dictionary<string, string>
            {
                ["TenKho"] = "Kho Trai Phep",
                ["DiaDiem"] = "Ha Noi"
            });
        Assert.Equal(HttpStatusCode.Redirect, postCreateKho.StatusCode);
        Assert.Contains("AccessDenied", postCreateKho.Headers.Location?.ToString());

        using var postDeleteKho = await PostFormAsync(
            "/Khoes/Delete/1",
            khoToken,
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, postDeleteKho.StatusCode);
        Assert.Contains("AccessDenied", postDeleteKho.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ResetPasswordQuick_AsAdmin_UpdatesPasswordToDefaultAndAllowsLogin()
    {
        await LoginAsAdminAsync();

        var nvkho = await WithDbAsync(db => db.TaiKhoans.FirstAsync(t => t.TenDangNhap == "nvkho"));
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/CapNhatProfile");

        var request = new HttpRequestMessage(HttpMethod.Post, "/TaiKhoan/ResetPasswordQuick");
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = nvkho.MaTaiKhoan.ToString(),
            ["__RequestVerificationToken"] = token
        });

        using var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", json);
        Assert.Contains("123456@", json);

        // Đăng xuất Admin
        using var logout = await Client.GetAsync("/TaiKhoan/DangXuat");
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        // Đăng nhập tài khoản nvkho với mật khẩu mặc định mới (123456@)
        var loginToken = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var nvkhoLogin = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            loginToken,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "nvkho",
                ["MatKhau"] = "123456@"
            });

        Assert.Equal(HttpStatusCode.Redirect, nvkhoLogin.StatusCode);
        Assert.True(nvkhoLogin.Headers.Location?.ToString() == "/" || nvkhoLogin.Headers.Location?.ToString().Contains("Home") == true);
    }

    [Fact]
    public async Task DoiTrangThai_AsAdmin_TogglesAccountStatusSuccessfully()
    {
        await LoginAsAdminAsync();

        var bophan = await WithDbAsync(db => db.TaiKhoans.FirstAsync(t => t.TenDangNhap == "bophan"));
        Assert.True(bophan.TrangThai);
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/CapNhatProfile");

        // 1. Tắt trạng thái (Khóa tài khoản)
        var reqLock = new HttpRequestMessage(HttpMethod.Post, "/TaiKhoan/DoiTrangThai");
        reqLock.Headers.Add("X-Requested-With", "XMLHttpRequest");
        reqLock.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = bophan.MaTaiKhoan.ToString(),
            ["__RequestVerificationToken"] = token
        });

        using var respLock = await Client.SendAsync(reqLock);
        Assert.Equal(HttpStatusCode.OK, respLock.StatusCode);
        var jsonLock = await respLock.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", jsonLock);
        Assert.Contains("\"trangThai\":false", jsonLock);

        var bophanAfterLock = await WithDbAsync(db => db.TaiKhoans.FirstAsync(t => t.TenDangNhap == "bophan"));
        Assert.False(bophanAfterLock.TrangThai);

        // 2. Bật lại trạng thái (Mở khóa tài khoản)
        var reqUnlock = new HttpRequestMessage(HttpMethod.Post, "/TaiKhoan/DoiTrangThai");
        reqUnlock.Headers.Add("X-Requested-With", "XMLHttpRequest");
        reqUnlock.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = bophan.MaTaiKhoan.ToString(),
            ["__RequestVerificationToken"] = token
        });

        using var respUnlock = await Client.SendAsync(reqUnlock);
        Assert.Equal(HttpStatusCode.OK, respUnlock.StatusCode);
        var jsonUnlock = await respUnlock.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", jsonUnlock);
        Assert.Contains("\"trangThai\":true", jsonUnlock);

        var bophanAfterUnlock = await WithDbAsync(db => db.TaiKhoans.FirstAsync(t => t.TenDangNhap == "bophan"));
        Assert.True(bophanAfterUnlock.TrangThai);

        // 3. Kiểm tra an toàn: Chặn Admin tự khóa tài khoản của chính mình
        var admin = await WithDbAsync(db => db.TaiKhoans.FirstAsync(t => t.TenDangNhap == "admin"));
        var reqSelf = new HttpRequestMessage(HttpMethod.Post, "/TaiKhoan/DoiTrangThai");
        reqSelf.Headers.Add("X-Requested-With", "XMLHttpRequest");
        reqSelf.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = admin.MaTaiKhoan.ToString(),
            ["__RequestVerificationToken"] = token
        });

        using var respSelf = await Client.SendAsync(reqSelf);
        Assert.Equal(HttpStatusCode.OK, respSelf.StatusCode);
        var jsonSelf = await respSelf.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":false", jsonSelf);
    }

    [Fact]
    public async Task QuickActions_NonAdminAccess_IsForbiddenOrRedirected()
    {
        // 1. Người dùng ẩn danh (chưa đăng nhập)
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        var reqAnon = new HttpRequestMessage(HttpMethod.Post, "/TaiKhoan/ResetPasswordQuick");
        reqAnon.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = "2",
            ["__RequestVerificationToken"] = token
        });

        using var respAnon = await Client.SendAsync(reqAnon);
        Assert.Equal(HttpStatusCode.Redirect, respAnon.StatusCode);
        Assert.True(respAnon.Headers.Location?.ToString().Contains("login", StringComparison.OrdinalIgnoreCase) == true
                 || respAnon.Headers.Location?.ToString().Contains("DangNhap", StringComparison.OrdinalIgnoreCase) == true);

        // 2. Đăng nhập với tài khoản không phải Admin (nvkho)
        using var loginResp = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "nvkho",
                ["MatKhau"] = "Kho@123"
            });
        Assert.Equal(HttpStatusCode.Redirect, loginResp.StatusCode);

        var tokenNv = await GetAntiForgeryTokenAsync("/TaiKhoan/CapNhatProfile");
        var reqForbidden = new HttpRequestMessage(HttpMethod.Post, "/TaiKhoan/DoiTrangThai");
        reqForbidden.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = "2",
            ["__RequestVerificationToken"] = tokenNv
        });

        using var respForbidden = await Client.SendAsync(reqForbidden);
        Assert.Equal(HttpStatusCode.Redirect, respForbidden.StatusCode);
        Assert.Contains("AccessDenied", respForbidden.Headers.Location?.ToString());
    }
}


