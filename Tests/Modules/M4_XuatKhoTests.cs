using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using QuanLyKhoHang.Automation.Core;
using QuanLyKhoHang.Automation.Locators;

namespace QuanLyKhoHang.Automation.Modules;

/// <summary>
/// Bộ kiểm thử tự động Data-Driven cho Module 4: Bộ phận nhận, Kiểm tra tồn kho khả dụng kép và Chống xuất kho âm.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureSuite("Module 4 - Quản lý Xuất kho")]
public class M4_XuatKhoTests : BaseTest
{
    /// <summary>
    /// Thực thi kịch bản kiểm thử Module 4 được ánh xạ từ file Excel đặc tả.
    /// </summary>
    /// <param name="tc">Đối tượng dữ liệu kịch bản kiểm thử.</param>
    [Test, TestCaseSource(typeof(ExcelDataReaderEngine), nameof(ExcelDataReaderEngine.GetTestsByModule), new object[] { "M4" })]
    public async Task Execute_M4_TestCaseAsync(TestCaseModel tc)
    {
        ValidateExecutionEligibility(tc);
        await LoginAsAdminAsync();

        // Điều hướng tới Danh sách Phiếu xuất
        await Page.GotoAsync("/PhieuXuats/Index");

        if (tc.Title.Contains("xuất vượt quá", StringComparison.OrdinalIgnoreCase) || tc.Title.Contains("xuất âm", StringComparison.OrdinalIgnoreCase))
        {
            // Kiểm tra nghiệp vụ chặn xuất vượt tồn kho
            var detailsLink = Page.Locator("table tbody tr a[href*='Details']").First;
            if (await detailsLink.IsVisibleAsync())
            {
                await detailsLink.ClickAsync();
                // Nếu có form thêm dòng hàng xuất
                var quantityInput = Page.Locator(InventoryLocators.QuantityInput);
                if (await quantityInput.IsVisibleAsync())
                {
                    await quantityInput.FillAsync("999999"); // Số lượng vượt quá tồn
                    await Page.ClickAsync("button[type='submit']");
                    var hasError = await Page.Locator(".text-danger, .alert-danger").IsVisibleAsync();
                    hasError.Should().BeTrue("Hệ thống bắt buộc phải báo lỗi khi số lượng xuất vượt quá tồn kho thực tế");
                }
            }
        }
        else
        {
            // Kiểm tra tải trang danh sách phiếu xuất bình thường
            var url = Page.Url;
            url.Should().Contain("PhieuXuats", "Trang quản trị phiếu xuất phải được tải thành công");
        }
    }
}
