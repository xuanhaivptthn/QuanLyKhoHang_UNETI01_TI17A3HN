// Họ và tên: Nguyễn Thị Cúc
// Mã sinh viên: 23103100178
// Phụ trách Module 2: ViewModel tìm kiếm, lọc, sắp xếp và phân trang Hàng hóa

using System;
using System.Collections.Generic;
using QuanLyKhoHang_UNETI01_TI17A3HN.Models;

namespace QuanLyKhoHang_UNETI01_TI17A3HN.ViewModels
{
    public class HangHoaFilterViewModel
    {
        public string? SearchString { get; set; }
        public int? MaLoaiHang { get; set; }
        public int? MaDonViTinh { get; set; }
        public bool? TrangThai { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }
        public int? MaKho { get; set; }
        public string? CanhBao { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 10));
        public List<HangHoa> Items { get; set; } = new List<HangHoa>();
    }
}
