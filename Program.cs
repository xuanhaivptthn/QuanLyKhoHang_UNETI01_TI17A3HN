// ====================================================================================
// ĐỀ TÀI: HỆ THỐNG QUẢN LÝ KHO HÀNG VÀ NHẬP – XUẤT – TỒN (ASP.NET CORE 10 MVC)
// DANH SÁCH THÀNH VIÊN ĐÓNG GÓP VÀ PHÂN CÔNG TRÁCH NHIỆM:
// - SV1: Trần Xuân Hải   (MSSV: 23103100135) - Module 1: Cấu hình hệ thống, Cookie Authentication, Session, Failover CSDL và DI
// - SV2: Nguyễn Thị Cúc  (MSSV: 23103100178) - Module 2: Quản lý Kho, Hàng hóa & Cấu hình Routing
// - SV3: Lê Văn Hùng    (MSSV: 23103100177) - Module 3: Nhà cung cấp, Phiếu nhập kho (Inbound Logistics)
// - SV4: Nguyễn Việt Dũng(MSSV: 23103100127) - Module 4: Bộ phận nhận, Phiếu xuất kho (Outbound Logistics)
// - SV5: Nguyễn Văn Cường(MSSV: 23103100132) - Module 5: Tồn kho, Cảnh báo, Lịch sử & Dashboard Thống kê
// ====================================================================================

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.Services;

var builder = WebApplication.CreateBuilder(args);

// CẤU HÌNH CƠ SỞ DỮ LIỆU & QUẢN LÝ KẾT NỐI ĐỘNG (FAILOVER & HEALTH CHECK)
// Đăng ký thông tin CSDL (DatabaseInfo) dạng Singleton hiển thị trạng thái lên giao diện
builder.Services.AddSingleton<DatabaseInfo>();

// Đăng ký trình quản lý kết nối CSDL (DatabaseConnectionManager):
// - Tự động phát hiện CSDL Remote (Azure SQL) hoặc Local (LocalDB)
// - Bật TrustServerCertificate=True để tránh lỗi bắt tay SSL trên môi trường Linux
builder.Services.AddSingleton<DatabaseConnectionManager>();

// Đăng ký các Interceptor bắt lỗi kết nối và câu lệnh EF Core để tự động chuyển CSDL tức thì
builder.Services.AddSingleton<DatabaseFailoverConnectionInterceptor>();
builder.Services.AddSingleton<DatabaseFailoverCommandInterceptor>();

// Tiến trình chạy ngầm (BackgroundService) định kỳ thăm dò RemoteDb mỗi 15s để tự động kết nối lại
builder.Services.AddHostedService<DatabaseHealthCheckService>();

// Cấu hình AppDbContext: Lấy chuỗi kết nối động từ DatabaseConnectionManager tại mỗi request
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var dbManager = serviceProvider.GetRequiredService<DatabaseConnectionManager>();
    var connInterceptor = serviceProvider.GetRequiredService<DatabaseFailoverConnectionInterceptor>();
    var cmdInterceptor = serviceProvider.GetRequiredService<DatabaseFailoverCommandInterceptor>();

    options.UseSqlServer(dbManager.GetActiveConnectionString())
           .AddInterceptors(connInterceptor, cmdInterceptor)
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// CẤU HÌNH PHIÊN LÀM VIỆC (SESSION) & BỘ NHỚ ĐỆM (CACHE)
builder.Services.AddDistributedMemoryCache(); // Đăng ký bộ nhớ đệm RAM cho Session
builder.Services.AddMemoryCache();            // Đăng ký bộ nhớ đệm In-Memory phục vụ Lockout & Cookie
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Phiên hết hạn sau 30 phút không hoạt động
    options.Cookie.HttpOnly = true;                 // Bảo mật chống tấn công XSS
    options.Cookie.IsEssential = true;             // Đảm bảo hoạt động theo chính sách Cookie GDPR
    options.Cookie.SameSite = SameSiteMode.Lax;    // Bảo mật chống tấn công CSRF
});

// CẤU HÌNH DỊCH VỤ MÃ HÓA MẬT KHẨU (ASP.NET CORE IDENTITY)
builder.Services.AddScoped<IPasswordHasher<TaiKhoan>, PasswordHasher<TaiKhoan>>();

// CẤU HÌNH XÁC THỰC BẰNG COOKIE (COOKIE AUTHENTICATION)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";                       // Đường dẫn khi chưa đăng nhập
        options.LogoutPath = "/logout";                     // Đường dẫn đăng xuất
        options.AccessDeniedPath = "/Home/AccessDenied";   // Đường dẫn khi không đủ quyền 403
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Hạn dùng mặc định của Cookie xác thực
        options.SlidingExpiration = true;                   // Tự động gia hạn khi người dùng thao tác
        options.Cookie.Name = ".QuanLyKhoHang.Auth";        // Tên Authentication Cookie
        options.Cookie.HttpOnly = true;                     // Chống XSS
        options.Cookie.SameSite = SameSiteMode.Lax;         // Chống CSRF
        options.Cookie.IsEssential = true;                  // Cookie thiết yếu cho xác thực
    });

