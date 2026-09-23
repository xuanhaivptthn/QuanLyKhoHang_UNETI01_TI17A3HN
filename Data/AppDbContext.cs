using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        // Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng, Đơn vị tính
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = default!;
        public DbSet<LoaiHang> LoaiHangs { get; set; } = default!;
        public DbSet<DonViTinh> DonViTinhs { get; set; } = default!;

        // Danh mục Kho (Đã có từ ban đầu)
        public DbSet<Kho> Kho { get; set; } = default!;
        public DbSet<Kho> Khoes => Kho;

        // Module 4: Bộ phận nhận, Phiếu xuất, Chi tiết phiếu xuất
        public DbSet<BoPhanNhan> BoPhanNhans { get; set; } = default!;
        public DbSet<PhieuXuat> PhieuXuats { get; set; } = default!;
        public DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = default!;

        // Module 5: Tồn kho, Lịch sử tồn kho
        public DbSet<TonKho> TonKhoes { get; set; } = default!;
        public DbSet<LichSuTonKho> LichSuTonKhoes { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique indexes Module 1
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            modelBuilder.Entity<LoaiHang>()
                .HasIndex(l => l.TenLoaiHang)
                .IsUnique();

            modelBuilder.Entity<DonViTinh>()
                .HasIndex(d => d.TenDonViTinh)
                .IsUnique();

            // Khóa chính hỗn hợp bảng Tồn kho (MaKho, MaHang)
            modelBuilder.Entity<TonKho>()
                .HasKey(t => new { t.MaKho, t.MaHang });

            // Quan hệ Module 4: Phiếu xuất - Bộ phận nhận - Kho
            modelBuilder.Entity<PhieuXuat>()
                .HasOne(p => p.BoPhanNhan)
                .WithMany(b => b.PhieuXuats)
                .HasForeignKey(p => p.MaBoPhan)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuXuat>()
                .HasOne(p => p.Kho)
                .WithMany()
                .HasForeignKey(p => p.MaKho)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietPhieuXuat>()
                .HasOne(c => c.PhieuXuat)
                .WithMany(p => p.ChiTietPhieuXuats)
                .HasForeignKey(c => c.MaPhieuXuat)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Module 5: Tồn kho & Lịch sử tồn kho với Kho
            modelBuilder.Entity<TonKho>()
                .HasOne(t => t.Kho)
                .WithMany()
                .HasForeignKey(t => t.MaKho)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuTonKho>()
                .HasOne(l => l.Kho)
                .WithMany()
                .HasForeignKey(l => l.MaKho)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
