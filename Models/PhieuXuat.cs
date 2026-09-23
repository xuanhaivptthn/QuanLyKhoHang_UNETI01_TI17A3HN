using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    public enum TrangThaiPhieuXuat
    {
        [Display(Name = "Nháp")]
        Nhap = 0,

        [Display(Name = "Chờ xác nhận")]
        ChoXacNhan = 1,

        [Display(Name = "Đã hoàn tất")]
        DaHoanTat = 2,

        [Display(Name = "Đã hủy")]
        DaHuy = 3
    }

    [Table("PhieuXuat")]
    public class PhieuXuat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã phiếu xuất")]
        public int MaPhieuXuat { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bộ phận nhận.")]
        [Display(Name = "Bộ phận nhận")]
        public int MaBoPhan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn kho xuất.")]
        [Display(Name = "Kho xuất")]
        public int MaKho { get; set; }

        [Required(ErrorMessage = "Ngày xuất không được để trống.")]
        [Display(Name = "Ngày xuất")]
        [DataType(DataType.Date)]
        public DateTime NgayXuat { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Người lập không được để trống.")]
        [StringLength(100, ErrorMessage = "Người lập không vượt quá 100 ký tự.")]
        [Display(Name = "Người lập")]
        public string NguoiLap { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public TrangThaiPhieuXuat TrangThai { get; set; } = TrangThaiPhieuXuat.Nhap;

        [StringLength(500, ErrorMessage = "Ghi chú không vượt quá 500 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Quan hệ điều hướng (Navigation properties)
        [ForeignKey(nameof(MaBoPhan))]
        public virtual BoPhanNhan? BoPhanNhan { get; set; }

        [ForeignKey(nameof(MaKho))]
        public virtual Kho? Kho { get; set; }

        public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();
    }
}
