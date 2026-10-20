using System.Text.RegularExpressions;
using QuanLyKhoHang.Tests.Infrastructure;
using Xunit;

namespace QuanLyKhoHang.Tests;

public sealed class TestcaseSourceConsistencyTests
{
    [Fact]
    public void Workbook_and_text_exports_contain_the_same_testcase_ids()
    {
        var workbookIds = ExcelTestCaseLoader.LoadAll()
            .Select(testCase => testCase.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.NotEmpty(workbookIds);
        Assert.Equal(workbookIds.Order(StringComparer.OrdinalIgnoreCase), ReadIds("testcase_dump.txt").Order(StringComparer.OrdinalIgnoreCase));
        Assert.Equal(workbookIds.Order(StringComparer.OrdinalIgnoreCase), ReadIds("testcase_summary.txt").Order(StringComparer.OrdinalIgnoreCase));
    }

    private static HashSet<string> ReadIds(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, fileName);
        Assert.True(File.Exists(path), $"Testcase file missing from test output: {path}");
        return Regex.Matches(File.ReadAllText(path), @"\bTC_M\d+_\d+\b")
            .Select(match => match.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
