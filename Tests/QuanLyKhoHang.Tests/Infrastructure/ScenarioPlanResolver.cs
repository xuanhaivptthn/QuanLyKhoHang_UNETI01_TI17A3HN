using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuanLyKhoHang.Tests.Infrastructure;

public static class ScenarioPlanResolver
{
    private static readonly Regex InputPair = new(
        @"(?<key>[A-Za-zÀ-ỹ_][A-Za-zÀ-ỹ0-9_]*)\s*=\s*(?<value>[^,;\r\n]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> ManualReviewIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "TC_M3_025", // Concurrent receipt completion needs provider-specific transaction guarantees.
        "TC_M4_021", // Concurrent dispatch completion needs a SQL Server concurrency profile.
        "TC_M6_006", // Migration target is a clean SQL Server instance.
        "TC_M6_010"  // Large data performance needs a dedicated benchmark threshold/environment.
    };

    public static IReadOnlyList<ScenarioPlan> ResolveAll(IEnumerable<ExcelTestCase> testCases)
    {
        return testCases.Select(Resolve).ToArray();
    }

    public static ScenarioPlan Resolve(ExcelTestCase testCase)
    {
        var group = ResolveGroup(testCase);
        var currentScenario = ExcelTestCaseIntegrationTests.ResolveScenario(testCase);
        var input = ParseInput(testCase);

        if (ManualReviewIds.Contains(testCase.Id))
        {
            return new ScenarioPlan(
                testCase, ScenarioGroup.ManualReview, "ManualReview", testCase.Feature,
                JsonSerializer.Serialize(input), testCase.ExpectedResult, testCase.ExpectedResult,
                SeedProfile.Default, AutomationStatus.ManualReview,
                GetManualReviewReason(testCase.Id));
        }

        if (currentScenario is not null)
        {
            var action = currentScenario.Value.ToString();
            return new ScenarioPlan(
                testCase, group, action, ResolveTarget(testCase), JsonSerializer.Serialize(input),
                testCase.ExpectedResult, testCase.ExpectedResult, ResolveSeedProfile(group),
                AutomationStatus.Executable);
        }

        return new ScenarioPlan(
            testCase, group, InferAction(testCase), ResolveTarget(testCase), JsonSerializer.Serialize(input),
            testCase.ExpectedResult, testCase.ExpectedResult, ResolveSeedProfile(group),
            AutomationStatus.NeedsHandler,
            "No shared handler currently verifies this action and its HTTP/database expectations.");
    }

    private static Dictionary<string, string> ParseInput(ExcelTestCase testCase)
    {
        var input = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["raw"] = testCase.TestData
        };

        foreach (Match match in InputPair.Matches(testCase.TestData))
        {
            input[match.Groups["key"].Value] = match.Groups["value"].Value.Trim().Trim('\'');
        }

        return input;
    }

    private static ScenarioGroup ResolveGroup(ExcelTestCase testCase)
    {
        var feature = testCase.Feature;
        if (ManualReviewIds.Contains(testCase.Id)) return ScenarioGroup.ManualReview;
        if (feature is "Tìm kiếm" or "Lọc" or "Sắp xếp" or "Phân trang") return ScenarioGroup.Query;
        if (feature is "Quản lý kho" or "Hàng hóa - CRUD" or "Loại hàng" or "Đơn vị tính"
            or "Nhà cung cấp" or "Bộ phận nhận") return ScenarioGroup.Catalog;
        if (feature is "Lập phiếu nhập" or "Chi tiết phiếu nhập" or "Tính tiền nhập"
            or "Trạng thái & hoàn tất nhập kho" or "Lập phiếu xuất" or "Kiểm tra tồn khi xuất"
            or "Hoàn tất & hủy phiếu xuất" or "Tồn kho" or "Cảnh báo tồn"
            or "Danh sách phiếu nhập" or "Danh sách phiếu xuất") return ScenarioGroup.WarehouseWorkflow;

        return ScenarioGroup.ReportingSecurity;
    }

    private static string InferAction(ExcelTestCase testCase)
    {
        var text = $"{testCase.Feature} {testCase.Title} {testCase.Steps}";
        if (testCase.Feature is "Tìm kiếm" or "Lọc" or "Sắp xếp" or "Phân trang") return "Query.List";
        if (text.Contains("đăng nhập", StringComparison.OrdinalIgnoreCase)) return "Security.Login";
        if (text.Contains("đăng xuất", StringComparison.OrdinalIgnoreCase)) return "Security.Logout";
        if (text.Contains("hoàn tất", StringComparison.OrdinalIgnoreCase)) return "Workflow.Complete";
        if (text.Contains("hủy", StringComparison.OrdinalIgnoreCase)) return "Workflow.Cancel";
        if (text.Contains("xóa", StringComparison.OrdinalIgnoreCase)) return "Catalog.Delete";
        if (text.Contains("sửa", StringComparison.OrdinalIgnoreCase)) return "Catalog.Update";
        if (text.Contains("thêm", StringComparison.OrdinalIgnoreCase)
            || text.Contains("tạo", StringComparison.OrdinalIgnoreCase)) return "Catalog.Create";
        if (text.Contains("xuất", StringComparison.OrdinalIgnoreCase)) return "Workflow.Dispatch";
        if (text.Contains("nhập", StringComparison.OrdinalIgnoreCase)) return "Workflow.Receipt";
        if (text.Contains("thống kê", StringComparison.OrdinalIgnoreCase)
            || text.Contains("dashboard", StringComparison.OrdinalIgnoreCase)) return "Report.Read";
        return "Review.RequiredAssertion";
    }

    private static string ResolveTarget(ExcelTestCase testCase)
    {
        var haystack = $"{testCase.Feature} {testCase.Title}";
        if (haystack.Contains("hàng hóa", StringComparison.OrdinalIgnoreCase)) return "HangHoa";
        if (haystack.Contains("đơn vị tính", StringComparison.OrdinalIgnoreCase)) return "DonViTinh";
        if (haystack.Contains("loại hàng", StringComparison.OrdinalIgnoreCase)) return "LoaiHang";
        if (haystack.Contains("nhà cung cấp", StringComparison.OrdinalIgnoreCase)) return "NhaCungCap";
        if (haystack.Contains("bộ phận", StringComparison.OrdinalIgnoreCase)) return "BoPhanNhan";
        if (haystack.Contains("tài khoản", StringComparison.OrdinalIgnoreCase)) return "TaiKhoan";
        if (haystack.Contains("kho", StringComparison.OrdinalIgnoreCase)) return "Kho";
        if (haystack.Contains("phiếu nhập", StringComparison.OrdinalIgnoreCase)) return "PhieuNhap";
        if (haystack.Contains("phiếu xuất", StringComparison.OrdinalIgnoreCase)) return "PhieuXuat";
        return testCase.Feature;
    }

    private static SeedProfile ResolveSeedProfile(ScenarioGroup group) => group switch
    {
        ScenarioGroup.Catalog => SeedProfile.Catalog,
        ScenarioGroup.Query => SeedProfile.Query,
        ScenarioGroup.WarehouseWorkflow => SeedProfile.StockBoundary,
        _ => SeedProfile.Default
    };

    private static string GetManualReviewReason(string id) => id switch
    {
        "TC_M3_025" or "TC_M4_021" => "Concurrent stock updates require a provider-specific test; SQLite results cannot stand in for SQL Server locking behavior.",
        "TC_M6_006" => "Requires a disposable SQL Server database to verify provider migrations.",
        "TC_M6_010" => "Requires a stable performance environment and an agreed latency/memory threshold.",
        _ => "Requires manual review."
    };
}
