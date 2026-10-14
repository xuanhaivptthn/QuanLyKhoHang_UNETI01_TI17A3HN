// Họ và tên: Trần Xuân Hải // MSSV: 23103100135
// Mã sinh viên: 23103100135
// Phụ trách Module 1: ViewModel cập nhật thông tin cá nhân Profile

using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels
{
    public class CapNhatProfileViewModel
    {
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(100, ErrorMessage = "Email không vượt quá 100 ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = string.Empty;
    }
}
