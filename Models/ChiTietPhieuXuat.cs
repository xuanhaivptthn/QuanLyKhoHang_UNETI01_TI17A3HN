using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    [Table("ChiTietPhieuXuat")]
    public class ChiTietPhieuXuat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã chi tiết xuất")]
        public int MaChiTietXuat { get; set; }

        [Required(ErrorMessage = "Mã phiếu xuất không được để trống.")]
        [Display(Name = "Mã phiếu xuất")]
        public int MaPhieuXuat { get; set; }

        [Required(ErrorMessage = "Mã hàng không được để trống.")]
        [Display(Name = "Mã hàng")]
        public int MaHang { get; set; }

        [Required(ErrorMessage = "Số lượng xuất không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng xuất phải lớn hơn 0.")]
        [Display(Name = "Số lượng xuất")]
        public int SoLuongXuat { get; set; }

        [Display(Name = "Đơn giá xuất tham chiếu")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không được âm.")]
        public decimal DonGiaXuatThamChieu { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không vượt quá 500 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Quan hệ điều hướng (Navigation properties)
        [ForeignKey(nameof(MaPhieuXuat))]
        public virtual PhieuXuat? PhieuXuat { get; set; }
    }
}
