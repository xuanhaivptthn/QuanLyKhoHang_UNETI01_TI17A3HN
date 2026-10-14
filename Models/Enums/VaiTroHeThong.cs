// Họ và tên: Trần Xuân Hải
// Mã sinh viên: 23103100135
// Phụ trách Module 1: Enum VaiTroHeThong chuẩn hóa vai trò hệ thống, loại trừ magic string

using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models.Enums
{
    public enum VaiTroHeThong
    {
        [Display(Name = "Quản trị viên")]
        Admin,

        [Display(Name = "Nhân viên kho")]
        NhanVienKho,

        [Display(Name = "Bộ phận nhận")]
        BoPhanNhan
    }

    public static class VaiTroHeThongConst
    {
        public const string Admin = "Admin";
        public const string NhanVienKho = "NhanVienKho";
        public const string BoPhanNhan = "BoPhanNhan";
    }
}
