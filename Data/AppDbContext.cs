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

        // Module 2: Hàng hóa, kho
        public DbSet<HangHoa> HangHoa { get; set; } = default!;
        public DbSet<HangHoa> HangHoas => HangHoa;
        public DbSet<Kho> Kho { get; set; } = default!;
        public DbSet<Kho> Khoes => Kho;

        // Module 3: Nhà cung cấp, Phiếu nhập, Chi tiết phiếu nhập
        public DbSet<QuanLyKhoHang_UNETI01_TI17A3HN.Models.PhieuNhap> PhieuNhap { get; set; } = default!;
        public DbSet<QuanLyKhoHang_UNETI01_TI17A3HN.Models.PhieuNhap> PhieuNhaps => PhieuNhap;
        public DbSet<QuanLyKhoHang_UNETI01_TI17A3HN.Models.ChiTietPhieuNhap> ChiTietPhieuNhap { get; set; } = default!;
        public DbSet<QuanLyKhoHang_UNETI01_TI17A3HN.Models.ChiTietPhieuNhap> ChiTietPhieuNhaps => ChiTietPhieuNhap;
        public DbSet<QuanLyKhoHang_UNETI01_TI17A3HN.Models.NhaCungCap> NhaCungCap { get; set; } = default!;
        public DbSet<QuanLyKhoHang_UNETI01_TI17A3HN.Models.NhaCungCap> NhaCungCaps => NhaCungCap;
        // Module 4: Bộ phận nhận, Phiếu xuất, Chi tiết phiếu xuất
        public DbSet<BoPhanNhan> BoPhanNhans { get; set; } = default!;
        public DbSet<PhieuXuat> PhieuXuats { get; set; } = default!;
        public DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = default!;


        // Module 5: Tồn kho, Lịch sử tồn kho
        public DbSet<TonKho> TonKhoes { get; set; } = default!;
        public DbSet<LichSuTonKho> LichSuTonKhoes { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

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

            // Unique indexes Module 2, 3, 4
            modelBuilder.Entity<Kho>()
                .HasIndex(k => k.TenKho)
                .IsUnique();

            modelBuilder.Entity<HangHoa>()
                .HasIndex(h => h.TenHang)
                .IsUnique();

            modelBuilder.Entity<NhaCungCap>()
                .HasIndex(n => n.TenNhaCungCap)
                .IsUnique();

            modelBuilder.Entity<BoPhanNhan>()
                .HasIndex(b => b.TenBoPhan)
                .IsUnique();

            // Khóa chính hỗn hợp bảng Tồn kho (MaKho, MaHang)
            modelBuilder.Entity<TonKho>()
                .HasKey(t => new { t.MaKho, t.MaHang });

            // Quan hệ Module 2: Hàng hóa - Loại hàng - Đơn vị tính, Kho - Phiếu nhập
            modelBuilder.Entity<HangHoa>()
                .HasOne(h => h.LoaiHang)
                .WithMany(l => l.HangHoas)
                .HasForeignKey(h => h.MaLoaiHang)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HangHoa>()
                .HasOne(h => h.DonViTinh)
                .WithMany(d => d.HangHoas)
                .HasForeignKey(h => h.MaDonViTinh)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuNhap>()
                .HasOne(p => p.Kho)
                .WithMany(k => k.PhieuNhaps)
                .HasForeignKey(p => p.MaKho)
                .OnDelete(DeleteBehavior.Restrict);

            // Module 3: PhieuNhap - NhaCungCap - ChiTietPhieuNhap relationships
            modelBuilder.Entity<PhieuNhap>()
                .HasOne(p => p.NhaCungCap)
                .WithMany(n => n.PhieuNhaps)
                .HasForeignKey(p => p.MaNhaCungCap)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietPhieuNhap>()
                .HasOne(c => c.PhieuNhap)
                .WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(c => c.MaPhieuNhap)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietPhieuNhap>()
                .HasOne(c => c.HangHoa)
                .WithMany(h => h.DanhSachChiTietNhap)
                .HasForeignKey(c => c.MaHang)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ Module 4: Phiếu xuất - Bộ phận nhận - Kho - Chi tiết
            modelBuilder.Entity<PhieuXuat>()
                .HasOne(p => p.BoPhanNhan)
                .WithMany(b => b.PhieuXuats)
                .HasForeignKey(p => p.MaBoPhan)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuXuat>()
                .HasOne(p => p.Kho)
                .WithMany(k => k.PhieuXuats)
                .HasForeignKey(p => p.MaKho)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietPhieuXuat>()
                .HasOne(c => c.PhieuXuat)
                .WithMany(p => p.ChiTietPhieuXuats)
                .HasForeignKey(c => c.MaPhieuXuat)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietPhieuXuat>()
                .HasOne(c => c.HangHoa)
                .WithMany(h => h.DanhSachChiTietXuat)
                .HasForeignKey(c => c.MaHang)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ Module 5: Tồn kho & Lịch sử tồn kho với Kho và Hàng hóa
            modelBuilder.Entity<TonKho>()
                .HasOne(t => t.Kho)
                .WithMany(k => k.DanhSachTonKho)
                .HasForeignKey(t => t.MaKho)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TonKho>()
                .HasOne(t => t.HangHoa)
                .WithMany(h => h.DanhSachTonKho)
                .HasForeignKey(t => t.MaHang)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuTonKho>()
                .HasOne(l => l.Kho)
                .WithMany(k => k.LichSuTonKhoes)
                .HasForeignKey(l => l.MaKho)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuTonKho>()
                .HasOne(l => l.HangHoa)
                .WithMany(h => h.LichSuTonKhoes)
                .HasForeignKey(l => l.MaHang)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
