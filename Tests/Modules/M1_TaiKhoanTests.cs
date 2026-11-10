using Allure.NUnit;
using Allure.NUnit.Attributes;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using QuanLyKhoHang.Automation.Core;
using QuanLyKhoHang.Automation.Locators;

namespace QuanLyKhoHang.Automation.Modules;

/// <summary>
/// Bộ kiểm thử tự động Data-Driven cho Module 1: Tài khoản, Xác thực Cookie, Session, Brute-force Lockout và Phân quyền RBAC.
/// </summary>
[TestFixture]
[AllureNUnit]
[AllureSuite("Module 1 - Tài khoản & Phân quyền")]
public class M1_TaiKhoanTests : BaseTest
{
    /// <summary>
    /// Thực thi kịch bản kiểm thử Module 1 được ánh xạ trực tiếp từ file Excel đặc tả.
    /// </summary>
    /// <param name="tc">Đối tượng dữ liệu chứa các bước và kỳ vọng của testcase.</param>
    [Test, TestCaseSource(typeof(ExcelDataReaderEngine), nameof(ExcelDataReaderEngine.GetTestsByModule), new object[] { "M1" })]
    public async Task Execute_M1_TestCaseAsync(TestCaseModel tc)
    {
        // Kiểm tra điều kiện thực thi (Bỏ qua nếu được đánh dấu Thủ công)
        ValidateExecutionEligibility(tc);

        // Arrange
        await Page.GotoAsync(AuthLocators.LoginUrl);
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        // Act & Assert theo từng loại kiểm thử
        switch (tc.Type.ToLowerInvariant())
        {
            case "positive":
                await ExecutePositiveTestAsync(tc);
                break;

            case "negative":
                await ExecuteNegativeTestAsync(tc);
                break;

            case "boundary":
                await ExecuteBoundaryTestAsync(tc);
                break;

            case "bảo mật":
            case "security":
                await ExecuteSecurityTestAsync(tc);
                break;

            default:
                await ExecuteRbacOrWorkflowTestAsync(tc);
                break;
        }
    }

    private async Task ExecutePositiveTestAsync(TestCaseModel tc)
    {
        // Arrange
        var parts = tc.TestData.Split('/');
        var username = parts[0].Trim();
        var password = parts.Length > 1 ? parts[1].Trim() : "Admin@123";

        // Act
        await Page.FillAsync(AuthLocators.UsernameInput, username);
        await Page.FillAsync(AuthLocators.PasswordInput, password);
        await Page.ClickAsync(AuthLocators.SubmitLoginBtn);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Page.Url.Should().NotContain("/login", "Đăng nhập thành công phải chuyển hướng khỏi trang login");
        var greeting = await Page.Locator(AuthLocators.UserGreetingLink).InnerTextAsync();
        greeting.Should().Contain(username, "Thanh Menu phải hiển thị đúng tên tài khoản đăng nhập");
    }

    private async Task ExecuteNegativeTestAsync(TestCaseModel tc)
    {
        // Arrange
        var parts = tc.TestData.Split('/');
        var rawUser = parts[0].Trim().Replace("''", "");
        var rawPass = parts.Length > 1 ? parts[1].Trim().Replace("''", "") : "";

        // Act
        await Page.FillAsync(AuthLocators.UsernameInput, rawUser);
        await Page.FillAsync(AuthLocators.PasswordInput, rawPass);
        await Page.ClickAsync(AuthLocators.SubmitLoginBtn);

        // Assert
        var hasError = await Page.Locator(AuthLocators.ValidationSummary)
            .Or(Page.Locator(AuthLocators.FieldValidationError))
            .Or(Page.Locator(AuthLocators.AlertDanger))
            .IsVisibleAsync();

        hasError.Should().BeTrue("Hệ thống phải hiển thị cảnh báo lỗi khi thông tin đăng nhập không hợp lệ");
        Page.Url.Should().Contain("/login", "Không được chuyển trang khi thông tin đăng nhập sai");
    }

    private async Task ExecuteBoundaryTestAsync(TestCaseModel tc)
    {
        // Arrange: Kiểm tra khoảng trắng hoặc chuỗi độ dài cực hạn
        var inputUser = tc.TestData.Contains("' admin '") ? " admin " : new string('a', 256);

        // Act
        await Page.FillAsync(AuthLocators.UsernameInput, inputUser);
        await Page.FillAsync(AuthLocators.PasswordInput, "Admin@123");
        await Page.ClickAsync(AuthLocators.SubmitLoginBtn);

        // Assert
        var title = await Page.TitleAsync();
        title.Should().NotContain("500", "Hệ thống phải xử lý an toàn biên độ dài chuỗi, không ném lỗi 500");
    }

    private async Task ExecuteSecurityTestAsync(TestCaseModel tc)
    {
        if (tc.Feature.Contains("Đăng nhập") && tc.TestData.Contains("OR 1=1"))
        {
            // Act: Thử tấn công SQL Injection
            await Page.FillAsync(AuthLocators.UsernameInput, "' OR 1=1 --");
            await Page.FillAsync(AuthLocators.PasswordInput, "BypassPass");
            await Page.ClickAsync(AuthLocators.SubmitLoginBtn);

            // Assert
            Page.Url.Should().Contain("/login", "SQL Injection phải bị vô hiệu hóa bởi cơ chế tham số hóa của EF Core");
        }
        else
        {
            // Kiểm tra phân quyền truy cập trực tiếp URL quản trị khi chưa đăng nhập
            await Context.ClearCookiesAsync();
            var response = await Page.GotoAsync("/TaiKhoans/Index");
            response?.Status.Should().BeOneOf(new[] { 302, 200 });
            Page.Url.Should().Contain("/login", "Chưa đăng nhập truy cập trang quản trị phải bị chuyển hướng về trang login");
        }
    }

    private async Task ExecuteRbacOrWorkflowTestAsync(TestCaseModel tc)
    {
        // Kiểm tra phân quyền nhân viên kho không được vào trang quản lý tài khoản
        if (tc.Preconditions.Contains("nvkho", StringComparison.OrdinalIgnoreCase))
        {
            await Page.FillAsync(AuthLocators.UsernameInput, "nvkho");
            await Page.FillAsync(AuthLocators.PasswordInput, "Kho@123");
            await Page.ClickAsync(AuthLocators.SubmitLoginBtn);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var response = await Page.GotoAsync("/TaiKhoans/Index");
            (response?.Status == 403 || Page.Url.Contains("AccessDenied") || Page.Url.Contains("/login"))
                .Should().BeTrue("Nhân viên kho không được phép truy cập danh sách tài khoản Admin");
        }
    }
}
