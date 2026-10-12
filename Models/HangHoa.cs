using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    // Họ và tên: Nguyễn Thị Cúc
    // Mã sinh viên: 23103100178
    // Phụ trách Module 2: Quản lý Kho, Hàng hóa & Tra cứu dữ liệu
    public class HangHoa
    {
        [Key]
        public int MaHang { get; set; }

        [Required(ErrorMessage = "Tên hàng không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên hàng")]
        public string TenHang { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Loại hàng")]
        public int MaLoaiHang { get; set; }
        [ForeignKey("MaLoaiHang")]
        public virtual LoaiHang? LoaiHang { get; set; }

        [Required]
        [Display(Name = "Đơn vị tính")]
        public int MaDonViTinh { get; set; }
        [ForeignKey("MaDonViTinh")]
        public virtual DonViTinh? DonViTinh { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập tham khảo phải >= 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá nhập tham khảo")]
        public decimal GiaNhapThamKhao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Mức tồn tối thiểu phải >= 0")]
        [Display(Name = "Mức tồn tối thiểu")]
        public int MucTonToiThieu { get; set; }

        [StringLength(255)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<ChiTietPhieuNhap> DanhSachChiTietNhap { get; set; } = new List<ChiTietPhieuNhap>();
        public virtual ICollection<ChiTietPhieuXuat> DanhSachChiTietXuat { get; set; } = new List<ChiTietPhieuXuat>();
        public virtual ICollection<TonKho> DanhSachTonKho { get; set; } = new List<TonKho>();
        public virtual ICollection<LichSuTonKho> LichSuTonKhoes { get; set; } = new List<LichSuTonKho>();

        [NotMapped]
        [Display(Name = "Tổng tồn kho")]
        public int TongTonKho => DanhSachTonKho?.Sum(t => t.SoLuongTon) ?? 0;

        [NotMapped]
        [Display(Name = "Trạng thái tồn")]
        public string TrangThaiTon => TongTonKho <= 0 ? "Hết hàng" : (TongTonKho <= MucTonToiThieu ? "Sắp hết" : "Còn hàng");
    }
}
