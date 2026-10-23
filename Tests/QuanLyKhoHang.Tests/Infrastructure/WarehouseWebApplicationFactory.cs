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

    public async Task InitializeDatabaseAsync(SeedProfile profile = SeedProfile.Default)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        await db.ChiTietPhieuXuats.ExecuteDeleteAsync();
        await db.PhieuXuats.ExecuteDeleteAsync();
        await db.ChiTietPhieuNhaps.ExecuteDeleteAsync();
        await db.PhieuNhap.ExecuteDeleteAsync();
        await db.LichSuTonKhoes.ExecuteDeleteAsync();
        await db.TonKhoes.ExecuteDeleteAsync();
        await db.HangHoa.ExecuteDeleteAsync();
        await db.BoPhanNhans.ExecuteDeleteAsync();
        await db.NhaCungCap.ExecuteDeleteAsync();
        await db.Kho.ExecuteDeleteAsync();
        await db.DonViTinhs.ExecuteDeleteAsync();
        await db.LoaiHangs.ExecuteDeleteAsync();
        await db.TaiKhoans.ExecuteDeleteAsync();

        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<TaiKhoan>();
        var adminUser = new TaiKhoan
        {
            MaTaiKhoan = 1,
            TenDangNhap = "admin",
            MatKhau = "Admin@123",
            HoTen = "Administrator",
            Email = "admin@example.test",
            VaiTro = "Admin",
            TrangThai = true
        };
        adminUser.MatKhau = passwordHasher.HashPassword(adminUser, adminUser.MatKhau);

        var nvkhoUser = new TaiKhoan
        {
            MaTaiKhoan = 2,
            TenDangNhap = "nvkho",
            MatKhau = "Kho@123",
            HoTen = "Nhan vien kho",
            Email = "warehouse@example.test",
            VaiTro = "NhanVienKho",
            TrangThai = true
        };
        nvkhoUser.MatKhau = passwordHasher.HashPassword(nvkhoUser, nvkhoUser.MatKhau);

        var boPhanUser = new TaiKhoan
        {
            MaTaiKhoan = 3,
            TenDangNhap = "bophan",
            MatKhau = "BoPhan@123",
            HoTen = "Dai dien Bo phan nhan",
            Email = "bophan@example.test",
            VaiTro = "BoPhanNhan",
            TrangThai = true
        };
        boPhanUser.MatKhau = passwordHasher.HashPassword(boPhanUser, boPhanUser.MatKhau);

        db.TaiKhoans.AddRange(adminUser, nvkhoUser, boPhanUser);

        db.Kho.Add(new Kho
        {
            MaKho = 1,
            TenKho = profile == SeedProfile.Catalog ? "Test warehouse" : "Kho Hà Nội",
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
            TenLoaiHang = profile == SeedProfile.Query ? "Điện tử" : "Test category",
            TrangThai = true
        });
        db.DonViTinhs.Add(new DonViTinh
        {
            MaDonViTinh = 1,
            TenDonViTinh = profile == SeedProfile.Query ? "Hộp" : "Piece",
            TrangThai = true
        });
        db.HangHoa.Add(new HangHoa
        {
            MaHang = 1,
            TenHang = "chuột quang",
            MaLoaiHang = 1,
            MaDonViTinh = 1,
            GiaNhapThamKhao = 25000,
            MucTonToiThieu = 5,
            TrangThai = true
        });
        db.HangHoa.AddRange(
            new HangHoa
            {
                MaHang = 2,
                TenHang = "Bàn phím",
                MaLoaiHang = 1,
                MaDonViTinh = 1,
                GiaNhapThamKhao = 50000,
                MucTonToiThieu = 5,
                TrangThai = true
            },
            new HangHoa
            {
                MaHang = 3,
                TenHang = "Màn hình",
                MaLoaiHang = 1,
                MaDonViTinh = 1,
                GiaNhapThamKhao = 75000,
                MucTonToiThieu = 5,
                TrangThai = true
            });

        if (profile != SeedProfile.Default)
        {
            db.Kho.AddRange(
                new Kho { MaKho = 2, TenKho = "Secondary warehouse", TrangThai = true },
                new Kho { MaKho = 3, TenKho = "Inactive warehouse", TrangThai = false });
            db.NhaCungCap.Add(new NhaCungCap
            {
                MaNhaCungCap = 2,
                TenNhaCungCap = "Secondary supplier",
                TrangThai = true
            });
            db.BoPhanNhans.Add(new BoPhanNhan
            {
                MaBoPhan = 2,
                TenBoPhan = "Secondary department",
                TrangThai = true
            });
            db.LoaiHangs.AddRange(
                new LoaiHang { MaLoaiHang = 2, TenLoaiHang = "Category B", TrangThai = true },
                new LoaiHang { MaLoaiHang = 3, TenLoaiHang = "Inactive category", TrangThai = false });
            db.DonViTinhs.AddRange(
                new DonViTinh { MaDonViTinh = 2, TenDonViTinh = "Box", TrangThai = true },
                new DonViTinh { MaDonViTinh = 3, TenDonViTinh = "Inactive unit", TrangThai = false });

            var extraProductCount = profile switch
            {
                SeedProfile.Query => 22,
                SeedProfile.StockBoundary => 3,
                _ => 2
            };
            var extraProducts = Enumerable.Range(4, extraProductCount)
                .Select(id => new HangHoa
                {
                    MaHang = id,
                    TenHang = profile == SeedProfile.Query ? $"chuột model {id:D2}" : $"Test item {id:D2}",
                    MaLoaiHang = profile == SeedProfile.Query ? 1 : id % 3 == 0 ? 2 : 1,
                    MaDonViTinh = id % 2 == 0 ? 2 : 1,
                    GiaNhapThamKhao = profile == SeedProfile.Query ? id * 25000 : 50 + (id * 25),
                    MucTonToiThieu = id % 2 == 0 ? 5 : 10,
                    TrangThai = id % 5 != 0
                })
                .ToArray();
            db.HangHoa.AddRange(extraProducts);
        }

        if (profile == SeedProfile.Default)
        {
            db.TonKhoes.Add(new TonKho
            {
                MaKho = 1,
                MaHang = 1,
                SoLuongTon = 100,
                NgayCapNhat = DateTime.Today
            });
        }
        else
        {
            var productCount = profile switch
            {
                SeedProfile.Query => 25,
                SeedProfile.StockBoundary => 6,
                _ => 5
            };
            for (var itemId = 1; itemId <= productCount; itemId++)
            {
                var quantity = profile == SeedProfile.StockBoundary
                    ? itemId switch { 1 => 100, 2 => 0, 3 => 5, 4 => 1, 5 => 10, _ => 25 }
                    : (itemId * 7) % 31;
                db.TonKhoes.Add(new TonKho
                {
                    MaKho = profile != SeedProfile.Query && itemId % 4 == 0 ? 2 : 1,
                    MaHang = itemId,
                    SoLuongTon = quantity,
                    NgayCapNhat = DateTime.Today.AddDays(-(itemId % 7))
                });
            }
        }

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
