using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    // Họ và tên: Trần Xuân Hải
    // Mã sinh viên: 23103100135
    // Phụ trách Module 1: Tài khoản, Đăng nhập, Phân quyền, Loại hàng & Đơn vị tính
    // Tạo model LoaiHang
    [Table("LoaiHang")]
    public class LoaiHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã loại hàng")]
        public int MaLoaiHang { get; set; }

        [Required(ErrorMessage = "Tên loại hàng không được để trống.")]
        [StringLength(150, ErrorMessage = "Tên loại hàng không vượt quá 150 ký tự.")]
        [Display(Name = "Tên loại hàng")]
        public string TenLoaiHang { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThai { get; set; } = true;

        public virtual ICollection<HangHoa> HangHoas { get; set; } = new List<HangHoa>();
    }
}
