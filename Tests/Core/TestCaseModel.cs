namespace QuanLyKhoHang.Automation.Core;

/// <summary>
/// Đại diện cho một bản ghi kiểm thử trích xuất tự động từ workbook Excel TestCase_QuanLyKhoHang_ASPNetCore10.xlsx.
/// </summary>
public sealed class TestCaseModel
{
    /// <summary>Mã định danh testcase (VD: TC_M1_001, TC_M2_005).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Tên nhóm Module (VD: M1 - Tài khoản &amp; Phân quyền).</summary>
    public string Module { get; init; } = string.Empty;

    /// <summary>Chức năng nghiệp vụ cần kiểm thử (VD: Đăng nhập, Hàng hóa - CRUD).</summary>
    public string Feature { get; init; } = string.Empty;

    /// <summary>Loại kiểm thử (Positive, Negative, Boundary, Bảo mật / Security).</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Tiêu đề tóm tắt mục tiêu kịch bản test.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Điều kiện trạng thái hệ thống cần có trước khi chạy test.</summary>
    public string Preconditions { get; init; } = string.Empty;

    /// <summary>Các bước thực hiện tương tác UI hoặc API.</summary>
    public string Steps { get; init; } = string.Empty;

    /// <summary>Dữ liệu đầu vào phục vụ kịch bản.</summary>
    public string TestData { get; init; } = string.Empty;

    /// <summary>Kết quả kỳ vọng cần assert.</summary>
    public string ExpectedResult { get; init; } = string.Empty;

    /// <summary>Mức độ ưu tiên (Cao, Trung bình, Thấp).</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Phương thức tự động hóa (Integration (WAF), Unit, Thủ công).</summary>
    public string AutomationType { get; init; } = string.Empty;

    /// <summary>Trạng thái hiện thời (Đã test, Chưa test, Skip).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Xác định xem testcase này có phải làm thủ công hoặc được chỉ định bỏ qua không.
    /// </summary>
    public bool IsManual => AutomationType.Contains("Thủ công", StringComparison.OrdinalIgnoreCase)
                         || Status.Contains("Skip", StringComparison.OrdinalIgnoreCase);

    public override string ToString() => $"[{Id}] {Module} - {Title} ({Type})";
}
