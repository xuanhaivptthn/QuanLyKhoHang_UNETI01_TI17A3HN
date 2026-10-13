using ClosedXML.Excel;

namespace QuanLyKhoHang.Tests.Infrastructure;

public sealed record ExcelTestCase(
    string Id,
    string Module,
    string Feature,
    string Type,
    string Title,
    string Preconditions,
    string Steps,
    string TestData,
    string ExpectedResult);

public static class ExcelTestCaseLoader
{
    private const string WorkbookName = "TestCase_QuanLyKhoHang_ASPNetCore10.xlsx";
    private const string WorksheetName = "Testcase";

    public static IReadOnlyList<ExcelTestCase> LoadAll()
    {
        var workbookPath = Path.Combine(AppContext.BaseDirectory, WorkbookName);
        if (!File.Exists(workbookPath))
        {
            throw new FileNotFoundException(
                $"Testcase workbook was not copied to the test output directory: {workbookPath}",
                workbookPath);
        }

        using var workbook = new XLWorkbook(workbookPath);
        if (!workbook.TryGetWorksheet(WorksheetName, out var worksheet))
        {
            throw new InvalidDataException($"Worksheet '{WorksheetName}' was not found in '{WorkbookName}'.");
        }

        var usedRange = worksheet.RangeUsed()
            ?? throw new InvalidDataException($"Worksheet '{WorksheetName}' has no testcase data.");
        var headerRow = usedRange.FirstRowUsed();
        var headers = headerRow.Cells()
            .ToDictionary(
                cell => NormalizeHeader(cell.GetString()),
                cell => cell.Address.ColumnNumber,
                StringComparer.OrdinalIgnoreCase);

        var requiredHeaders = new[]
        {
            "ID",
            "Module",
            "Chức năng",
            "Loại test",
            "Tiêu đề / Mô tả",
            "Điều kiện tiên quyết",
            "Các bước thực hiện",
            "Dữ liệu test",
            "Kết quả mong đợi"
        };
        var missingHeaders = requiredHeaders
            .Where(header => !headers.ContainsKey(NormalizeHeader(header)))
            .ToArray();
        if (missingHeaders.Length > 0)
        {
            throw new InvalidDataException(
                $"Worksheet '{WorksheetName}' is missing required columns: {string.Join(", ", missingHeaders)}.");
        }

        var cases = new List<ExcelTestCase>();
        foreach (var row in usedRange.RowsUsed().Skip(1))
        {
            string Read(string name) => row.Cell(headers[NormalizeHeader(name)]).GetString().Trim();

            var id = Read("ID");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            cases.Add(new ExcelTestCase(
                id,
                Read("Module"),
                Read("Chức năng"),
                Read("Loại test"),
                Read("Tiêu đề / Mô tả"),
                Read("Điều kiện tiên quyết"),
                Read("Các bước thực hiện"),
                Read("Dữ liệu test"),
                Read("Kết quả mong đợi")));
        }

        var duplicateIds = cases
            .GroupBy(testCase => testCase.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicateIds.Length > 0)
        {
            throw new InvalidDataException(
                $"Worksheet '{WorksheetName}' contains duplicate testcase IDs: {string.Join(", ", duplicateIds)}.");
        }

        return cases;
    }

    public static ExcelTestCase GetById(string id)
    {
        return LoadAll().SingleOrDefault(testCase =>
                   string.Equals(testCase.Id, id, StringComparison.OrdinalIgnoreCase))
               ?? throw new KeyNotFoundException($"Testcase '{id}' was not found in '{WorkbookName}'.");
    }

    private static string NormalizeHeader(string header)
    {
        return string.Join(' ', header.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
