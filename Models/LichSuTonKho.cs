using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.Models
{
    // Họ và tên: Nguyễn Văn Cường
    // Mã sinh viên: 23103100132
    // Phần này để lưu lịch sử nhập xuất kho. 
    // Nội dung thực hiện: Tồn kho, cảnh báo tồn, lịch sử nhập xuất, Dashboard và thống kê LINQ.
    public class LichSuTonKho
    {
        [Key]
        public int MaLichSu { get; set; } 

        [Required]
        public int MaHang { get; set; }

        [Required]
        public int MaKho { get; set; }

        [Display(Name = "Ngày phát sinh")]
        public DateTime NgayPhatSinh { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Loại giao dịch")]
        public string LoaiGiaoDich { get; set; } = string.Empty; // sẽ lưu giá trị nhập hoặc xuất

        [StringLength(50)]
        [Display(Name = "Mã Phiếu")]
        public string? MaPhieu { get; set; } 

        [Required]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        [Display(Name = "Tồn sau giao dịch")]
        public int TonSauGiaoDich { get; set; } 

        [StringLength(100)]
        [Display(Name = "Người thực hiện")]
        public string? NguoiThucHien { get; set; } 

        [StringLength(255)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation Properties
        // [ForeignKey("MaHang")]
        // public virtual HangHoa? HangHoa { get; set; }

        [ForeignKey("MaKho")]
        public virtual Kho? Kho { get; set; }
    }
}