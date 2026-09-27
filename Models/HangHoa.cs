using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
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
        public LoaiHang? LoaiHang { get; set; }

        [Required]
        [Display(Name = "Đơn vị tính")]
        public int MaDonViTinh { get; set; }
        [ForeignKey("MaDonViTinh")]
        public DonViTinh? DonViTinh { get; set; }

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

        public ICollection<ChiTietPhieuNhap>? DanhSachChiTietNhap { get; set; }
        public ICollection<ChiTietPhieuXuat>? DanhSachChiTietXuat { get; set; }
        public ICollection<TonKho>? DanhSachTonKho { get; set; }
    }
}
