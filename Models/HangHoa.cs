using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    public class HangHoa
    {
        [Key]
        [Display(Name = "Mã hàng")]
        public string MaHang { get; set; }

        [Required(ErrorMessage = "Tên hàng là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên hàng")]
        public string TenHang { get; set; }

        [Required(ErrorMessage = "Mã loại hàng là bắt buộc")]
        [Display(Name = "Loại hàng")]
        public string MaLoaiHang { get; set; }

        [Required(ErrorMessage = "Mã đơn vị tính là bắt buộc")]
        [Display(Name = "Đơn vị tính")]
        public string MaDonViTinh { get; set; }

        [Required(ErrorMessage = "Giá nhập tham khảo là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập tham khảo phải >= 0")]
        [Display(Name = "Giá nhập tham khảo")]
        public decimal GiaNhapThamKhao { get; set; }

        [Required(ErrorMessage = "Mức tồn tối thiểu là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Mức tồn tối thiểu phải >= 0")]
        [Display(Name = "Mức tồn tối thiểu")]
        public int MucTonToiThieu { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; }

        [ForeignKey("MaLoaiHang")]
        public virtual LoaiHang LoaiHang { get; set; }

        [ForeignKey("MaDonViTinh")]
        public virtual DonViTinh DonViTinh { get; set; }
    }
}
