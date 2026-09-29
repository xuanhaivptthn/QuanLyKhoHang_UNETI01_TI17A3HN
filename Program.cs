using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang_UNETI01_TI17A3HN.Data;

var builder = WebApplication.CreateBuilder(args);

var primaryConnection = builder.Configuration.GetConnectionString("RemoteDb");

var localDbConnection = builder.Configuration.GetConnectionString("LocalDb");

string connectionString;
if (!string.IsNullOrWhiteSpace(primaryConnection) && !string.IsNullOrWhiteSpace(localDbConnection))
{
    try
    {
        var testBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(primaryConnection);
        if (testBuilder.ConnectTimeout < 60)
        {
            testBuilder.ConnectTimeout = 60;
        }
        using var testConn = new Microsoft.Data.SqlClient.SqlConnection(testBuilder.ConnectionString);
        testConn.Open();
        connectionString = testBuilder.ConnectionString;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warning] Failed to connect to RemoteDb ({ex.Message}). Falling back to LocalDb.");
        connectionString = localDbConnection;
    }
}
else
{
    // cuong moi sua o day
    connectionString = string.IsNullOrWhiteSpace(primaryConnection) ? localDbConnection : primaryConnection;

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'RemoteDb' or 'LocalDb' not found.");
    }
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

string serverName = "";
string databaseName = "";
try
{
    var csb = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
    serverName = csb.DataSource;
    databaseName = csb.InitialCatalog;
}
catch { }

bool isRemote = serverName.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase)
    || (connectionString == primaryConnection && !serverName.Contains("localdb", StringComparison.OrdinalIgnoreCase));

builder.Services.AddSingleton(new QuanLyKhoHang_UNETI01_TI17A3HN.Models.DatabaseInfo
{
    IsRemote = isRemote,
    ServerName = serverName,
    DatabaseName = databaseName
});

// Add services to the container.
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-10.0
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "bophannhan_alias",
    pattern: "BoPhanNhan/{action=Index}/{id?}",
    defaults: new { controller = "BoPhanNhans" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
