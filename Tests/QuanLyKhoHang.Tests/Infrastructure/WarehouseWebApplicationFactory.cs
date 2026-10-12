using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using QuanLyKhoHang_UNETI01_TI17A3HN.Services;

namespace QuanLyKhoHang.Tests.Infrastructure;

public sealed class WarehouseWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public WarehouseWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            var databaseHealthCheck = services.FirstOrDefault(
                descriptor => descriptor.ServiceType == typeof(IHostedService)
                    && descriptor.ImplementationType == typeof(DatabaseHealthCheckService));
            if (databaseHealthCheck is not null)
            {
                services.Remove(databaseHealthCheck);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        db.TaiKhoans.AddRange(
            new TaiKhoan
            {
                MaTaiKhoan = 1,
                TenDangNhap = "admin",
                MatKhau = "Admin@123",
                HoTen = "Administrator",
                Email = "admin@example.test",
                VaiTro = "Admin",
                TrangThai = true
            },
            new TaiKhoan
            {
                MaTaiKhoan = 2,
                TenDangNhap = "nvkho",
                MatKhau = "Kho@123",
                HoTen = "Nhan vien kho",
                Email = "warehouse@example.test",
                VaiTro = "NhanVienKho",
                TrangThai = true
            });

        db.Kho.Add(new Kho
        {
            MaKho = 1,
            TenKho = "Test warehouse",
            TrangThai = true
        });
        db.NhaCungCap.Add(new NhaCungCap
        {
            MaNhaCungCap = 1,
            TenNhaCungCap = "Test supplier",
            TrangThai = true
        });
        db.BoPhanNhans.Add(new BoPhanNhan
        {
            MaBoPhan = 1,
            TenBoPhan = "Test department",
            TrangThai = true
        });
        db.LoaiHangs.Add(new LoaiHang
        {
            MaLoaiHang = 1,
            TenLoaiHang = "Test category",
            TrangThai = true
        });
        db.DonViTinhs.Add(new DonViTinh
        {
            MaDonViTinh = 1,
            TenDonViTinh = "Piece",
            TrangThai = true
        });
        db.HangHoa.Add(new HangHoa
        {
            MaHang = 1,
            TenHang = "Test item",
            MaLoaiHang = 1,
            MaDonViTinh = 1,
            GiaNhapThamKhao = 100,
            MucTonToiThieu = 5,
            TrangThai = true
        });
        db.TonKhoes.Add(new TonKho
        {
            MaKho = 1,
            MaHang = 1,
            SoLuongTon = 100,
            NgayCapNhat = DateTime.Today
        });

        await db.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
