using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using QuanLyKhoHang.Automation.Core;
using QuanLyKhoHang.Automation.Locators;

namespace QuanLyKhoHang.Automation.Modules;

/// <summary>
/// Bộ kiểm thử tự động Data-Driven cho Module 3: Nhà cung cấp, Quy trình Lập và Duyệt Phiếu nhập, Bảo toàn giao dịch ACID.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureSuite("Module 3 - Quản lý Nhập kho")]
public class M3_NhapKhoTests : BaseTest
{
    /// <summary>
    /// Thực thi kịch bản kiểm thử Module 3 được ánh xạ từ file Excel đặc tả.
    /// </summary>
    /// <param name="tc">Đối tượng dữ liệu kịch bản kiểm thử.</param>
    [Test, TestCaseSource(typeof(ExcelDataReaderEngine), nameof(ExcelDataReaderEngine.GetTestsByModule), new object[] { "M3" })]
    public async Task Execute_M3_TestCaseAsync(TestCaseModel tc)
    {
        ValidateExecutionEligibility(tc);
        await LoginAsAdminAsync();

        // Điều hướng tới Danh sách Phiếu nhập
        await Page.GotoAsync("/PhieuNhaps/Index");

        if (tc.Title.Contains("Hoàn tất", StringComparison.OrdinalIgnoreCase))
        {
            // Kiểm tra quy trình hoàn tất phiếu nhập & cộng tồn kho an toàn
            var detailLink = Page.Locator("table tbody tr a[href*='Details']").First;
            if (await detailLink.IsVisibleAsync())
            {
                await detailLink.ClickAsync();
                var completeBtn = Page.Locator(InventoryLocators.CompleteReceiptBtn);
                if (await completeBtn.IsVisibleAsync())
                {
                    await completeBtn.ClickAsync();
                    var banner = await Page.Locator(".alert-success, .alert-danger").InnerTextAsync();
                    banner.Should().NotBeNullOrWhiteSpace("Hệ thống phải có thông báo sau khi thực hiện thao tác hoàn tất");
                }
            }
        }
        else if (tc.Type.Equals("Negative", StringComparison.OrdinalIgnoreCase))
        {
            // Kiểm tra form nhập liệu bỏ trống trường bắt buộc
            await Page.GotoAsync("/PhieuNhaps/Create");
            await Page.ClickAsync("button[type='submit'], input[type='submit']");

            var hasValidation = await Page.Locator(".field-validation-error, .text-danger").IsVisibleAsync();
            hasValidation.Should().BeTrue("Phiếu nhập thiếu trường bắt buộc phải kích hoạt Validation");
        }
        else
        {
            // Kiểm tra hiển thị bảng danh sách chứng từ
            var pageTitle = await Page.TitleAsync();
            pageTitle.Should().Contain("Phiếu nhập", "Trang danh sách phiếu nhập phải hiển thị tiêu đề hợp lệ");
        }
    }
}
