using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    // Họ và tên: Trần Xuân Hải
    // Mã sinh viên: 23103100135
    // Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính
    // Tạo model DonViTinh
    [Table("DonViTinh")]
    public class DonViTinh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã đơn vị tính")]
        public int MaDonViTinh { get; set; }

        [Required(ErrorMessage = "Tên đơn vị tính không được để trống.")]
        [StringLength(50, ErrorMessage = "Tên đơn vị tính không vượt quá 50 ký tự.")]
        [Display(Name = "Tên đơn vị tính")]
        public string TenDonViTinh { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThai { get; set; } = true;

        public virtual ICollection<HangHoa> HangHoas { get; set; } = new List<HangHoa>();
    }
}
