using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Tests.Infrastructure;
using Xunit;

namespace QuanLyKhoHang.Tests;

public sealed class ExcelTestCaseIntegrationTests : WarehouseIntegrationTest
{
    public static IEnumerable<object[]> ExcelTestCases =>
        ExcelTestCaseLoader.LoadAll()
            .Select(ScenarioPlanResolver.Resolve)
            .Where(plan => plan.Status == AutomationStatus.Executable)
            .Select(plan => new object[] { plan.TestCase.Id, plan.Action, plan.SeedProfile });

    [Theory]
    [MemberData(nameof(ExcelTestCases))]
    public async Task Execute_testcase_from_workbook(string testcaseId, string action, SeedProfile seedProfile)
    {
        var testCase = ExcelTestCaseLoader.GetById(testcaseId);
        var plan = ScenarioPlanResolver.Resolve(testCase);
        Assert.Equal(AutomationStatus.Executable, plan.Status);
        Assert.Equal(action, plan.Action);
        Assert.False(string.IsNullOrWhiteSpace(plan.ExpectedResponse));
        Assert.False(string.IsNullOrWhiteSpace(plan.ExpectedDatabase));
        await Factory.InitializeDatabaseAsync(seedProfile);
        var runner = new SharedScenarioRunner(new Dictionary<string, Func<ScenarioPlan, Task>>
        {
            [nameof(Scenario.Login)] = current => VerifyLoginAsync(current.Input["raw"]),
            [nameof(Scenario.InvalidLogin)] = current => VerifyInvalidLoginAsync(current.Input["raw"], current.TestCase.Type),
            [nameof(Scenario.Logout)] = _ => VerifyLogoutAsync(),
            [nameof(Scenario.AnonymousAccess)] = _ => VerifyAnonymousAccessAsync(),
            [nameof(Scenario.WarehouseAuthorization)] = _ => VerifyWarehouseUserAuthorizationAsync(),
            [nameof(Scenario.DraftReceipt)] = current => VerifyDraftReceiptAsync(current.Input["raw"]),
            [nameof(Scenario.ReceiptDetail)] = current => VerifyReceiptDetailAsync(current.Input["raw"]),
            [nameof(Scenario.ReceiptTotal)] = current => VerifyReceiptTotalAsync(current.Input["raw"]),
            [nameof(Scenario.DraftDispatch)] = current => VerifyDraftDispatchAsync(current.Input["raw"]),
            [nameof(Scenario.CompleteDispatch)] = current => VerifyCompletedDispatchAsync(current.TestCase.Title, current.Input["raw"]),
            [nameof(Scenario.ReceiptHistory)] = current => VerifyReceiptHistoryAsync(current.Input["raw"]),
            [nameof(Scenario.QueryPagination)] = current => VerifyQueryPaginationAsync(current.Input["raw"]),
            [nameof(Scenario.QueryList)] = current => VerifyQueryListAsync(current.Input["raw"]),
            [nameof(Scenario.CatalogCreate)] = current => VerifyCatalogCreateAsync(current),
            [nameof(Scenario.CatalogEdit)] = current => VerifyCatalogEditAsync(current),
            [nameof(Scenario.CatalogRejectEmptyName)] = current => VerifyCatalogRejectEmptyNameAsync(current),
            [nameof(Scenario.CatalogRejectInvalidProduct)] = current => VerifyCatalogRejectInvalidProductAsync(current)
        });
        await runner.ExecuteAsync(plan);
    }

    public enum Scenario
    {
        Login, InvalidLogin, Logout, AnonymousAccess, WarehouseAuthorization,
        DraftReceipt, ReceiptDetail, ReceiptTotal, DraftDispatch, CompleteDispatch, ReceiptHistory,
        QueryPagination, QueryList, CatalogCreate, CatalogEdit, CatalogRejectEmptyName, CatalogRejectInvalidProduct
    }

