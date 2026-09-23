using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("QuanLyKhoHang_UNETI01_TI17A3HNContext") ?? throw new InvalidOperationException("Connection string 'QuanLyKhoHang_UNETI01_TI17A3HNContext' not found.");

builder.Services.AddDbContext<QuanLyKhoHang_UNETI01_TI17A3HNContext>(options => options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "bophannhan_alias",
    pattern: "BoPhanNhan/{action=Index}/{id?}",
    defaults: new { controller = "BoPhanNhans" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
