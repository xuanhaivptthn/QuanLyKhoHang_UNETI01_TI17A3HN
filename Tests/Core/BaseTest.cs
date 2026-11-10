using Allure.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;
using QuanLyKhoHang.Automation.Locators;
using System.Text.Json;

namespace QuanLyKhoHang.Automation.Core;

/// <summary>
/// Lớp kiểm thử cơ sở cung cấp hạ tầng Playwright, cô lập phiên làm việc và bắt ngoại lệ tự động.
/// </summary>
public abstract class BaseTest
{
    /// <summary>Instance Playwright toàn cục.</summary>
    protected IPlaywright PlaywrightInstance = null!;

    /// <summary>Instance Trình duyệt Playwright.</summary>
    protected IBrowser Browser = null!;

    /// <summary>Ngữ cảnh trình duyệt độc lập cho từng kịch bản test (Data Isolation).</summary>
    protected IBrowserContext Context = null!;

    /// <summary>Trang web tương tác hiện hành.</summary>
    protected IPage Page = null!;

    /// <summary>Dữ liệu cấu hình môi trường test.</summary>
    protected JsonElement Config;

    /// <summary>URL gốc của hệ thống ASP.NET Core.</summary>
    protected string BaseUrl = "http://localhost:5000";

    /// <summary>Lưu trữ testcase đang được thực thi trong lượt hiện tại.</summary>
    protected TestCaseModel? CurrentTestCase;

    [OneTimeSetUp]
    public virtual async Task GlobalSetupAsync()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.test.json");
        if (!File.Exists(configPath))
        {
            configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.test.json");
        }

        if (File.Exists(configPath))
        {
            var configContent = await File.ReadAllTextAsync(configPath);
            Config = JsonDocument.Parse(configContent).RootElement.GetProperty("TestSettings");
            BaseUrl = Config.GetProperty("BaseUrl").GetString() ?? "http://localhost:5000";
        }

        PlaywrightInstance = await Playwright.CreateAsync();

        var headless = Config.ValueKind != JsonValueKind.Undefined && Config.TryGetProperty("Headless", out var h) 
            ? h.GetBoolean() 
            : true;

        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless
        });
    }

    [SetUp]
    public virtual async Task EachTestSetupAsync()
    {
        // Khởi tạo context trình duyệt mới để đảm bảo tính cô lập hoàn toàn (Data Isolation)
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 1440, Height = 900 }
        });

        Page = await Context.NewPageAsync();

        var timeout = Config.ValueKind != JsonValueKind.Undefined && Config.TryGetProperty("DefaultTimeoutMs", out var t)
            ? t.GetSingle()
            : 15000;

        Page.SetDefaultTimeout(timeout);
    }

    [TearDown]
    public virtual async Task EachTestTearDownAsync()
    {
        if (CurrentTestCase != null)
        {
            await TestReportListener.HandleTestOutcomeAsync(Page, CurrentTestCase);
        }

        if (Page != null && !Page.IsClosed)
        {
            await Page.CloseAsync();
        }

        if (Context != null)
        {
            await Context.CloseAsync();
        }
    }

    [OneTimeTearDown]
    public virtual async Task GlobalTearDownAsync()
    {
        if (Browser != null)
        {
            await Browser.DisposeAsync();
        }
        PlaywrightInstance?.Dispose();
    }

    /// <summary>
    /// Đăng nhập với tài khoản Quản trị viên (Admin) mặc định.
    /// </summary>
    protected async Task LoginAsAdminAsync()
    {
        await Page.GotoAsync(AuthLocators.LoginUrl);
        await Page.FillAsync(AuthLocators.UsernameInput, "admin");
        await Page.FillAsync(AuthLocators.PasswordInput, "Admin@123");
        await Page.ClickAsync(AuthLocators.SubmitLoginBtn);
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của kịch bản kiểm thử dựa trên thiết lập trong file Excel.
    /// Nếu kịch bản là 'Thủ công' hoặc trạng thái 'Skip', lập tức gọi Assert.Ignore.
    /// </summary>
    /// <param name="testCase">Kịch bản kiểm thử cần kiểm tra.</param>
    protected void ValidateExecutionEligibility(TestCaseModel testCase)
    {
        CurrentTestCase = testCase;
        if (testCase.IsManual)
        {
            Assert.Ignore($"[SKIP DỰA TRÊN EXCEL] Testcase '{testCase.Id}' được định dạng '{testCase.AutomationType}' trong file Excel.");
        }
    }
}