    public static Scenario? ResolveScenario(ExcelTestCase testCase)
    {
        var feature = testCase.Feature;
        var title = testCase.Title;
        var type = testCase.Type;

        if (feature.Contains("Phân trang", StringComparison.OrdinalIgnoreCase)
            && Regex.IsMatch(testCase.TestData, @"^\s*page\s*=\s*(?:1|2|N|0\s*/\s*-1|9999)\s*$", RegexOptions.IgnoreCase))
            return Scenario.QueryPagination;
        if ((feature is "Tìm kiếm" or "Lọc" or "Sắp xếp" or "Phân trang")
            && Regex.IsMatch(testCase.TestData,
                @"\b(?:search|maLoaiHang|maDonViTinh|maKho|trangThai|giaTu|giaDen|canhBao|sort|page)\s*=",
                RegexOptions.IgnoreCase)
            && !testCase.TestData.Contains("HH001", StringComparison.OrdinalIgnoreCase)
            && !testCase.Type.Contains("Boundary", StringComparison.OrdinalIgnoreCase))
            return Scenario.QueryList;
        if (testCase.Type.Contains("Positive", StringComparison.OrdinalIgnoreCase)
            && title.Contains("Thêm", StringComparison.OrdinalIgnoreCase)
            && (feature is "Quản lý kho" or "Loại hàng" or "Đơn vị tính" or "Hàng hóa - CRUD" or "Nhà cung cấp" or "Bộ phận nhận"))
            return Scenario.CatalogCreate;
        if (feature == "Hàng hóa - CRUD"
            && testCase.Type.Contains("Boundary", StringComparison.OrdinalIgnoreCase)
            && title.Contains("bằng 0", StringComparison.OrdinalIgnoreCase))
            return Scenario.CatalogCreate;
        if (testCase.Type.Contains("Positive", StringComparison.OrdinalIgnoreCase)
            && title.Contains("Sửa", StringComparison.OrdinalIgnoreCase)
            && (feature is "Quản lý kho" or "Loại hàng" or "Đơn vị tính" or "Hàng hóa - CRUD" or "Nhà cung cấp" or "Bộ phận nhận"))
            return Scenario.CatalogEdit;
        if (testCase.Type.Contains("Negative", StringComparison.OrdinalIgnoreCase)
            && title.Contains("trống", StringComparison.OrdinalIgnoreCase)
            && (feature is "Quản lý kho" or "Loại hàng" or "Đơn vị tính" or "Hàng hóa - CRUD" or "Nhà cung cấp" or "Bộ phận nhận"))
            return Scenario.CatalogRejectEmptyName;
        if (feature == "Hàng hóa - CRUD"
            && testCase.Type.Contains("Negative", StringComparison.OrdinalIgnoreCase)
            && (testCase.TestData.Contains("GiaNhapThamKhao", StringComparison.OrdinalIgnoreCase)
                || testCase.TestData.Contains("MucTonToiThieu", StringComparison.OrdinalIgnoreCase)))
            return Scenario.CatalogRejectInvalidProduct;

        if (feature.Contains("Đăng xuất", StringComparison.OrdinalIgnoreCase)) return Scenario.Logout;
        if (feature.Contains("Đăng nhập", StringComparison.OrdinalIgnoreCase))
        {
            if (title.Contains("chưa đăng nhập", StringComparison.OrdinalIgnoreCase)) return Scenario.AnonymousAccess;
            if (type.Contains("Positive", StringComparison.OrdinalIgnoreCase)
                && testCase.TestData.Contains('/')) return Scenario.Login;
            if (type.Contains("Negative", StringComparison.OrdinalIgnoreCase)
                || type.Contains("Security", StringComparison.OrdinalIgnoreCase)
                || type.Contains("Bảo mật", StringComparison.OrdinalIgnoreCase)
                || type.Contains("Boundary", StringComparison.OrdinalIgnoreCase))
            {
                if (testCase.TestData.Contains('/')) return Scenario.InvalidLogin;
            }
        }
        if (feature.Contains("Phân quyền", StringComparison.OrdinalIgnoreCase)
            && title.Contains("trang quản lý tài khoản", StringComparison.OrdinalIgnoreCase)) return Scenario.WarehouseAuthorization;
        if (feature.Contains("Lập phiếu nhập", StringComparison.OrdinalIgnoreCase)
            && type.Contains("Positive", StringComparison.OrdinalIgnoreCase)
            && Regex.IsMatch(testCase.TestData, @"NCC\s*1\b.*\bK\s*1\b", RegexOptions.IgnoreCase)) return Scenario.DraftReceipt;
        if (feature.Contains("Tính tiền nhập", StringComparison.OrdinalIgnoreCase)
            && Regex.Matches(testCase.TestData, @"\(\s*\d+\s*x\s*\d+\s*\)", RegexOptions.IgnoreCase).Count >= 2) return Scenario.ReceiptTotal;
        if ((feature.Contains("Chi tiết phiếu nhập", StringComparison.OrdinalIgnoreCase)
                || (feature.Contains("Tính tiền nhập", StringComparison.OrdinalIgnoreCase)
                    && title.Contains("ThanhTien", StringComparison.OrdinalIgnoreCase))
                || feature.Contains("Tính tiền nhập", StringComparison.OrdinalIgnoreCase))
            && (type.Contains("Positive", StringComparison.OrdinalIgnoreCase)
                || type.Contains("Unit", StringComparison.OrdinalIgnoreCase)
                || type.Contains("Nghiệp vụ", StringComparison.OrdinalIgnoreCase))
            && Regex.IsMatch(testCase.TestData, @"(?:HH\s*1\b.*)?SL\s*=\s*[1-9]\d*.*DonGia\s*=\s*[1-9]\d*", RegexOptions.IgnoreCase)) return Scenario.ReceiptDetail;
        if (feature.Contains("Lập phiếu xuất", StringComparison.OrdinalIgnoreCase)
            && type.Contains("Positive", StringComparison.OrdinalIgnoreCase)
            && Regex.IsMatch(testCase.TestData, @"BP\s*1\b.*\bK\s*1\b", RegexOptions.IgnoreCase)) return Scenario.DraftDispatch;
        if (feature.Contains("Hoàn tất & hủy phiếu xuất", StringComparison.OrdinalIgnoreCase)
            && type.Contains("Nghiệp vụ", StringComparison.OrdinalIgnoreCase)
            && Regex.IsMatch(testCase.TestData, @"Tồn\s*=\s*100", RegexOptions.IgnoreCase)
            && Regex.IsMatch(testCase.TestData, @"xuất\s*=?\s*\d+", RegexOptions.IgnoreCase)) return Scenario.CompleteDispatch;
        if (feature.Contains("Lịch sử nhập xuất", StringComparison.OrdinalIgnoreCase)
            && title.Contains("phiếu sinh bản ghi lịch sử", StringComparison.OrdinalIgnoreCase)) return Scenario.ReceiptHistory;
        return null;
    }

    private async Task VerifyQueryPaginationAsync(string testData)
    {
        await LoginAsync("admin", "Admin@123");
        const int pageSize = 10;
        var totalCount = await WithDbAsync(db => db.HangHoa.CountAsync());
        var requestedPage = Regex.Match(testData, @"page\s*=\s*(?<page>\d+)", RegexOptions.IgnoreCase);
        var requestedValue = requestedPage.Success
            ? int.Parse(requestedPage.Groups["page"].Value, CultureInfo.InvariantCulture)
            : 1;
        var requestedPages = testData.Contains("-1", StringComparison.Ordinal)
            ? new[] { 0, -1 }
            : new[] { requestedValue };
        foreach (var requested in requestedPages)
        {
            await VerifyPaginationPageAsync(requested, totalCount, pageSize);
        }
    }

    private async Task VerifyPaginationPageAsync(int requestedValue, int totalCount, int pageSize)
    {
        var expectedPage = Math.Max(1, requestedValue);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages > 0)
        {
            expectedPage = Math.Min(expectedPage, totalPages);
        }

