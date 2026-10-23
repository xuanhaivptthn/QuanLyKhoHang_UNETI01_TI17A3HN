using System.Text.Json;

namespace QuanLyKhoHang.Tests.Infrastructure;

public enum ScenarioGroup
{
    Catalog,
    Query,
    WarehouseWorkflow,
    ReportingSecurity,
    ManualReview
}

public enum AutomationStatus
{
    Executable,
    NeedsHandler,
    ManualReview
}

public enum SeedProfile
{
    Default,
    Catalog,
    Query,
    StockBoundary
}

public sealed record ScenarioPlan(
    ExcelTestCase TestCase,
    ScenarioGroup Group,
    string Action,
    string Target,
    string InputJson,
    string ExpectedResponse,
    string ExpectedDatabase,
    SeedProfile SeedProfile,
    AutomationStatus Status,
    string? ReviewReason = null)
{
    public IReadOnlyDictionary<string, string> Input =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(InputJson)
        ?? new Dictionary<string, string>();
}