// CẤU HÌNH PHÂN QUYỀN CHÍNH SÁCH (POLICY-BASED AUTHORIZATION)
builder.Services.AddAuthorization(options =>
{
    // Policy dành riêng cho Quản trị viên (Admin)
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    // Policy dành cho Nhân viên kho hoặc Admin
    options.AddPolicy("WarehouseStaff", policy => policy.RequireRole("Admin", "NhanVienKho"));

    // Custom Policy kết hợp Role + Custom Claim Department
    options.AddPolicy("DepartmentIT", policy => policy.RequireRole("Admin").RequireClaim("Department", "Admin"));
});

// CẤU HÌNH MVC CONTROLLERS & BẢO MẬT TOÀN CỤC (GLOBAL FILTER)
builder.Services.AddControllersWithViews(options =>
{
    // Bắt buộc xác thực đăng nhập trên toàn bộ Web App qua AuthorizeRoleAttribute (bỏ qua nếu có [AllowAnonymous])
    options.Filters.Add(new QuanLyKhoHang_UNETI01_TI17A3HN.Filters.AuthorizeRoleAttribute());
});

var app = builder.Build();

// NẠP DỮ LIỆU MẪU (SEED DATA - CHỈ CHẠY KHI TRUYỀN THAM SỐ --seed)
if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.ReseedData();
        Console.WriteLine("[AppDbContext] Xóa toàn bộ dữ liệu cũ và nạp lại dữ liệu mẫu thành công!");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi trong quá trình khởi tạo dữ liệu mẫu (AppDbContext.ReseedData).");
    }
    return;
}

// TỰ ĐỘNG KIỂM TRA VÀ NẠP DỮ LIỆU MẪU BAN ĐẦU KHI DATABASE TRỐNG (MỤC 16)
using (var initScope = app.Services.CreateScope())
{
    try
    {
        var context = initScope.ServiceProvider.GetRequiredService<AppDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = initScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Không thể tự động nạp dữ liệu mẫu ban đầu qua DbInitializer.");
    }
}

// CẤU HÌNH HTTP REQUEST PIPELINE & MIDDLEWARE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

// CẤU HÌNH THỨ TỰ MIDDLEWARE PIPELINE
// 1. Session Middleware: Đọc và gán Session ID từ Cookie
app.UseSession();

// 2. Authentication Middleware: Giải mã Cookie Authentication thành HttpContext.User
app.UseAuthentication();

// 3. Authorization Middleware: Kiểm tra phân quyền [Authorize] và Roles/Policies
app.UseAuthorization();

app.MapStaticAssets();

// CẤU HÌNH CÁC ROUTE ĐIỀU HƯỚNG (ROUTING)

// Định tuyến bí danh cho các Controller nghiệp vụ
app.MapControllerRoute(
    name: "kho_alias",
    pattern: "Kho/{action=Index}/{id?}",
    defaults: new { controller = "Khoes" });

app.MapControllerRoute(
    name: "hanghoa_alias",
    pattern: "HangHoa/{action=Index}/{id?}",
    defaults: new { controller = "HangHoas" });

app.MapControllerRoute(
    name: "bophannhan_alias",
    pattern: "BoPhanNhan/{action=Index}/{id?}",
    defaults: new { controller = "BoPhanNhans" });

app.MapControllerRoute(
    name: "nhacungcap_alias",
    pattern: "NhaCungCap/{action=Index}/{id?}",
    defaults: new { controller = "NhaCungCaps" });

app.MapControllerRoute(
    name: "phieunhap_alias",
    pattern: "PhieuNhap/{action=Index}/{id?}",
    defaults: new { controller = "PhieuNhaps" });

app.MapControllerRoute(
    name: "phieuxuat_alias",
    pattern: "PhieuXuat/{action=Index}/{id?}",
    defaults: new { controller = "PhieuXuats" });

app.MapControllerRoute(
    name: "chitietphieunhap_alias",
    pattern: "ChiTietPhieuNhap/{action=Index}/{id?}",
    defaults: new { controller = "ChiTietPhieuNhaps" });

app.MapControllerRoute(
    name: "chitietphieuxuat_alias",
    pattern: "ChiTietPhieuXuat/{action=Index}/{id?}",
    defaults: new { controller = "ChiTietPhieuXuats" });

app.MapControllerRoute(
    name: "loaihang_alias",
    pattern: "LoaiHang/{action=Index}/{id?}",
    defaults: new { controller = "LoaiHangs" });

app.MapControllerRoute(
    name: "donvitinh_alias",
    pattern: "DonViTinh/{action=Index}/{id?}",
    defaults: new { controller = "DonViTinhs" });

// Route trực tiếp cho đăng nhập và đăng xuất
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "TaiKhoans", action = "DangNhap" });

app.MapControllerRoute(
    name: "logout",
    pattern: "logout",
    defaults: new { controller = "TaiKhoans", action = "DangXuat" });

app.MapControllerRoute(
    name: "taikhoan_alias",
    pattern: "TaiKhoan/{action=DangNhap}/{id?}",
    defaults: new { controller = "TaiKhoans" });

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }
