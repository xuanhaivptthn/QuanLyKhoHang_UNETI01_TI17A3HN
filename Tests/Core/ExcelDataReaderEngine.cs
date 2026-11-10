using ClosedXML.Excel;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace QuanLyKhoHang.Automation.Core;

/// <summary>
/// Engine phân tích tệp Excel đặc tả, đọc dữ liệu động và chuyển đổi thành tập dữ liệu NUnit TestCaseSource.
/// </summary>
public static class ExcelDataReaderEngine
{
    private static readonly Dictionary<string, List<TestCaseModel>> CacheByModule = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object LockObj = new();

    /// <summary>
    /// Nạp danh sách testcase từ Excel lọc theo mã module (M1, M2, M3, M4, M5, M6).
    /// </summary>
    /// <param name="moduleCode">Mã tiền tố module cần lấy.</param>
    /// <returns>Tập hợp các TestCaseModel tương ứng.</returns>
    public static IReadOnlyList<TestCaseModel> LoadModuleCases(string moduleCode)
    {
        lock (LockObj)
        {
            if (CacheByModule.Count == 0)
            {
                ReadExcelWorkbook();
            }

            return CacheByModule.TryGetValue(moduleCode, out var list) ? list : Array.Empty<TestCaseModel>();
        }
    }

    /// <summary>
    /// Cung cấp dữ liệu cho thuộc tính [TestCaseSource] của NUnit theo Module.
    /// </summary>
    /// <param name="moduleCode">Mã Module (M1, M2, M3, M4, M5, M6).</param>
    /// <returns>Tập hợp TestCaseData cho NUnit Runner.</returns>
    public static IEnumerable<TestCaseData> GetTestsByModule(string moduleCode)
    {
        var cases = LoadModuleCases(moduleCode);
        foreach (var tc in cases)
        {
            var testData = new TestCaseData(tc)
                .SetName($"{tc.Id}_{SanitizeTestName(tc.Title)}")
                .SetDescription($"[{tc.Type}] {tc.Title} | Kỳ vọng: {tc.ExpectedResult}")
                .SetCategory(moduleCode.ToUpperInvariant())
                .SetProperty("Id", tc.Id)
                .SetProperty("Priority", tc.Priority)
                .SetProperty("Feature", tc.Feature)
                .SetProperty("TestType", tc.Type);

            yield return testData;
        }
    }

    private static void ReadExcelWorkbook()
    {
        var basePath = AppContext.BaseDirectory;
        var possiblePaths = new[]
        {
            Path.Combine(basePath, "TestData", "TestCase_QuanLyKhoHang_ASPNetCore10.xlsx"),
            Path.Combine(basePath, "TestCase_QuanLyKhoHang_ASPNetCore10.xlsx"),
            Path.Combine(Directory.GetCurrentDirectory(), "TestData", "TestCase_QuanLyKhoHang_ASPNetCore10.xlsx"),
            Path.Combine(Directory.GetCurrentDirectory(), "TestCase_QuanLyKhoHang_ASPNetCore10.xlsx")
        };

        var filePath = possiblePaths.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("Không tìm thấy file Excel testcase trong các thư mục tìm kiếm.");

        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet("Testcase");
        var rangeUsed = worksheet.RangeUsed()
            ?? throw new InvalidDataException("Sheet 'Testcase' không chứa dữ liệu.");

        var rows = rangeUsed.RowsUsed().Skip(1); // Bỏ qua dòng tiêu đề

        foreach (var row in rows)
        {
            var id = row.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(id)) continue;

            var moduleRaw = row.Cell(2).GetString().Trim();
            var moduleKey = moduleRaw.Split('-')[0].Trim(); // Lấy "M1", "M2", ...

            var model = new TestCaseModel
            {
                Id = id,
                Module = moduleRaw,
                Feature = row.Cell(3).GetString().Trim(),
                Type = row.Cell(4).GetString().Trim(),
                Title = row.Cell(5).GetString().Trim(),
                Preconditions = row.Cell(6).GetString().Trim(),
                Steps = row.Cell(7).GetString().Trim(),
                TestData = row.Cell(8).GetString().Trim(),
                ExpectedResult = row.Cell(9).GetString().Trim(),
                Priority = row.Cell(10).GetString().Trim(),
                AutomationType = row.Cell(11).GetString().Trim(),
                Status = row.Cell(13).GetString().Trim()
            };

            if (!CacheByModule.ContainsKey(moduleKey))
            {
                CacheByModule[moduleKey] = new List<TestCaseModel>();
            }
            CacheByModule[moduleKey].Add(model);
        }
    }

    private static string SanitizeTestName(string input)
    {
        var sanitized = Regex.Replace(input, @"[^\w\d_]", "_");
        return sanitized.Length > 60 ? sanitized.Substring(0, 60) : sanitized;
    }
}
