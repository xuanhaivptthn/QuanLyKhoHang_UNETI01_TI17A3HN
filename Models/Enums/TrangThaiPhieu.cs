// Họ và tên: Lê Văn Hùng & Nguyễn Việt Dũng
// Mã sinh viên: 23103100177 - 23103100127
// Phụ trách Module 3 & 4: Enum TrangThaiPhieu chuẩn hóa trạng thái phiếu nhập và phiếu xuất

using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models.Enums
{
    public enum TrangThaiPhieu
    {
        [Display(Name = "Nháp")]
        Nhap = 0,

        [Display(Name = "Chờ xác nhận")]
        ChoXacNhan = 1,

        [Display(Name = "Đã hoàn tất")]
        DaHoanTat = 2,

        [Display(Name = "Đã hủy")]
        DaHuy = 3
    }
}
