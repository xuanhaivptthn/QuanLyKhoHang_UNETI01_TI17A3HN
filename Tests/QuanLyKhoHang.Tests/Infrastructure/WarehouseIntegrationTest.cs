using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;
using Xunit;

namespace QuanLyKhoHang.Tests.Infrastructure;

public abstract class WarehouseIntegrationTest : IAsyncLifetime
{
    protected WarehouseWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new WarehouseWebApplicationFactory();
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        await Factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    protected async Task LoginAsAdminAsync()
    {
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var response = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = "admin",
                ["MatKhau"] = "Admin@123"
            });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    protected async Task<string> GetAntiForgeryTokenAsync(string path)
    {
        using var response = await Client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "<input\\b(?=[^>]*\\bname=\"__RequestVerificationToken\")(?=[^>]*\\bvalue=\"([^\"]+)\")[^>]*>",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, $"No anti-forgery token was rendered at {path}.");
        return HttpUtility.HtmlDecode(match.Groups[1].Value);
    }

    protected async Task<HttpResponseMessage> PostFormAsync(
        string path,
        string token,
        IDictionary<string, string> values)
    {
        var form = new Dictionary<string, string>(values)
        {
            ["__RequestVerificationToken"] = token
        };
        return await Client.PostAsync(path, new FormUrlEncodedContent(form));
    }

    protected async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

}
