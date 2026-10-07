using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    public class ChiTietPhieuNhap
    {
        // Họ và tên: Lê Văn Hùng
        // Mã sinh viên: 23103100177
        // Phần này để quản lý thông tin chi tiết hàng hóa số lượng, đơn giá. 
        [Key]
        public int MaChiTietNhap { get; set; }

        [Required]
        public int MaPhieuNhap { get; set; }

        [Required]
        [Display(Name = "Hàng hóa")]
        public int MaHang { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0.")]
        [Display(Name = "Số lượng nhập")]
        public int SoLuongNhap { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá nhập phải >= 0.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá nhập")]
        public decimal DonGiaNhap { get; set; }

        // Thành tiền = SoLuongNhap * DonGiaNhap (Chỉ xem, không cho nhập)
        [NotMapped]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien => SoLuongNhap * DonGiaNhap;

        // Navigation Properties
        [ForeignKey("MaPhieuNhap")]
        public virtual PhieuNhap? PhieuNhap { get; set; }

        [ForeignKey("MaHang")]
        public virtual HangHoa? HangHoa { get; set; }
    }
}
