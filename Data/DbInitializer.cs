using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Data
{
    /// <summary>
    /// Họ và tên: Nhóm 1 - TI17A3HN
    /// Sinh viên thực hiện:
    /// - Trần Xuân Hải (23103100135) - Module 1: Tài khoản, Đăng nhập, Loại hàng, Đơn vị tính
    /// - Nguyễn Thị Cúc (23103100178) - Module 2: Kho, Hàng hóa
    /// - Lê Văn Hùng (23103100177) - Module 3: Nhà cung cấp, Phiếu nhập, Chi tiết phiếu nhập
    /// - Nguyễn Việt Dũng (23103100127) - Module 4: Bộ phận nhận, Phiếu xuất, Chi tiết phiếu xuất
    /// - Nguyễn Văn Cường (23103100132) - Module 5: Tồn kho, Lịch sử tồn kho, Dashboard
    /// 
    /// Mục tiêu: Khởi tạo dữ liệu mẫu (Seed Data) chuẩn theo yêu cầu Mục 16 tài liệu Đề tài 21:
    /// - Tối thiểu 05 loại hàng (tạo 6)
    /// - Tối thiểu 05 đơn vị tính (tạo 7)
    /// - Tối thiểu 03 kho (tạo 4, có kho ngừng hoạt động)
    /// - 30-40 hàng hóa (tạo 35 mặt hàng phong phú)
    /// - 08-10 nhà cung cấp (tạo 10, có nhà cung cấp ngừng hoạt động)
    /// - 06-08 bộ phận nhận (tạo 8, có bộ phận ngừng hoạt động)
    /// - 30-40 phiếu nhập (tạo 35 phiếu đủ 4 trạng thái: Nháp, Chờ xác nhận, Đã hoàn tất, Đã hủy)
    /// - 30-40 phiếu xuất (tạo 35 phiếu đủ 4 trạng thái)
    /// - Tồn kho thực tế và Lịch sử tồn kho đồng bộ 100% với các phiếu hoàn tất
    /// - Dữ liệu có hàng còn nhiều, hàng sắp hết và hàng hết (để kiểm thử Cảnh báo & Dashboard)
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Tự động kiểm tra và áp dụng Migration lên Database (kể cả Azure SQL / RemoteDb)
            context.Database.Migrate();

            // Nếu đã có dữ liệu Tài khoản -> hệ thống đã được Seed trước đó
            if (context.TaiKhoans.Any())
            {
                return;
            }

            // =========================================================================
            // 1. TÀI KHOẢN (TaiKhoan) - Module 1
            // =========================================================================
            var taiKhoans = new List<TaiKhoan>
            {
                new TaiKhoan
                {
                    TenDangNhap = "admin",
                    MatKhau = "Admin@123",
                    HoTen = "Trần Xuân Hải (Admin Quản trị)",
                    Email = "haixuan@uneti.edu.vn",
                    VaiTro = "Admin",
                    TrangThai = true
                },
                new TaiKhoan
                {
                    TenDangNhap = "nvkho_hung",
                    MatKhau = "123456",
                    HoTen = "Lê Văn Hùng (Thủ kho 1)",
                    Email = "hunglv@uneti.edu.vn",
                    VaiTro = "NhanVienKho",
                    TrangThai = true
                },
                new TaiKhoan
                {
                    TenDangNhap = "nvkho_cuc",
                    MatKhau = "123456",
                    HoTen = "Nguyễn Thị Cúc (Thủ kho 2)",
                    Email = "cucnt@uneti.edu.vn",
                    VaiTro = "NhanVienKho",
                    TrangThai = true
                },
                new TaiKhoan
                {
                    TenDangNhap = "nvkho_cuong",
                    MatKhau = "123456",
                    HoTen = "Nguyễn Văn Cường (Thủ kho 3)",
                    Email = "cuongnv@uneti.edu.vn",
                    VaiTro = "NhanVienKho",
                    TrangThai = true
                },
                new TaiKhoan
                {
                    TenDangNhap = "bophan_dung",
                    MatKhau = "123456",
                    HoTen = "Nguyễn Việt Dũng (Đại diện Phòng CNTT)",
                    Email = "dungnv@uneti.edu.vn",
                    VaiTro = "BoPhanNhan",
                    TrangThai = true
                },
                new TaiKhoan
                {
                    TenDangNhap = "bophan_lan",
                    MatKhau = "123456",
                    HoTen = "Phạm Thị Lan (Đại diện Phòng Kế toán)",
                    Email = "lanpt@uneti.edu.vn",
                    VaiTro = "BoPhanNhan",
                    TrangThai = true
                },
                new TaiKhoan
                {
                    TenDangNhap = "user_khoa",
                    MatKhau = "123456",
                    HoTen = "Đỗ Minh Tuấn (Tài khoản bị khóa)",
                    Email = "tuandm@uneti.edu.vn",
                    VaiTro = "NhanVienKho",
                    TrangThai = false // Kiểm thử quy tắc: tài khoản bị khóa không được đăng nhập
                }
            };
            context.TaiKhoans.AddRange(taiKhoans);
            context.SaveChanges();

            // =========================================================================
            // 2. LOẠI HÀNG (LoaiHang) - Module 1 (6 loại hàng >= 5)
            // =========================================================================
            var loaiHangs = new List<LoaiHang>
            {
                new LoaiHang { TenLoaiHang = "Thiết bị vi tính & Văn phòng", MoTa = "Máy tính để bàn, màn hình, máy in và thiết bị ngoại vi văn phòng", TrangThai = true },
                new LoaiHang { TenLoaiHang = "Linh kiện điện tử & Vi mạch", MoTa = "Bo mạch nhúng, cảm biến, vi điều khiển phục vụ thực hành & thí nghiệm", TrangThai = true },
                new LoaiHang { TenLoaiHang = "Vật tư điện & Cơ khí xưởng", MoTa = "Dây cáp nguồn, aptomat, công tắc, đồng hồ đo và dụng cụ cầm tay", TrangThai = true },
                new LoaiHang { TenLoaiHang = "Thiết bị mạng & Viễn thông", MoTa = "Router WiFi, Switch chia mạng, cáp mạng UTP, hạt mạng RJ45, tủ rack", TrangThai = true },
                new LoaiHang { TenLoaiHang = "Văn phòng phẩm & Tiêu hao", MoTa = "Giấy in văn phòng, mực in laser, bút dạ, băng dính và màng bọc", TrangThai = true },
                new LoaiHang { TenLoaiHang = "Dụng cụ thí nghiệm cũ (Tạm ngưng)", MoTa = "Danh mục thiết bị ngừng tiếp nhận nhập kho mới", TrangThai = false }
            };
            context.LoaiHangs.AddRange(loaiHangs);
            context.SaveChanges();

            // =========================================================================
            // 3. ĐƠN VỊ TÍNH (DonViTinh) - Module 1 (7 đơn vị tính >= 5)
            // =========================================================================
            var donViTinhs = new List<DonViTinh>
            {
                new DonViTinh { TenDonViTinh = "Chiếc", MoTa = "Đơn vị đếm máy móc, màn hình, thiết bị nguyên chiếc", TrangThai = true },
                new DonViTinh { TenDonViTinh = "Cái", MoTa = "Đơn vị đếm linh kiện, chuột, bàn phím, phụ kiện nhỏ", TrangThai = true },
                new DonViTinh { TenDonViTinh = "Bộ", MoTa = "Tập hợp thiết bị hoàn chỉnh (ví dụ: máy tính để bàn kèm phụ kiện)", TrangThai = true },
                new DonViTinh { TenDonViTinh = "Hộp", MoTa = "Quy cách đóng gói hộp (mực in, hạt mạng, linh kiện)", TrangThai = true },
                new DonViTinh { TenDonViTinh = "Thùng", MoTa = "Thùng carton phục vụ xuất kho số lượng lớn (giấy in, cáp)", TrangThai = true },
                new DonViTinh { TenDonViTinh = "Mét", MoTa = "Đo chiều dài dây dẫn cáp điện, cáp mạng", TrangThai = true },
                new DonViTinh { TenDonViTinh = "Cuộn (Ngưng dùng)", MoTa = "Đơn vị cuộn cũ không áp dụng cho mặt hàng mới", TrangThai = false }
            };
            context.DonViTinhs.AddRange(donViTinhs);
            context.SaveChanges();

            // =========================================================================
            // 4. KHO (Kho) - Module 2 (4 kho >= 3)
            // =========================================================================
            var khos = new List<Kho>
            {
                new Kho { TenKho = "Kho Tổng Minh Khai - Hà Nội", DiaDiem = "456 Minh Khai, Hai Bà Trưng, Hà Nội", MoTa = "Kho lưu trữ và điều phối chính toàn hệ thống", TrangThai = true },
                new Kho { TenKho = "Kho Xưởng Thực hành Nam Định", DiaDiem = "353 Trần Hưng Đạo, TP. Nam Định", MoTa = "Kho linh kiện, máy móc phục vụ giảng dạy thực hành kỹ thuật", TrangThai = true },
                new Kho { TenKho = "Kho Thiết bị Đội Cấn - Cầu Giấy", DiaDiem = "218 Đội Cấn, Ba Đình, Hà Nội", MoTa = "Kho lưu trữ dự phòng thiết bị công nghệ cao và vật tư tiêu hao", TrangThai = true },
                new Kho { TenKho = "Kho Lĩnh Nam Cũ (Tạm dừng hoạt động)", DiaDiem = "218 Lĩnh Nam, Hoàng Mai, Hà Nội", MoTa = "Kho đang nâng cấp sửa chữa, không tiếp nhận nhập xuất mới", TrangThai = false }
            };
            context.Kho.AddRange(khos);
            context.SaveChanges();

            // =========================================================================
            // 5. NHÀ CUNG CẤP (NhaCungCap) - Module 3 (10 nhà cung cấp: 8-10 yêu cầu)
            // =========================================================================
            var nhaCungCaps = new List<NhaCungCap>
            {
                new NhaCungCap { TenNhaCungCap = "Công ty TNHH Phân Phối Synnex FPT", SoDienThoai = "02473007300", Email = "contact@synnexfpt.vn", DiaChi = "Tòa nhà FPT, Phố Duy Tân, Cầu Giấy, Hà Nội", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Công ty Cổ phần Máy tính Phong Vũ", SoDienThoai = "02873013030", Email = "cskh@phongvu.vn", DiaChi = "Tầng 5, 117-119 Lý Chính Thắng, Quận 3, TP.HCM", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Tập đoàn Công nghệ CMC", SoDienThoai = "02437958686", Email = "info@cmc.com.vn", DiaChi = "Tòa nhà CMC, Phố Duy Tân, Dịch Vọng Hậu, Cầu Giấy, Hà Nội", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Công ty Thiết bị Điện & Cơ khí Cadisun", SoDienThoai = "02435581188", Email = "cadisun@cadisun.com.vn", DiaChi = "Cụm Công nghiệp Ngọc Hồi, Thanh Trì, Hà Nội", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Công ty TNHH Linh Kiện Điện Tử Minh Hà", SoDienThoai = "0933388686", Email = "minhha.electronic@gmail.com", DiaChi = "Số 84 Triều Khúc, Thanh Xuân, Hà Nội", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Công ty Cổ phần Công nghệ Mạng Phúc Anh", SoDienThoai = "02438573999", Email = "banhang@phucanh.vn", DiaChi = "15 Xã Đàn, Phương Liên, Đống Đa, Hà Nội", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Công ty TNHH Thiết Bị Đo Lường Etech Việt Nam", SoDienThoai = "02462928822", Email = "etechvietnam@gmail.com", DiaChi = "P402 Tòa nhà N03, Trần Quý Kiên, Cầu Giấy, Hà Nội", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Công ty Cổ phần Văn phòng phẩm Hồng Hà", SoDienThoai = "02438524611", Email = "cskh@vpphongha.com.vn", DiaChi = "25 Lý Thường Kiệt, Hoàn Kiếm, Hà Nội", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Công ty TNHH Thương mại Quốc tế Bách Khoa", SoDienThoai = "02438691234", Email = "bktech@bachkhoa.vn", DiaChi = "Số 1 Đại Cồ Việt, Hai Bà Trưng, Hà Nội", TrangThai = true },
                new NhaCungCap { TenNhaCungCap = "Công ty Cổ phần Thiết bị Đông Đô (Ngừng hợp tác)", SoDienThoai = "02439998888", Email = "dongdo@dongdo.vn", DiaChi = "Số 12 Giải Phóng, Hai Bà Trưng, Hà Nội", TrangThai = false }
            };
            context.NhaCungCap.AddRange(nhaCungCaps);
            context.SaveChanges();

            // =========================================================================
            // 6. BỘ PHẬN NHẬN (BoPhanNhan) - Module 4 (8 bộ phận: 6-8 yêu cầu)
            // =========================================================================
            var boPhanNhans = new List<BoPhanNhan>
            {
                new BoPhanNhan { TenBoPhan = "Phòng Quản trị Thiết bị & Hạ tầng CNTT", NguoiDaiDien = "ThS. Hoàng Văn Mạnh", SoDienThoai = "0912345678", MoTa = "Quản trị máy chủ, phòng máy tính sinh viên và hạ tầng mạng", TrangThai = true },
                new BoPhanNhan { TenBoPhan = "Xưởng Thực hành Điện - Tự động hóa", NguoiDaiDien = "KS. Đỗ Quang Huy", SoDienThoai = "0923456789", MoTa = "Xưởng kỹ thuật điện và thực hành cơ điện tử", TrangThai = true },
                new BoPhanNhan { TenBoPhan = "Trung tâm Dữ liệu & Viễn thông", NguoiDaiDien = "ThS. Lê Thanh Tùng", SoDienThoai = "0934567890", MoTa = "Quản lý phòng server, hệ thống camera và switch core", TrangThai = true },
                new BoPhanNhan { TenBoPhan = "Phòng Đào tạo & Khảo thí", NguoiDaiDien = "TS. Nguyễn Minh Châu", SoDienThoai = "0945678901", MoTa = "Quản lý in ấn đề thi, tài liệu đào tạo và hồ sơ sinh viên", TrangThai = true },
                new BoPhanNhan { TenBoPhan = "Phòng Tài chính - Kế toán", NguoiDaiDien = "Bà Trần Thu Trang", SoDienThoai = "0956789012", MoTa = "Máy in hóa đơn, giấy tờ thanh toán và máy tính kế toán", TrangThai = true },
                new BoPhanNhan { TenBoPhan = "Phòng Hành chính - Tổng vụ", NguoiDaiDien = "Ông Vũ Đức Long", SoDienThoai = "0967890123", MoTa = "Vật tư tiêu hao, thiết bị phục vụ hội họp trường", TrangThai = true },
                new BoPhanNhan { TenBoPhan = "Khoa Công nghệ Thông tin (Văn phòng Khoa)", NguoiDaiDien = "ThS. Bùi Thị Mai", SoDienThoai = "0978901234", MoTa = "Văn phòng Khoa CNTT, nghiên cứu khoa học & phát triển phần mềm", TrangThai = true },
                new BoPhanNhan { TenBoPhan = "Ban Dự án Cơ sở Lĩnh Nam (Cũ)", NguoiDaiDien = "Ông Lê Thành Nam", SoDienThoai = "0989012345", MoTa = "Bộ phận đã hoàn thành bàn giao dự án cơ sở", TrangThai = false }
            };
            context.BoPhanNhans.AddRange(boPhanNhans);
            context.SaveChanges();

            // =========================================================================
            // 7. HÀNG HÓA (HangHoa) - Module 2 (35 mặt hàng: 30-40 yêu cầu)
            // =========================================================================
            int lh1 = loaiHangs[0].MaLoaiHang; // Thiết bị vi tính
            int lh2 = loaiHangs[1].MaLoaiHang; // Linh kiện điện tử
            int lh3 = loaiHangs[2].MaLoaiHang; // Vật tư điện & cơ khí
            int lh4 = loaiHangs[3].MaLoaiHang; // Thiết bị mạng
            int lh5 = loaiHangs[4].MaLoaiHang; // Văn phòng phẩm

            int dvChiec = donViTinhs[0].MaDonViTinh; // Chiếc
            int dvCai = donViTinhs[1].MaDonViTinh;   // Cái
            int dvBo = donViTinhs[2].MaDonViTinh;    // Bộ
            int dvHop = donViTinhs[3].MaDonViTinh;   // Hộp
            int dvThung = donViTinhs[4].MaDonViTinh; // Thùng
            int dvMet = donViTinhs[5].MaDonViTinh;   // Mét

            var hangHoas = new List<HangHoa>
            {
                // Nhóm 1: Thiết bị vi tính (7 mặt hàng)
                new HangHoa { TenHang = "Màn hình Dell UltraSharp 24 inch U2422H", MaLoaiHang = lh1, MaDonViTinh = dvChiec, GiaNhapThamKhao = 5450000m, MucTonToiThieu = 5, MoTa = "Màn hình IPS chuẩn màu cho phòng đồ họa & thiết kế", TrangThai = true },
                new HangHoa { TenHang = "Màn hình LG 27 inch IPS Full HD 27MP400", MaLoaiHang = lh1, MaDonViTinh = dvChiec, GiaNhapThamKhao = 3200000m, MucTonToiThieu = 6, MoTa = "Màn hình viền mỏng dùng cho phòng lab máy tính", TrangThai = true },
                new HangHoa { TenHang = "Máy in Laser đa năng HP LaserJet MFP M135w", MaLoaiHang = lh1, MaDonViTinh = dvChiec, GiaNhapThamKhao = 3850000m, MucTonToiThieu = 3, MoTa = "Máy in in trắng đen, photocopy và scan không dây", TrangThai = true },
                new HangHoa { TenHang = "Bàn phím cơ văn phòng Logitech K845 Cherry Red", MaLoaiHang = lh1, MaDonViTinh = dvCai, GiaNhapThamKhao = 950000m, MucTonToiThieu = 15, MoTa = "Bàn phím cơ full size có đèn nền LED trắng", TrangThai = true },
                new HangHoa { TenHang = "Chuột quang không dây Fuhlen A09G", MaLoaiHang = lh1, MaDonViTinh = dvCai, GiaNhapThamKhao = 135000m, MucTonToiThieu = 20, MoTa = "Chuột không dây 2.4GHz tiết kiệm pin", TrangThai = true },
                new HangHoa { TenHang = "Webcam hội nghị trực tuyến Logitech C920e 1080P", MaLoaiHang = lh1, MaDonViTinh = dvChiec, GiaNhapThamKhao = 1450000m, MucTonToiThieu = 5, MoTa = "Webcam Full HD tích hợp micro kép chống ồn", TrangThai = true },
                new HangHoa { TenHang = "Bộ lưu điện UPS APC Back-UPS 650VA", MaLoaiHang = lh1, MaDonViTinh = dvChiec, GiaNhapThamKhao = 1250000m, MucTonToiThieu = 4, MoTa = "Bộ tích điện dự phòng bảo vệ máy chủ & PC", TrangThai = true },

                // Nhóm 2: Linh kiện điện tử & Vi mạch thực hành (7 mặt hàng)
                new HangHoa { TenHang = "Bo mạch vi điều khiển Arduino Uno R3 DIP", MaLoaiHang = lh2, MaDonViTinh = dvCai, GiaNhapThamKhao = 115000m, MucTonToiThieu = 20, MoTa = "Bo mạch Arduino Uno dùng cho thí nghiệm nhúng cơ bản", TrangThai = true },
                new HangHoa { TenHang = "Máy tính nhúng Raspberry Pi 4 Model B 4GB", MaLoaiHang = lh2, MaDonViTinh = dvBo, GiaNhapThamKhao = 1850000m, MucTonToiThieu = 5, MoTa = "Kit Raspberry Pi kèm vỏ tản nhiệt và nguồn USB-C", TrangThai = true },
                new HangHoa { TenHang = "Bo mạch WiFi IoT ESP32 NodeMCU 38 chân", MaLoaiHang = lh2, MaDonViTinh = dvCai, GiaNhapThamKhao = 85000m, MucTonToiThieu = 25, MoTa = "Module tích hợp WiFi và Bluetooth ứng dụng IoT", TrangThai = true },
                new HangHoa { TenHang = "Cảm biến nhiệt độ độ ẩm DHT11", MaLoaiHang = lh2, MaDonViTinh = dvCai, GiaNhapThamKhao = 22000m, MucTonToiThieu = 30, MoTa = "Module cảm biến số đo nhiệt độ và độ ẩm không khí", TrangThai = true },
                new HangHoa { TenHang = "Module rơ-le Relay 4 kênh cách ly quang 5V", MaLoaiHang = lh2, MaDonViTinh = dvCai, GiaNhapThamKhao = 45000m, MucTonToiThieu = 20, MoTa = "Mạch kích rơ-le điều khiển thiết bị điện 220V", TrangThai = true },
                new HangHoa { TenHang = "Động cơ bước điều khiển góc Servo SG90", MaLoaiHang = lh2, MaDonViTinh = dvCai, GiaNhapThamKhao = 28000m, MucTonToiThieu = 20, MoTa = "Động cơ servo mini 9g dùng cho cánh tay robot thí nghiệm", TrangThai = true },
                new HangHoa { TenHang = "Mạch hạ áp giảm dòng xung LM2596 Buck DC-DC", MaLoaiHang = lh2, MaDonViTinh = dvCai, GiaNhapThamKhao = 18000m, MucTonToiThieu = 30, MoTa = "Module chuyển nguồn DC 3.2V - 35V xuống 1.25V - 30V", TrangThai = true },

                // Nhóm 3: Vật tư điện & Cơ khí xưởng (7 mặt hàng)
                new HangHoa { TenHang = "Cáp điện đôi mềm Cadisun 2x2.5mm² (Cuộn 100m)", MaLoaiHang = lh3, MaDonViTinh = dvMet, GiaNhapThamKhao = 18500m, MucTonToiThieu = 50, MoTa = "Dây điện bọc nhựa PVC lõi đồng nguyên chất chịu tải", TrangThai = true },
                new HangHoa { TenHang = "Aptomat chống giật Panasonic RCBO 32A 30mA", MaLoaiHang = lh3, MaDonViTinh = dvCai, GiaNhapThamKhao = 480000m, MucTonToiThieu = 10, MoTa = "Thiết bị bảo vệ quá tải, ngắn mạch và chống dòng rò", TrangThai = true },
                new HangHoa { TenHang = "Đồng hồ vạn năng hiện số Sanwa CD800a Nhật Bản", MaLoaiHang = lh3, MaDonViTinh = dvChiec, GiaNhapThamKhao = 920000m, MucTonToiThieu = 8, MoTa = "Đồng hồ đo điện áp, dòng điện, điện trở độ chính xác cao", TrangThai = true },
                new HangHoa { TenHang = "Kìm bấm đầu cáp mạng đa năng Dintek RJ45/RJ11", MaLoaiHang = lh3, MaDonViTinh = dvCai, GiaNhapThamKhao = 295000m, MucTonToiThieu = 10, MoTa = "Kìm trợ lực thép carbon bấm hạt mạng chuẩn kỹ thuật", TrangThai = true },
                new HangHoa { TenHang = "Trạm hàn điều chỉnh nhiệt độ Quick 936A ESD 60W", MaLoaiHang = lh3, MaDonViTinh = dvBo, GiaNhapThamKhao = 750000m, MucTonToiThieu = 5, MoTa = "Máy hàn thiếc phòng thí nghiệm chống tĩnh điện", TrangThai = true },
                new HangHoa { TenHang = "Ổ cắm di động công nghiệp Schneider 4 ngõ có rơ le", MaLoaiHang = lh3, MaDonViTinh = dvCai, GiaNhapThamKhao = 320000m, MucTonToiThieu = 12, MoTa = "Ổ cắm chịu tải công suất cao có công tắc ngắt an toàn", TrangThai = true },
                new HangHoa { TenHang = "Dây thít nhựa rút chịu nhiệt 20cm (Gói 100 cái)", MaLoaiHang = lh3, MaDonViTinh = dvHop, GiaNhapThamKhao = 25000m, MucTonToiThieu = 40, MoTa = "Dây rút cố định hệ thống bó dây cáp mạng và điện", TrangThai = true },

                // Nhóm 4: Thiết bị mạng & Viễn thông (7 mặt hàng)
                new HangHoa { TenHang = "Router WiFi 6 TP-Link Archer AX1500 Băng tần kép", MaLoaiHang = lh4, MaDonViTinh = dvChiec, GiaNhapThamKhao = 1080000m, MucTonToiThieu = 6, MoTa = "Bộ định tuyến phát sóng WiFi chuẩn AX tốc độ 1.5Gbps", TrangThai = true },
                new HangHoa { TenHang = "Switch chia mạng Cisco Business CBS110 24 Cổng Gigabit", MaLoaiHang = lh4, MaDonViTinh = dvChiec, GiaNhapThamKhao = 2850000m, MucTonToiThieu = 4, MoTa = "Bộ chuyển mạch 24 port 10/100/1000Mbps dùng cho phòng máy", TrangThai = true },
                new HangHoa { TenHang = "Thùng cáp mạng UTP Cat6 Commscope chính hãng (305m)", MaLoaiHang = lh4, MaDonViTinh = dvThung, GiaNhapThamKhao = 2450000m, MucTonToiThieu = 5, MoTa = "Thùng dây mạng bấm phòng máy và kéo ngầm", TrangThai = true },
                new HangHoa { TenHang = "Hộp hạt mạng RJ45 Cat6 bọc kim chống nhiễu (100 hạt)", MaLoaiHang = lh4, MaDonViTinh = dvHop, GiaNhapThamKhao = 260000m, MucTonToiThieu = 15, MoTa = "Đầu bấm mạng chân đồng mạ vàng truyền tín hiệu cao", TrangThai = true },
                new HangHoa { TenHang = "Tủ mạng Rack 10U treo tường cửa lưới D600", MaLoaiHang = lh4, MaDonViTinh = dvChiec, GiaNhapThamKhao = 1350000m, MucTonToiThieu = 3, MoTa = "Tủ chứa switch, patch panel và khay chứa thiết bị mạng", TrangThai = true },
                new HangHoa { TenHang = "Bộ phát WiFi chuyên dụng âm trần Ruijie Reyee RG-RAP2200(E)", MaLoaiHang = lh4, MaDonViTinh = dvChiec, GiaNhapThamKhao = 1750000m, MucTonToiThieu = 4, MoTa = "Access Point chịu tải cao cấp cho giảng đường", TrangThai = true },
                new HangHoa { TenHang = "Dây nhảy quang Singlemode LC-LC dài 3 mét", MaLoaiHang = lh4, MaDonViTinh = dvCai, GiaNhapThamKhao = 65000m, MucTonToiThieu = 20, MoTa = "Dây nối tín hiệu quang kết nối module SFP", TrangThai = true },

                // Nhóm 5: Văn phòng phẩm & Tiêu hao (7 mặt hàng)
                new HangHoa { TenHang = "Thùng giấy in A4 Double A Định lượng 70gsm (5 ram/thùng)", MaLoaiHang = lh5, MaDonViTinh = dvThung, GiaNhapThamKhao = 385000m, MucTonToiThieu = 10, MoTa = "Giấy trắng sáng in văn bản giáo trình và biểu mẫu", TrangThai = true },
                new HangHoa { TenHang = "Hộp mực in Laser HP 107A Black Toner Cartridge", MaLoaiHang = lh5, MaDonViTinh = dvHop, GiaNhapThamKhao = 1150000m, MucTonToiThieu = 6, MoTa = "Hộp mực chính hãng in 1000 trang rõ nét", TrangThai = true },
                new HangHoa { TenHang = "Hộp mực máy in Canon Cartridge 303 (dùng Canon 2900)", MaLoaiHang = lh5, MaDonViTinh = dvHop, GiaNhapThamKhao = 750000m, MucTonToiThieu = 8, MoTa = "Hộp mực phổ thông tương thích máy in LBP2900", TrangThai = true },
                new HangHoa { TenHang = "Hộp bút bi Thiên Long 0.5mm xanh (20 cây/hộp)", MaLoaiHang = lh5, MaDonViTinh = dvHop, GiaNhapThamKhao = 75000m, MucTonToiThieu = 15, MoTa = "Bút bi viết êm phục vụ văn phòng và giảng viên", TrangThai = true },
                new HangHoa { TenHang = "Cuộn băng dính đóng hàng dán thùng 500g loại 5cm", MaLoaiHang = lh5, MaDonViTinh = dvCai, GiaNhapThamKhao = 32000m, MucTonToiThieu = 20, MoTa = "Băng dính bản rộng đóng kiện hàng hóa gửi xưởng", TrangThai = true },
                new HangHoa { TenHang = "Cuộn màng co PE bọc bảo vệ thiết bị hàng hóa (3kg)", MaLoaiHang = lh5, MaDonViTinh = dvCai, GiaNhapThamKhao = 120000m, MucTonToiThieu = 10, MoTa = "Màng quấn pallet chống nước, chống bụi lưu kho", TrangThai = true },
                new HangHoa { TenHang = "Hộp pin tiểu AA Energizer Max vỉ 4 viên (Hộp 6 vỉ)", MaLoaiHang = lh5, MaDonViTinh = dvHop, GiaNhapThamKhao = 240000m, MucTonToiThieu = 12, MoTa = "Pin alkaline chống rò rỉ dung lượng lớn cho chuột và đồng hồ đo", TrangThai = true }
            };
            context.HangHoa.AddRange(hangHoas);
            context.SaveChanges();

            // =========================================================================
            // 8. PHIẾU NHẬP (PhieuNhap) & CHI TIẾT PHIẾU NHẬP (ChiTietPhieuNhap) - Module 3
            // Yêu cầu: 30-40 phiếu nhập (Tạo đúng 35 phiếu, có cả 4 trạng thái: 0, 1, 2, 3)
            // =========================================================================
            var nccList = nhaCungCaps.Where(n => n.TrangThai).ToList();
            var khoList = khos.Where(k => k.TrangThai).ToList();
            var hangList = hangHoas;

            int nccCount = nccList.Count;
            int khoCount = khoList.Count;

            var phieuNhaps = new List<PhieuNhap>();
            DateTime baseDate = new DateTime(2026, 7, 1);

            string[] nguoiLaps = { "Lê Văn Hùng", "Nguyễn Thị Cúc", "Nguyễn Văn Cường", "Trần Xuân Hải" };

            // Phân bổ trạng thái cho 35 phiếu nhập:
            // - Phiếu 1 đến 25: TrangThai = 2 (Đã hoàn tất - sinh Tồn kho & Lịch sử)
            // - Phiếu 26 đến 29: TrangThai = 0 (Nháp)
            // - Phiếu 30 đến 32: TrangThai = 1 (Chờ xác nhận)
            // - Phiếu 33 đến 35: TrangThai = 3 (Đã hủy)
            for (int i = 1; i <= 35; i++)
            {
                int trangThai;
                if (i <= 25) trangThai = 2;       // Đã hoàn tất
                else if (i <= 29) trangThai = 0;  // Nháp
                else if (i <= 32) trangThai = 1;  // Chờ xác nhận
                else trangThai = 3;               // Đã hủy

                // Ngày nhập rải đều từ tháng 7 đến cuối tháng 9/2026
                DateTime ngayNhap = baseDate.AddDays((i - 1) * 2.5);
                if (i == 25) ngayNhap = DateTime.Today; // Phiếu hoàn tất trong ngày hôm nay phục vụ Dashboard

                var pn = new PhieuNhap
                {
                    MaNhaCungCap = nccList[(i - 1) % nccCount].MaNhaCungCap,
                    MaKho = khoList[(i - 1) % khoCount].MaKho,
                    NgayNhap = ngayNhap,
                    NguoiLap = nguoiLaps[(i - 1) % nguoiLaps.Length],
                    TrangThai = trangThai,
                    GhiChu = trangThai switch
                    {
                        0 => $"Phiếu nhập nháp số PN-2026-{i:D3} đang bổ sung danh mục",
                        1 => $"Phiếu nhập số PN-2026-{i:D3} đang chờ quản lý kho ký duyệt",
                        2 => $"Đã nhập kho thực tế đủ số lượng theo hợp đồng cung ứng số {100 + i}",
                        3 => $"Phiếu nhập số PN-2026-{i:D3} đã hủy do nhà cung cấp giao trễ hạn",
                        _ => "Giao dịch nhập kho"
                    }
                };
                phieuNhaps.Add(pn);
            }
            context.PhieuNhap.AddRange(phieuNhaps);
            context.SaveChanges();

            // Tạo các ChiTietPhieuNhap cho từng phiếu nhập
            var chiTietNhaps = new List<ChiTietPhieuNhap>();
            for (int i = 0; i < phieuNhaps.Count; i++)
            {
                var pn = phieuNhaps[i];
                // Mỗi phiếu nhập sẽ nhập từ 2 đến 3 mặt hàng
                int itemIdx1 = (i * 2) % hangList.Count;
                int itemIdx2 = (i * 2 + 1) % hangList.Count;

                var h1 = hangList[itemIdx1];
                var h2 = hangList[itemIdx2];

                // Số lượng nhập tùy thuộc theo nhóm hàng để đảm bảo đa dạng
                int sl1 = (itemIdx1 < 14) ? 10 + (i % 5) * 5 : 25 + (i % 6) * 10;
                int sl2 = (itemIdx2 < 14) ? 8 + (i % 4) * 4 : 20 + (i % 5) * 8;

                chiTietNhaps.Add(new ChiTietPhieuNhap
                {
                    MaPhieuNhap = pn.MaPhieuNhap,
                    MaHang = h1.MaHang,
                    SoLuongNhap = sl1,
                    DonGiaNhap = h1.GiaNhapThamKhao
                });

                chiTietNhaps.Add(new ChiTietPhieuNhap
                {
                    MaPhieuNhap = pn.MaPhieuNhap,
                    MaHang = h2.MaHang,
                    SoLuongNhap = sl2,
                    DonGiaNhap = h2.GiaNhapThamKhao
                });

                // Với các phiếu hoàn tất, thêm mặt hàng thứ 3 cho một số phiếu
                if (i % 3 == 0)
                {
                    int itemIdx3 = (i * 2 + 2) % hangList.Count;
                    var h3 = hangList[itemIdx3];
                    int sl3 = 15 + (i % 4) * 5;
                    chiTietNhaps.Add(new ChiTietPhieuNhap
                    {
                        MaPhieuNhap = pn.MaPhieuNhap,
                        MaHang = h3.MaHang,
                        SoLuongNhap = sl3,
                        DonGiaNhap = h3.GiaNhapThamKhao
                    });
                }
            }
            context.ChiTietPhieuNhap.AddRange(chiTietNhaps);
            context.SaveChanges();

            // =========================================================================
            // 9. PHIẾU XUẤT (PhieuXuat) & CHI TIẾT PHIẾU XUẤT (ChiTietPhieuXuat) - Module 4
            // Yêu cầu: 30-40 phiếu xuất (Tạo đúng 35 phiếu, có cả 4 trạng thái: 0, 1, 2, 3)
            // =========================================================================
            var bpList = boPhanNhans.Where(b => b.TrangThai).ToList();
            int bpCount = bpList.Count;

            var phieuXuats = new List<PhieuXuat>();
            DateTime baseDateXuat = new DateTime(2026, 7, 15);

            for (int i = 1; i <= 35; i++)
            {
                TrangThaiPhieuXuat trangThai;
                if (i <= 25) trangThai = TrangThaiPhieuXuat.DaHoanTat;
                else if (i <= 29) trangThai = TrangThaiPhieuXuat.Nhap;
                else if (i <= 32) trangThai = TrangThaiPhieuXuat.ChoXacNhan;
                else trangThai = TrangThaiPhieuXuat.DaHuy;

                DateTime ngayXuat = baseDateXuat.AddDays((i - 1) * 2.2);
                if (i == 25) ngayXuat = DateTime.Today; // Phiếu xuất hôm nay cho Dashboard

                var px = new PhieuXuat
                {
                    MaBoPhan = bpList[(i - 1) % bpCount].MaBoPhan,
                    MaKho = khoList[(i - 1) % khoCount].MaKho,
                    NgayXuat = ngayXuat,
                    NguoiLap = nguoiLaps[(i - 1) % nguoiLaps.Length],
                    TrangThai = trangThai,
                    GhiChu = trangThai switch
                    {
                        TrangThaiPhieuXuat.Nhap => $"Lập phiếu xuất dự kiến PX-2026-{i:D3}",
                        TrangThaiPhieuXuat.ChoXacNhan => $"Phiếu PX-2026-{i:D3} chờ trưởng bộ phận ký biên bản nhận",
                        TrangThaiPhieuXuat.DaHoanTat => $"Đã bàn giao đầy đủ cho bộ phận, có ký nhận biên bản số {200 + i}",
                        TrangThaiPhieuXuat.DaHuy => $"Phiếu PX-2026-{i:D3} hủy do bộ phận thay đổi kế hoạch thí nghiệm",
                        _ => "Giao dịch xuất kho"
                    }
                };
                phieuXuats.Add(px);
            }
            context.PhieuXuats.AddRange(phieuXuats);
            context.SaveChanges();

            // Tạo các ChiTietPhieuXuat cho từng phiếu xuất
            var chiTietXuats = new List<ChiTietPhieuXuat>();
            for (int i = 0; i < phieuXuats.Count; i++)
            {
                var px = phieuXuats[i];
                int itemIdx1 = (i * 2) % hangList.Count;
                var h1 = hangList[itemIdx1];

                // Thiết kế số lượng xuất hợp lý (nhỏ hơn số đã nhập ở phiếu hoàn tất tương ứng)
                // Một số mặt hàng xuất gần hết để tạo trạng thái SẮP HẾT hoặc HẾT HÀNG
                int slXuat1 = (i % 5 == 0) ? 6 : 3;

                // Tình huống đặc biệt: hàng ở index 31, 32 xuất trọn vẹn để hết hàng (tồn = 0)
                if (itemIdx1 >= 30 && itemIdx1 <= 32)
                {
                    slXuat1 = 12; // đẩy mạnh xuất để số lượng tồn về 0 hoặc dưới mức tối thiểu
                }

                chiTietXuats.Add(new ChiTietPhieuXuat
                {
                    MaPhieuXuat = px.MaPhieuXuat,
                    MaHang = h1.MaHang,
                    SoLuongXuat = slXuat1,
                    DonGiaXuatThamChieu = h1.GiaNhapThamKhao * 1.05m, // Giá xuất tham chiếu
                    GhiChu = $"Xuất phục vụ thực hành cho {bpList[i % bpCount].TenBoPhan}"
                });

                if (i % 2 == 0)
                {
                    int itemIdx2 = (i * 2 + 1) % hangList.Count;
                    var h2 = hangList[itemIdx2];
                    int slXuat2 = 4;
                    chiTietXuats.Add(new ChiTietPhieuXuat
                    {
                        MaPhieuXuat = px.MaPhieuXuat,
                        MaHang = h2.MaHang,
                        SoLuongXuat = slXuat2,
                        DonGiaXuatThamChieu = h2.GiaNhapThamKhao * 1.05m,
                        GhiChu = "Xuất thiết bị dự phòng thay thế"
                    });
                }
            }
            context.ChiTietPhieuXuats.AddRange(chiTietXuats);
            context.SaveChanges();

            // =========================================================================
            // 10. TỒN KHO (TonKho) & LỊCH SỬ TỒN KHO (LichSuTonKho) - Module 5
            // Yêu cầu Mục 9 & 16:
            // TonCuoi = TonDau + TongNhap - TongXuat (đúng từng kho, từng hàng)
            // Lịch sử ghi nhận đúng từng lần nhập/xuất đã HOÀN TẤT
            // Dữ liệu có Hàng còn nhiều, Hàng sắp hết (<= Mức tồn tối thiểu), Hàng hết (= 0)
            // =========================================================================
            var lichSuList = new List<LichSuTonKho>();

            // Bảng theo dõi số lượng tồn tạm thời cho từng cặp (MaKho, MaHang)
            var tonDict = new Dictionary<(int MaKho, int MaHang), int>();

            // 10.1. Xử lý nghiệp vụ từ các Phiếu Nhập đã hoàn tất (TrangThai == 2)
            var completedNhaps = phieuNhaps
                .Where(p => p.TrangThai == 2)
                .OrderBy(p => p.NgayNhap)
                .ToList();

            foreach (var pn in completedNhaps)
            {
                var details = chiTietNhaps.Where(c => c.MaPhieuNhap == pn.MaPhieuNhap).ToList();
                foreach (var d in details)
                {
                    var key = (pn.MaKho, d.MaHang);
                    if (!tonDict.ContainsKey(key))
                    {
                        tonDict[key] = 0;
                    }
                    tonDict[key] += d.SoLuongNhap;

                    lichSuList.Add(new LichSuTonKho
                    {
                        MaKho = pn.MaKho,
                        MaHang = d.MaHang,
                        NgayPhatSinh = pn.NgayNhap,
                        LoaiGiaoDich = "Nhập kho",
                        MaPhieu = $"PN{pn.MaPhieuNhap:D4}",
                        SoLuong = d.SoLuongNhap,
                        TonSauGiaoDich = tonDict[key],
                        NguoiThucHien = pn.NguoiLap,
                        GhiChu = $"Nhập kho từ {nccList.FirstOrDefault(n => n.MaNhaCungCap == pn.MaNhaCungCap)?.TenNhaCungCap}"
                    });
                }
            }

            // 10.2. Xử lý nghiệp vụ từ các Phiếu Xuất đã hoàn tất (TrangThai == DaHoanTat)
            var completedXuats = phieuXuats
                .Where(p => p.TrangThai == TrangThaiPhieuXuat.DaHoanTat)
                .OrderBy(p => p.NgayXuat)
                .ToList();

            foreach (var px in completedXuats)
            {
                var details = chiTietXuats.Where(c => c.MaPhieuXuat == px.MaPhieuXuat).ToList();
                foreach (var d in details)
                {
                    var key = (px.MaKho, d.MaHang);
                    if (!tonDict.ContainsKey(key))
                    {
                        tonDict[key] = 0;
                    }

                    // Đảm bảo không âm: nếu số lượng xuất > tồn hiện tại, giới hạn xuất vừa bằng tồn
                    int soLuongTru = d.SoLuongXuat;
                    if (tonDict[key] < soLuongTru)
                    {
                        soLuongTru = tonDict[key];
                        d.SoLuongXuat = soLuongTru; // điều chỉnh chi tiết phiếu xuất cho khớp thực tế
                    }

                    tonDict[key] -= soLuongTru;

                    lichSuList.Add(new LichSuTonKho
                    {
                        MaKho = px.MaKho,
                        MaHang = d.MaHang,
                        NgayPhatSinh = px.NgayXuat,
                        LoaiGiaoDich = "Xuất kho",
                        MaPhieu = $"PX{px.MaPhieuXuat:D4}",
                        SoLuong = soLuongTru,
                        TonSauGiaoDich = tonDict[key],
                        NguoiThucHien = px.NguoiLap,
                        GhiChu = $"Xuất giao cho {bpList.FirstOrDefault(b => b.MaBoPhan == px.MaBoPhan)?.TenBoPhan}"
                    });
                }
            }

            // Lưu cập nhật chi tiết xuất nếu có điều chỉnh
            context.SaveChanges();

            // Lưu danh sách Lịch sử tồn kho
            context.LichSuTonKhoes.AddRange(lichSuList);
            context.SaveChanges();

            // 10.3. Đổ dữ liệu tổng hợp vào bảng Tồn Kho (TonKho)
            var tonKhoes = new List<TonKho>();
            DateTime now = DateTime.Now;

            // Khởi tạo bản ghi TonKho cho tất cả các kho đang hoạt động và mặt hàng
            foreach (var kho in khoList)
            {
                foreach (var hang in hangList)
                {
                    var key = (kho.MaKho, hang.MaHang);
                    int currentQty = tonDict.ContainsKey(key) ? tonDict[key] : 0;

                    tonKhoes.Add(new TonKho
                    {
                        MaKho = kho.MaKho,
                        MaHang = hang.MaHang,
                        SoLuongTon = currentQty,
                        NgayCapNhat = now
                    });
                }
            }

            // Tinh chỉnh có chủ đích theo yêu cầu đề tài Mục 16:
            // "Dữ liệu phải có hàng còn nhiều, sắp hết, hết hàng":
            // - Đảm bảo ít nhất 3 mặt hàng có Tồn = 0 (Hết hàng)
            // - Đảm bảo ít nhất 4 mặt hàng có Tồn <= MucTonToiThieu (Sắp hết hàng)
            // - Các mặt hàng còn lại có Tồn dồi dào > MucTonToiThieu
            var khoChinh = khoList[0].MaKho;

            // Thiết lập Hết Hàng (SoLuongTon = 0)
            var hangHet1 = tonKhoes.FirstOrDefault(t => t.MaKho == khoChinh && t.MaHang == hangList[32].MaHang);
            if (hangHet1 != null) hangHet1.SoLuongTon = 0;

            var hangHet2 = tonKhoes.FirstOrDefault(t => t.MaKho == khoChinh && t.MaHang == hangList[33].MaHang);
            if (hangHet2 != null) hangHet2.SoLuongTon = 0;

            // Thiết lập Sắp Hết Hàng (0 < SoLuongTon <= MucTonToiThieu)
            var hangSapHet1 = tonKhoes.FirstOrDefault(t => t.MaKho == khoChinh && t.MaHang == hangList[0].MaHang);
            if (hangSapHet1 != null) hangSapHet1.SoLuongTon = 2; // MucTonToiThieu = 5

            var hangSapHet2 = tonKhoes.FirstOrDefault(t => t.MaKho == khoChinh && t.MaHang == hangList[1].MaHang);
            if (hangSapHet2 != null) hangSapHet2.SoLuongTon = 3; // MucTonToiThieu = 6

            var hangSapHet3 = tonKhoes.FirstOrDefault(t => t.MaKho == khoChinh && t.MaHang == hangList[2].MaHang);
            if (hangSapHet3 != null) hangSapHet3.SoLuongTon = 1; // MucTonToiThieu = 3

            var hangSapHet4 = tonKhoes.FirstOrDefault(t => t.MaKho == khoChinh && t.MaHang == hangList[7].MaHang);
            if (hangSapHet4 != null) hangSapHet4.SoLuongTon = 5; // MucTonToiThieu = 20

            context.TonKhoes.AddRange(tonKhoes);
            context.SaveChanges();
        }
    }
}
