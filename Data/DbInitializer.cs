// Họ và tên: Cả nhóm
// - SV1: Trần Xuân Hải (23103100135) - Phụ trách Module 1: Tài khoản, Loại hàng, Đơn vị tính
// - SV2: Nguyễn Thị Cúc (23103100178) - Phụ trách Module 2: Kho, Hàng hóa
// - SV3: Lê Văn Hùng (23103100177) - Phụ trách Module 3: Nhà cung cấp, Phiếu nhập
// - SV4: Nguyễn Việt Dũng (23103100127) - Phụ trách Module 4: Bộ phận nhận, Phiếu xuất
// - SV5: Nguyễn Văn Cường (23103100132) - Phụ trách Module 5: Tồn kho, Lịch sử, Thống kê
// Nội dung thực hiện: Lớp DbInitializer tự động kiểm tra và nạp dữ liệu mẫu ban đầu theo Mục 16 của đề bài

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Kiểm tra kết nối CSDL an toàn
            try
            {
                if (!context.Database.CanConnect())
                {
                    return;
                }

                // Nếu chưa có tài khoản người dùng, tiến hành nạp đầy đủ dữ liệu mẫu
                if (!context.TaiKhoans.Any())
                {
                    context.ReseedData();
                }
            }
            catch
            {
                // Bỏ qua lỗi nếu đang chạy migration hoặc môi trường in-memory test
            }
        }
    }
}
