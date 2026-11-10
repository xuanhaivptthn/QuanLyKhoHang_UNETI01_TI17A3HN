namespace QuanLyKhoHang.Automation.Locators;

/// <summary>
/// Bộ lưu trữ định vị DOM Elements của Modules 2, 3, 4, 5 (Kho, Hàng hóa, Nhập kho, Xuất kho, Tồn kho, Dashboard).
/// </summary>
public static class InventoryLocators
{
    // ==========================================
    // MODULE 2: KHO & HÀNG HÓA
    // ==========================================
    /// <summary>Ô tìm kiếm hàng hóa trên giao diện danh mục.</summary>
    public const string ProductSearchInput = "input[name='searchString'], input[name='timKiem']";

    /// <summary>Dropdown lọc theo loại hàng.</summary>
    public const string CategoryFilterSelect = "select[name='maLoaiHang']";

    /// <summary>Dropdown lọc theo đơn vị tính.</summary>
    public const string UnitFilterSelect = "select[name='maDonViTinh']";

    /// <summary>Dropdown chọn kho lưu trữ.</summary>
    public const string WarehouseSelect = "select[name='MaKho'], select[name='maKho']";

    /// <summary>Nút bấm tạo mới hàng hóa.</summary>
    public const string CreateProductBtn = "a[href*='HangHoas/Create']";

    /// <summary>Các dòng trong bảng danh mục hàng hóa.</summary>
    public const string ProductTableRows = "table.table tbody tr";

    // ==========================================
    // MODULE 3 & 4: CHỨNG TỪ NHẬP - XUẤT KHO
    // ==========================================
    /// <summary>Dropdown chọn nhà cung cấp đối tác.</summary>
    public const string SupplierSelect = "select[name='MaNhaCungCap']";

    /// <summary>Dropdown chọn bộ phận nhận xuất kho.</summary>
    public const string RecipientSelect = "select[name='MaBoPhanNhan']";

    /// <summary>Dropdown chọn mặt hàng trong dòng chi tiết.</summary>
    public const string ProductDetailSelect = "select[name='MaHang'], #selectMaHang";

    /// <summary>Ô nhập số lượng hàng hóa trong chứng từ.</summary>
    public const string QuantityInput = "input[name='SoLuong'], input[name='SoLuongNhap'], input[name='SoLuongXuat']";

    /// <summary>Ô nhập đơn giá của dòng chứng từ.</summary>
    public const string UnitPriceInput = "input[name='DonGiaNhap'], input[name='DonGiaXuat']";

    /// <summary>Nút hoàn tất chứng từ (Kích hoạt Database Transaction).</summary>
    public const string CompleteReceiptBtn = "form[action*='HoanTat'] button[type='submit'], form[action*='HoanTat'] input[type='submit']";

    /// <summary>Badge trạng thái phiếu (Nháp, Đã hoàn tất, Đã hủy).</summary>
    public const string ReceiptStatusBadge = "table tbody tr .badge";

    // ==========================================
    // MODULE 5: TỒN KHO, CẢNH BÁO & DASHBOARD
    // ==========================================
    /// <summary>Các thẻ chỉ số KPI trên màn hình Dashboard.</summary>
    public const string DashboardKpiValues = ".dashboard-card .kpi-value, .dashboard-card h3";

    /// <summary>Canvas biểu đồ cơ cấu loại hàng (Doughnut Chart).</summary>
    public const string CategoryChartCanvas = "canvas#chartLoaiHang";

    /// <summary>Canvas biểu đồ biến động nhập - xuất 7 ngày (Bar Chart).</summary>
    public const string MovementChartCanvas = "canvas#chartNhapXuat";

    /// <summary>Badge cảnh báo hết hàng màu đỏ.</summary>
    public const string StockBadgeDanger = ".badge.bg-danger";

    /// <summary>Badge cảnh báo sắp hết hàng màu vàng.</summary>
    public const string StockBadgeWarning = ".badge.bg-warning";
}
