using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    public class NhaCungCap
    {
        // Họ và tên: Lê Văn Hùng
        // Mã sinh viên: 23103100177
        // Phần này để quản lý nguồn cung hàng hóa. 
        [Key]
        public int MaNhaCungCap { get; set; }

        [Required(ErrorMessage = "Tên nhà cung cấp bắt buộc nhập.")]
        [StringLength(150)]
        [Display(Name = "Tên nhà cung cấp")]
        public string TenNhaCungCap { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [RegularExpression(@"^(?:0(?:[ .()-]?[0-9]){9}|\+84(?:[ .()-]?[0-9]){9})$", ErrorMessage = "Số điện thoại phải có 10 chữ số Việt Nam, bắt đầu bằng 0 hoặc +84.")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [RegularExpression(@"^[^@\s]+@[^@\s.]+(?:\.[^@\s.]+)+$", ErrorMessage = "Email phải đúng định dạng, ví dụ ten@congty.com.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true = Hoạt động, false = Ngừng hoạt động
        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();
    }
}