        var expectedNames = await WithDbAsync(db => db.HangHoa
            .OrderBy(item => item.MaHang)
            .Skip((expectedPage - 1) * pageSize)
            .Take(pageSize)
            .Select(item => item.TenHang)
            .ToListAsync());

        using var response = await Client.GetAsync($"/HangHoa?page={requestedValue}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.NotEmpty(expectedNames);
        Assert.All(expectedNames, name => Assert.Contains(name, html, StringComparison.Ordinal));

        var outsidePageNames = await WithDbAsync(db => db.HangHoa
            .OrderBy(item => item.MaHang)
            .Skip((expectedPage - 1) * pageSize + expectedNames.Count)
            .Select(item => item.TenHang)
            .ToListAsync());
        Assert.All(outsidePageNames, name => Assert.DoesNotContain(name, html, StringComparison.Ordinal));
    }

    private async Task VerifyQueryListAsync(string testData)
    {
        await LoginAsync("admin", "Admin@123");
        var parameters = ParseListQueryParameters(testData);
        string[] sortValues;
        if (!parameters.TryGetValue("sort", out var rawSort))
        {
            sortValues = [string.Empty];
        }
        else if (rawSort.Contains("mucton_asc", StringComparison.OrdinalIgnoreCase))
        {
            sortValues = ["mucton_asc", "mucton_desc"];
        }
        else if (rawSort.Contains("ton_asc", StringComparison.OrdinalIgnoreCase))
        {
            sortValues = ["ton_asc", "ton_desc"];
        }
        else
        {
            sortValues = [rawSort.Trim()];
        }

        foreach (var sortValue in sortValues)
        {
            var queryParameters = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);
            queryParameters.Remove("sort");
            if (!string.IsNullOrWhiteSpace(sortValue))
            {
                queryParameters["sortOrder"] = MapSortValue(sortValue);
            }

            await VerifyListQueryVariantAsync(queryParameters);
        }
    }

    private async Task VerifyListQueryVariantAsync(IReadOnlyDictionary<string, string> input)
    {
        var query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in input)
        {
            switch (key.ToLowerInvariant())
            {
                case "search": query["searchString"] = value; break;
                case "maloaihang": query["maLoaiHang"] = value.StartsWith("LH", StringComparison.OrdinalIgnoreCase) ? value[2..] : value; break;
                case "madonvitinh": query["maDonViTinh"] = value.Equals("Hộp", StringComparison.OrdinalIgnoreCase) ? "1" : value; break;
                case "makho": query["maKho"] = value.StartsWith("K", StringComparison.OrdinalIgnoreCase) ? value[1..] : value; break;
                case "trangthai": query["trangThai"] = value.Equals("Ngừng", StringComparison.OrdinalIgnoreCase) ? "false" : "true"; break;
                case "giatu": query["giaTu"] = value; break;
                case "giaden": query["giaDen"] = value; break;
                case "canhbao": query["canhBao"] = value.Equals("SapHet", StringComparison.OrdinalIgnoreCase) ? "canh_bao" : "binh_thuong"; break;
                case "sortorder": query["sortOrder"] = value; break;
                case "page": query["page"] = value; break;
            }
        }

        var queryString = string.Join('&', query.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        var htmlResponse = await Client.GetAsync($"/HangHoa?{queryString}");
        Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
        var rawHtml = await htmlResponse.Content.ReadAsStringAsync();
        var html = WebUtility.HtmlDecode(rawHtml);

        var expectedRows = await WithDbAsync(async db =>
        {
            var products = await db.HangHoa
                .Include(item => item.LoaiHang)
                .Include(item => item.DonViTinh)
                .Include(item => item.DanhSachTonKho!).ThenInclude(stock => stock.Kho)
                .ToListAsync();

            IEnumerable<QuanLyKhoHang_UNETI01_TI17A3HN.Models.HangHoa> selected = products;
            if (query.TryGetValue("searchString", out var search))
            {
                selected = selected.Where(item => item.TenHang.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || item.MaHang.ToString(CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase)
                    || item.LoaiHang?.TenLoaiHang.Contains(search, StringComparison.OrdinalIgnoreCase) == true
                    || item.DanhSachTonKho?.Any(stock => stock.Kho?.TenKho.Contains(search, StringComparison.OrdinalIgnoreCase) == true) == true);
            }
            if (query.TryGetValue("maLoaiHang", out var category)) selected = selected.Where(item => item.MaLoaiHang == int.Parse(category, CultureInfo.InvariantCulture));
            if (query.TryGetValue("maDonViTinh", out var unit)) selected = selected.Where(item => item.MaDonViTinh == int.Parse(unit, CultureInfo.InvariantCulture));
            if (query.TryGetValue("maKho", out var warehouse)) selected = selected.Where(item => item.DanhSachTonKho?.Any(stock => stock.MaKho == int.Parse(warehouse, CultureInfo.InvariantCulture)) == true);
            if (query.TryGetValue("trangThai", out var status)) selected = selected.Where(item => item.TrangThai == bool.Parse(status));
            if (query.TryGetValue("giaTu", out var minimum)) selected = selected.Where(item => item.GiaNhapThamKhao >= decimal.Parse(minimum, CultureInfo.InvariantCulture));
            if (query.TryGetValue("giaDen", out var maximum)) selected = selected.Where(item => item.GiaNhapThamKhao <= decimal.Parse(maximum, CultureInfo.InvariantCulture));
            if (query.TryGetValue("canhBao", out var alert)) selected = selected.Where(item =>
                (item.DanhSachTonKho?.Sum(stock => stock.SoLuongTon) ?? 0) <= item.MucTonToiThieu == (alert == "canh_bao"));

            var selectedIds = selected.Select(item => item.MaHang).ToArray();
            var selectedQuery = db.HangHoa.Where(item => selectedIds.Contains(item.MaHang));
            var ordered = query.GetValueOrDefault("sortOrder") switch
            {
                "ten_tang_dan" => selectedQuery.OrderBy(item => item.TenHang),
                "ten_giam_dan" => selectedQuery.OrderByDescending(item => item.TenHang),
                "gia_tang_dan" => selectedQuery.OrderBy(item => item.GiaNhapThamKhao),
                "gia_giam_dan" => selectedQuery.OrderByDescending(item => item.GiaNhapThamKhao),
                "muc_ton_tang" => selectedQuery.OrderBy(item => item.MucTonToiThieu),
                "muc_ton_giam" => selectedQuery.OrderByDescending(item => item.MucTonToiThieu),
                "ton_tang" => selectedQuery.OrderBy(item => item.DanhSachTonKho!.Sum(stock => stock.SoLuongTon)),
                "ton_giam" => selectedQuery.OrderByDescending(item => item.DanhSachTonKho!.Sum(stock => stock.SoLuongTon)),
                _ => selectedQuery.OrderBy(item => item.MaHang)
            };
            var page = query.TryGetValue("page", out var requestedPage)
                ? Math.Max(1, int.Parse(requestedPage, CultureInfo.InvariantCulture))
                : 1;
            var pageCount = await selectedQuery.CountAsync();
            var pageTotal = (int)Math.Ceiling(pageCount / 10d);
            if (pageTotal > 0) page = Math.Min(page, pageTotal);
            return await ordered.ThenBy(item => item.MaHang).Skip((page - 1) * 10).Take(10)
                .Select(item => item.TenHang).ToArrayAsync();
        });

        foreach (var name in expectedRows)
        {
            Assert.Contains(name, html, StringComparison.Ordinal);
        }
        var allNames = await WithDbAsync(db => db.HangHoa.Select(item => item.TenHang).ToListAsync());
        Assert.All(allNames.Except(expectedRows), name => Assert.DoesNotContain(name, html, StringComparison.Ordinal));
        if (query.TryGetValue("searchString", out var unsafeSearch) && unsafeSearch.Contains("<script", StringComparison.OrdinalIgnoreCase))
        {
            Assert.DoesNotContain(unsafeSearch, rawHtml, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static Dictionary<string, string> ParseListQueryParameters(string testData)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in Regex.Matches(testData,
            @"(?<key>search|maLoaiHang|maDonViTinh|maKho|trangThai|giaTu|giaDen|canhBao|sort|page)\s*=\s*(?<value>[^,;]+)",
                     RegexOptions.IgnoreCase))
        {
            result[match.Groups["key"].Value] = match.Groups["value"].Value.Trim();
        }
        return result;
    }

    private static string MapSortValue(string value) => value.Trim().ToLowerInvariant() switch
    {
        "ten_asc" => "ten_tang_dan", "ten_desc" => "ten_giam_dan",
        "gia_asc" => "gia_tang_dan", "gia_desc" => "gia_giam_dan",
        "mucton_asc" => "muc_ton_tang", "mucton_desc" => "muc_ton_giam",
        "ton_asc" => "ton_tang", "ton_desc" => "ton_giam",
        _ => value
    };

    private async Task VerifyCatalogCreateAsync(ScenarioPlan plan)
    {
        await LoginAsync("admin", "Admin@123");
        var testCase = plan.TestCase;
        var fields = ParseFormValues(testCase.TestData);
        var route = plan.Target switch
        {
            "Kho" => "/Kho/Create",
            "LoaiHang" => "/LoaiHang/Create",
            "DonViTinh" => "/DonViTinh/Create",
            "HangHoa" => "/HangHoa/Create",
            "NhaCungCap" => "/NhaCungCap/Create",
            "BoPhanNhan" => "/BoPhanNhan/Create",
            _ => throw new InvalidOperationException($"Unsupported catalog target '{plan.Target}'.")
        };

        var beforeCount = await GetCatalogCountAsync(plan.Target);
        var form = BuildCatalogCreateForm(plan.Target, fields);
        var token = await GetAntiForgeryTokenAsync(route);
        using var response = await PostFormAsync(route, token, form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(beforeCount + 1, await GetCatalogCountAsync(plan.Target));

        var expectedNameField = plan.Target switch
        {
            "Kho" => "TenKho",
            "LoaiHang" => "TenLoaiHang",
            "DonViTinh" => "TenDonViTinh",
            "HangHoa" => "TenHang",
            "NhaCungCap" => "TenNhaCungCap",
            "BoPhanNhan" => "TenBoPhan",
            _ => throw new InvalidOperationException()
        };
        var expectedName = form[expectedNameField];
        var storedName = await GetCatalogNameAsync(plan.Target, expectedName);
        Assert.Equal(expectedName, storedName);
    }

    private static Dictionary<string, string> ParseFormValues(string raw)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in Regex.Matches(raw, @"(?<key>[A-Za-z]+)\s*=\s*(?<value>[^,;]+)"))
        {
            result[match.Groups["key"].Value] = match.Groups["value"].Value.Trim().Trim('\'');
        }
        return result;
    }

    private static Dictionary<string, string> BuildCatalogCreateForm(string target, IReadOnlyDictionary<string, string> input)
    {
        string Value(string key, string fallback = "") => input.TryGetValue(key, out var value) ? value : fallback;
        return target switch
        {
            "Kho" => new Dictionary<string, string>
            {
                ["TenKho"] = Value("TenKho"), ["DiaDiem"] = Value("DiaDiem"), ["MoTa"] = "Excel data-driven", ["TrangThai"] = "true"
            },
            "LoaiHang" => new Dictionary<string, string>
            {
                ["TenLoaiHang"] = Value("TenLoaiHang"), ["MoTa"] = Value("MoTa", "Excel data-driven"), ["TrangThai"] = "true"
            },
            "DonViTinh" => new Dictionary<string, string>
            {
                ["TenDonViTinh"] = Value("TenDonViTinh"), ["MoTa"] = Value("MoTa", "Excel data-driven"), ["TrangThai"] = "true"
            },
            "HangHoa" => new Dictionary<string, string>
            {
                ["TenHang"] = Value("TenHang", "Excel product"), ["MaLoaiHang"] = "1", ["MaDonViTinh"] = "1",
                ["GiaNhapThamKhao"] = Value("GiaNhapThamKhao", "0"), ["MucTonToiThieu"] = Value("MucTonToiThieu", "0"),
                ["TrangThai"] = "true"
            },
            "NhaCungCap" => new Dictionary<string, string>
            {
                ["TenNhaCungCap"] = Value("TenNCC", Value("TenNhaCungCap")),
                ["SoDienThoai"] = Value("SDT", Value("SoDienThoai")), ["Email"] = Value("Email"),
                ["DiaChi"] = Value("DiaChi", "Hà Nội"), ["TrangThai"] = "true"
            },
            "BoPhanNhan" => new Dictionary<string, string>
            {
                ["TenBoPhan"] = Value("TenBoPhan"), ["NguoiDaiDien"] = Value("NguoiDaiDien"),
                ["SoDienThoai"] = Value("SDT", Value("SoDienThoai")), ["MoTa"] = "Excel data-driven", ["TrangThai"] = "true"
            },
            _ => throw new InvalidOperationException($"Unsupported catalog target '{target}'.")
        };
    }

    private Task<int> GetCatalogCountAsync(string target) => WithDbAsync(db => target switch
    {
        "Kho" => db.Kho.CountAsync(),
        "LoaiHang" => db.LoaiHangs.CountAsync(),
        "DonViTinh" => db.DonViTinhs.CountAsync(),
        "HangHoa" => db.HangHoa.CountAsync(),
        "NhaCungCap" => db.NhaCungCap.CountAsync(),
        "BoPhanNhan" => db.BoPhanNhans.CountAsync(),
        _ => throw new InvalidOperationException($"Unsupported catalog target '{target}'.")
    });

    private Task<string?> GetCatalogNameAsync(string target, string name) => WithDbAsync(db => target switch
    {
        "Kho" => db.Kho.Where(item => item.TenKho == name).Select(item => item.TenKho).SingleOrDefaultAsync(),
        "LoaiHang" => db.LoaiHangs.Where(item => item.TenLoaiHang == name).Select(item => item.TenLoaiHang).SingleOrDefaultAsync(),
        "DonViTinh" => db.DonViTinhs.Where(item => item.TenDonViTinh == name).Select(item => item.TenDonViTinh).SingleOrDefaultAsync(),
        "HangHoa" => db.HangHoa.Where(item => item.TenHang == name).Select(item => item.TenHang).SingleOrDefaultAsync(),
        "NhaCungCap" => db.NhaCungCap.Where(item => item.TenNhaCungCap == name).Select(item => item.TenNhaCungCap).SingleOrDefaultAsync(),
        "BoPhanNhan" => db.BoPhanNhans.Where(item => item.TenBoPhan == name).Select(item => item.TenBoPhan).SingleOrDefaultAsync(),
        _ => throw new InvalidOperationException($"Unsupported catalog target '{target}'.")
    });

    private async Task VerifyCatalogEditAsync(ScenarioPlan plan)
    {
        await LoginAsync("admin", "Admin@123");
        const int id = 1;
        var fields = ParseFormValues(plan.TestCase.TestData);
        var routeBase = plan.Target switch
        {
            "Kho" => "/Kho",
            "LoaiHang" => "/LoaiHang",
            "DonViTinh" => "/DonViTinh",
            "HangHoa" => "/HangHoa",
            "NhaCungCap" => "/NhaCungCap",
            "BoPhanNhan" => "/BoPhanNhan",
            _ => throw new InvalidOperationException($"Unsupported catalog target '{plan.Target}'.")
        };
        var form = BuildCatalogEditForm(plan.Target, fields, id);
        var token = await GetAntiForgeryTokenAsync($"{routeBase}/Edit/{id}");
        using var response = await PostFormAsync($"{routeBase}/Edit/{id}", token, form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var field = plan.Target switch
        {
            "Kho" => "DiaDiem",
            "LoaiHang" or "DonViTinh" => "MoTa",
            "HangHoa" => "GiaNhapThamKhao",
            "NhaCungCap" => "DiaChi",
            "BoPhanNhan" => "NguoiDaiDien",
            _ => throw new InvalidOperationException()
        };
        var expected = form[field];
        Assert.Equal(expected, await GetCatalogFieldAsync(plan.Target, id, field));
    }

    private static Dictionary<string, string> BuildCatalogEditForm(
        string target, IReadOnlyDictionary<string, string> input, int id)
    {
        string Value(string key, string fallback = "") => input.TryGetValue(key, out var value) ? value : fallback;
        return target switch
        {
            "Kho" => new Dictionary<string, string>
            {
                ["MaKho"] = id.ToString(CultureInfo.InvariantCulture), ["TenKho"] = "Test warehouse",
                ["DiaDiem"] = Value("DiaDiem"), ["MoTa"] = "Test", ["TrangThai"] = "true"
            },
            "LoaiHang" => new Dictionary<string, string>
            {
                ["MaLoaiHang"] = id.ToString(CultureInfo.InvariantCulture), ["TenLoaiHang"] = "Test category",
                ["MoTa"] = Value("MoTa"), ["TrangThai"] = "true"
            },
            "DonViTinh" => new Dictionary<string, string>
            {
                ["MaDonViTinh"] = id.ToString(CultureInfo.InvariantCulture), ["TenDonViTinh"] = "Piece",
                ["MoTa"] = Value("MoTa"), ["TrangThai"] = "true"
            },
            "HangHoa" => new Dictionary<string, string>
            {
                ["MaHang"] = id.ToString(CultureInfo.InvariantCulture), ["TenHang"] = "chuột quang",
                ["MaLoaiHang"] = "1", ["MaDonViTinh"] = "1", ["GiaNhapThamKhao"] = Value("GiaNhapThamKhao"),
                ["MucTonToiThieu"] = "5", ["MoTa"] = "Test", ["TrangThai"] = "true"
            },
            "NhaCungCap" => new Dictionary<string, string>
            {
                ["MaNhaCungCap"] = id.ToString(CultureInfo.InvariantCulture), ["TenNhaCungCap"] = "Test supplier",
                ["SoDienThoai"] = "0912345678", ["Email"] = "supplier@example.test", ["DiaChi"] = Value("DiaChi"),
                ["TrangThai"] = "true"
            },
            "BoPhanNhan" => new Dictionary<string, string>
            {
                ["MaBoPhan"] = id.ToString(CultureInfo.InvariantCulture), ["TenBoPhan"] = "Test department",
                ["NguoiDaiDien"] = Value("NguoiDaiDien"), ["SoDienThoai"] = "0987654321",
                ["MoTa"] = "Test", ["TrangThai"] = "true"
            },
            _ => throw new InvalidOperationException($"Unsupported catalog target '{target}'.")
        };
    }

    private Task<string?> GetCatalogFieldAsync(string target, int id, string field) => WithDbAsync(db => (target, field) switch
    {
        ("Kho", "DiaDiem") => db.Kho.Where(item => item.MaKho == id).Select(item => item.DiaDiem).SingleOrDefaultAsync(),
        ("LoaiHang", "MoTa") => db.LoaiHangs.Where(item => item.MaLoaiHang == id).Select(item => item.MoTa).SingleOrDefaultAsync(),
        ("DonViTinh", "MoTa") => db.DonViTinhs.Where(item => item.MaDonViTinh == id).Select(item => item.MoTa).SingleOrDefaultAsync(),
        ("HangHoa", "GiaNhapThamKhao") => db.HangHoa.Where(item => item.MaHang == id).Select(item => item.GiaNhapThamKhao.ToString()).SingleOrDefaultAsync(),
        ("NhaCungCap", "DiaChi") => db.NhaCungCap.Where(item => item.MaNhaCungCap == id).Select(item => item.DiaChi).SingleOrDefaultAsync(),
        ("BoPhanNhan", "NguoiDaiDien") => db.BoPhanNhans.Where(item => item.MaBoPhan == id).Select(item => item.NguoiDaiDien).SingleOrDefaultAsync(),
        _ => throw new InvalidOperationException($"Unsupported catalog field {target}.{field}.")
    });

    private async Task VerifyCatalogRejectEmptyNameAsync(ScenarioPlan plan)
    {
        await LoginAsync("admin", "Admin@123");
        var beforeCount = await GetCatalogCountAsync(plan.Target);
        var route = plan.Target switch
        {
            "Kho" => "/Kho/Create",
            "LoaiHang" => "/LoaiHang/Create",
            "DonViTinh" => "/DonViTinh/Create",
            "HangHoa" => "/HangHoa/Create",
            "NhaCungCap" => "/NhaCungCap/Create",
            "BoPhanNhan" => "/BoPhanNhan/Create",
            _ => throw new InvalidOperationException($"Unsupported catalog target '{plan.Target}'.")
        };
        var fields = ParseFormValues(plan.TestCase.TestData);
        var form = BuildCatalogCreateForm(plan.Target, fields);
        var token = await GetAntiForgeryTokenAsync(route);
        using var response = await PostFormAsync(route, token, form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.True(html.Contains("không được để trống", StringComparison.OrdinalIgnoreCase)
            || html.Contains("bắt buộc", StringComparison.OrdinalIgnoreCase),
            $"The invalid form response did not show a required-name validation error for {plan.TestCase.Id}.");
        Assert.Equal(beforeCount, await GetCatalogCountAsync(plan.Target));
    }

    private async Task VerifyCatalogRejectInvalidProductAsync(ScenarioPlan plan)
    {
        await LoginAsync("admin", "Admin@123");
        var beforeCount = await GetCatalogCountAsync("HangHoa");
        var fields = ParseFormValues(plan.TestCase.TestData);
        var form = BuildCatalogCreateForm("HangHoa", fields);
        var route = "/HangHoa/Create";
        var token = await GetAntiForgeryTokenAsync(route);
        using var response = await PostFormAsync(route, token, form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("field-validation-error", html, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeCount, await GetCatalogCountAsync("HangHoa"));
    }

    private async Task VerifyLoginAsync(string testData)
    {
        var credentials = ParseCredentials(testData);
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var login = await PostFormAsync("/TaiKhoan/DangNhap", token, new Dictionary<string, string>
        {
            ["TenDangNhap"] = credentials.Username,
            ["MatKhau"] = credentials.Password
        });
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        var path = credentials.Username.Trim() == "admin" ? "/TaiKhoan/Index" : "/PhieuNhap";
        using var response = await Client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task VerifyInvalidLoginAsync(string testData, string testType)
    {
        var credentials = ParseCredentials(testData);
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var response = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = credentials.Username,
                ["MatKhau"] = credentials.Password
            });

        if (testType.Contains("Boundary", StringComparison.OrdinalIgnoreCase))
        {
            Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            return;
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var protectedResponse = await Client.GetAsync("/TaiKhoan/Index");
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
    }

    private async Task VerifyLogoutAsync()
    {
        await LoginAsync("admin", "Admin@123");

        using var logout = await Client.GetAsync("/TaiKhoan/DangXuat");
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        using var protectedResponse = await Client.GetAsync("/PhieuNhap");
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
    }

    private async Task VerifyAnonymousAccessAsync()
    {
        using var response = await Client.GetAsync("/PhieuNhap");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("login", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task VerifyWarehouseUserAuthorizationAsync()
    {
        await LoginAsync("nvkho", "Kho@123");

        using var response = await Client.GetAsync("/TaiKhoan/Index");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("AccessDenied", response.Headers.Location?.ToString());
    }

    private async Task VerifyDraftReceiptAsync(string testData)
    {
        var (supplierId, warehouseId) = ParseReceiptReferences(testData);
        var receiptId = await CreateReceiptAsync(supplierId, warehouseId);
        var receipt = await WithDbAsync(db => db.PhieuNhap.SingleAsync(item => item.MaPhieuNhap == receiptId));

        Assert.Equal(0, (int)receipt.TrangThai);
        Assert.Equal("Administrator", receipt.NguoiLap);
    }

    private async Task VerifyReceiptDetailAsync(string testData)
    {
        var receiptId = await CreateReceiptAsync(1, 1);
        var (quantity, unitPrice) = ParseQuantityAndPrice(testData);
        await AddReceiptDetailAsync(receiptId, 1, quantity, unitPrice);

        await WithDbAsync(async db =>
        {
            var receipt = await db.PhieuNhap
                .Include(item => item.ChiTietPhieuNhaps)
                .SingleAsync(item => item.MaPhieuNhap == receiptId);
            var detail = Assert.Single(receipt.ChiTietPhieuNhaps);
            Assert.Equal(quantity, detail.SoLuongNhap);
            Assert.Equal(unitPrice, detail.DonGiaNhap);
            Assert.Equal(quantity * unitPrice, detail.ThanhTien);
            Assert.Equal(quantity * unitPrice, receipt.TongTienNhap);
            return true;
        });
    }

    private async Task VerifyReceiptTotalAsync(string testData)
    {
        var receiptId = await CreateReceiptAsync(1, 1);
        var lines = Regex.Matches(testData, @"\((?<quantity>\d+)\s*x\s*(?<price>\d+)\)", RegexOptions.IgnoreCase)
            .Select(match => (
                Quantity: int.Parse(match.Groups["quantity"].Value, CultureInfo.InvariantCulture),
                UnitPrice: decimal.Parse(match.Groups["price"].Value, CultureInfo.InvariantCulture)))
            .ToArray();
        Assert.Equal(3, lines.Length);

        for (var index = 0; index < lines.Length; index++)
        {
            await AddReceiptDetailAsync(receiptId, index + 1, lines[index].Quantity, lines[index].UnitPrice);
        }

        await WithDbAsync(async db =>
        {
            var receipt = await db.PhieuNhap
                .Include(item => item.ChiTietPhieuNhaps)
                .SingleAsync(item => item.MaPhieuNhap == receiptId);

            Assert.Equal(lines.Length, receipt.ChiTietPhieuNhaps.Count);
            Assert.Equal(lines.Sum(line => line.Quantity * line.UnitPrice), receipt.TongTienNhap);
            return true;
        });
    }

    private async Task VerifyDraftDispatchAsync(string testData)
    {
        var (recipientId, warehouseId) = ParseDispatchReferences(testData);
        var dispatchId = await CreateDispatchAsync(recipientId, warehouseId);
        var dispatch = await WithDbAsync(db => db.PhieuXuats.SingleAsync(item => item.MaPhieuXuat == dispatchId));

        Assert.Equal(0, (int)dispatch.TrangThai);
        Assert.Equal("Administrator", dispatch.NguoiLap);
    }

    private async Task VerifyCompletedDispatchAsync(string title, string testData)
    {
        var quantity = ParseInteger(testData, @"xuất\s*=?\s*(?<value>\d+)");
        var dispatchId = await CreateDispatchAsync(1, 1);
        await AddDispatchDetailAsync(dispatchId, quantity);
        await ChangeDispatchStatusAsync(dispatchId, 1);
        await ChangeDispatchStatusAsync(dispatchId, 2);

        await WithDbAsync(async db =>
        {
            var dispatch = await db.PhieuXuats.SingleAsync(item => item.MaPhieuXuat == dispatchId);
            var stock = await db.TonKhoes.SingleAsync(item => item.MaKho == 1 && item.MaHang == 1);
            var history = await db.LichSuTonKhoes
                .Where(item => item.MaPhieu == $"PX{dispatchId:D4}" && item.LoaiGiaoDich == "Xuất kho")
                .ToListAsync();

            Assert.Equal(2, (int)dispatch.TrangThai);
            Assert.Equal(100 - quantity, stock.SoLuongTon);
            var transaction = Assert.Single(history);
            Assert.Equal(quantity, transaction.SoLuong);
            Assert.Equal(100 - quantity, transaction.TonSauGiaoDich);
            return true;
        });

        if (title.Contains("hoàn tất lại", StringComparison.OrdinalIgnoreCase))
        {
            var token = await GetAntiForgeryTokenAsync($"/PhieuXuat/Details/{dispatchId}");
            using var repeatedCompletion = await PostFormAsync(
                $"/PhieuXuat/HoanTat/{dispatchId}",
                token,
                new Dictionary<string, string>());
            Assert.Equal(HttpStatusCode.Redirect, repeatedCompletion.StatusCode);

            await WithDbAsync(async db =>
            {
                var stock = await db.TonKhoes.SingleAsync(item => item.MaKho == 1 && item.MaHang == 1);
                var exportHistoryCount = await db.LichSuTonKhoes.CountAsync(
                    item => item.MaPhieu == $"PX{dispatchId:D4}" && item.LoaiGiaoDich == "Xuất kho");
                Assert.Equal(100 - quantity, stock.SoLuongTon);
                Assert.Equal(1, exportHistoryCount);
                return true;
            });
        }
    }

    private async Task VerifyReceiptHistoryAsync(string testData)
    {
        var quantity = ParseInteger(testData, @"(?<value>\d+)");
        var receiptId = await CreateReceiptAsync(1, 1);
        await AddReceiptDetailAsync(receiptId, 1, quantity, 1m);

        var token = await GetAntiForgeryTokenAsync($"/PhieuNhap/Details/{receiptId}");
        using var completion = await PostFormAsync(
            $"/PhieuNhap/HoanTat/{receiptId}",
            token,
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, completion.StatusCode);

        await WithDbAsync(async db =>
        {
            var stock = await db.TonKhoes.SingleAsync(item => item.MaKho == 1 && item.MaHang == 1);
            var history = await db.LichSuTonKhoes
                .Where(item => item.MaPhieu == $"PN{receiptId:D4}" && item.LoaiGiaoDich == "Nhập kho")
                .ToListAsync();

            Assert.Equal(100 + quantity, stock.SoLuongTon);
            var transaction = Assert.Single(history);
            Assert.Equal(quantity, transaction.SoLuong);
            Assert.Equal(100 + quantity, transaction.TonSauGiaoDich);
            return true;
        });
    }

    private async Task<int> CreateReceiptAsync(int supplierId, int warehouseId)
    {
        await LoginAsync("admin", "Admin@123");
        var token = await GetAntiForgeryTokenAsync("/PhieuNhap/Create");
        using var response = await PostFormAsync(
            "/PhieuNhap/Create",
            token,
            new Dictionary<string, string>
            {
                ["MaNhaCungCap"] = supplierId.ToString(CultureInfo.InvariantCulture),
                ["MaKho"] = warehouseId.ToString(CultureInfo.InvariantCulture),
                ["NgayNhap"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["TrangThai"] = "0",
                ["GhiChu"] = "Excel data-driven test"
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        return await WithDbAsync(async db =>
            (await db.PhieuNhap.OrderByDescending(item => item.MaPhieuNhap).FirstAsync()).MaPhieuNhap);
    }

    private async Task AddReceiptDetailAsync(int receiptId, int itemId, int quantity, decimal unitPrice)
    {
        var path = $"/ChiTietPhieuNhap/Create?phieuId={receiptId}";
        var token = await GetAntiForgeryTokenAsync(path);
        using var response = await PostFormAsync(
            path,
            token,
            new Dictionary<string, string>
            {
                ["MaPhieuNhap"] = receiptId.ToString(CultureInfo.InvariantCulture),
                ["MaHang"] = itemId.ToString(CultureInfo.InvariantCulture),
                ["SoLuongNhap"] = quantity.ToString(CultureInfo.InvariantCulture),
                ["DonGiaNhap"] = unitPrice.ToString(CultureInfo.InvariantCulture)
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private async Task<int> CreateDispatchAsync(int recipientId, int warehouseId)
    {
        await LoginAsync("admin", "Admin@123");
        var token = await GetAntiForgeryTokenAsync("/PhieuXuat/Create");
        using var response = await PostFormAsync(
            "/PhieuXuat/Create",
            token,
            new Dictionary<string, string>
            {
                ["MaBoPhan"] = recipientId.ToString(CultureInfo.InvariantCulture),
                ["MaKho"] = warehouseId.ToString(CultureInfo.InvariantCulture),
                ["NgayXuat"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["GhiChu"] = "Excel data-driven test"
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        return await WithDbAsync(async db =>
            (await db.PhieuXuats.OrderByDescending(item => item.MaPhieuXuat).FirstAsync()).MaPhieuXuat);
    }

    private async Task AddDispatchDetailAsync(int dispatchId, int quantity)
    {
        var path = $"/ChiTietPhieuXuat/Create?maPhieuXuat={dispatchId}";
        var token = await GetAntiForgeryTokenAsync(path);
        using var response = await PostFormAsync(
            path,
            token,
            new Dictionary<string, string>
            {
                ["MaPhieuXuat"] = dispatchId.ToString(CultureInfo.InvariantCulture),
                ["MaHang"] = "1",
                ["SoLuongXuat"] = quantity.ToString(CultureInfo.InvariantCulture),
                ["DonGiaXuatThamChieu"] = "100"
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private async Task ChangeDispatchStatusAsync(int dispatchId, int status)
    {
        var path = $"/PhieuXuat/Edit/{dispatchId}";
        var token = await GetAntiForgeryTokenAsync(path);
        using var response = await PostFormAsync(
            path,
            token,
            new Dictionary<string, string>
            {
                ["MaPhieuXuat"] = dispatchId.ToString(CultureInfo.InvariantCulture),
                ["MaBoPhan"] = "1",
                ["MaKho"] = "1",
                ["NgayXuat"] = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["NguoiLap"] = "Administrator",
                ["TrangThai"] = status.ToString(CultureInfo.InvariantCulture)
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private async Task LoginAsync(string username, string password)
    {
        var token = await GetAntiForgeryTokenAsync("/TaiKhoan/DangNhap");
        using var response = await PostFormAsync(
            "/TaiKhoan/DangNhap",
            token,
            new Dictionary<string, string>
            {
                ["TenDangNhap"] = username,
                ["MatKhau"] = password
            });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static (string Username, string Password) ParseCredentials(string testData)
    {
        var separator = testData.IndexOf('/');
        Assert.True(separator > 0, $"Could not parse credentials from testcase data: '{testData}'.");
        var username = testData[..separator].Trim();
        var password = testData[(separator + 1)..].Trim();
        static string Unquote(string value)
        {
            value = value.Trim();
            return value.Length >= 2 && value[0] == '\'' && value[^1] == '\'' ? value[1..^1] : value;
        }
        return (Unquote(username), Unquote(password));
    }

    private static (int SupplierId, int WarehouseId) ParseReceiptReferences(string testData)
    {
        var supplierId = ParseInteger(testData, @"NCC\s*(?<value>\d+)");
        var warehouseId = ParseInteger(testData, @"K\s*(?<value>\d+)");
        return (supplierId, warehouseId);
    }

    private static (int RecipientId, int WarehouseId) ParseDispatchReferences(string testData)
    {
        var recipientId = ParseInteger(testData, @"BP\s*(?<value>\d+)");
        var warehouseId = ParseInteger(testData, @"K\s*(?<value>\d+)");
        return (recipientId, warehouseId);
    }

    private static (int Quantity, decimal UnitPrice) ParseQuantityAndPrice(string testData)
    {
        var quantity = ParseInteger(testData, @"SL\s*=\s*(?<value>\d+)");
        var unitPrice = decimal.Parse(
            Regex.Match(testData, @"DonGia\s*=\s*(?<value>\d+(?:[.,]\d+)?)", RegexOptions.IgnoreCase)
                .Groups["value"].Value.Replace(',', '.'),
            CultureInfo.InvariantCulture);
        return (quantity, unitPrice);
    }

    private static int ParseInteger(string input, string pattern)
    {
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        Assert.True(match.Success, $"Could not find an integer matching '{pattern}' in testcase data: '{input}'.");
        return int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
    }
}
