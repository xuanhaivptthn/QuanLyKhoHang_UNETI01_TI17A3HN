using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    [Table("Kho")]
    public class Kho
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã kho")]
        public int MaKho { get; set; }

        [Required(ErrorMessage = "Tên kho không được để trống.")]
        [StringLength(150, ErrorMessage = "Tên kho không vượt quá 150 ký tự.")]
        [Display(Name = "Tên kho")]
        public string TenKho { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Địa điểm không vượt quá 255 ký tự.")]
        [Display(Name = "Địa điểm")]
        public string? DiaDiem { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThai { get; set; } = true; // true: Đang hoạt động, false: Ngừng hoạt động

        // Quan hệ điều hướng (Navigation properties - ví dụ: Phiếu Nhập / Phiếu Xuất)
        // public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();
        // public virtual ICollection<PhieuXuat> PhieuXuats { get; set; } = new List<PhieuXuat>();
    }
}