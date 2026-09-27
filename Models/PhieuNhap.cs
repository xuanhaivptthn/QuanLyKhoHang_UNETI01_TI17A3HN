using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    public class PhieuNhap
    {
        [Key]
        public int MaPhieuNhap { get; set; }

        [Required]
        [Display(Name = "Nhà cung cấp")]
        public int MaNhaCungCap { get; set; }

        [Required]
        [Display(Name = "Kho nhập")]
        public int MaKho { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày nhập")]
        public DateTime NgayNhap { get; set; }

        [StringLength(100)]
        [Display(Name = "Người lập")]
        public string? NguoiLap { get; set; }

        // Trạng thái: 0: Nháp, 1: Chờ xác nhận, 2: Đã hoàn tất, 3: Đã hủy
        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 0;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation Properties
        [ForeignKey("MaNhaCungCap")]
        public virtual NhaCungCap? NhaCungCap { get; set; }

        [ForeignKey("MaKho")]
        public virtual Kho? Kho { get; set; }

        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();
    }
}
