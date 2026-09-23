using Microsoft.EntityFrameworkCore;

public class QuanLyKhoHang_UNETI01_TI17A3HNContext(DbContextOptions<QuanLyKhoHang_UNETI01_TI17A3HNContext> options) : DbContext(options)
{
    public DbSet<QuanLyKhoHang_UNETI01_TI17A3HN.Models.Kho> Kho { get; set; } = default!;
}
