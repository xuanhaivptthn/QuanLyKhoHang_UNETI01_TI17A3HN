// Họ và tên: Trần Xuân Hải // MSSV: 23103100135
// Mã sinh viên: 23103100135
// Phụ trách Module 1: ViewModel đổi mật khẩu tài khoản

using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels
{
    public class DoiMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string MatKhauHienTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có tối thiểu 6 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không trùng khớp với mật khẩu mới.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}
