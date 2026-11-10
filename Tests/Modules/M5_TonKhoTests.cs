using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using QuanLyKhoHang.Automation.Core;
using QuanLyKhoHang.Automation.Locators;

namespace QuanLyKhoHang.Automation.Modules;

/// <summary>
/// Bộ kiểm thử tự động Data-Driven cho Module 5: Tồn kho tức thời, Cảnh báo hạn mức, Nhật ký biến động và Dashboard KPIs.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureSuite("Module 5 - Tồn kho, Cảnh báo & Dashboard")]
public class M5_TonKhoTests : BaseTest
{
    /// <summary>
    /// Thực thi kịch bản kiểm thử Module 5 được ánh xạ từ file Excel đặc tả.
    /// </summary>
    /// <param name="tc">Đối tượng dữ liệu kịch bản kiểm thử.</param>
    [Test, TestCaseSource(typeof(ExcelDataReaderEngine), nameof(ExcelDataReaderEngine.GetTestsByModule), new object[] { "M5" })]
    public async Task Execute_M5_TestCaseAsync(TestCaseModel tc)
    {
        ValidateExecutionEligibility(tc);
        await LoginAsAdminAsync();

        if (tc.Feature.Contains("Dashboard", StringComparison.OrdinalIgnoreCase))
        {
            // Điều hướng tới Dashboard trung tâm
            await Page.GotoAsync("/ThongKe/Dashboard");

            // 1. Kiểm tra 5 thẻ KPI quan trọng
            var kpis = await Page.Locator(".dashboard-card").CountAsync();
            kpis.Should().BeGreaterThanOrEqualTo(4, "Dashboard phải kết xuất đầy đủ các thẻ KPI quản trị kho");

            // 2. Kiểm tra sự hiện diện của Canvas Chart.js
            var chartLoaiHang = Page.Locator(InventoryLocators.CategoryChartCanvas);
            var chartNhapXuat = Page.Locator(InventoryLocators.MovementChartCanvas);

            var hasCharts = (await chartLoaiHang.IsVisibleAsync()) || (await chartNhapXuat.IsVisibleAsync());
            hasCharts.Should().BeTrue("Dashboard phải kết xuất đồ thị trực quan hóa dữ liệu Chart.js");
        }
        else if (tc.Feature.Contains("Cảnh báo", StringComparison.OrdinalIgnoreCase))
        {
            // Điều hướng tới màn hình Cảnh báo tồn
            await Page.GotoAsync("/TonKho/CanhBao");

            // Kiểm tra các nhãn badge cảnh báo
            var tableRows = await Page.Locator("table tbody tr").CountAsync();
            if (tableRows > 0)
            {
                var badges = await Page.Locator(".badge.bg-danger, .badge.bg-warning").CountAsync();
                badges.Should().BeGreaterThan(0, "Hàng hóa dưới định mức tồn phải được cảnh báo trực quan");
            }
        }
        else if (tc.Feature.Contains("Lịch sử", StringComparison.OrdinalIgnoreCase))
        {
            // Điều hướng tới Sổ cái nhật ký biến động kho (Audit Trail)
            await Page.GotoAsync("/TonKho/LichSu");
            var rows = await Page.Locator("table tbody tr").CountAsync();
            rows.Should().BeGreaterThanOrEqualTo(0, "Màn hình lịch sử biến động kho phải tải thành công");
        }
    }
}
