using Allure.Net.Commons;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace QuanLyKhoHang.Automation.Core;

/// <summary>
/// Listener chuyên trách ghi nhận kết quả kiểm thử, bắt ngoại lệ và đính kèm báo cáo Allure.
/// </summary>
public static class TestReportListener
{
    /// <summary>
    /// Xử lý kết quả kiểm thử sau khi hoàn tất mỗi kịch bản test.
    /// </summary>
    /// <param name="page">Đối tượng Playwright Page đang tương tác.</param>
    /// <param name="testCase">Mô hình dữ liệu kịch bản test tương ứng.</param>
    public static async Task HandleTestOutcomeAsync(IPage? page, TestCaseModel testCase)
    {
        var context = TestContext.CurrentContext;
        var status = context.Result.Outcome.Status;

        AllureLifecycle.Instance.UpdateTestCase(t => t.name = $"[{testCase.Id}] {testCase.Title}");
        AllureApi.AddLabels(
            Label.Suite(testCase.Module),
            Label.Feature(testCase.Feature),
            Label.Severity(MapSeverity(testCase.Priority))
        );

        if (status == TestStatus.Failed)
        {
            var message = context.Result.Message ?? "Test failed with no message.";
            var stackTrace = context.Result.StackTrace ?? string.Empty;

            TestContext.WriteLine($"[FAIL DETECTED] Testcase: {testCase.Id}");
            TestContext.WriteLine($"[Expected]: {testCase.ExpectedResult}");
            TestContext.WriteLine($"[Actual Error]: {message}");

            // Đính kèm dữ liệu phân tích lỗi vào Allure
            AllureApi.AddAttachment("Chi Tiết Kỳ Vọng vs Thực Tế", "text/plain", 
                System.Text.Encoding.UTF8.GetBytes(
                    $"Testcase ID: {testCase.Id}\n" +
                    $"Mục tiêu: {testCase.Title}\n" +
                    $"Dữ liệu đầu vào: {testCase.TestData}\n\n" +
                    $"[EXPECTED RESULT]:\n{testCase.ExpectedResult}\n\n" +
                    $"[ACTUAL RESULT / EXCEPTION]:\n{message}\n\n" +
                    $"[STACK TRACE]:\n{stackTrace}"
                ));

            // Chụp ảnh màn hình đính kèm nếu Page khả dụng
            if (page != null && !page.IsClosed)
            {
                try
                {
                    var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
                    {
                        FullPage = true
                    });
                    AllureApi.AddAttachment($"Screenshot_{testCase.Id}_FAIL", "image/png", screenshotBytes);
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Không thể chụp màn hình lỗi: {ex.Message}");
                }
            }
        }
        else if (status == TestStatus.Skipped)
        {
            TestContext.WriteLine($"[SKIPPED] Testcase: {testCase.Id} - Lý do: {context.Result.Message}");
        }
        else if (status == TestStatus.Passed)
        {
            TestContext.WriteLine($"[PASSED] Testcase: {testCase.Id} - {testCase.Title}");
        }
    }

    private static SeverityLevel MapSeverity(string priority)
    {
        return priority.ToLowerInvariant() switch
        {
            "cao" => SeverityLevel.critical,
            "trung bình" => SeverityLevel.normal,
            "thấp" => SeverityLevel.minor,
            _ => SeverityLevel.normal
        };
    }
}
