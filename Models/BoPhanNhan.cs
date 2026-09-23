using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    [Table("BoPhanNhan")]
    public class BoPhanNhan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã bộ phận")]
        public int MaBoPhan { get; set; }

        [Required(ErrorMessage = "Tên bộ phận không được để trống.")]
        [StringLength(150, ErrorMessage = "Tên bộ phận không vượt quá 150 ký tự.")]
        [Display(Name = "Tên bộ phận")]
        public string TenBoPhan { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Người đại diện không vượt quá 100 ký tự.")]
        [Display(Name = "Người đại diện")]
        public string? NguoiDaiDien { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không vượt quá 20 ký tự.")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? SoDienThoai { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThai { get; set; } = true; // true: Đang hoạt động, false: Ngừng hoạt động

        // Quan hệ điều hướng (Navigation properties)
        public virtual ICollection<PhieuXuat> PhieuXuats { get; set; } = new List<PhieuXuat>();
    }
}
