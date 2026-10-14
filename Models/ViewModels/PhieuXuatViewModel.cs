// Họ và tên: Nguyễn Việt Dũng
// Mã sinh viên: 23103100127
// Phụ trách Module 4: ViewModel Master-Detail Phiếu xuất kho & Check tồn

using System.Collections.Generic;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels
{
    public class PhieuXuatViewModel
    {
        public PhieuXuat PhieuXuat { get; set; } = new PhieuXuat();
        public List<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();
        public decimal TongTienXuat { get; set; }
        public int TongSoLuong { get; set; }
        public int? SoLuongTonKhaDung { get; set; }
    }
}
