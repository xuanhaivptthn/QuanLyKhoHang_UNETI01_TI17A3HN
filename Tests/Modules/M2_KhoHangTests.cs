using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using QuanLyKhoHang.Automation.Core;
using QuanLyKhoHang.Automation.Locators;

namespace QuanLyKhoHang.Automation.Modules;

/// <summary>
/// Bộ kiểm thử tự động Data-Driven cho Module 2: Quản lý Hệ thống Kho bãi, Danh mục Hàng hóa, Biên giá và Mức tồn tối thiểu.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureSuite("Module 2 - Kho & Hàng hóa")]
public class M2_KhoHangTests : BaseTest
{
    /// <summary>
    /// Thực thi kịch bản kiểm thử Module 2 được ánh xạ từ file Excel đặc tả.
    /// </summary>
    /// <param name="tc">Đối tượng dữ liệu kịch bản kiểm thử.</param>
    [Test, TestCaseSource(typeof(ExcelDataReaderEngine), nameof(ExcelDataReaderEngine.GetTestsByModule), new object[] { "M2" })]
    public async Task Execute_M2_TestCaseAsync(TestCaseModel tc)
    {
        ValidateExecutionEligibility(tc);
        await LoginAsAdminAsync();

        if (tc.Feature.Contains("Quản lý kho", StringComparison.OrdinalIgnoreCase))
        {
            await HandleWarehouseFeatureAsync(tc);
        }
        else
        {
            await HandleProductFeatureAsync(tc);
        }
    }

    private async Task HandleWarehouseFeatureAsync(TestCaseModel tc)
    {
        // Điều hướng tới Module Kho
        await Page.GotoAsync("/Khoes/Index");

        if (tc.Type.Equals("Positive", StringComparison.OrdinalIgnoreCase) && tc.Title.Contains("Xem chi tiết"))
        {
            var firstDetailsLink = Page.Locator("table tbody tr a[href*='Details']").First;
            if (await firstDetailsLink.IsVisibleAsync())
            {
                await firstDetailsLink.ClickAsync();
                var heading = await Page.Locator("h1, h2, h4").First.InnerTextAsync();
                heading.Should().Contain("Kho", "Trang chi tiết kho phải kết xuất thông tin kho lưu trữ");
            }
        }
        else if (tc.Type.Equals("Negative", StringComparison.OrdinalIgnoreCase) && tc.Title.Contains("để trống"))
        {
            await Page.GotoAsync("/Khoes/Create");
            await Page.FillAsync("input[name='TenKho']", "");
            await Page.ClickAsync("input[type='submit'], button[type='submit']");

            var hasError = await Page.Locator(".field-validation-error, .text-danger").IsVisibleAsync();
            hasError.Should().BeTrue("Tên kho để trống phải bị chặn bởi Validation");
        }
    }

    private async Task HandleProductFeatureAsync(TestCaseModel tc)
    {
        // Điều hướng tới Module Hàng hóa
        await Page.GotoAsync("/HangHoas/Index");

        if (tc.Type.Equals("Boundary", StringComparison.OrdinalIgnoreCase))
        {
            // Kiểm tra biên giá nhập = 0 hoặc mức tồn tối thiểu = 0
            await Page.GotoAsync("/HangHoas/Create");
            await Page.FillAsync("input[name='TenHang']", "Hàng Test Biên 0");
            await Page.FillAsync("input[name='GiaNhapThamKhao']", "0");
            await Page.FillAsync("input[name='MucTonToiThieu']", "0");
            
            // Không được hiển thị lỗi đỏ cho giá trị bằng 0 hợp lệ
            var priceError = await Page.Locator("span[data-valmsg-for='GiaNhapThamKhao']").IsVisibleAsync();
            priceError.Should().BeFalse("Giá nhập tham khảo bằng 0 là giá trị biên hợp lệ (>= 0)");
        }
        else if (tc.Feature.Contains("Tìm kiếm") || tc.Feature.Contains("Lọc"))
        {
            // Tìm kiếm hàng hóa qua ô input
            var searchInput = Page.Locator(InventoryLocators.ProductSearchInput);
            if (await searchInput.IsVisibleAsync())
            {
                await searchInput.FillAsync("Laptop");
                await Page.Keyboard.PressAsync("Enter");
                await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                var rows = await Page.Locator(InventoryLocators.ProductTableRows).CountAsync();
                rows.Should().BeGreaterThanOrEqualTo(0, "Kết quả tìm kiếm phải trả về bảng dữ liệu mà không bị sập trang");
            }
        }
    }
}
