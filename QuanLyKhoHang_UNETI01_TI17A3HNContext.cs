using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_DHTI17A3HN.Models;

public class QuanLyKhoHang_UNETI01_TI17A3HNContext(DbContextOptions<QuanLyKhoHang_UNETI01_TI17A3HNContext> options) : DbContext(options)
{
    public DbSet<QuanLyKhoHang_UNETI01_TI17A3HN.Models.Kho> Kho { get; set; } = default!;
    public DbSet<TonKho> TonKhoes { get; set; }
    public DbSet<LichSuTonKho> LichSuTonKhoes { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình khóa chính cho Entity TonKho gồm MaKho và MaHang
        modelBuilder.Entity<TonKho>().HasKey(t => new { t.MaKho, t.MaHang });
    }
}
