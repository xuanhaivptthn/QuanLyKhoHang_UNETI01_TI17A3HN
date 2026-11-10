namespace QuanLyKhoHang.Automation.Locators;

/// <summary>
/// Bộ lưu trữ định vị DOM Elements của Module 1 (Tài khoản, Xác thực đăng nhập, Phân quyền).
/// </summary>
public static class AuthLocators
{
    /// <summary>Đường dẫn trang đăng nhập hệ thống.</summary>
    public const string LoginUrl = "/login";

    /// <summary>Ô nhập tên đăng nhập.</summary>
    public const string UsernameInput = "input[name='TenDangNhap'], #TenDangNhap";

    /// <summary>Ô nhập mật khẩu.</summary>
    public const string PasswordInput = "input[name='MatKhau'], #MatKhau";

    /// <summary>Nút bấm xác nhận đăng nhập.</summary>
    public const string SubmitLoginBtn = "input[type='submit'], button[type='submit']";

    /// <summary>Hộp chọn ghi nhớ đăng nhập (Remember Me).</summary>
    public const string RememberMeCheckbox = "#rememberMeCheck, input[name='GhiNhoDangNhap']";

    /// <summary>Thông báo lỗi tổng hợp từ ModelOnly.</summary>
    public const string ValidationSummary = "div.validation-summary-errors, .text-danger";

    /// <summary>Thông báo lỗi chi tiết theo trường (Field validation).</summary>
    public const string FieldValidationError = "span.field-validation-error";

    /// <summary>Khung cảnh báo màu đỏ.</summary>
    public const string AlertDanger = ".alert-danger";

    /// <summary>Khung thông báo thành công màu xanh.</summary>
    public const string AlertSuccess = ".alert-success";

    /// <summary>Badge hiển thị tên người dùng trên thanh điều hướng.</summary>
    public const string UserGreetingLink = "nav a[href*='Profile']";

    /// <summary>Badge vai trò người dùng (Admin, NhanVienKho, BoPhanNhan).</summary>
    public const string RoleBadge = "nav .badge";

    /// <summary>Nút đăng xuất hệ thống.</summary>
    public const string LogoutButton = "a[href*='DangXuat']";

    /// <summary>Các dòng danh sách tài khoản trong trang quản trị.</summary>
    public const string AccountTableRows = "table tbody tr";
}
