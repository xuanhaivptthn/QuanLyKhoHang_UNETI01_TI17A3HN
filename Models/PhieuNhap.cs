using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    public enum TrangThaiPhieuNhap
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

    public class PhieuNhap
    {
        // Họ và tên: Lê Văn Hùng
        // Mã sinh viên: 23103100177
        // Phụ trách Module 3: Nhà cung cấp, Phiếu nhập, Chi tiết phiếu nhập
        [Key]
        public int MaPhieuNhap { get; set; }

        [Required]
        [Display(Name = "Nhà cung cấp")]
        public int MaNhaCungCap { get; set; }

        [Required]
        [Display(Name = "Kho nhập")]
        public int MaKho { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày nhập")]
        public DateTime NgayNhap { get; set; } = DateTime.Today;

        [StringLength(100)]
        [Display(Name = "Người lập")]
        public string? NguoiLap { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiPhieuNhap TrangThai { get; set; } = TrangThaiPhieuNhap.Nhap;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation Properties
        [ForeignKey("MaNhaCungCap")]
        public virtual NhaCungCap? NhaCungCap { get; set; }

        [ForeignKey("MaKho")]
        public virtual Kho? Kho { get; set; }

        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

        [NotMapped]
        [Display(Name = "Tổng tiền")]
        public decimal TongTien => ChiTietPhieuNhaps?.Sum(d => d.ThanhTien) ?? 0m;

        [NotMapped]
        [Display(Name = "Tổng tiền nhập")]
        public decimal TongTienNhap => TongTien;

        [NotMapped]
        [Display(Name = "Tổng số lượng")]
        public int TongSoLuong => ChiTietPhieuNhaps?.Sum(d => d.SoLuongNhap) ?? 0;

        [NotMapped]
        [Display(Name = "Số mặt hàng")]
        public int SoMatHang => ChiTietPhieuNhaps?.Count ?? 0;
    }
}
