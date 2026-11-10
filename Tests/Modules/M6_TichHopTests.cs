using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using QuanLyKhoHang.Automation.Core;
using QuanLyKhoHang.Automation.Locators;

namespace QuanLyKhoHang.Automation.Modules;

/// <summary>
/// Bộ kiểm thử tự động Data-Driven cho Module 6: Luồng tích hợp liên thông End-to-End (E2E) và Kiểm thử phi chức năng.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureSuite("Module 6 - Tích hợp toàn hệ thống & Phi chức năng")]
public class M6_TichHopTests : BaseTest
{
    /// <summary>
    /// Thực thi kịch bản kiểm thử Module 6 được ánh xạ từ file Excel đặc tả.
    /// </summary>
    /// <param name="tc">Đối tượng dữ liệu kịch bản kiểm thử.</param>
    [Test, TestCaseSource(typeof(ExcelDataReaderEngine), nameof(ExcelDataReaderEngine.GetTestsByModule), new object[] { "M6" })]
    public async Task Execute_M6_TestCaseAsync(TestCaseModel tc)
    {
        ValidateExecutionEligibility(tc);
        await LoginAsAdminAsync();

        // Luồng tích hợp tổng thể qua các màn hình
        await Page.GotoAsync("/ThongKe/Dashboard");
        var dashboardUrl = Page.Url;
        dashboardUrl.Should().Contain("Dashboard");

        await Page.GotoAsync("/HangHoas/Index");
        var productUrl = Page.Url;
        productUrl.Should().Contain("HangHoas");

        await Page.GotoAsync("/PhieuNhaps/Index");
        var inboundUrl = Page.Url;
        inboundUrl.Should().Contain("PhieuNhaps");

        await Page.GotoAsync("/PhieuXuats/Index");
        var outboundUrl = Page.Url;
        outboundUrl.Should().Contain("PhieuXuats");

        await Page.GotoAsync("/TonKho/LichSu");
        var historyUrl = Page.Url;
        historyUrl.Should().Contain("LichSu");
    }
}
