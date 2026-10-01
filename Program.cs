using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;

var builder = WebApplication.CreateBuilder(args);

var primaryConnection = builder.Configuration.GetConnectionString("RemoteDb");

var localDbConnection = builder.Configuration.GetConnectionString("LocalDb");

string connectionString;
if (!string.IsNullOrWhiteSpace(primaryConnection) && !string.IsNullOrWhiteSpace(localDbConnection))
{
    try
    {
        var testBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(primaryConnection);
        if (testBuilder.ConnectTimeout < 60)
        {
            testBuilder.ConnectTimeout = 60;
        }
        using var testConn = new Microsoft.Data.SqlClient.SqlConnection(testBuilder.ConnectionString);
        testConn.Open();
        connectionString = testBuilder.ConnectionString;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warning] Failed to connect to RemoteDb ({ex.Message}). Falling back to LocalDb.");
        connectionString = localDbConnection;
    }
}
else
{
    // cuong moi sua o day
    connectionString = string.IsNullOrWhiteSpace(primaryConnection) ? localDbConnection : primaryConnection;

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'RemoteDb' or 'LocalDb' not found.");
    }
}

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(connectionString)
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

string serverName = "";
string databaseName = "";
try
{
    var csb = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
    serverName = csb.DataSource;
    databaseName = csb.InitialCatalog;
}
catch { }

bool isRemote = serverName.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase)
    || (connectionString == primaryConnection && !serverName.Contains("localdb", StringComparison.OrdinalIgnoreCase));

builder.Services.AddSingleton(new QuanLyKhoHang_UNETI01_TI17A3HN.Models.DatabaseInfo
{
    IsRemote = isRemote,
    ServerName = serverName,
    DatabaseName = databaseName
});

// Add services to the container.
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-10.0
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Dữ liệu mẫu đã được nạp hoàn tất trên Database.
// Tắt tự động nạp lại dữ liệu mỗi lần chạy ứng dụng (chỉ chạy khi truyền tham số: dotnet run -- --seed)
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

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

app.MapControllerRoute(
    name: "taikhoan_alias",
    pattern: "TaiKhoan/{action=DangNhap}/{id?}",
    defaults: new { controller = "TaiKhoans" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
