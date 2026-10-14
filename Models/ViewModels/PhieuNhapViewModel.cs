// Họ và tên: Lê Văn Hùng
// Mã sinh viên: 23103100177
// Phụ trách Module 3: ViewModel Master-Detail Phiếu nhập kho

using System.Collections.Generic;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels
{
    public class PhieuNhapViewModel
    {
        public PhieuNhap PhieuNhap { get; set; } = new PhieuNhap();
        public List<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();
        public decimal TongTienNhap { get; set; }
        public int TongSoLuong { get; set; }
    }
}
