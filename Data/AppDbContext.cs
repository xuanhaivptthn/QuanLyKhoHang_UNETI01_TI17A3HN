using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        public DbSet<PhieuNhap> PhieuNhap { get; set; } = default!;
        public DbSet<PhieuNhap> PhieuNhaps => PhieuNhap;
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhap { get; set; } = default!;
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps => ChiTietPhieuNhap;
        public DbSet<NhaCungCap> NhaCungCap { get; set; } = default!;
        public DbSet<NhaCungCap> NhaCungCaps => NhaCungCap;

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
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique indexes Module 1
            modelBuilder.Entity<TaiKhoan>().HasIndex(t => t.TenDangNhap).IsUnique();
            modelBuilder.Entity<LoaiHang>().HasIndex(l => l.TenLoaiHang).IsUnique();
            modelBuilder.Entity<DonViTinh>().HasIndex(d => d.TenDonViTinh).IsUnique();

            // Unique indexes Module 2, 3, 4
            modelBuilder.Entity<Kho>().HasIndex(k => k.TenKho).IsUnique();
            modelBuilder.Entity<HangHoa>().HasIndex(h => h.TenHang).IsUnique();
            modelBuilder.Entity<NhaCungCap>().HasIndex(n => n.TenNhaCungCap).IsUnique();
            modelBuilder.Entity<BoPhanNhan>().HasIndex(b => b.TenBoPhan).IsUnique();

            // Khóa chính hỗn hợp bảng Tồn kho (MaKho, MaHang)
            modelBuilder.Entity<TonKho>().HasKey(t => new { t.MaKho, t.MaHang });

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

            // Nạp dữ liệu mẫu ban đầu qua Fluent API OnModelCreating
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var seed = GenerateSeedData();
            modelBuilder.Entity<TaiKhoan>().HasData(seed.TaiKhoans);
            modelBuilder.Entity<LoaiHang>().HasData(seed.LoaiHangs);
            modelBuilder.Entity<DonViTinh>().HasData(seed.DonViTinhs);
            modelBuilder.Entity<Kho>().HasData(seed.Khos);
            modelBuilder.Entity<NhaCungCap>().HasData(seed.NhaCungCaps);
            modelBuilder.Entity<BoPhanNhan>().HasData(seed.BoPhanNhans);
            modelBuilder.Entity<HangHoa>().HasData(seed.HangHoas);
            modelBuilder.Entity<PhieuNhap>().HasData(seed.PhieuNhaps);
            modelBuilder.Entity<ChiTietPhieuNhap>().HasData(seed.ChiTietNhaps);
            modelBuilder.Entity<PhieuXuat>().HasData(seed.PhieuXuats);
            modelBuilder.Entity<ChiTietPhieuXuat>().HasData(seed.ChiTietXuats);
            modelBuilder.Entity<TonKho>().HasData(seed.TonKhoes);
            modelBuilder.Entity<LichSuTonKho>().HasData(seed.LichSuList);
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu hiện có trong CSDL và nạp lại toàn bộ dữ liệu mẫu chuẩn.
        /// Dùng khi ứng dụng chạy với tham số CLI --seed.
        /// </summary>
        public void ReseedData()
        {
            Database.Migrate();

            // Xóa theo thứ tự ràng buộc khóa ngoại (bảng con trước, bảng cha sau)
            ChiTietPhieuXuats.ExecuteDelete();
            PhieuXuats.ExecuteDelete();
            ChiTietPhieuNhaps.ExecuteDelete();
            PhieuNhap.ExecuteDelete();
            LichSuTonKhoes.ExecuteDelete();
            TonKhoes.ExecuteDelete();
            HangHoa.ExecuteDelete();
            BoPhanNhans.ExecuteDelete();
            NhaCungCap.ExecuteDelete();
            Kho.ExecuteDelete();
            DonViTinhs.ExecuteDelete();
            LoaiHangs.ExecuteDelete();
            TaiKhoans.ExecuteDelete();

            var seed = GenerateSeedData();
            bool isSqlServer = Database.IsSqlServer();

            if (isSqlServer)
            {
                using var tx = Database.BeginTransaction();

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[TaiKhoan] ON;");
                TaiKhoans.AddRange(seed.TaiKhoans);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[TaiKhoan] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[TaiKhoan]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[LoaiHang] ON;");
                LoaiHangs.AddRange(seed.LoaiHangs);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[LoaiHang] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[LoaiHang]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[DonViTinh] ON;");
                DonViTinhs.AddRange(seed.DonViTinhs);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[DonViTinh] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[DonViTinh]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Kho] ON;");
                Kho.AddRange(seed.Khos);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Kho] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[Kho]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[NhaCungCap] ON;");
                NhaCungCap.AddRange(seed.NhaCungCaps);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[NhaCungCap] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[NhaCungCap]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[BoPhanNhan] ON;");
                BoPhanNhans.AddRange(seed.BoPhanNhans);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[BoPhanNhan] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[BoPhanNhan]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[HangHoa] ON;");
                HangHoa.AddRange(seed.HangHoas);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[HangHoa] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[HangHoa]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[PhieuNhap] ON;");
                PhieuNhap.AddRange(seed.PhieuNhaps);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[PhieuNhap] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[PhieuNhap]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[ChiTietPhieuNhap] ON;");
                ChiTietPhieuNhap.AddRange(seed.ChiTietNhaps);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[ChiTietPhieuNhap] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[ChiTietPhieuNhap]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[PhieuXuat] ON;");
                PhieuXuats.AddRange(seed.PhieuXuats);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[PhieuXuat] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[PhieuXuat]', RESEED);");

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[ChiTietPhieuXuat] ON;");
                ChiTietPhieuXuats.AddRange(seed.ChiTietXuats);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[ChiTietPhieuXuat] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[ChiTietPhieuXuat]', RESEED);");

                TonKhoes.AddRange(seed.TonKhoes);
                SaveChanges();

                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[LichSuTonKhoes] ON;");
                LichSuTonKhoes.AddRange(seed.LichSuList);
                SaveChanges();
                Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[LichSuTonKhoes] OFF;");
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ('[dbo].[LichSuTonKhoes]', RESEED);");

                tx.Commit();
            }
            else
            {
                TaiKhoans.AddRange(seed.TaiKhoans);
                LoaiHangs.AddRange(seed.LoaiHangs);
                DonViTinhs.AddRange(seed.DonViTinhs);
                Kho.AddRange(seed.Khos);
                NhaCungCap.AddRange(seed.NhaCungCaps);
                BoPhanNhans.AddRange(seed.BoPhanNhans);
                HangHoa.AddRange(seed.HangHoas);
                PhieuNhap.AddRange(seed.PhieuNhaps);
                ChiTietPhieuNhap.AddRange(seed.ChiTietNhaps);
                PhieuXuats.AddRange(seed.PhieuXuats);
                ChiTietPhieuXuats.AddRange(seed.ChiTietXuats);
                TonKhoes.AddRange(seed.TonKhoes);
                LichSuTonKhoes.AddRange(seed.LichSuList);
                SaveChanges();
            }
        }

        private static readonly PasswordHasher<TaiKhoan> _hasher = new();
        private static readonly string AdminHash = _hasher.HashPassword(null!, "Admin@123");
        private static readonly string DefaultHash = _hasher.HashPassword(null!, "123456");

        public static SeedDataContainer GenerateSeedData()
        {
            var taiKhoans = new List<TaiKhoan>
            {
                new TaiKhoan { MaTaiKhoan = 1, TenDangNhap = "admin", MatKhau = AdminHash, HoTen = "Trần Xuân Hải", Email = "haixuan@uneti.edu.vn", VaiTro = "Admin", TrangThai = true },
                new TaiKhoan { MaTaiKhoan = 2, TenDangNhap = "nvkho_hung", MatKhau = DefaultHash, HoTen = "Lê Văn Hùng", Email = "hunglv@uneti.edu.vn", VaiTro = "NhanVienKho", TrangThai = true },
                new TaiKhoan { MaTaiKhoan = 3, TenDangNhap = "nvkho_cuc", MatKhau = DefaultHash, HoTen = "Nguyễn Thị Cúc", Email = "cucnt@uneti.edu.vn", VaiTro = "NhanVienKho", TrangThai = true },
                new TaiKhoan { MaTaiKhoan = 4, TenDangNhap = "nvkho_cuong", MatKhau = DefaultHash, HoTen = "Nguyễn Văn Cường", Email = "cuongnv@uneti.edu.vn", VaiTro = "NhanVienKho", TrangThai = true },
                new TaiKhoan { MaTaiKhoan = 5, TenDangNhap = "bophan_dung", MatKhau = DefaultHash, HoTen = "Nguyễn Việt Dũng", Email = "dungnv@uneti.edu.vn", VaiTro = "BoPhanNhan", TrangThai = true },
                new TaiKhoan { MaTaiKhoan = 6, TenDangNhap = "bophan_lan", MatKhau = DefaultHash, HoTen = "Phạm Thị Lan", Email = "lanpt@uneti.edu.vn", VaiTro = "BoPhanNhan", TrangThai = true },
                new TaiKhoan { MaTaiKhoan = 7, TenDangNhap = "user_khoa", MatKhau = DefaultHash, HoTen = "Đỗ Minh Tuấn", Email = "tuandm@uneti.edu.vn", VaiTro = "NhanVienKho", TrangThai = false },
                new TaiKhoan { MaTaiKhoan = 8, TenDangNhap = "nvkho_mai", MatKhau = DefaultHash, HoTen = "Bùi Thị Mai", Email = "maibt@uneti.edu.vn", VaiTro = "NhanVienKho", TrangThai = true },
                new TaiKhoan { MaTaiKhoan = 9, TenDangNhap = "bophan_manh", MatKhau = DefaultHash, HoTen = "Hoàng Văn Mạnh", Email = "manhhv@uneti.edu.vn", VaiTro = "BoPhanNhan", TrangThai = true },
            };

            var loaiHangs = new List<LoaiHang>
            {
                new LoaiHang { MaLoaiHang = 1, TenLoaiHang = "Thiết bị vi tính & Văn phòng", MoTa = "Máy tính để bàn, màn hình, máy in và thiết bị ngoại vi văn phòng", TrangThai = true },
                new LoaiHang { MaLoaiHang = 2, TenLoaiHang = "Linh kiện điện tử & Vi mạch", MoTa = "Bo mạch nhúng, cảm biến, vi điều khiển phục vụ thực hành & thí nghiệm", TrangThai = true },
                new LoaiHang { MaLoaiHang = 3, TenLoaiHang = "Vật tư điện & Cơ khí xưởng", MoTa = "Dây cáp nguồn, aptomat, công tắc, đồng hồ đo và dụng cụ cầm tay", TrangThai = true },
                new LoaiHang { MaLoaiHang = 4, TenLoaiHang = "Thiết bị mạng & Viễn thông", MoTa = "Router WiFi, Switch chia mạng, cáp mạng UTP, hạt mạng RJ45, tủ rack", TrangThai = true },
                new LoaiHang { MaLoaiHang = 5, TenLoaiHang = "Văn phòng phẩm & Tiêu hao", MoTa = "Giấy in văn phòng, mực in laser, bút dạ, băng dính và màng bọc", TrangThai = true },
                new LoaiHang { MaLoaiHang = 6, TenLoaiHang = "Thiết bị Âm thanh & Trình chiếu", MoTa = "Máy chiếu, màn chiếu, loa trợ giảng, micro và thiết bị truyền hình", TrangThai = true },
                new LoaiHang { MaLoaiHang = 7, TenLoaiHang = "Dụng cụ đo lường & Kiểm định", MoTa = "Máy hiện sóng, nguồn đa năng, ampe kìm, thước kẹp điện tử, cân phân tích", TrangThai = true },
                new LoaiHang { MaLoaiHang = 8, TenLoaiHang = "Dụng cụ thí nghiệm cũ (Tạm ngưng)", MoTa = "Danh mục thiết bị ngừng tiếp nhận nhập kho mới", TrangThai = false },
            };

            var donViTinhs = new List<DonViTinh>
            {
                new DonViTinh { MaDonViTinh = 1, TenDonViTinh = "Chiếc", MoTa = "Đơn vị đếm máy móc, màn hình, thiết bị nguyên chiếc", TrangThai = true },
                new DonViTinh { MaDonViTinh = 2, TenDonViTinh = "Cái", MoTa = "Đơn vị đếm linh kiện, chuột, bàn phím, phụ kiện nhỏ", TrangThai = true },
                new DonViTinh { MaDonViTinh = 3, TenDonViTinh = "Bộ", MoTa = "Tập hợp thiết bị hoàn chỉnh (máy tính kèm phụ kiện, trạm hàn)", TrangThai = true },
                new DonViTinh { MaDonViTinh = 4, TenDonViTinh = "Hộp", MoTa = "Quy cách đóng gói hộp (mực in, hạt mạng, linh kiện)", TrangThai = true },
                new DonViTinh { MaDonViTinh = 5, TenDonViTinh = "Thùng", MoTa = "Thùng carton phục vụ xuất kho số lượng lớn (giấy in, cáp mạng)", TrangThai = true },
                new DonViTinh { MaDonViTinh = 6, TenDonViTinh = "Mét", MoTa = "Đo chiều dài dây dẫn cáp điện, cáp mạng", TrangThai = true },
                new DonViTinh { MaDonViTinh = 7, TenDonViTinh = "Gói", MoTa = "Quy cách đóng gói vật tư phụ trợ", TrangThai = true },
                new DonViTinh { MaDonViTinh = 8, TenDonViTinh = "Cuộn (Ngưng dùng)", MoTa = "Đơn vị cuộn cũ không áp dụng cho mặt hàng mới", TrangThai = false },
            };

            var khos = new List<Kho>
            {
                new Kho { MaKho = 1, TenKho = "Kho Tổng Minh Khai - Hà Nội", DiaDiem = "456 Minh Khai, Hai Bà Trưng, Hà Nội", MoTa = "Kho lưu trữ và điều phối chính toàn hệ thống", TrangThai = true },
                new Kho { MaKho = 2, TenKho = "Kho Xưởng Thực hành Nam Định", DiaDiem = "353 Trần Hưng Đạo, TP. Nam Định", MoTa = "Kho linh kiện, máy móc phục vụ giảng dạy thực hành kỹ thuật", TrangThai = true },
                new Kho { MaKho = 3, TenKho = "Kho Thiết bị Đội Cấn - Cầu Giấy", DiaDiem = "218 Đội Cấn, Ba Đình, Hà Nội", MoTa = "Kho lưu trữ dự phòng thiết bị công nghệ cao và vật tư tiêu hao", TrangThai = true },
                new Kho { MaKho = 4, TenKho = "Kho Trung tâm Dữ liệu Lĩnh Nam", DiaDiem = "350 Lĩnh Nam, Hoàng Mai, Hà Nội", MoTa = "Kho thiết bị viễn thông và máy chủ trung tâm", TrangThai = true },
                new Kho { MaKho = 5, TenKho = "Kho Lĩnh Nam Cũ (Tạm dừng hoạt động)", DiaDiem = "218 Lĩnh Nam, Hoàng Mai, Hà Nội", MoTa = "Kho đang nâng cấp sửa chữa, không tiếp nhận nhập xuất mới", TrangThai = false },
            };

            var nhaCungCaps = new List<NhaCungCap>
            {
                new NhaCungCap { MaNhaCungCap = 1, TenNhaCungCap = "Công ty TNHH Phân Phối Synnex FPT", SoDienThoai = "02473007300", Email = "contact@synnexfpt.vn", DiaChi = "Tòa nhà FPT, Phố Duy Tân, Cầu Giấy, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 2, TenNhaCungCap = "Công ty Cổ phần Máy tính Phong Vũ", SoDienThoai = "02873013030", Email = "cskh@phongvu.vn", DiaChi = "Tầng 5, 117-119 Lý Chính Thắng, Quận 3, TP.HCM", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 3, TenNhaCungCap = "Tập đoàn Công nghệ CMC", SoDienThoai = "02437958686", Email = "info@cmc.com.vn", DiaChi = "Tòa nhà CMC, Phố Duy Tân, Dịch Vọng Hậu, Cầu Giấy, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 4, TenNhaCungCap = "Công ty Thiết bị Điện & Cơ khí Cadisun", SoDienThoai = "02435581188", Email = "cadisun@cadisun.com.vn", DiaChi = "Cụm Công nghiệp Ngọc Hồi, Thanh Trì, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 5, TenNhaCungCap = "Công ty TNHH Linh Kiện Điện Tử Minh Hà", SoDienThoai = "0933388686", Email = "minhha.electronic@gmail.com", DiaChi = "Số 84 Triều Khúc, Thanh Xuân, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 6, TenNhaCungCap = "Công ty Cổ phần Công nghệ Mạng Phúc Anh", SoDienThoai = "02438573999", Email = "banhang@phucanh.vn", DiaChi = "15 Xã Đàn, Phương Liên, Đống Đa, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 7, TenNhaCungCap = "Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam", SoDienThoai = "02462928822", Email = "etechvietnam@gmail.com", DiaChi = "P402 Tòa nhà N03, Trần Quý Kiên, Cầu Giấy, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 8, TenNhaCungCap = "Công ty Cổ phần Văn phòng phẩm Hồng Hà", SoDienThoai = "02438524611", Email = "cskh@vpphongha.com.vn", DiaChi = "25 Lý Thường Kiệt, Hoàn Kiếm, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 9, TenNhaCungCap = "Công ty TNHH Thương mại Quốc tế Bách Khoa", SoDienThoai = "02438691234", Email = "bktech@bachkhoa.vn", DiaChi = "Số 1 Đại Cồ Việt, Hai Bà Trưng, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 10, TenNhaCungCap = "Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt", SoDienThoai = "02439871122", Email = "contact@asv.vn", DiaChi = "Số 45 Lê Duẩn, Hoàn Kiếm, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 11, TenNhaCungCap = "Công ty TNHH Giải pháp Viễn thông Sao Mai", SoDienThoai = "02437668899", Email = "saomai@telecom.vn", DiaChi = "88 Láng Hạ, Đống Đa, Hà Nội", TrangThai = true },
                new NhaCungCap { MaNhaCungCap = 12, TenNhaCungCap = "Công ty Cổ phần Thiết bị Đông Đô (Ngừng hợp tác)", SoDienThoai = "02439998888", Email = "dongdo@dongdo.vn", DiaChi = "Số 12 Giải Phóng, Hai Bà Trưng, Hà Nội", TrangThai = false },
            };

            var boPhanNhans = new List<BoPhanNhan>
            {
                new BoPhanNhan { MaBoPhan = 1, TenBoPhan = "Phòng Quản trị Thiết bị & Hạ tầng CNTT", NguoiDaiDien = "Hoàng Văn Mạnh", SoDienThoai = "0912345678", MoTa = "Quản trị máy chủ, phòng máy tính sinh viên và hạ tầng mạng", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 2, TenBoPhan = "Xưởng Thực hành Điện - Tự động hóa", NguoiDaiDien = "Đỗ Quang Huy", SoDienThoai = "0923456789", MoTa = "Xưởng kỹ thuật điện và thực hành cơ điện tử", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 3, TenBoPhan = "Trung tâm Dữ liệu & Viễn thông", NguoiDaiDien = "Lê Thanh Tùng", SoDienThoai = "0934567890", MoTa = "Quản lý phòng server, hệ thống camera và switch core", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 4, TenBoPhan = "Phòng Đào tạo & Khảo thí", NguoiDaiDien = "Nguyễn Minh Châu", SoDienThoai = "0945678901", MoTa = "Quản lý in ấn đề thi, tài liệu đào tạo và hồ sơ sinh viên", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 5, TenBoPhan = "Phòng Tài chính - Kế toán", NguoiDaiDien = "Trần Thu Trang", SoDienThoai = "0956789012", MoTa = "Máy in hóa đơn, giấy tờ thanh toán và máy tính kế toán", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 6, TenBoPhan = "Phòng Hành chính - Tổng vụ", NguoiDaiDien = "Vũ Đức Long", SoDienThoai = "0967890123", MoTa = "Vật tư tiêu hao, thiết bị phục vụ hội họp trường", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 7, TenBoPhan = "Khoa Công nghệ Thông tin (Văn phòng Khoa)", NguoiDaiDien = "Bùi Thị Mai", SoDienThoai = "0978901234", MoTa = "Văn phòng Khoa CNTT, nghiên cứu khoa học & phát triển phần mềm", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 8, TenBoPhan = "Khoa Điện tử & Kỹ thuật Máy tính", NguoiDaiDien = "Ngô Trọng Nghĩa", SoDienThoai = "0981122334", MoTa = "Phòng thực hành nhúng vi điều khiển và IoT", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 9, TenBoPhan = "Trung tâm Thí nghiệm & Chuyển giao Công nghệ", NguoiDaiDien = "Trần Văn Phong", SoDienThoai = "0901234567", MoTa = "Phòng thí nghiệm đo kiểm và chuyển giao công nghệ kỹ thuật", TrangThai = true },
                new BoPhanNhan { MaBoPhan = 10, TenBoPhan = "Ban Dự án Cơ sở Lĩnh Nam (Cũ)", NguoiDaiDien = "Lê Thành Nam", SoDienThoai = "0989012345", MoTa = "Bộ phận đã hoàn thành bàn giao dự án cơ sở", TrangThai = false },
            };

            var hangHoas = new List<HangHoa>
            {
                new HangHoa { MaHang = 1, TenHang = "Màn hình Dell UltraSharp 24 inch U2422H", MaLoaiHang = 1, MaDonViTinh = 1, GiaNhapThamKhao = 5450000m, MucTonToiThieu = 5, MoTa = "Màn hình IPS chuẩn màu cho phòng đồ họa & thiết kế", TrangThai = true },
                new HangHoa { MaHang = 2, TenHang = "Màn hình LG 27 inch IPS Full HD 27MP400", MaLoaiHang = 1, MaDonViTinh = 1, GiaNhapThamKhao = 3200000m, MucTonToiThieu = 6, MoTa = "Màn hình viền mỏng dùng cho phòng lab máy tính", TrangThai = true },
                new HangHoa { MaHang = 3, TenHang = "Máy in Laser đa năng HP LaserJet MFP M135w", MaLoaiHang = 1, MaDonViTinh = 1, GiaNhapThamKhao = 3850000m, MucTonToiThieu = 3, MoTa = "Máy in in trắng đen, photocopy và scan không dây", TrangThai = true },
                new HangHoa { MaHang = 4, TenHang = "Bàn phím cơ văn phòng Logitech K845 Cherry Red", MaLoaiHang = 1, MaDonViTinh = 2, GiaNhapThamKhao = 950000m, MucTonToiThieu = 15, MoTa = "Bàn phím cơ full size có đèn nền LED trắng", TrangThai = true },
                new HangHoa { MaHang = 5, TenHang = "Chuột quang không dây Fuhlen A09G", MaLoaiHang = 1, MaDonViTinh = 2, GiaNhapThamKhao = 135000m, MucTonToiThieu = 20, MoTa = "Chuột không dây 2.4GHz tiết kiệm pin", TrangThai = true },
                new HangHoa { MaHang = 6, TenHang = "Webcam hội nghị trực tuyến Logitech C920e 1080P", MaLoaiHang = 1, MaDonViTinh = 1, GiaNhapThamKhao = 1450000m, MucTonToiThieu = 5, MoTa = "Webcam Full HD tích hợp micro kép chống ồn", TrangThai = true },
                new HangHoa { MaHang = 7, TenHang = "Bộ lưu điện UPS APC Back-UPS 650VA", MaLoaiHang = 1, MaDonViTinh = 1, GiaNhapThamKhao = 1250000m, MucTonToiThieu = 4, MoTa = "Bộ tích điện dự phòng bảo vệ máy chủ & PC", TrangThai = true },
                new HangHoa { MaHang = 8, TenHang = "Chuột công thái học không dây Logitech MX Master 3S", MaLoaiHang = 1, MaDonViTinh = 2, GiaNhapThamKhao = 1950000m, MucTonToiThieu = 5, MoTa = "Chuột cao cấp hỗ trợ cuộn siêu tốc đệm cổ tay", TrangThai = true },
                new HangHoa { MaHang = 9, TenHang = "Máy quét tài liệu hai mặt Canon imageFORMULA DR-C225 II", MaLoaiHang = 1, MaDonViTinh = 1, GiaNhapThamKhao = 8200000m, MucTonToiThieu = 2, MoTa = "Máy scan khay nạp tự động tốc độ cao lưu trữ hồ sơ số", TrangThai = true },
                new HangHoa { MaHang = 10, TenHang = "Ổ cứng di động chống sốc SSD Kingston XS1000 1TB USB 3.2", MaLoaiHang = 1, MaDonViTinh = 2, GiaNhapThamKhao = 1850000m, MucTonToiThieu = 10, MoTa = "Ổ cứng di động sao lưu dữ liệu nghiên cứu tốc độ 1050MB/s", TrangThai = true },
                new HangHoa { MaHang = 11, TenHang = "Bo mạch vi điều khiển Arduino Uno R3 DIP", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 115000m, MucTonToiThieu = 20, MoTa = "Bo mạch Arduino Uno dùng cho thí nghiệm nhúng cơ bản", TrangThai = true },
                new HangHoa { MaHang = 12, TenHang = "Máy tính nhúng Raspberry Pi 4 Model B 4GB", MaLoaiHang = 2, MaDonViTinh = 3, GiaNhapThamKhao = 1850000m, MucTonToiThieu = 5, MoTa = "Kit Raspberry Pi kèm vỏ tản nhiệt và nguồn USB-C", TrangThai = true },
                new HangHoa { MaHang = 13, TenHang = "Bo mạch WiFi IoT ESP32 NodeMCU 38 chân", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 85000m, MucTonToiThieu = 25, MoTa = "Module tích hợp WiFi và Bluetooth ứng dụng IoT", TrangThai = true },
                new HangHoa { MaHang = 14, TenHang = "Cảm biến nhiệt độ độ ẩm DHT11", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 22000m, MucTonToiThieu = 30, MoTa = "Module cảm biến số đo nhiệt độ và độ ẩm không khí", TrangThai = true },
                new HangHoa { MaHang = 15, TenHang = "Module rơ-le Relay 4 kênh cách ly quang 5V", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 45000m, MucTonToiThieu = 20, MoTa = "Mạch kích rơ-le điều khiển thiết bị điện 220V", TrangThai = true },
                new HangHoa { MaHang = 16, TenHang = "Động cơ bước điều khiển góc Servo SG90", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 28000m, MucTonToiThieu = 20, MoTa = "Động cơ servo mini 9g dùng cho cánh tay robot thí nghiệm", TrangThai = true },
                new HangHoa { MaHang = 17, TenHang = "Mạch hạ áp giảm dòng xung LM2596 Buck DC-DC", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 18000m, MucTonToiThieu = 30, MoTa = "Module chuyển nguồn DC 3.2V - 35V xuống 1.25V - 30V", TrangThai = true },
                new HangHoa { MaHang = 18, TenHang = "Cảm biến khoảng cách siêu âm HC-SR04", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 25000m, MucTonToiThieu = 25, MoTa = "Module đo khoảng cách 2cm - 400cm cho robot dò đường", TrangThai = true },
                new HangHoa { MaHang = 19, TenHang = "Module sạc xả bảo vệ pin Lithium 1S TP4056 Type-C", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 12000m, MucTonToiThieu = 40, MoTa = "Mạch tích hợp cổng sạc Type-C bảo vệ quá dòng quá áp", TrangThai = true },
                new HangHoa { MaHang = 20, TenHang = "Màn hình LCD 1602 kèm mạch giao tiếp I2C xanh lam", MaLoaiHang = 2, MaDonViTinh = 2, GiaNhapThamKhao = 55000m, MucTonToiThieu = 20, MoTa = "Màn hình hiển thị 16 ký tự 2 dòng giao tiếp 2 dây I2C", TrangThai = true },
                new HangHoa { MaHang = 21, TenHang = "Cáp điện đôi mềm Cadisun 2x2.5mm² (Cuộn 100m)", MaLoaiHang = 3, MaDonViTinh = 6, GiaNhapThamKhao = 18500m, MucTonToiThieu = 50, MoTa = "Dây điện bọc nhựa PVC lõi đồng nguyên chất chịu tải", TrangThai = true },
                new HangHoa { MaHang = 22, TenHang = "Aptomat chống giật Panasonic RCBO 32A 30mA", MaLoaiHang = 3, MaDonViTinh = 2, GiaNhapThamKhao = 480000m, MucTonToiThieu = 10, MoTa = "Thiết bị bảo vệ quá tải, ngắn mạch và chống dòng rò", TrangThai = true },
                new HangHoa { MaHang = 23, TenHang = "Đồng hồ vạn năng hiện số Sanwa CD800a Nhật Bản", MaLoaiHang = 3, MaDonViTinh = 1, GiaNhapThamKhao = 920000m, MucTonToiThieu = 8, MoTa = "Đồng hồ đo điện áp, dòng điện, điện trở độ chính xác cao", TrangThai = true },
                new HangHoa { MaHang = 24, TenHang = "Kìm bấm đầu cáp mạng đa năng Dintek RJ45/RJ11", MaLoaiHang = 3, MaDonViTinh = 2, GiaNhapThamKhao = 295000m, MucTonToiThieu = 10, MoTa = "Kìm trợ lực thép carbon bấm hạt mạng chuẩn kỹ thuật", TrangThai = true },
                new HangHoa { MaHang = 25, TenHang = "Trạm hàn điều chỉnh nhiệt độ Quick 936A ESD 60W", MaLoaiHang = 3, MaDonViTinh = 3, GiaNhapThamKhao = 750000m, MucTonToiThieu = 5, MoTa = "Máy hàn thiếc phòng thí nghiệm chống tĩnh điện", TrangThai = true },
                new HangHoa { MaHang = 26, TenHang = "Ổ cắm di động công nghiệp Schneider 4 ngõ có rơ le", MaLoaiHang = 3, MaDonViTinh = 2, GiaNhapThamKhao = 320000m, MucTonToiThieu = 12, MoTa = "Ổ cắm chịu tải công suất cao có công tắc ngắt an toàn", TrangThai = true },
                new HangHoa { MaHang = 27, TenHang = "Dây thít nhựa rút chịu nhiệt 20cm (Gói 100 cái)", MaLoaiHang = 3, MaDonViTinh = 4, GiaNhapThamKhao = 25000m, MucTonToiThieu = 40, MoTa = "Dây rút cố định hệ thống bó dây cáp mạng và điện", TrangThai = true },
                new HangHoa { MaHang = 28, TenHang = "Bộ tuốc nơ vít đa năng Jakemy 53 món cơ khí chính xác", MaLoaiHang = 3, MaDonViTinh = 3, GiaNhapThamKhao = 210000m, MucTonToiThieu = 15, MoTa = "Bộ đầu vặn tháo lắp laptop, smartphone và bo mạch", TrangThai = true },
                new HangHoa { MaHang = 29, TenHang = "Máy khoan động lực cầm tay Bosch GSB 550 Professional", MaLoaiHang = 3, MaDonViTinh = 3, GiaNhapThamKhao = 1150000m, MucTonToiThieu = 4, MoTa = "Máy khoan 550W khoan tường sắt gỗ lắp đặt thiết bị xưởng", TrangThai = true },
                new HangHoa { MaHang = 30, TenHang = "Router WiFi 6 TP-Link Archer AX1500 Băng tần kép", MaLoaiHang = 4, MaDonViTinh = 1, GiaNhapThamKhao = 1080000m, MucTonToiThieu = 6, MoTa = "Bộ định tuyến phát sóng WiFi chuẩn AX tốc độ 1.5Gbps", TrangThai = true },
                new HangHoa { MaHang = 31, TenHang = "Switch chia mạng Cisco Business CBS110 24 Cổng Gigabit", MaLoaiHang = 4, MaDonViTinh = 1, GiaNhapThamKhao = 2850000m, MucTonToiThieu = 4, MoTa = "Bộ chuyển mạch 24 port 10/100/1000Mbps dùng cho phòng máy", TrangThai = true },
                new HangHoa { MaHang = 32, TenHang = "Thùng cáp mạng UTP Cat6 Commscope chính hãng (305m)", MaLoaiHang = 4, MaDonViTinh = 5, GiaNhapThamKhao = 2450000m, MucTonToiThieu = 5, MoTa = "Thùng dây mạng bấm phòng máy và kéo ngầm", TrangThai = true },
                new HangHoa { MaHang = 33, TenHang = "Hộp hạt mạng RJ45 Cat6 bọc kim chống nhiễu (100 hạt)", MaLoaiHang = 4, MaDonViTinh = 4, GiaNhapThamKhao = 260000m, MucTonToiThieu = 15, MoTa = "Đầu bấm mạng chân đồng mạ vàng truyền tín hiệu cao", TrangThai = true },
                new HangHoa { MaHang = 34, TenHang = "Tủ mạng Rack 10U treo tường cửa lưới D600", MaLoaiHang = 4, MaDonViTinh = 1, GiaNhapThamKhao = 1350000m, MucTonToiThieu = 3, MoTa = "Tủ chứa switch, patch panel và khay chứa thiết bị mạng", TrangThai = true },
                new HangHoa { MaHang = 35, TenHang = "Bộ phát WiFi chuyên dụng âm trần Ruijie Reyee RG-RAP2200(E)", MaLoaiHang = 4, MaDonViTinh = 1, GiaNhapThamKhao = 1750000m, MucTonToiThieu = 4, MoTa = "Access Point chịu tải cao cấp cho giảng đường", TrangThai = true },
                new HangHoa { MaHang = 36, TenHang = "Dây nhảy quang Singlemode LC-LC dài 3 mét", MaLoaiHang = 4, MaDonViTinh = 2, GiaNhapThamKhao = 65000m, MucTonToiThieu = 20, MoTa = "Dây nối tín hiệu quang kết nối module SFP", TrangThai = true },
                new HangHoa { MaHang = 37, TenHang = "Module quang Cisco SFP GLC-SX-MMD 1Gbps đa mốt", MaLoaiHang = 4, MaDonViTinh = 2, GiaNhapThamKhao = 780000m, MucTonToiThieu = 8, MoTa = "Module chuyển đổi quang điện cắm nóng cho Switch Cisco", TrangThai = true },
                new HangHoa { MaHang = 38, TenHang = "Bộ chuyển đổi quang điện Media Converter Gigabit TP-Link MC220L", MaLoaiHang = 4, MaDonViTinh = 1, GiaNhapThamKhao = 620000m, MucTonToiThieu = 6, MoTa = "Converter quang điện 1 khe cắm SFP 1000Mbps", TrangThai = true },
                new HangHoa { MaHang = 39, TenHang = "Thùng giấy in A4 Double A Định lượng 70gsm (5 ram/thùng)", MaLoaiHang = 5, MaDonViTinh = 5, GiaNhapThamKhao = 385000m, MucTonToiThieu = 10, MoTa = "Giấy trắng sáng in văn bản giáo trình và biểu mẫu", TrangThai = true },
                new HangHoa { MaHang = 40, TenHang = "Hộp mực in Laser HP 107A Black Toner Cartridge", MaLoaiHang = 5, MaDonViTinh = 4, GiaNhapThamKhao = 1150000m, MucTonToiThieu = 6, MoTa = "Hộp mực chính hãng in 1000 trang rõ nét", TrangThai = true },
                new HangHoa { MaHang = 41, TenHang = "Hộp mực máy in Canon Cartridge 303 (dùng Canon 2900)", MaLoaiHang = 5, MaDonViTinh = 4, GiaNhapThamKhao = 750000m, MucTonToiThieu = 8, MoTa = "Hộp mực phổ thông tương thích máy in LBP2900", TrangThai = true },
                new HangHoa { MaHang = 42, TenHang = "Hộp bút bi Thiên Long 0.5mm xanh (20 cây/hộp)", MaLoaiHang = 5, MaDonViTinh = 4, GiaNhapThamKhao = 75000m, MucTonToiThieu = 15, MoTa = "Bút bi viết êm phục vụ văn phòng và giảng viên", TrangThai = true },
                new HangHoa { MaHang = 43, TenHang = "Cuộn băng dính đóng hàng dán thùng 500g loại 5cm", MaLoaiHang = 5, MaDonViTinh = 2, GiaNhapThamKhao = 32000m, MucTonToiThieu = 20, MoTa = "Băng dính bản rộng đóng kiện hàng hóa gửi xưởng", TrangThai = true },
                new HangHoa { MaHang = 44, TenHang = "Cuộn màng co PE bọc bảo vệ thiết bị hàng hóa (3kg)", MaLoaiHang = 5, MaDonViTinh = 2, GiaNhapThamKhao = 120000m, MucTonToiThieu = 10, MoTa = "Màng quấn pallet chống nước, chống bụi lưu kho", TrangThai = true },
                new HangHoa { MaHang = 45, TenHang = "Hộp pin tiểu AA Energizer Max vỉ 4 viên (Hộp 6 vỉ)", MaLoaiHang = 5, MaDonViTinh = 4, GiaNhapThamKhao = 240000m, MucTonToiThieu = 12, MoTa = "Pin alkaline chống rò rỉ dung lượng lớn cho chuột và đồng hồ đo", TrangThai = true },
                new HangHoa { MaHang = 46, TenHang = "Bìa còng lưu trữ hồ sơ tài liệu KingJim 7cm A4", MaLoaiHang = 5, MaDonViTinh = 2, GiaNhapThamKhao = 68000m, MucTonToiThieu = 25, MoTa = "Bìa còng lưu trữ chứng từ xuất nhập kho và giáo án", TrangThai = true },
                new HangHoa { MaHang = 47, TenHang = "Máy chiếu văn phòng hội thảo Panasonic PT-LB386", MaLoaiHang = 6, MaDonViTinh = 1, GiaNhapThamKhao = 14200000m, MucTonToiThieu = 2, MoTa = "Máy chiếu độ sáng 3800 Ansi Lumens độ phân giải XGA", TrangThai = true },
                new HangHoa { MaHang = 48, TenHang = "Màn chiếu di động 3 chân 100 inch Dalite Tripod", MaLoaiHang = 6, MaDonViTinh = 1, GiaNhapThamKhao = 1250000m, MucTonToiThieu = 3, MoTa = "Màn chiếu chân đứng di chuyển tiện lợi cho phòng họp xưởng", TrangThai = true },
                new HangHoa { MaHang = 49, TenHang = "Loa trợ giảng không dây công suất lớn Sony SRS-XP500", MaLoaiHang = 6, MaDonViTinh = 1, GiaNhapThamKhao = 6800000m, MucTonToiThieu = 2, MoTa = "Loa kéo không dây phục vụ thực tập hội trường lớn", TrangThai = true },
                new HangHoa { MaHang = 50, TenHang = "Bộ micro không dây cài áo đa năng Shure BLX14/CVL", MaLoaiHang = 6, MaDonViTinh = 3, GiaNhapThamKhao = 8500000m, MucTonToiThieu = 2, MoTa = "Micro cài ve áo giảng viên bắt âm nhạy lọc tạp âm", TrangThai = true },
                new HangHoa { MaHang = 51, TenHang = "Bút trình chiếu Laser không dây Logitech R400", MaLoaiHang = 6, MaDonViTinh = 2, GiaNhapThamKhao = 420000m, MucTonToiThieu = 8, MoTa = "Bút lật slide không dây tia laser đỏ phạm vi 15 mét", TrangThai = true },
                new HangHoa { MaHang = 52, TenHang = "Cáp tín hiệu HDMI 2.0 bọc dù Ugreen dài 10 mét", MaLoaiHang = 6, MaDonViTinh = 2, GiaNhapThamKhao = 360000m, MucTonToiThieu = 10, MoTa = "Cáp HDMI truyền hình ảnh 4K 60Hz chống nhiễu bọc dù", TrangThai = true },
                new HangHoa { MaHang = 53, TenHang = "Bộ chia tín hiệu HDMI 1 vào 4 ra chuẩn 4K Ugreen", MaLoaiHang = 6, MaDonViTinh = 1, GiaNhapThamKhao = 580000m, MucTonToiThieu = 5, MoTa = "Thiết bị nhân bản màn hình máy tính ra 4 màn hình chiếu", TrangThai = true },
                new HangHoa { MaHang = 54, TenHang = "Máy hiện sóng số để bàn Hantek DSO5102P 100MHz 2 Kênh", MaLoaiHang = 7, MaDonViTinh = 1, GiaNhapThamKhao = 6200000m, MucTonToiThieu = 2, MoTa = "Oscilloscope hiển thị dạng sóng xung thí nghiệm điện tử", TrangThai = true },
                new HangHoa { MaHang = 55, TenHang = "Nguồn DC điều chỉnh lập trình Longwei 30V 10A", MaLoaiHang = 7, MaDonViTinh = 3, GiaNhapThamKhao = 1850000m, MucTonToiThieu = 3, MoTa = "Bộ nguồn DC ổn dòng ổn áp màn hình LED kỹ thuật số", TrangThai = true },
                new HangHoa { MaHang = 56, TenHang = "Ampe kìm đo dòng xoay chiều Kyoritsu 2002PA Nhật Bản", MaLoaiHang = 7, MaDonViTinh = 1, GiaNhapThamKhao = 1750000m, MucTonToiThieu = 4, MoTa = "Ampe kìm đo dòng điện xoay chiều AC lên tới 2000A", TrangThai = true },
                new HangHoa { MaHang = 57, TenHang = "Máy đo khoảng cách laser đa năng Bosch GLM 50-27 CG", MaLoaiHang = 7, MaDonViTinh = 1, GiaNhapThamKhao = 2650000m, MucTonToiThieu = 3, MoTa = "Thước đo laser tia xanh độ chính xác cao đo phòng lab", TrangThai = true },
                new HangHoa { MaHang = 58, TenHang = "Thước kẹp điện tử chống nước Mitutoyo 150mm Nhật Bản", MaLoaiHang = 7, MaDonViTinh = 1, GiaNhapThamKhao = 2350000m, MucTonToiThieu = 4, MoTa = "Thước đo cơ khí dung sai 0.01mm tiêu chuẩn công nghiệp", TrangThai = true },
                new HangHoa { MaHang = 59, TenHang = "Cân điện tử độ chính xác cao Ohaus Scout SPX 220g/0.01g", MaLoaiHang = 7, MaDonViTinh = 1, GiaNhapThamKhao = 5600000m, MucTonToiThieu = 2, MoTa = "Cân tiểu ly kỹ thuật dùng cân hóa chất vật liệu thí nghiệm", TrangThai = true },
                new HangHoa { MaHang = 60, TenHang = "Thiết bị đo và định vị lỗi cáp mạng đa năng Noyafa NF-8209", MaLoaiHang = 7, MaDonViTinh = 3, GiaNhapThamKhao = 1150000m, MucTonToiThieu = 4, MoTa = "Máy dò cáp ngầm kiểm tra chiều dài đo thông mạch cáp", TrangThai = true },
            };

            var phieuNhaps = new List<PhieuNhap>
            {
                new PhieuNhap { MaPhieuNhap = 1, MaNhaCungCap = 1, MaKho = 1, NgayNhap = new DateTime(2026, 7, 1), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 101" },
                new PhieuNhap { MaPhieuNhap = 2, MaNhaCungCap = 2, MaKho = 2, NgayNhap = new DateTime(2026, 7, 3), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 102" },
                new PhieuNhap { MaPhieuNhap = 3, MaNhaCungCap = 3, MaKho = 3, NgayNhap = new DateTime(2026, 7, 5), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 103" },
                new PhieuNhap { MaPhieuNhap = 4, MaNhaCungCap = 4, MaKho = 4, NgayNhap = new DateTime(2026, 7, 7), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 104" },
                new PhieuNhap { MaPhieuNhap = 5, MaNhaCungCap = 5, MaKho = 1, NgayNhap = new DateTime(2026, 7, 9), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 105" },
                new PhieuNhap { MaPhieuNhap = 6, MaNhaCungCap = 6, MaKho = 2, NgayNhap = new DateTime(2026, 7, 11), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 106" },
                new PhieuNhap { MaPhieuNhap = 7, MaNhaCungCap = 7, MaKho = 3, NgayNhap = new DateTime(2026, 7, 13), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 107" },
                new PhieuNhap { MaPhieuNhap = 8, MaNhaCungCap = 8, MaKho = 4, NgayNhap = new DateTime(2026, 7, 15), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 108" },
                new PhieuNhap { MaPhieuNhap = 9, MaNhaCungCap = 9, MaKho = 1, NgayNhap = new DateTime(2026, 7, 17), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 109" },
                new PhieuNhap { MaPhieuNhap = 10, MaNhaCungCap = 10, MaKho = 2, NgayNhap = new DateTime(2026, 7, 19), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 110" },
                new PhieuNhap { MaPhieuNhap = 11, MaNhaCungCap = 11, MaKho = 3, NgayNhap = new DateTime(2026, 7, 22), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 111" },
                new PhieuNhap { MaPhieuNhap = 12, MaNhaCungCap = 1, MaKho = 4, NgayNhap = new DateTime(2026, 7, 24), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 112" },
                new PhieuNhap { MaPhieuNhap = 13, MaNhaCungCap = 2, MaKho = 1, NgayNhap = new DateTime(2026, 7, 26), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 113" },
                new PhieuNhap { MaPhieuNhap = 14, MaNhaCungCap = 3, MaKho = 2, NgayNhap = new DateTime(2026, 7, 28), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 114" },
                new PhieuNhap { MaPhieuNhap = 15, MaNhaCungCap = 4, MaKho = 3, NgayNhap = new DateTime(2026, 7, 30), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 115" },
                new PhieuNhap { MaPhieuNhap = 16, MaNhaCungCap = 5, MaKho = 4, NgayNhap = new DateTime(2026, 8, 1), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 116" },
                new PhieuNhap { MaPhieuNhap = 17, MaNhaCungCap = 6, MaKho = 1, NgayNhap = new DateTime(2026, 8, 3), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 117" },
                new PhieuNhap { MaPhieuNhap = 18, MaNhaCungCap = 7, MaKho = 2, NgayNhap = new DateTime(2026, 8, 5), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 118" },
                new PhieuNhap { MaPhieuNhap = 19, MaNhaCungCap = 8, MaKho = 3, NgayNhap = new DateTime(2026, 8, 7), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 119" },
                new PhieuNhap { MaPhieuNhap = 20, MaNhaCungCap = 9, MaKho = 4, NgayNhap = new DateTime(2026, 8, 9), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 120" },
                new PhieuNhap { MaPhieuNhap = 21, MaNhaCungCap = 10, MaKho = 1, NgayNhap = new DateTime(2026, 8, 12), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 121" },
                new PhieuNhap { MaPhieuNhap = 22, MaNhaCungCap = 11, MaKho = 2, NgayNhap = new DateTime(2026, 8, 14), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 122" },
                new PhieuNhap { MaPhieuNhap = 23, MaNhaCungCap = 1, MaKho = 3, NgayNhap = new DateTime(2026, 8, 16), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 123" },
                new PhieuNhap { MaPhieuNhap = 24, MaNhaCungCap = 2, MaKho = 4, NgayNhap = new DateTime(2026, 8, 18), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 124" },
                new PhieuNhap { MaPhieuNhap = 25, MaNhaCungCap = 3, MaKho = 1, NgayNhap = new DateTime(2026, 8, 20), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 125" },
                new PhieuNhap { MaPhieuNhap = 26, MaNhaCungCap = 4, MaKho = 2, NgayNhap = new DateTime(2026, 8, 22), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 126" },
                new PhieuNhap { MaPhieuNhap = 27, MaNhaCungCap = 5, MaKho = 3, NgayNhap = new DateTime(2026, 8, 24), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 127" },
                new PhieuNhap { MaPhieuNhap = 28, MaNhaCungCap = 6, MaKho = 4, NgayNhap = new DateTime(2026, 8, 26), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 128" },
                new PhieuNhap { MaPhieuNhap = 29, MaNhaCungCap = 7, MaKho = 1, NgayNhap = new DateTime(2026, 8, 28), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 129" },
                new PhieuNhap { MaPhieuNhap = 30, MaNhaCungCap = 8, MaKho = 2, NgayNhap = new DateTime(2026, 8, 30), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 130" },
                new PhieuNhap { MaPhieuNhap = 31, MaNhaCungCap = 9, MaKho = 3, NgayNhap = new DateTime(2026, 9, 2), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 131" },
                new PhieuNhap { MaPhieuNhap = 32, MaNhaCungCap = 10, MaKho = 4, NgayNhap = new DateTime(2026, 9, 4), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 132" },
                new PhieuNhap { MaPhieuNhap = 33, MaNhaCungCap = 11, MaKho = 1, NgayNhap = new DateTime(2026, 9, 6), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 133" },
                new PhieuNhap { MaPhieuNhap = 34, MaNhaCungCap = 1, MaKho = 2, NgayNhap = new DateTime(2026, 9, 8), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 134" },
                new PhieuNhap { MaPhieuNhap = 35, MaNhaCungCap = 2, MaKho = 3, NgayNhap = new DateTime(2026, 9, 10), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 135" },
                new PhieuNhap { MaPhieuNhap = 36, MaNhaCungCap = 3, MaKho = 4, NgayNhap = new DateTime(2026, 9, 12), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)2, GhiChu = "Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số 136" },
                new PhieuNhap { MaPhieuNhap = 37, MaNhaCungCap = 4, MaKho = 1, NgayNhap = new DateTime(2026, 9, 14), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)0, GhiChu = "Phiếu nhập nháp số PN-2026-037 đang bổ sung danh mục" },
                new PhieuNhap { MaPhieuNhap = 38, MaNhaCungCap = 5, MaKho = 2, NgayNhap = new DateTime(2026, 9, 16), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)0, GhiChu = "Phiếu nhập nháp số PN-2026-038 đang bổ sung danh mục" },
                new PhieuNhap { MaPhieuNhap = 39, MaNhaCungCap = 6, MaKho = 3, NgayNhap = new DateTime(2026, 9, 18), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)0, GhiChu = "Phiếu nhập nháp số PN-2026-039 đang bổ sung danh mục" },
                new PhieuNhap { MaPhieuNhap = 40, MaNhaCungCap = 7, MaKho = 4, NgayNhap = new DateTime(2026, 9, 20), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)0, GhiChu = "Phiếu nhập nháp số PN-2026-040 đang bổ sung danh mục" },
                new PhieuNhap { MaPhieuNhap = 41, MaNhaCungCap = 8, MaKho = 1, NgayNhap = new DateTime(2026, 9, 23), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)0, GhiChu = "Phiếu nhập nháp số PN-2026-041 đang bổ sung danh mục" },
                new PhieuNhap { MaPhieuNhap = 42, MaNhaCungCap = 9, MaKho = 2, NgayNhap = new DateTime(2026, 9, 25), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)1, GhiChu = "Phiếu nhập số PN-2026-042 đang chờ quản lý kho ký duyệt" },
                new PhieuNhap { MaPhieuNhap = 43, MaNhaCungCap = 10, MaKho = 3, NgayNhap = new DateTime(2026, 9, 27), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)1, GhiChu = "Phiếu nhập số PN-2026-043 đang chờ quản lý kho ký duyệt" },
                new PhieuNhap { MaPhieuNhap = 44, MaNhaCungCap = 11, MaKho = 4, NgayNhap = new DateTime(2026, 9, 29), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuNhap)1, GhiChu = "Phiếu nhập số PN-2026-044 đang chờ quản lý kho ký duyệt" },
                new PhieuNhap { MaPhieuNhap = 45, MaNhaCungCap = 1, MaKho = 1, NgayNhap = new DateTime(2026, 10, 1), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuNhap)1, GhiChu = "Phiếu nhập số PN-2026-045 đang chờ quản lý kho ký duyệt" },
                new PhieuNhap { MaPhieuNhap = 46, MaNhaCungCap = 2, MaKho = 2, NgayNhap = new DateTime(2026, 10, 3), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuNhap)3, GhiChu = "Phiếu nhập số PN-2026-046 đã hủy do nhà cung cấp giao trễ hạn" },
                new PhieuNhap { MaPhieuNhap = 47, MaNhaCungCap = 3, MaKho = 3, NgayNhap = new DateTime(2026, 10, 5), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuNhap)3, GhiChu = "Phiếu nhập số PN-2026-047 đã hủy do nhà cung cấp giao trễ hạn" },
                new PhieuNhap { MaPhieuNhap = 48, MaNhaCungCap = 4, MaKho = 4, NgayNhap = new DateTime(2026, 10, 7), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuNhap)3, GhiChu = "Phiếu nhập số PN-2026-048 đã hủy do nhà cung cấp giao trễ hạn" },
            };

            var chiTietNhaps = new List<ChiTietPhieuNhap>
            {
                new ChiTietPhieuNhap { MaChiTietNhap = 1, MaPhieuNhap = 1, MaHang = 3, SoLuongNhap = 20, DonGiaNhap = 3850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 2, MaPhieuNhap = 1, MaHang = 4, SoLuongNhap = 16, DonGiaNhap = 950000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 3, MaPhieuNhap = 2, MaHang = 5, SoLuongNhap = 25, DonGiaNhap = 135000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 4, MaPhieuNhap = 2, MaHang = 6, SoLuongNhap = 20, DonGiaNhap = 1450000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 5, MaPhieuNhap = 2, MaHang = 7, SoLuongNhap = 20, DonGiaNhap = 1250000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 6, MaPhieuNhap = 3, MaHang = 7, SoLuongNhap = 30, DonGiaNhap = 1250000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 7, MaPhieuNhap = 3, MaHang = 8, SoLuongNhap = 24, DonGiaNhap = 1950000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 8, MaPhieuNhap = 4, MaHang = 9, SoLuongNhap = 35, DonGiaNhap = 8200000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 9, MaPhieuNhap = 4, MaHang = 10, SoLuongNhap = 12, DonGiaNhap = 1850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 10, MaPhieuNhap = 4, MaHang = 11, SoLuongNhap = 10, DonGiaNhap = 115000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 11, MaPhieuNhap = 5, MaHang = 11, SoLuongNhap = 15, DonGiaNhap = 115000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 12, MaPhieuNhap = 5, MaHang = 12, SoLuongNhap = 16, DonGiaNhap = 1850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 13, MaPhieuNhap = 6, MaHang = 13, SoLuongNhap = 20, DonGiaNhap = 85000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 14, MaPhieuNhap = 6, MaHang = 14, SoLuongNhap = 20, DonGiaNhap = 22000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 15, MaPhieuNhap = 6, MaHang = 15, SoLuongNhap = 20, DonGiaNhap = 45000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 16, MaPhieuNhap = 7, MaHang = 15, SoLuongNhap = 25, DonGiaNhap = 45000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 17, MaPhieuNhap = 7, MaHang = 16, SoLuongNhap = 24, DonGiaNhap = 28000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 18, MaPhieuNhap = 8, MaHang = 17, SoLuongNhap = 30, DonGiaNhap = 18000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 19, MaPhieuNhap = 8, MaHang = 18, SoLuongNhap = 12, DonGiaNhap = 25000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 20, MaPhieuNhap = 8, MaHang = 19, SoLuongNhap = 10, DonGiaNhap = 12000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 21, MaPhieuNhap = 9, MaHang = 19, SoLuongNhap = 35, DonGiaNhap = 12000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 22, MaPhieuNhap = 9, MaHang = 20, SoLuongNhap = 16, DonGiaNhap = 55000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 23, MaPhieuNhap = 10, MaHang = 21, SoLuongNhap = 70, DonGiaNhap = 18500m },
                new ChiTietPhieuNhap { MaChiTietNhap = 24, MaPhieuNhap = 10, MaHang = 22, SoLuongNhap = 25, DonGiaNhap = 480000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 25, MaPhieuNhap = 10, MaHang = 23, SoLuongNhap = 20, DonGiaNhap = 920000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 26, MaPhieuNhap = 11, MaHang = 23, SoLuongNhap = 80, DonGiaNhap = 920000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 27, MaPhieuNhap = 11, MaHang = 24, SoLuongNhap = 33, DonGiaNhap = 295000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 28, MaPhieuNhap = 12, MaHang = 25, SoLuongNhap = 30, DonGiaNhap = 750000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 29, MaPhieuNhap = 12, MaHang = 26, SoLuongNhap = 41, DonGiaNhap = 320000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 30, MaPhieuNhap = 12, MaHang = 27, SoLuongNhap = 10, DonGiaNhap = 25000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 31, MaPhieuNhap = 13, MaHang = 27, SoLuongNhap = 40, DonGiaNhap = 25000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 32, MaPhieuNhap = 13, MaHang = 28, SoLuongNhap = 49, DonGiaNhap = 210000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 33, MaPhieuNhap = 14, MaHang = 29, SoLuongNhap = 50, DonGiaNhap = 1150000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 34, MaPhieuNhap = 14, MaHang = 30, SoLuongNhap = 57, DonGiaNhap = 1080000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 35, MaPhieuNhap = 14, MaHang = 31, SoLuongNhap = 20, DonGiaNhap = 2850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 36, MaPhieuNhap = 15, MaHang = 31, SoLuongNhap = 60, DonGiaNhap = 2850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 37, MaPhieuNhap = 15, MaHang = 32, SoLuongNhap = 25, DonGiaNhap = 2450000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 38, MaPhieuNhap = 16, MaHang = 33, SoLuongNhap = 70, DonGiaNhap = 260000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 39, MaPhieuNhap = 16, MaHang = 34, SoLuongNhap = 33, DonGiaNhap = 1350000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 40, MaPhieuNhap = 16, MaHang = 35, SoLuongNhap = 10, DonGiaNhap = 1750000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 41, MaPhieuNhap = 17, MaHang = 35, SoLuongNhap = 80, DonGiaNhap = 1750000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 42, MaPhieuNhap = 17, MaHang = 36, SoLuongNhap = 41, DonGiaNhap = 65000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 43, MaPhieuNhap = 18, MaHang = 37, SoLuongNhap = 30, DonGiaNhap = 780000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 44, MaPhieuNhap = 18, MaHang = 38, SoLuongNhap = 49, DonGiaNhap = 620000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 45, MaPhieuNhap = 18, MaHang = 39, SoLuongNhap = 20, DonGiaNhap = 385000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 46, MaPhieuNhap = 19, MaHang = 39, SoLuongNhap = 40, DonGiaNhap = 385000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 47, MaPhieuNhap = 19, MaHang = 40, SoLuongNhap = 57, DonGiaNhap = 1150000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 48, MaPhieuNhap = 20, MaHang = 41, SoLuongNhap = 50, DonGiaNhap = 750000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 49, MaPhieuNhap = 20, MaHang = 42, SoLuongNhap = 25, DonGiaNhap = 75000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 50, MaPhieuNhap = 20, MaHang = 43, SoLuongNhap = 10, DonGiaNhap = 32000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 51, MaPhieuNhap = 21, MaHang = 43, SoLuongNhap = 60, DonGiaNhap = 32000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 52, MaPhieuNhap = 21, MaHang = 44, SoLuongNhap = 33, DonGiaNhap = 120000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 53, MaPhieuNhap = 22, MaHang = 45, SoLuongNhap = 70, DonGiaNhap = 240000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 54, MaPhieuNhap = 22, MaHang = 46, SoLuongNhap = 41, DonGiaNhap = 68000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 55, MaPhieuNhap = 22, MaHang = 47, SoLuongNhap = 20, DonGiaNhap = 14200000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 56, MaPhieuNhap = 23, MaHang = 47, SoLuongNhap = 80, DonGiaNhap = 14200000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 57, MaPhieuNhap = 23, MaHang = 48, SoLuongNhap = 49, DonGiaNhap = 1250000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 58, MaPhieuNhap = 24, MaHang = 49, SoLuongNhap = 30, DonGiaNhap = 6800000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 59, MaPhieuNhap = 24, MaHang = 50, SoLuongNhap = 57, DonGiaNhap = 8500000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 60, MaPhieuNhap = 24, MaHang = 51, SoLuongNhap = 10, DonGiaNhap = 420000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 61, MaPhieuNhap = 25, MaHang = 51, SoLuongNhap = 40, DonGiaNhap = 420000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 62, MaPhieuNhap = 25, MaHang = 52, SoLuongNhap = 25, DonGiaNhap = 360000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 63, MaPhieuNhap = 26, MaHang = 53, SoLuongNhap = 50, DonGiaNhap = 580000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 64, MaPhieuNhap = 26, MaHang = 54, SoLuongNhap = 33, DonGiaNhap = 6200000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 65, MaPhieuNhap = 26, MaHang = 55, SoLuongNhap = 20, DonGiaNhap = 1850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 66, MaPhieuNhap = 27, MaHang = 55, SoLuongNhap = 60, DonGiaNhap = 1850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 67, MaPhieuNhap = 27, MaHang = 56, SoLuongNhap = 41, DonGiaNhap = 1750000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 68, MaPhieuNhap = 28, MaHang = 57, SoLuongNhap = 70, DonGiaNhap = 2650000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 69, MaPhieuNhap = 28, MaHang = 58, SoLuongNhap = 49, DonGiaNhap = 2350000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 70, MaPhieuNhap = 28, MaHang = 59, SoLuongNhap = 10, DonGiaNhap = 5600000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 71, MaPhieuNhap = 29, MaHang = 59, SoLuongNhap = 80, DonGiaNhap = 5600000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 72, MaPhieuNhap = 29, MaHang = 60, SoLuongNhap = 57, DonGiaNhap = 1150000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 73, MaPhieuNhap = 30, MaHang = 1, SoLuongNhap = 15, DonGiaNhap = 5450000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 74, MaPhieuNhap = 30, MaHang = 2, SoLuongNhap = 20, DonGiaNhap = 3200000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 75, MaPhieuNhap = 30, MaHang = 3, SoLuongNhap = 20, DonGiaNhap = 3850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 76, MaPhieuNhap = 31, MaHang = 3, SoLuongNhap = 20, DonGiaNhap = 3850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 77, MaPhieuNhap = 31, MaHang = 4, SoLuongNhap = 24, DonGiaNhap = 950000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 78, MaPhieuNhap = 32, MaHang = 5, SoLuongNhap = 25, DonGiaNhap = 135000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 79, MaPhieuNhap = 32, MaHang = 6, SoLuongNhap = 12, DonGiaNhap = 1450000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 80, MaPhieuNhap = 32, MaHang = 7, SoLuongNhap = 10, DonGiaNhap = 1250000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 81, MaPhieuNhap = 33, MaHang = 7, SoLuongNhap = 30, DonGiaNhap = 1250000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 82, MaPhieuNhap = 33, MaHang = 8, SoLuongNhap = 16, DonGiaNhap = 1950000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 83, MaPhieuNhap = 34, MaHang = 9, SoLuongNhap = 35, DonGiaNhap = 8200000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 84, MaPhieuNhap = 34, MaHang = 10, SoLuongNhap = 20, DonGiaNhap = 1850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 85, MaPhieuNhap = 34, MaHang = 11, SoLuongNhap = 20, DonGiaNhap = 115000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 86, MaPhieuNhap = 35, MaHang = 11, SoLuongNhap = 15, DonGiaNhap = 115000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 87, MaPhieuNhap = 35, MaHang = 12, SoLuongNhap = 24, DonGiaNhap = 1850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 88, MaPhieuNhap = 36, MaHang = 13, SoLuongNhap = 20, DonGiaNhap = 85000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 89, MaPhieuNhap = 36, MaHang = 14, SoLuongNhap = 12, DonGiaNhap = 22000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 90, MaPhieuNhap = 36, MaHang = 15, SoLuongNhap = 10, DonGiaNhap = 45000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 91, MaPhieuNhap = 37, MaHang = 15, SoLuongNhap = 25, DonGiaNhap = 45000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 92, MaPhieuNhap = 37, MaHang = 16, SoLuongNhap = 16, DonGiaNhap = 28000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 93, MaPhieuNhap = 38, MaHang = 17, SoLuongNhap = 30, DonGiaNhap = 18000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 94, MaPhieuNhap = 38, MaHang = 18, SoLuongNhap = 20, DonGiaNhap = 25000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 95, MaPhieuNhap = 38, MaHang = 19, SoLuongNhap = 20, DonGiaNhap = 12000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 96, MaPhieuNhap = 39, MaHang = 19, SoLuongNhap = 35, DonGiaNhap = 12000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 97, MaPhieuNhap = 39, MaHang = 20, SoLuongNhap = 24, DonGiaNhap = 55000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 98, MaPhieuNhap = 40, MaHang = 21, SoLuongNhap = 70, DonGiaNhap = 18500m },
                new ChiTietPhieuNhap { MaChiTietNhap = 99, MaPhieuNhap = 40, MaHang = 22, SoLuongNhap = 25, DonGiaNhap = 480000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 100, MaPhieuNhap = 40, MaHang = 23, SoLuongNhap = 10, DonGiaNhap = 920000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 101, MaPhieuNhap = 41, MaHang = 23, SoLuongNhap = 80, DonGiaNhap = 920000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 102, MaPhieuNhap = 41, MaHang = 24, SoLuongNhap = 33, DonGiaNhap = 295000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 103, MaPhieuNhap = 42, MaHang = 25, SoLuongNhap = 30, DonGiaNhap = 750000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 104, MaPhieuNhap = 42, MaHang = 26, SoLuongNhap = 41, DonGiaNhap = 320000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 105, MaPhieuNhap = 42, MaHang = 27, SoLuongNhap = 20, DonGiaNhap = 25000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 106, MaPhieuNhap = 43, MaHang = 27, SoLuongNhap = 40, DonGiaNhap = 25000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 107, MaPhieuNhap = 43, MaHang = 28, SoLuongNhap = 49, DonGiaNhap = 210000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 108, MaPhieuNhap = 44, MaHang = 29, SoLuongNhap = 50, DonGiaNhap = 1150000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 109, MaPhieuNhap = 44, MaHang = 30, SoLuongNhap = 57, DonGiaNhap = 1080000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 110, MaPhieuNhap = 44, MaHang = 31, SoLuongNhap = 10, DonGiaNhap = 2850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 111, MaPhieuNhap = 45, MaHang = 31, SoLuongNhap = 60, DonGiaNhap = 2850000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 112, MaPhieuNhap = 45, MaHang = 32, SoLuongNhap = 25, DonGiaNhap = 2450000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 113, MaPhieuNhap = 46, MaHang = 33, SoLuongNhap = 70, DonGiaNhap = 260000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 114, MaPhieuNhap = 46, MaHang = 34, SoLuongNhap = 33, DonGiaNhap = 1350000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 115, MaPhieuNhap = 46, MaHang = 35, SoLuongNhap = 20, DonGiaNhap = 1750000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 116, MaPhieuNhap = 47, MaHang = 35, SoLuongNhap = 80, DonGiaNhap = 1750000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 117, MaPhieuNhap = 47, MaHang = 36, SoLuongNhap = 41, DonGiaNhap = 65000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 118, MaPhieuNhap = 48, MaHang = 37, SoLuongNhap = 30, DonGiaNhap = 780000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 119, MaPhieuNhap = 48, MaHang = 38, SoLuongNhap = 49, DonGiaNhap = 620000m },
                new ChiTietPhieuNhap { MaChiTietNhap = 120, MaPhieuNhap = 48, MaHang = 39, SoLuongNhap = 10, DonGiaNhap = 385000m },
            };

            var phieuXuats = new List<PhieuXuat>
            {
                new PhieuXuat { MaPhieuXuat = 1, MaBoPhan = 1, MaKho = 1, NgayXuat = new DateTime(2026, 7, 15), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 201" },
                new PhieuXuat { MaPhieuXuat = 2, MaBoPhan = 2, MaKho = 2, NgayXuat = new DateTime(2026, 7, 17), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 202" },
                new PhieuXuat { MaPhieuXuat = 3, MaBoPhan = 3, MaKho = 3, NgayXuat = new DateTime(2026, 7, 19), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 203" },
                new PhieuXuat { MaPhieuXuat = 4, MaBoPhan = 4, MaKho = 4, NgayXuat = new DateTime(2026, 7, 21), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 204" },
                new PhieuXuat { MaPhieuXuat = 5, MaBoPhan = 5, MaKho = 1, NgayXuat = new DateTime(2026, 7, 23), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 205" },
                new PhieuXuat { MaPhieuXuat = 6, MaBoPhan = 6, MaKho = 2, NgayXuat = new DateTime(2026, 7, 25), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 206" },
                new PhieuXuat { MaPhieuXuat = 7, MaBoPhan = 7, MaKho = 3, NgayXuat = new DateTime(2026, 7, 27), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 207" },
                new PhieuXuat { MaPhieuXuat = 8, MaBoPhan = 8, MaKho = 4, NgayXuat = new DateTime(2026, 7, 29), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 208" },
                new PhieuXuat { MaPhieuXuat = 9, MaBoPhan = 9, MaKho = 1, NgayXuat = new DateTime(2026, 7, 31), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 209" },
                new PhieuXuat { MaPhieuXuat = 10, MaBoPhan = 10, MaKho = 2, NgayXuat = new DateTime(2026, 8, 2), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 210" },
                new PhieuXuat { MaPhieuXuat = 11, MaBoPhan = 1, MaKho = 3, NgayXuat = new DateTime(2026, 8, 4), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 211" },
                new PhieuXuat { MaPhieuXuat = 12, MaBoPhan = 2, MaKho = 4, NgayXuat = new DateTime(2026, 8, 6), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 212" },
                new PhieuXuat { MaPhieuXuat = 13, MaBoPhan = 3, MaKho = 1, NgayXuat = new DateTime(2026, 8, 8), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 213" },
                new PhieuXuat { MaPhieuXuat = 14, MaBoPhan = 4, MaKho = 2, NgayXuat = new DateTime(2026, 8, 10), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 214" },
                new PhieuXuat { MaPhieuXuat = 15, MaBoPhan = 5, MaKho = 3, NgayXuat = new DateTime(2026, 8, 12), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 215" },
                new PhieuXuat { MaPhieuXuat = 16, MaBoPhan = 6, MaKho = 4, NgayXuat = new DateTime(2026, 8, 14), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 216" },
                new PhieuXuat { MaPhieuXuat = 17, MaBoPhan = 7, MaKho = 1, NgayXuat = new DateTime(2026, 8, 16), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 217" },
                new PhieuXuat { MaPhieuXuat = 18, MaBoPhan = 8, MaKho = 2, NgayXuat = new DateTime(2026, 8, 18), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 218" },
                new PhieuXuat { MaPhieuXuat = 19, MaBoPhan = 9, MaKho = 3, NgayXuat = new DateTime(2026, 8, 20), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 219" },
                new PhieuXuat { MaPhieuXuat = 20, MaBoPhan = 10, MaKho = 4, NgayXuat = new DateTime(2026, 8, 22), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 220" },
                new PhieuXuat { MaPhieuXuat = 21, MaBoPhan = 1, MaKho = 1, NgayXuat = new DateTime(2026, 8, 24), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 221" },
                new PhieuXuat { MaPhieuXuat = 22, MaBoPhan = 2, MaKho = 2, NgayXuat = new DateTime(2026, 8, 26), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 222" },
                new PhieuXuat { MaPhieuXuat = 23, MaBoPhan = 3, MaKho = 3, NgayXuat = new DateTime(2026, 8, 28), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 223" },
                new PhieuXuat { MaPhieuXuat = 24, MaBoPhan = 4, MaKho = 4, NgayXuat = new DateTime(2026, 8, 30), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 224" },
                new PhieuXuat { MaPhieuXuat = 25, MaBoPhan = 5, MaKho = 1, NgayXuat = new DateTime(2026, 9, 1), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 225" },
                new PhieuXuat { MaPhieuXuat = 26, MaBoPhan = 6, MaKho = 2, NgayXuat = new DateTime(2026, 9, 3), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 226" },
                new PhieuXuat { MaPhieuXuat = 27, MaBoPhan = 7, MaKho = 3, NgayXuat = new DateTime(2026, 9, 5), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 227" },
                new PhieuXuat { MaPhieuXuat = 28, MaBoPhan = 8, MaKho = 4, NgayXuat = new DateTime(2026, 9, 7), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 228" },
                new PhieuXuat { MaPhieuXuat = 29, MaBoPhan = 9, MaKho = 1, NgayXuat = new DateTime(2026, 9, 9), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 229" },
                new PhieuXuat { MaPhieuXuat = 30, MaBoPhan = 10, MaKho = 2, NgayXuat = new DateTime(2026, 9, 11), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 230" },
                new PhieuXuat { MaPhieuXuat = 31, MaBoPhan = 1, MaKho = 3, NgayXuat = new DateTime(2026, 9, 13), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 231" },
                new PhieuXuat { MaPhieuXuat = 32, MaBoPhan = 2, MaKho = 4, NgayXuat = new DateTime(2026, 9, 15), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 232" },
                new PhieuXuat { MaPhieuXuat = 33, MaBoPhan = 3, MaKho = 1, NgayXuat = new DateTime(2026, 9, 17), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)2, GhiChu = "Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số 233" },
                new PhieuXuat { MaPhieuXuat = 34, MaBoPhan = 4, MaKho = 2, NgayXuat = new DateTime(2026, 9, 19), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)0, GhiChu = "Lập phiếu xuất dự kiến PX-2026-034" },
                new PhieuXuat { MaPhieuXuat = 35, MaBoPhan = 5, MaKho = 3, NgayXuat = new DateTime(2026, 9, 21), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)0, GhiChu = "Lập phiếu xuất dự kiến PX-2026-035" },
                new PhieuXuat { MaPhieuXuat = 36, MaBoPhan = 6, MaKho = 4, NgayXuat = new DateTime(2026, 9, 23), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)0, GhiChu = "Lập phiếu xuất dự kiến PX-2026-036" },
                new PhieuXuat { MaPhieuXuat = 37, MaBoPhan = 7, MaKho = 1, NgayXuat = new DateTime(2026, 9, 25), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)0, GhiChu = "Lập phiếu xuất dự kiến PX-2026-037" },
                new PhieuXuat { MaPhieuXuat = 38, MaBoPhan = 8, MaKho = 2, NgayXuat = new DateTime(2026, 9, 27), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)0, GhiChu = "Lập phiếu xuất dự kiến PX-2026-038" },
                new PhieuXuat { MaPhieuXuat = 39, MaBoPhan = 9, MaKho = 3, NgayXuat = new DateTime(2026, 9, 29), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)1, GhiChu = "Phiếu PX-2026-039 chờ trưởng bộ phận ký biên bản nhận" },
                new PhieuXuat { MaPhieuXuat = 40, MaBoPhan = 10, MaKho = 4, NgayXuat = new DateTime(2026, 10, 1), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)1, GhiChu = "Phiếu PX-2026-040 chờ trưởng bộ phận ký biên bản nhận" },
                new PhieuXuat { MaPhieuXuat = 41, MaBoPhan = 1, MaKho = 1, NgayXuat = new DateTime(2026, 10, 3), NguoiLap = "Lê Văn Hùng", TrangThai = (TrangThaiPhieuXuat)1, GhiChu = "Phiếu PX-2026-041 chờ trưởng bộ phận ký biên bản nhận" },
                new PhieuXuat { MaPhieuXuat = 42, MaBoPhan = 2, MaKho = 2, NgayXuat = new DateTime(2026, 10, 5), NguoiLap = "Nguyễn Thị Cúc", TrangThai = (TrangThaiPhieuXuat)1, GhiChu = "Phiếu PX-2026-042 chờ trưởng bộ phận ký biên bản nhận" },
                new PhieuXuat { MaPhieuXuat = 43, MaBoPhan = 3, MaKho = 3, NgayXuat = new DateTime(2026, 10, 7), NguoiLap = "Nguyễn Văn Cường", TrangThai = (TrangThaiPhieuXuat)3, GhiChu = "Phiếu PX-2026-043 hủy do bộ phận thay đổi kế hoạch" },
                new PhieuXuat { MaPhieuXuat = 44, MaBoPhan = 4, MaKho = 4, NgayXuat = new DateTime(2026, 10, 9), NguoiLap = "Trần Xuân Hải", TrangThai = (TrangThaiPhieuXuat)3, GhiChu = "Phiếu PX-2026-044 hủy do bộ phận thay đổi kế hoạch" },
                new PhieuXuat { MaPhieuXuat = 45, MaBoPhan = 5, MaKho = 1, NgayXuat = new DateTime(2026, 10, 11), NguoiLap = "Bùi Thị Mai", TrangThai = (TrangThaiPhieuXuat)3, GhiChu = "Phiếu PX-2026-045 hủy do bộ phận thay đổi kế hoạch" },
            };

            var chiTietXuats = new List<ChiTietPhieuXuat>
            {
                new ChiTietPhieuXuat { MaChiTietXuat = 1, MaPhieuXuat = 1, MaHang = 3, SoLuongXuat = 2, DonGiaXuatThamChieu = 4042500m, GhiChu = "Xuất phục vụ thực hành cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new ChiTietPhieuXuat { MaChiTietXuat = 2, MaPhieuXuat = 2, MaHang = 5, SoLuongXuat = 2, DonGiaXuatThamChieu = 141750m, GhiChu = "Xuất phục vụ thực hành cho Xưởng Thực hành Điện - Tự động hóa" },
                new ChiTietPhieuXuat { MaChiTietXuat = 3, MaPhieuXuat = 2, MaHang = 6, SoLuongXuat = 3, DonGiaXuatThamChieu = 1522500m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 4, MaPhieuXuat = 3, MaHang = 7, SoLuongXuat = 2, DonGiaXuatThamChieu = 1312500m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Dữ liệu & Viễn thông" },
                new ChiTietPhieuXuat { MaChiTietXuat = 5, MaPhieuXuat = 4, MaHang = 9, SoLuongXuat = 4, DonGiaXuatThamChieu = 8610000m, GhiChu = "Xuất phục vụ thực hành cho Phòng Đào tạo & Khảo thí" },
                new ChiTietPhieuXuat { MaChiTietXuat = 6, MaPhieuXuat = 4, MaHang = 10, SoLuongXuat = 3, DonGiaXuatThamChieu = 1942500m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 7, MaPhieuXuat = 5, MaHang = 11, SoLuongXuat = 2, DonGiaXuatThamChieu = 120750m, GhiChu = "Xuất phục vụ thực hành cho Phòng Tài chính - Kế toán" },
                new ChiTietPhieuXuat { MaChiTietXuat = 8, MaPhieuXuat = 6, MaHang = 13, SoLuongXuat = 2, DonGiaXuatThamChieu = 89250m, GhiChu = "Xuất phục vụ thực hành cho Phòng Hành chính - Tổng vụ" },
                new ChiTietPhieuXuat { MaChiTietXuat = 9, MaPhieuXuat = 6, MaHang = 14, SoLuongXuat = 3, DonGiaXuatThamChieu = 23100m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 10, MaPhieuXuat = 7, MaHang = 15, SoLuongXuat = 2, DonGiaXuatThamChieu = 47250m, GhiChu = "Xuất phục vụ thực hành cho Khoa Công nghệ Thông tin (Văn phòng Khoa)" },
                new ChiTietPhieuXuat { MaChiTietXuat = 11, MaPhieuXuat = 8, MaHang = 17, SoLuongXuat = 4, DonGiaXuatThamChieu = 18900m, GhiChu = "Xuất phục vụ thực hành cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new ChiTietPhieuXuat { MaChiTietXuat = 12, MaPhieuXuat = 8, MaHang = 18, SoLuongXuat = 3, DonGiaXuatThamChieu = 26250m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 13, MaPhieuXuat = 9, MaHang = 19, SoLuongXuat = 2, DonGiaXuatThamChieu = 12600m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Thí nghiệm & Chuyển giao Công nghệ" },
                new ChiTietPhieuXuat { MaChiTietXuat = 14, MaPhieuXuat = 10, MaHang = 21, SoLuongXuat = 2, DonGiaXuatThamChieu = 19425m, GhiChu = "Xuất phục vụ thực hành cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new ChiTietPhieuXuat { MaChiTietXuat = 15, MaPhieuXuat = 10, MaHang = 22, SoLuongXuat = 3, DonGiaXuatThamChieu = 504000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 16, MaPhieuXuat = 11, MaHang = 23, SoLuongXuat = 2, DonGiaXuatThamChieu = 966000m, GhiChu = "Xuất phục vụ thực hành cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new ChiTietPhieuXuat { MaChiTietXuat = 17, MaPhieuXuat = 12, MaHang = 25, SoLuongXuat = 4, DonGiaXuatThamChieu = 787500m, GhiChu = "Xuất phục vụ thực hành cho Xưởng Thực hành Điện - Tự động hóa" },
                new ChiTietPhieuXuat { MaChiTietXuat = 18, MaPhieuXuat = 12, MaHang = 26, SoLuongXuat = 3, DonGiaXuatThamChieu = 336000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 19, MaPhieuXuat = 13, MaHang = 27, SoLuongXuat = 2, DonGiaXuatThamChieu = 26250m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Dữ liệu & Viễn thông" },
                new ChiTietPhieuXuat { MaChiTietXuat = 20, MaPhieuXuat = 14, MaHang = 29, SoLuongXuat = 2, DonGiaXuatThamChieu = 1207500m, GhiChu = "Xuất phục vụ thực hành cho Phòng Đào tạo & Khảo thí" },
                new ChiTietPhieuXuat { MaChiTietXuat = 21, MaPhieuXuat = 14, MaHang = 30, SoLuongXuat = 3, DonGiaXuatThamChieu = 1134000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 22, MaPhieuXuat = 15, MaHang = 31, SoLuongXuat = 2, DonGiaXuatThamChieu = 2992500m, GhiChu = "Xuất phục vụ thực hành cho Phòng Tài chính - Kế toán" },
                new ChiTietPhieuXuat { MaChiTietXuat = 23, MaPhieuXuat = 16, MaHang = 33, SoLuongXuat = 4, DonGiaXuatThamChieu = 273000m, GhiChu = "Xuất phục vụ thực hành cho Phòng Hành chính - Tổng vụ" },
                new ChiTietPhieuXuat { MaChiTietXuat = 24, MaPhieuXuat = 16, MaHang = 34, SoLuongXuat = 3, DonGiaXuatThamChieu = 1417500m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 25, MaPhieuXuat = 17, MaHang = 35, SoLuongXuat = 2, DonGiaXuatThamChieu = 1837500m, GhiChu = "Xuất phục vụ thực hành cho Khoa Công nghệ Thông tin (Văn phòng Khoa)" },
                new ChiTietPhieuXuat { MaChiTietXuat = 26, MaPhieuXuat = 18, MaHang = 37, SoLuongXuat = 2, DonGiaXuatThamChieu = 819000m, GhiChu = "Xuất phục vụ thực hành cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new ChiTietPhieuXuat { MaChiTietXuat = 27, MaPhieuXuat = 18, MaHang = 38, SoLuongXuat = 3, DonGiaXuatThamChieu = 651000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 28, MaPhieuXuat = 19, MaHang = 39, SoLuongXuat = 2, DonGiaXuatThamChieu = 404250m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Thí nghiệm & Chuyển giao Công nghệ" },
                new ChiTietPhieuXuat { MaChiTietXuat = 29, MaPhieuXuat = 20, MaHang = 41, SoLuongXuat = 4, DonGiaXuatThamChieu = 787500m, GhiChu = "Xuất phục vụ thực hành cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new ChiTietPhieuXuat { MaChiTietXuat = 30, MaPhieuXuat = 20, MaHang = 42, SoLuongXuat = 3, DonGiaXuatThamChieu = 78750m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 31, MaPhieuXuat = 21, MaHang = 43, SoLuongXuat = 2, DonGiaXuatThamChieu = 33600m, GhiChu = "Xuất phục vụ thực hành cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new ChiTietPhieuXuat { MaChiTietXuat = 32, MaPhieuXuat = 22, MaHang = 45, SoLuongXuat = 2, DonGiaXuatThamChieu = 252000m, GhiChu = "Xuất phục vụ thực hành cho Xưởng Thực hành Điện - Tự động hóa" },
                new ChiTietPhieuXuat { MaChiTietXuat = 33, MaPhieuXuat = 22, MaHang = 46, SoLuongXuat = 3, DonGiaXuatThamChieu = 71400m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 34, MaPhieuXuat = 23, MaHang = 47, SoLuongXuat = 2, DonGiaXuatThamChieu = 14910000m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Dữ liệu & Viễn thông" },
                new ChiTietPhieuXuat { MaChiTietXuat = 35, MaPhieuXuat = 24, MaHang = 49, SoLuongXuat = 4, DonGiaXuatThamChieu = 7140000m, GhiChu = "Xuất phục vụ thực hành cho Phòng Đào tạo & Khảo thí" },
                new ChiTietPhieuXuat { MaChiTietXuat = 36, MaPhieuXuat = 24, MaHang = 50, SoLuongXuat = 3, DonGiaXuatThamChieu = 8925000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 37, MaPhieuXuat = 25, MaHang = 51, SoLuongXuat = 2, DonGiaXuatThamChieu = 441000m, GhiChu = "Xuất phục vụ thực hành cho Phòng Tài chính - Kế toán" },
                new ChiTietPhieuXuat { MaChiTietXuat = 38, MaPhieuXuat = 26, MaHang = 53, SoLuongXuat = 2, DonGiaXuatThamChieu = 609000m, GhiChu = "Xuất phục vụ thực hành cho Phòng Hành chính - Tổng vụ" },
                new ChiTietPhieuXuat { MaChiTietXuat = 39, MaPhieuXuat = 26, MaHang = 54, SoLuongXuat = 3, DonGiaXuatThamChieu = 6510000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 40, MaPhieuXuat = 27, MaHang = 55, SoLuongXuat = 2, DonGiaXuatThamChieu = 1942500m, GhiChu = "Xuất phục vụ thực hành cho Khoa Công nghệ Thông tin (Văn phòng Khoa)" },
                new ChiTietPhieuXuat { MaChiTietXuat = 41, MaPhieuXuat = 28, MaHang = 57, SoLuongXuat = 4, DonGiaXuatThamChieu = 2782500m, GhiChu = "Xuất phục vụ thực hành cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new ChiTietPhieuXuat { MaChiTietXuat = 42, MaPhieuXuat = 28, MaHang = 58, SoLuongXuat = 3, DonGiaXuatThamChieu = 2467500m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 43, MaPhieuXuat = 29, MaHang = 59, SoLuongXuat = 2, DonGiaXuatThamChieu = 5880000m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Thí nghiệm & Chuyển giao Công nghệ" },
                new ChiTietPhieuXuat { MaChiTietXuat = 44, MaPhieuXuat = 30, MaHang = 1, SoLuongXuat = 2, DonGiaXuatThamChieu = 5722500m, GhiChu = "Xuất phục vụ thực hành cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new ChiTietPhieuXuat { MaChiTietXuat = 45, MaPhieuXuat = 30, MaHang = 2, SoLuongXuat = 3, DonGiaXuatThamChieu = 3360000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 46, MaPhieuXuat = 31, MaHang = 3, SoLuongXuat = 2, DonGiaXuatThamChieu = 4042500m, GhiChu = "Xuất phục vụ thực hành cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new ChiTietPhieuXuat { MaChiTietXuat = 47, MaPhieuXuat = 32, MaHang = 5, SoLuongXuat = 4, DonGiaXuatThamChieu = 141750m, GhiChu = "Xuất phục vụ thực hành cho Xưởng Thực hành Điện - Tự động hóa" },
                new ChiTietPhieuXuat { MaChiTietXuat = 48, MaPhieuXuat = 32, MaHang = 6, SoLuongXuat = 3, DonGiaXuatThamChieu = 1522500m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 49, MaPhieuXuat = 33, MaHang = 7, SoLuongXuat = 2, DonGiaXuatThamChieu = 1312500m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Dữ liệu & Viễn thông" },
                new ChiTietPhieuXuat { MaChiTietXuat = 50, MaPhieuXuat = 34, MaHang = 9, SoLuongXuat = 2, DonGiaXuatThamChieu = 8610000m, GhiChu = "Xuất phục vụ thực hành cho Phòng Đào tạo & Khảo thí" },
                new ChiTietPhieuXuat { MaChiTietXuat = 51, MaPhieuXuat = 34, MaHang = 10, SoLuongXuat = 3, DonGiaXuatThamChieu = 1942500m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 52, MaPhieuXuat = 35, MaHang = 11, SoLuongXuat = 2, DonGiaXuatThamChieu = 120750m, GhiChu = "Xuất phục vụ thực hành cho Phòng Tài chính - Kế toán" },
                new ChiTietPhieuXuat { MaChiTietXuat = 53, MaPhieuXuat = 36, MaHang = 13, SoLuongXuat = 4, DonGiaXuatThamChieu = 89250m, GhiChu = "Xuất phục vụ thực hành cho Phòng Hành chính - Tổng vụ" },
                new ChiTietPhieuXuat { MaChiTietXuat = 54, MaPhieuXuat = 36, MaHang = 14, SoLuongXuat = 3, DonGiaXuatThamChieu = 23100m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 55, MaPhieuXuat = 37, MaHang = 15, SoLuongXuat = 2, DonGiaXuatThamChieu = 47250m, GhiChu = "Xuất phục vụ thực hành cho Khoa Công nghệ Thông tin (Văn phòng Khoa)" },
                new ChiTietPhieuXuat { MaChiTietXuat = 56, MaPhieuXuat = 38, MaHang = 17, SoLuongXuat = 2, DonGiaXuatThamChieu = 18900m, GhiChu = "Xuất phục vụ thực hành cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new ChiTietPhieuXuat { MaChiTietXuat = 57, MaPhieuXuat = 38, MaHang = 18, SoLuongXuat = 3, DonGiaXuatThamChieu = 26250m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 58, MaPhieuXuat = 39, MaHang = 19, SoLuongXuat = 2, DonGiaXuatThamChieu = 12600m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Thí nghiệm & Chuyển giao Công nghệ" },
                new ChiTietPhieuXuat { MaChiTietXuat = 59, MaPhieuXuat = 40, MaHang = 21, SoLuongXuat = 4, DonGiaXuatThamChieu = 19425m, GhiChu = "Xuất phục vụ thực hành cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new ChiTietPhieuXuat { MaChiTietXuat = 60, MaPhieuXuat = 40, MaHang = 22, SoLuongXuat = 3, DonGiaXuatThamChieu = 504000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 61, MaPhieuXuat = 41, MaHang = 23, SoLuongXuat = 2, DonGiaXuatThamChieu = 966000m, GhiChu = "Xuất phục vụ thực hành cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new ChiTietPhieuXuat { MaChiTietXuat = 62, MaPhieuXuat = 42, MaHang = 25, SoLuongXuat = 2, DonGiaXuatThamChieu = 787500m, GhiChu = "Xuất phục vụ thực hành cho Xưởng Thực hành Điện - Tự động hóa" },
                new ChiTietPhieuXuat { MaChiTietXuat = 63, MaPhieuXuat = 42, MaHang = 26, SoLuongXuat = 3, DonGiaXuatThamChieu = 336000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 64, MaPhieuXuat = 43, MaHang = 27, SoLuongXuat = 2, DonGiaXuatThamChieu = 26250m, GhiChu = "Xuất phục vụ thực hành cho Trung tâm Dữ liệu & Viễn thông" },
                new ChiTietPhieuXuat { MaChiTietXuat = 65, MaPhieuXuat = 44, MaHang = 29, SoLuongXuat = 4, DonGiaXuatThamChieu = 1207500m, GhiChu = "Xuất phục vụ thực hành cho Phòng Đào tạo & Khảo thí" },
                new ChiTietPhieuXuat { MaChiTietXuat = 66, MaPhieuXuat = 44, MaHang = 30, SoLuongXuat = 3, DonGiaXuatThamChieu = 1134000m, GhiChu = "Xuất thiết bị dự phòng thay thế" },
                new ChiTietPhieuXuat { MaChiTietXuat = 67, MaPhieuXuat = 45, MaHang = 31, SoLuongXuat = 2, DonGiaXuatThamChieu = 2992500m, GhiChu = "Xuất phục vụ thực hành cho Phòng Tài chính - Kế toán" },
            };

            var tonKhoes = new List<TonKho>
            {
                new TonKho { MaKho = 1, MaHang = 1, SoLuongTon = 2, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 2, SoLuongTon = 3, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 3, SoLuongTon = 1, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 4, SoLuongTon = 16, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 5, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 6, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 7, SoLuongTon = 28, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 8, SoLuongTon = 2, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 9, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 10, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 11, SoLuongTon = 13, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 12, SoLuongTon = 3, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 13, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 14, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 15, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 16, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 17, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 18, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 19, SoLuongTon = 33, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 20, SoLuongTon = 16, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 21, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 22, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 23, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 24, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 25, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 26, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 27, SoLuongTon = 38, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 28, SoLuongTon = 49, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 29, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 30, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 31, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 32, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 33, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 34, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 35, SoLuongTon = 78, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 36, SoLuongTon = 41, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 37, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 38, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 39, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 40, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 41, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 42, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 43, SoLuongTon = 58, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 44, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 45, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 46, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 47, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 48, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 49, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 50, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 51, SoLuongTon = 38, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 52, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 53, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 54, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 55, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 56, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 57, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 58, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 59, SoLuongTon = 78, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 1, MaHang = 60, SoLuongTon = 57, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 1, SoLuongTon = 13, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 2, SoLuongTon = 17, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 3, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 4, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 5, SoLuongTon = 23, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 6, SoLuongTon = 17, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 7, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 8, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 9, SoLuongTon = 35, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 10, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 11, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 12, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 13, SoLuongTon = 18, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 14, SoLuongTon = 17, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 15, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 16, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 17, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 18, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 19, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 20, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 21, SoLuongTon = 68, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 22, SoLuongTon = 22, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 23, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 24, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 25, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 26, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 27, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 28, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 29, SoLuongTon = 48, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 30, SoLuongTon = 54, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 31, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 32, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 33, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 34, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 35, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 36, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 37, SoLuongTon = 28, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 38, SoLuongTon = 46, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 39, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 40, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 41, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 42, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 43, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 44, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 45, SoLuongTon = 68, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 46, SoLuongTon = 38, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 47, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 48, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 49, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 50, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 51, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 52, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 53, SoLuongTon = 48, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 54, SoLuongTon = 30, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 55, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 56, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 57, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 58, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 59, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 2, MaHang = 60, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 1, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 2, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 3, SoLuongTon = 18, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 4, SoLuongTon = 24, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 5, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 6, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 7, SoLuongTon = 28, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 8, SoLuongTon = 24, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 9, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 10, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 11, SoLuongTon = 15, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 12, SoLuongTon = 24, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 13, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 14, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 15, SoLuongTon = 23, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 16, SoLuongTon = 24, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 17, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 18, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 19, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 20, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 21, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 22, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 23, SoLuongTon = 78, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 24, SoLuongTon = 33, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 25, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 26, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 27, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 28, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 29, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 30, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 31, SoLuongTon = 58, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 32, SoLuongTon = 25, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 33, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 34, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 35, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 36, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 37, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 38, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 39, SoLuongTon = 38, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 40, SoLuongTon = 57, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 41, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 42, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 43, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 44, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 45, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 46, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 47, SoLuongTon = 78, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 48, SoLuongTon = 49, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 49, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 50, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 51, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 52, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 53, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 54, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 55, SoLuongTon = 58, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 56, SoLuongTon = 41, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 57, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 58, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 59, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 3, MaHang = 60, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 1, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 2, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 3, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 4, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 5, SoLuongTon = 21, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 6, SoLuongTon = 9, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 7, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 8, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 9, SoLuongTon = 31, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 10, SoLuongTon = 9, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 11, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 12, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 13, SoLuongTon = 20, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 14, SoLuongTon = 12, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 15, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 16, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 17, SoLuongTon = 26, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 18, SoLuongTon = 9, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 19, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 20, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 21, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 22, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 23, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 24, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 25, SoLuongTon = 26, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 26, SoLuongTon = 38, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 27, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 28, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 29, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 30, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 31, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 32, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 33, SoLuongTon = 66, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 34, SoLuongTon = 30, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 35, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 36, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 37, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 38, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 39, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 40, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 41, SoLuongTon = 46, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 42, SoLuongTon = 22, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 43, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 44, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 45, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 46, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 47, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 48, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 49, SoLuongTon = 26, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 50, SoLuongTon = 54, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 51, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 52, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 53, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 54, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 55, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 56, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 57, SoLuongTon = 66, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 58, SoLuongTon = 46, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 59, SoLuongTon = 10, NgayCapNhat = new DateTime(2026, 10, 14) },
                new TonKho { MaKho = 4, MaHang = 60, SoLuongTon = 0, NgayCapNhat = new DateTime(2026, 10, 14) },
            };

            var lichSuList = new List<LichSuTonKho>
            {
                new LichSuTonKho { MaLichSu = 1, MaKho = 1, MaHang = 3, NgayPhatSinh = new DateTime(2026, 7, 1), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0001", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 2, MaKho = 1, MaHang = 4, NgayPhatSinh = new DateTime(2026, 7, 1), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0001", SoLuong = 16, TonSauGiaoDich = 16, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 3, MaKho = 2, MaHang = 5, NgayPhatSinh = new DateTime(2026, 7, 3), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0002", SoLuong = 25, TonSauGiaoDich = 25, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 4, MaKho = 2, MaHang = 6, NgayPhatSinh = new DateTime(2026, 7, 3), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0002", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 5, MaKho = 2, MaHang = 7, NgayPhatSinh = new DateTime(2026, 7, 3), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0002", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 6, MaKho = 3, MaHang = 7, NgayPhatSinh = new DateTime(2026, 7, 5), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0003", SoLuong = 30, TonSauGiaoDich = 30, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 7, MaKho = 3, MaHang = 8, NgayPhatSinh = new DateTime(2026, 7, 5), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0003", SoLuong = 24, TonSauGiaoDich = 24, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 8, MaKho = 4, MaHang = 9, NgayPhatSinh = new DateTime(2026, 7, 7), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0004", SoLuong = 35, TonSauGiaoDich = 35, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty Thiết bị Điện & Cơ khí Cadisun" },
                new LichSuTonKho { MaLichSu = 9, MaKho = 4, MaHang = 10, NgayPhatSinh = new DateTime(2026, 7, 7), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0004", SoLuong = 12, TonSauGiaoDich = 12, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty Thiết bị Điện & Cơ khí Cadisun" },
                new LichSuTonKho { MaLichSu = 10, MaKho = 4, MaHang = 11, NgayPhatSinh = new DateTime(2026, 7, 7), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0004", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty Thiết bị Điện & Cơ khí Cadisun" },
                new LichSuTonKho { MaLichSu = 11, MaKho = 1, MaHang = 11, NgayPhatSinh = new DateTime(2026, 7, 9), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0005", SoLuong = 15, TonSauGiaoDich = 15, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty TNHH Linh Kiện Điện Tử Minh Hà" },
                new LichSuTonKho { MaLichSu = 12, MaKho = 1, MaHang = 12, NgayPhatSinh = new DateTime(2026, 7, 9), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0005", SoLuong = 16, TonSauGiaoDich = 16, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty TNHH Linh Kiện Điện Tử Minh Hà" },
                new LichSuTonKho { MaLichSu = 13, MaKho = 2, MaHang = 13, NgayPhatSinh = new DateTime(2026, 7, 11), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0006", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty Cổ phần Công nghệ Mạng Phúc Anh" },
                new LichSuTonKho { MaLichSu = 14, MaKho = 2, MaHang = 14, NgayPhatSinh = new DateTime(2026, 7, 11), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0006", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty Cổ phần Công nghệ Mạng Phúc Anh" },
                new LichSuTonKho { MaLichSu = 15, MaKho = 2, MaHang = 15, NgayPhatSinh = new DateTime(2026, 7, 11), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0006", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty Cổ phần Công nghệ Mạng Phúc Anh" },
                new LichSuTonKho { MaLichSu = 16, MaKho = 3, MaHang = 15, NgayPhatSinh = new DateTime(2026, 7, 13), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0007", SoLuong = 25, TonSauGiaoDich = 25, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam" },
                new LichSuTonKho { MaLichSu = 17, MaKho = 3, MaHang = 16, NgayPhatSinh = new DateTime(2026, 7, 13), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0007", SoLuong = 24, TonSauGiaoDich = 24, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam" },
                new LichSuTonKho { MaLichSu = 18, MaKho = 4, MaHang = 17, NgayPhatSinh = new DateTime(2026, 7, 15), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0008", SoLuong = 30, TonSauGiaoDich = 30, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty Cổ phần Văn phòng phẩm Hồng Hà" },
                new LichSuTonKho { MaLichSu = 19, MaKho = 4, MaHang = 18, NgayPhatSinh = new DateTime(2026, 7, 15), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0008", SoLuong = 12, TonSauGiaoDich = 12, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty Cổ phần Văn phòng phẩm Hồng Hà" },
                new LichSuTonKho { MaLichSu = 20, MaKho = 4, MaHang = 19, NgayPhatSinh = new DateTime(2026, 7, 15), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0008", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty Cổ phần Văn phòng phẩm Hồng Hà" },
                new LichSuTonKho { MaLichSu = 21, MaKho = 1, MaHang = 19, NgayPhatSinh = new DateTime(2026, 7, 17), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0009", SoLuong = 35, TonSauGiaoDich = 35, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty TNHH Thương mại Quốc tế Bách Khoa" },
                new LichSuTonKho { MaLichSu = 22, MaKho = 1, MaHang = 20, NgayPhatSinh = new DateTime(2026, 7, 17), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0009", SoLuong = 16, TonSauGiaoDich = 16, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty TNHH Thương mại Quốc tế Bách Khoa" },
                new LichSuTonKho { MaLichSu = 23, MaKho = 2, MaHang = 21, NgayPhatSinh = new DateTime(2026, 7, 19), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0010", SoLuong = 70, TonSauGiaoDich = 70, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt" },
                new LichSuTonKho { MaLichSu = 24, MaKho = 2, MaHang = 22, NgayPhatSinh = new DateTime(2026, 7, 19), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0010", SoLuong = 25, TonSauGiaoDich = 25, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt" },
                new LichSuTonKho { MaLichSu = 25, MaKho = 2, MaHang = 23, NgayPhatSinh = new DateTime(2026, 7, 19), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0010", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt" },
                new LichSuTonKho { MaLichSu = 26, MaKho = 3, MaHang = 23, NgayPhatSinh = new DateTime(2026, 7, 22), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0011", SoLuong = 80, TonSauGiaoDich = 80, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Giải pháp Viễn thông Sao Mai" },
                new LichSuTonKho { MaLichSu = 27, MaKho = 3, MaHang = 24, NgayPhatSinh = new DateTime(2026, 7, 22), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0011", SoLuong = 33, TonSauGiaoDich = 33, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Giải pháp Viễn thông Sao Mai" },
                new LichSuTonKho { MaLichSu = 28, MaKho = 4, MaHang = 25, NgayPhatSinh = new DateTime(2026, 7, 24), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0012", SoLuong = 30, TonSauGiaoDich = 30, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 29, MaKho = 4, MaHang = 26, NgayPhatSinh = new DateTime(2026, 7, 24), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0012", SoLuong = 41, TonSauGiaoDich = 41, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 30, MaKho = 4, MaHang = 27, NgayPhatSinh = new DateTime(2026, 7, 24), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0012", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 31, MaKho = 1, MaHang = 27, NgayPhatSinh = new DateTime(2026, 7, 26), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0013", SoLuong = 40, TonSauGiaoDich = 40, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 32, MaKho = 1, MaHang = 28, NgayPhatSinh = new DateTime(2026, 7, 26), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0013", SoLuong = 49, TonSauGiaoDich = 49, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 33, MaKho = 2, MaHang = 29, NgayPhatSinh = new DateTime(2026, 7, 28), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0014", SoLuong = 50, TonSauGiaoDich = 50, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 34, MaKho = 2, MaHang = 30, NgayPhatSinh = new DateTime(2026, 7, 28), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0014", SoLuong = 57, TonSauGiaoDich = 57, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 35, MaKho = 2, MaHang = 31, NgayPhatSinh = new DateTime(2026, 7, 28), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0014", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 36, MaKho = 3, MaHang = 31, NgayPhatSinh = new DateTime(2026, 7, 30), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0015", SoLuong = 60, TonSauGiaoDich = 60, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Thiết bị Điện & Cơ khí Cadisun" },
                new LichSuTonKho { MaLichSu = 37, MaKho = 3, MaHang = 32, NgayPhatSinh = new DateTime(2026, 7, 30), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0015", SoLuong = 25, TonSauGiaoDich = 25, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Thiết bị Điện & Cơ khí Cadisun" },
                new LichSuTonKho { MaLichSu = 38, MaKho = 4, MaHang = 33, NgayPhatSinh = new DateTime(2026, 8, 1), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0016", SoLuong = 70, TonSauGiaoDich = 70, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Linh Kiện Điện Tử Minh Hà" },
                new LichSuTonKho { MaLichSu = 39, MaKho = 4, MaHang = 34, NgayPhatSinh = new DateTime(2026, 8, 1), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0016", SoLuong = 33, TonSauGiaoDich = 33, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Linh Kiện Điện Tử Minh Hà" },
                new LichSuTonKho { MaLichSu = 40, MaKho = 4, MaHang = 35, NgayPhatSinh = new DateTime(2026, 8, 1), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0016", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Linh Kiện Điện Tử Minh Hà" },
                new LichSuTonKho { MaLichSu = 41, MaKho = 1, MaHang = 35, NgayPhatSinh = new DateTime(2026, 8, 3), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0017", SoLuong = 80, TonSauGiaoDich = 80, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty Cổ phần Công nghệ Mạng Phúc Anh" },
                new LichSuTonKho { MaLichSu = 42, MaKho = 1, MaHang = 36, NgayPhatSinh = new DateTime(2026, 8, 3), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0017", SoLuong = 41, TonSauGiaoDich = 41, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty Cổ phần Công nghệ Mạng Phúc Anh" },
                new LichSuTonKho { MaLichSu = 43, MaKho = 2, MaHang = 37, NgayPhatSinh = new DateTime(2026, 8, 5), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0018", SoLuong = 30, TonSauGiaoDich = 30, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam" },
                new LichSuTonKho { MaLichSu = 44, MaKho = 2, MaHang = 38, NgayPhatSinh = new DateTime(2026, 8, 5), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0018", SoLuong = 49, TonSauGiaoDich = 49, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam" },
                new LichSuTonKho { MaLichSu = 45, MaKho = 2, MaHang = 39, NgayPhatSinh = new DateTime(2026, 8, 5), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0018", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam" },
                new LichSuTonKho { MaLichSu = 46, MaKho = 3, MaHang = 39, NgayPhatSinh = new DateTime(2026, 8, 7), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0019", SoLuong = 40, TonSauGiaoDich = 40, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty Cổ phần Văn phòng phẩm Hồng Hà" },
                new LichSuTonKho { MaLichSu = 47, MaKho = 3, MaHang = 40, NgayPhatSinh = new DateTime(2026, 8, 7), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0019", SoLuong = 57, TonSauGiaoDich = 57, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty Cổ phần Văn phòng phẩm Hồng Hà" },
                new LichSuTonKho { MaLichSu = 48, MaKho = 4, MaHang = 41, NgayPhatSinh = new DateTime(2026, 8, 9), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0020", SoLuong = 50, TonSauGiaoDich = 50, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty TNHH Thương mại Quốc tế Bách Khoa" },
                new LichSuTonKho { MaLichSu = 49, MaKho = 4, MaHang = 42, NgayPhatSinh = new DateTime(2026, 8, 9), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0020", SoLuong = 25, TonSauGiaoDich = 25, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty TNHH Thương mại Quốc tế Bách Khoa" },
                new LichSuTonKho { MaLichSu = 50, MaKho = 4, MaHang = 43, NgayPhatSinh = new DateTime(2026, 8, 9), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0020", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty TNHH Thương mại Quốc tế Bách Khoa" },
                new LichSuTonKho { MaLichSu = 51, MaKho = 1, MaHang = 43, NgayPhatSinh = new DateTime(2026, 8, 12), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0021", SoLuong = 60, TonSauGiaoDich = 60, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt" },
                new LichSuTonKho { MaLichSu = 52, MaKho = 1, MaHang = 44, NgayPhatSinh = new DateTime(2026, 8, 12), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0021", SoLuong = 33, TonSauGiaoDich = 33, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt" },
                new LichSuTonKho { MaLichSu = 53, MaKho = 2, MaHang = 45, NgayPhatSinh = new DateTime(2026, 8, 14), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0022", SoLuong = 70, TonSauGiaoDich = 70, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Giải pháp Viễn thông Sao Mai" },
                new LichSuTonKho { MaLichSu = 54, MaKho = 2, MaHang = 46, NgayPhatSinh = new DateTime(2026, 8, 14), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0022", SoLuong = 41, TonSauGiaoDich = 41, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Giải pháp Viễn thông Sao Mai" },
                new LichSuTonKho { MaLichSu = 55, MaKho = 2, MaHang = 47, NgayPhatSinh = new DateTime(2026, 8, 14), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0022", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Giải pháp Viễn thông Sao Mai" },
                new LichSuTonKho { MaLichSu = 56, MaKho = 3, MaHang = 47, NgayPhatSinh = new DateTime(2026, 8, 16), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0023", SoLuong = 80, TonSauGiaoDich = 80, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 57, MaKho = 3, MaHang = 48, NgayPhatSinh = new DateTime(2026, 8, 16), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0023", SoLuong = 49, TonSauGiaoDich = 49, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 58, MaKho = 4, MaHang = 49, NgayPhatSinh = new DateTime(2026, 8, 18), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0024", SoLuong = 30, TonSauGiaoDich = 30, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 59, MaKho = 4, MaHang = 50, NgayPhatSinh = new DateTime(2026, 8, 18), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0024", SoLuong = 57, TonSauGiaoDich = 57, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 60, MaKho = 4, MaHang = 51, NgayPhatSinh = new DateTime(2026, 8, 18), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0024", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 61, MaKho = 1, MaHang = 51, NgayPhatSinh = new DateTime(2026, 8, 20), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0025", SoLuong = 40, TonSauGiaoDich = 40, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 62, MaKho = 1, MaHang = 52, NgayPhatSinh = new DateTime(2026, 8, 20), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0025", SoLuong = 25, TonSauGiaoDich = 25, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 63, MaKho = 2, MaHang = 53, NgayPhatSinh = new DateTime(2026, 8, 22), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0026", SoLuong = 50, TonSauGiaoDich = 50, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty Thiết bị Điện & Cơ khí Cadisun" },
                new LichSuTonKho { MaLichSu = 64, MaKho = 2, MaHang = 54, NgayPhatSinh = new DateTime(2026, 8, 22), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0026", SoLuong = 33, TonSauGiaoDich = 33, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty Thiết bị Điện & Cơ khí Cadisun" },
                new LichSuTonKho { MaLichSu = 65, MaKho = 2, MaHang = 55, NgayPhatSinh = new DateTime(2026, 8, 22), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0026", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty Thiết bị Điện & Cơ khí Cadisun" },
                new LichSuTonKho { MaLichSu = 66, MaKho = 3, MaHang = 55, NgayPhatSinh = new DateTime(2026, 8, 24), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0027", SoLuong = 60, TonSauGiaoDich = 60, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Linh Kiện Điện Tử Minh Hà" },
                new LichSuTonKho { MaLichSu = 67, MaKho = 3, MaHang = 56, NgayPhatSinh = new DateTime(2026, 8, 24), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0027", SoLuong = 41, TonSauGiaoDich = 41, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty TNHH Linh Kiện Điện Tử Minh Hà" },
                new LichSuTonKho { MaLichSu = 68, MaKho = 4, MaHang = 57, NgayPhatSinh = new DateTime(2026, 8, 26), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0028", SoLuong = 70, TonSauGiaoDich = 70, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty Cổ phần Công nghệ Mạng Phúc Anh" },
                new LichSuTonKho { MaLichSu = 69, MaKho = 4, MaHang = 58, NgayPhatSinh = new DateTime(2026, 8, 26), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0028", SoLuong = 49, TonSauGiaoDich = 49, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty Cổ phần Công nghệ Mạng Phúc Anh" },
                new LichSuTonKho { MaLichSu = 70, MaKho = 4, MaHang = 59, NgayPhatSinh = new DateTime(2026, 8, 26), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0028", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty Cổ phần Công nghệ Mạng Phúc Anh" },
                new LichSuTonKho { MaLichSu = 71, MaKho = 1, MaHang = 59, NgayPhatSinh = new DateTime(2026, 8, 28), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0029", SoLuong = 80, TonSauGiaoDich = 80, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam" },
                new LichSuTonKho { MaLichSu = 72, MaKho = 1, MaHang = 60, NgayPhatSinh = new DateTime(2026, 8, 28), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0029", SoLuong = 57, TonSauGiaoDich = 57, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam" },
                new LichSuTonKho { MaLichSu = 73, MaKho = 2, MaHang = 1, NgayPhatSinh = new DateTime(2026, 8, 30), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0030", SoLuong = 15, TonSauGiaoDich = 15, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Cổ phần Văn phòng phẩm Hồng Hà" },
                new LichSuTonKho { MaLichSu = 74, MaKho = 2, MaHang = 2, NgayPhatSinh = new DateTime(2026, 8, 30), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0030", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Cổ phần Văn phòng phẩm Hồng Hà" },
                new LichSuTonKho { MaLichSu = 75, MaKho = 2, MaHang = 3, NgayPhatSinh = new DateTime(2026, 8, 30), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0030", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Cổ phần Văn phòng phẩm Hồng Hà" },
                new LichSuTonKho { MaLichSu = 76, MaKho = 3, MaHang = 3, NgayPhatSinh = new DateTime(2026, 9, 2), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0031", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Thương mại Quốc tế Bách Khoa" },
                new LichSuTonKho { MaLichSu = 77, MaKho = 3, MaHang = 4, NgayPhatSinh = new DateTime(2026, 9, 2), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0031", SoLuong = 24, TonSauGiaoDich = 24, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Công ty TNHH Thương mại Quốc tế Bách Khoa" },
                new LichSuTonKho { MaLichSu = 78, MaKho = 4, MaHang = 5, NgayPhatSinh = new DateTime(2026, 9, 4), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0032", SoLuong = 25, TonSauGiaoDich = 25, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt" },
                new LichSuTonKho { MaLichSu = 79, MaKho = 4, MaHang = 6, NgayPhatSinh = new DateTime(2026, 9, 4), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0032", SoLuong = 12, TonSauGiaoDich = 12, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt" },
                new LichSuTonKho { MaLichSu = 80, MaKho = 4, MaHang = 7, NgayPhatSinh = new DateTime(2026, 9, 4), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0032", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Nhập kho từ Công ty Cổ phần Thiết bị Âm thanh Ánh sáng Việt" },
                new LichSuTonKho { MaLichSu = 81, MaKho = 1, MaHang = 7, NgayPhatSinh = new DateTime(2026, 9, 6), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0033", SoLuong = 30, TonSauGiaoDich = 30, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty TNHH Giải pháp Viễn thông Sao Mai" },
                new LichSuTonKho { MaLichSu = 82, MaKho = 1, MaHang = 8, NgayPhatSinh = new DateTime(2026, 9, 6), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0033", SoLuong = 16, TonSauGiaoDich = 16, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Nhập kho từ Công ty TNHH Giải pháp Viễn thông Sao Mai" },
                new LichSuTonKho { MaLichSu = 83, MaKho = 2, MaHang = 9, NgayPhatSinh = new DateTime(2026, 9, 8), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0034", SoLuong = 35, TonSauGiaoDich = 35, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 84, MaKho = 2, MaHang = 10, NgayPhatSinh = new DateTime(2026, 9, 8), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0034", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 85, MaKho = 2, MaHang = 11, NgayPhatSinh = new DateTime(2026, 9, 8), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0034", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Nhập kho từ Công ty TNHH Phân Phối Synnex FPT" },
                new LichSuTonKho { MaLichSu = 86, MaKho = 3, MaHang = 11, NgayPhatSinh = new DateTime(2026, 9, 10), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0035", SoLuong = 15, TonSauGiaoDich = 15, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 87, MaKho = 3, MaHang = 12, NgayPhatSinh = new DateTime(2026, 9, 10), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0035", SoLuong = 24, TonSauGiaoDich = 24, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Nhập kho từ Công ty Cổ phần Máy tính Phong Vũ" },
                new LichSuTonKho { MaLichSu = 88, MaKho = 4, MaHang = 13, NgayPhatSinh = new DateTime(2026, 9, 12), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0036", SoLuong = 20, TonSauGiaoDich = 20, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 89, MaKho = 4, MaHang = 14, NgayPhatSinh = new DateTime(2026, 9, 12), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0036", SoLuong = 12, TonSauGiaoDich = 12, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 90, MaKho = 4, MaHang = 15, NgayPhatSinh = new DateTime(2026, 9, 12), LoaiGiaoDich = "Nhập kho", MaPhieu = "PN0036", SoLuong = 10, TonSauGiaoDich = 10, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Nhập kho từ Tập đoàn Công nghệ CMC" },
                new LichSuTonKho { MaLichSu = 91, MaKho = 1, MaHang = 3, NgayPhatSinh = new DateTime(2026, 7, 15), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0001", SoLuong = 2, TonSauGiaoDich = 18, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new LichSuTonKho { MaLichSu = 92, MaKho = 2, MaHang = 5, NgayPhatSinh = new DateTime(2026, 7, 17), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0002", SoLuong = 2, TonSauGiaoDich = 23, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Xưởng Thực hành Điện - Tự động hóa" },
                new LichSuTonKho { MaLichSu = 93, MaKho = 2, MaHang = 6, NgayPhatSinh = new DateTime(2026, 7, 17), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0002", SoLuong = 3, TonSauGiaoDich = 17, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Xưởng Thực hành Điện - Tự động hóa" },
                new LichSuTonKho { MaLichSu = 94, MaKho = 3, MaHang = 7, NgayPhatSinh = new DateTime(2026, 7, 19), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0003", SoLuong = 2, TonSauGiaoDich = 28, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Trung tâm Dữ liệu & Viễn thông" },
                new LichSuTonKho { MaLichSu = 95, MaKho = 4, MaHang = 9, NgayPhatSinh = new DateTime(2026, 7, 21), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0004", SoLuong = 4, TonSauGiaoDich = 31, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Phòng Đào tạo & Khảo thí" },
                new LichSuTonKho { MaLichSu = 96, MaKho = 4, MaHang = 10, NgayPhatSinh = new DateTime(2026, 7, 21), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0004", SoLuong = 3, TonSauGiaoDich = 9, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Phòng Đào tạo & Khảo thí" },
                new LichSuTonKho { MaLichSu = 97, MaKho = 1, MaHang = 11, NgayPhatSinh = new DateTime(2026, 7, 23), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0005", SoLuong = 2, TonSauGiaoDich = 13, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Phòng Tài chính - Kế toán" },
                new LichSuTonKho { MaLichSu = 98, MaKho = 2, MaHang = 13, NgayPhatSinh = new DateTime(2026, 7, 25), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0006", SoLuong = 2, TonSauGiaoDich = 18, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Hành chính - Tổng vụ" },
                new LichSuTonKho { MaLichSu = 99, MaKho = 2, MaHang = 14, NgayPhatSinh = new DateTime(2026, 7, 25), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0006", SoLuong = 3, TonSauGiaoDich = 17, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Hành chính - Tổng vụ" },
                new LichSuTonKho { MaLichSu = 100, MaKho = 3, MaHang = 15, NgayPhatSinh = new DateTime(2026, 7, 27), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0007", SoLuong = 2, TonSauGiaoDich = 23, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Khoa Công nghệ Thông tin (Văn phòng Khoa)" },
                new LichSuTonKho { MaLichSu = 101, MaKho = 4, MaHang = 17, NgayPhatSinh = new DateTime(2026, 7, 29), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0008", SoLuong = 4, TonSauGiaoDich = 26, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new LichSuTonKho { MaLichSu = 102, MaKho = 4, MaHang = 18, NgayPhatSinh = new DateTime(2026, 7, 29), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0008", SoLuong = 3, TonSauGiaoDich = 9, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new LichSuTonKho { MaLichSu = 103, MaKho = 1, MaHang = 19, NgayPhatSinh = new DateTime(2026, 7, 31), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0009", SoLuong = 2, TonSauGiaoDich = 33, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Trung tâm Thí nghiệm & Chuyển giao Công nghệ" },
                new LichSuTonKho { MaLichSu = 104, MaKho = 2, MaHang = 21, NgayPhatSinh = new DateTime(2026, 8, 2), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0010", SoLuong = 2, TonSauGiaoDich = 68, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new LichSuTonKho { MaLichSu = 105, MaKho = 2, MaHang = 22, NgayPhatSinh = new DateTime(2026, 8, 2), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0010", SoLuong = 3, TonSauGiaoDich = 22, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new LichSuTonKho { MaLichSu = 106, MaKho = 3, MaHang = 23, NgayPhatSinh = new DateTime(2026, 8, 4), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0011", SoLuong = 2, TonSauGiaoDich = 78, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new LichSuTonKho { MaLichSu = 107, MaKho = 4, MaHang = 25, NgayPhatSinh = new DateTime(2026, 8, 6), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0012", SoLuong = 4, TonSauGiaoDich = 26, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Xưởng Thực hành Điện - Tự động hóa" },
                new LichSuTonKho { MaLichSu = 108, MaKho = 4, MaHang = 26, NgayPhatSinh = new DateTime(2026, 8, 6), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0012", SoLuong = 3, TonSauGiaoDich = 38, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Xưởng Thực hành Điện - Tự động hóa" },
                new LichSuTonKho { MaLichSu = 109, MaKho = 1, MaHang = 27, NgayPhatSinh = new DateTime(2026, 8, 8), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0013", SoLuong = 2, TonSauGiaoDich = 38, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Trung tâm Dữ liệu & Viễn thông" },
                new LichSuTonKho { MaLichSu = 110, MaKho = 2, MaHang = 29, NgayPhatSinh = new DateTime(2026, 8, 10), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0014", SoLuong = 2, TonSauGiaoDich = 48, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Phòng Đào tạo & Khảo thí" },
                new LichSuTonKho { MaLichSu = 111, MaKho = 2, MaHang = 30, NgayPhatSinh = new DateTime(2026, 8, 10), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0014", SoLuong = 3, TonSauGiaoDich = 54, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Phòng Đào tạo & Khảo thí" },
                new LichSuTonKho { MaLichSu = 112, MaKho = 3, MaHang = 31, NgayPhatSinh = new DateTime(2026, 8, 12), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0015", SoLuong = 2, TonSauGiaoDich = 58, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Phòng Tài chính - Kế toán" },
                new LichSuTonKho { MaLichSu = 113, MaKho = 4, MaHang = 33, NgayPhatSinh = new DateTime(2026, 8, 14), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0016", SoLuong = 4, TonSauGiaoDich = 66, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Hành chính - Tổng vụ" },
                new LichSuTonKho { MaLichSu = 114, MaKho = 4, MaHang = 34, NgayPhatSinh = new DateTime(2026, 8, 14), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0016", SoLuong = 3, TonSauGiaoDich = 30, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Hành chính - Tổng vụ" },
                new LichSuTonKho { MaLichSu = 115, MaKho = 1, MaHang = 35, NgayPhatSinh = new DateTime(2026, 8, 16), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0017", SoLuong = 2, TonSauGiaoDich = 78, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Khoa Công nghệ Thông tin (Văn phòng Khoa)" },
                new LichSuTonKho { MaLichSu = 116, MaKho = 2, MaHang = 37, NgayPhatSinh = new DateTime(2026, 8, 18), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0018", SoLuong = 2, TonSauGiaoDich = 28, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new LichSuTonKho { MaLichSu = 117, MaKho = 2, MaHang = 38, NgayPhatSinh = new DateTime(2026, 8, 18), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0018", SoLuong = 3, TonSauGiaoDich = 46, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new LichSuTonKho { MaLichSu = 118, MaKho = 3, MaHang = 39, NgayPhatSinh = new DateTime(2026, 8, 20), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0019", SoLuong = 2, TonSauGiaoDich = 38, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Trung tâm Thí nghiệm & Chuyển giao Công nghệ" },
                new LichSuTonKho { MaLichSu = 119, MaKho = 4, MaHang = 41, NgayPhatSinh = new DateTime(2026, 8, 22), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0020", SoLuong = 4, TonSauGiaoDich = 46, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new LichSuTonKho { MaLichSu = 120, MaKho = 4, MaHang = 42, NgayPhatSinh = new DateTime(2026, 8, 22), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0020", SoLuong = 3, TonSauGiaoDich = 22, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new LichSuTonKho { MaLichSu = 121, MaKho = 1, MaHang = 43, NgayPhatSinh = new DateTime(2026, 8, 24), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0021", SoLuong = 2, TonSauGiaoDich = 58, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new LichSuTonKho { MaLichSu = 122, MaKho = 2, MaHang = 45, NgayPhatSinh = new DateTime(2026, 8, 26), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0022", SoLuong = 2, TonSauGiaoDich = 68, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Xưởng Thực hành Điện - Tự động hóa" },
                new LichSuTonKho { MaLichSu = 123, MaKho = 2, MaHang = 46, NgayPhatSinh = new DateTime(2026, 8, 26), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0022", SoLuong = 3, TonSauGiaoDich = 38, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Xưởng Thực hành Điện - Tự động hóa" },
                new LichSuTonKho { MaLichSu = 124, MaKho = 3, MaHang = 47, NgayPhatSinh = new DateTime(2026, 8, 28), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0023", SoLuong = 2, TonSauGiaoDich = 78, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Trung tâm Dữ liệu & Viễn thông" },
                new LichSuTonKho { MaLichSu = 125, MaKho = 4, MaHang = 49, NgayPhatSinh = new DateTime(2026, 8, 30), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0024", SoLuong = 4, TonSauGiaoDich = 26, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Phòng Đào tạo & Khảo thí" },
                new LichSuTonKho { MaLichSu = 126, MaKho = 4, MaHang = 50, NgayPhatSinh = new DateTime(2026, 8, 30), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0024", SoLuong = 3, TonSauGiaoDich = 54, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Phòng Đào tạo & Khảo thí" },
                new LichSuTonKho { MaLichSu = 127, MaKho = 1, MaHang = 51, NgayPhatSinh = new DateTime(2026, 9, 1), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0025", SoLuong = 2, TonSauGiaoDich = 38, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Phòng Tài chính - Kế toán" },
                new LichSuTonKho { MaLichSu = 128, MaKho = 2, MaHang = 53, NgayPhatSinh = new DateTime(2026, 9, 3), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0026", SoLuong = 2, TonSauGiaoDich = 48, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Hành chính - Tổng vụ" },
                new LichSuTonKho { MaLichSu = 129, MaKho = 2, MaHang = 54, NgayPhatSinh = new DateTime(2026, 9, 3), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0026", SoLuong = 3, TonSauGiaoDich = 30, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Hành chính - Tổng vụ" },
                new LichSuTonKho { MaLichSu = 130, MaKho = 3, MaHang = 55, NgayPhatSinh = new DateTime(2026, 9, 5), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0027", SoLuong = 2, TonSauGiaoDich = 58, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Khoa Công nghệ Thông tin (Văn phòng Khoa)" },
                new LichSuTonKho { MaLichSu = 131, MaKho = 4, MaHang = 57, NgayPhatSinh = new DateTime(2026, 9, 7), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0028", SoLuong = 4, TonSauGiaoDich = 66, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new LichSuTonKho { MaLichSu = 132, MaKho = 4, MaHang = 58, NgayPhatSinh = new DateTime(2026, 9, 7), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0028", SoLuong = 3, TonSauGiaoDich = 46, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Khoa Điện tử & Kỹ thuật Máy tính" },
                new LichSuTonKho { MaLichSu = 133, MaKho = 1, MaHang = 59, NgayPhatSinh = new DateTime(2026, 9, 9), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0029", SoLuong = 2, TonSauGiaoDich = 78, NguoiThucHien = "Trần Xuân Hải", GhiChu = "Xuất giao cho Trung tâm Thí nghiệm & Chuyển giao Công nghệ" },
                new LichSuTonKho { MaLichSu = 134, MaKho = 2, MaHang = 1, NgayPhatSinh = new DateTime(2026, 9, 11), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0030", SoLuong = 2, TonSauGiaoDich = 13, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new LichSuTonKho { MaLichSu = 135, MaKho = 2, MaHang = 2, NgayPhatSinh = new DateTime(2026, 9, 11), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0030", SoLuong = 3, TonSauGiaoDich = 17, NguoiThucHien = "Bùi Thị Mai", GhiChu = "Xuất giao cho Ban Dự án Cơ sở Lĩnh Nam (Cũ)" },
                new LichSuTonKho { MaLichSu = 136, MaKho = 3, MaHang = 3, NgayPhatSinh = new DateTime(2026, 9, 13), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0031", SoLuong = 2, TonSauGiaoDich = 18, NguoiThucHien = "Lê Văn Hùng", GhiChu = "Xuất giao cho Phòng Quản trị Thiết bị & Hạ tầng CNTT" },
                new LichSuTonKho { MaLichSu = 137, MaKho = 4, MaHang = 5, NgayPhatSinh = new DateTime(2026, 9, 15), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0032", SoLuong = 4, TonSauGiaoDich = 21, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Xưởng Thực hành Điện - Tự động hóa" },
                new LichSuTonKho { MaLichSu = 138, MaKho = 4, MaHang = 6, NgayPhatSinh = new DateTime(2026, 9, 15), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0032", SoLuong = 3, TonSauGiaoDich = 9, NguoiThucHien = "Nguyễn Thị Cúc", GhiChu = "Xuất giao cho Xưởng Thực hành Điện - Tự động hóa" },
                new LichSuTonKho { MaLichSu = 139, MaKho = 1, MaHang = 7, NgayPhatSinh = new DateTime(2026, 9, 17), LoaiGiaoDich = "Xuất kho", MaPhieu = "PX0033", SoLuong = 2, TonSauGiaoDich = 28, NguoiThucHien = "Nguyễn Văn Cường", GhiChu = "Xuất giao cho Trung tâm Dữ liệu & Viễn thông" },
            };

            return new SeedDataContainer(
                taiKhoans, loaiHangs, donViTinhs, khos, nhaCungCaps, boPhanNhans,
                hangHoas, phieuNhaps, chiTietNhaps, phieuXuats, chiTietXuats,
                tonKhoes, lichSuList);
        }
    }

    public record SeedDataContainer(
        List<TaiKhoan> TaiKhoans,
        List<LoaiHang> LoaiHangs,
        List<DonViTinh> DonViTinhs,
        List<Kho> Khos,
        List<NhaCungCap> NhaCungCaps,
        List<BoPhanNhan> BoPhanNhans,
        List<HangHoa> HangHoas,
        List<PhieuNhap> PhieuNhaps,
        List<ChiTietPhieuNhap> ChiTietNhaps,
        List<PhieuXuat> PhieuXuats,
        List<ChiTietPhieuXuat> ChiTietXuats,
        List<TonKho> TonKhoes,
        List<LichSuTonKho> LichSuList);
}