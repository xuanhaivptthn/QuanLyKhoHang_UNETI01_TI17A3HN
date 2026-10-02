using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.Services;

// Họ và tên: Trần Xuân Hải
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính

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
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Phiên hết hạn sau 30 phút không hoạt động
    options.Cookie.HttpOnly = true;                 // Ngăn chặn truy cập cookie qua JavaScript (chống XSS)
    options.Cookie.IsEssential = true;             // Đảm bảo hoạt động theo chính sách Cookie
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
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi trong quá trình khởi tạo dữ liệu mẫu (DbInitializer).");
    }
}

// CẤU HÌNH HTTP REQUEST PIPELINE & MIDDLEWARE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

// Middleware UseSession phải nằm sau UseRouting và trước UseAuthorization
app.UseSession();
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
