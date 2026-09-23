using QuanLyKhoHang_UNETI01_TI17A3HN.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_DHTI17A3HN.Models 
{
    // Họ và tên: Nguyễn Văn Cường
    // Mã sinh viên: 23103100132
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.
    public class TonKho
    {
        [Required(ErrorMessage = "Vui lòng chọn kho")]
        [Display(Name = "Mã Kho")]
        public string MaKho { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hàng hóa")]
        [Display(Name = "Mã Hàng")]
        public string MaHang { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không được âm")] 
        [Display(Name = "Số lượng tồn")]
        public int SoLuongTon { get; set; }

        [Required]
        [Display(Name = "Ngày cập nhật")]
        public DateTime NgayCapNhat { get; set; }

        [ForeignKey("MaKho")]
        public virtual Kho Kho { get; set; }

        [ForeignKey("MaHang")]
        public virtual HangHoa HangHoa { get; set; }
    }
}