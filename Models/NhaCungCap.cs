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
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
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
