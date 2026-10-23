# Automated test model

`TestCase_QuanLyKhoHang_ASPNetCore10.xlsx`, worksheet `Testcase`, is the source
of truth. `ExcelTestCaseLoader` loads testcase rows and a shared resolver creates
a `ScenarioPlan` with group, action, target, input, expectations, and seed
profile. A row that fits an implemented handler requires no per-ID test code.

Run the dynamically supported testcases directly:

```powershell
dotnet test .\Tests\QuanLyKhoHang.Tests\QuanLyKhoHang.Tests.csproj --filter "FullyQualifiedName~ExcelTestCaseIntegrationTests"
```

Run the current automated suite from the repository root. The wrapper shows
each succeeded/skipped test by name and prints error messages and stack traces
for failures:

```powershell
.\tests\Run-Tests.ps1
```

The wrapper reads the workbook directly and discovers dynamically executable
IDs from the test runner. Select by testcase ID, module, feature, test type,
title, or test data with `-TestCase`. Matching is case-insensitive and allows
partial text:

```powershell
.\Tests\Run-Tests.ps1 -TestCase "TC_M1_001"
.\Tests\Run-Tests.ps1 -TestCase "Đăng nhập"
```

If matching workbook rows are not automated yet, the script reports those IDs
and exits without running unrelated tests. With no arguments it reports total,
automated, and not-yet-automated testcase counts, then runs the test suite.
`-TestFilter` remains available for a raw VSTest filter:

```powershell
.\Tests\Run-Tests.ps1 -TestFilter "FullyQualifiedName~AuthenticationIntegrationTests"
```

Compare testcase IDs and see which cases have an xUnit mapping:

```powershell
.\Tests\Show-TestcaseCoverage.ps1
```

## Structure

- `QuanLyKhoHang.Tests` runs the real MVC pipeline with `WebApplicationFactory`.
- Each test gets an isolated SQLite in-memory database and deterministic seed data.
- `ScenarioPlanResolver` separates executable cases, cases needing a handler,
  and manual/provider review. Review cases never enter the passing xUnit set.
- Seed profiles (`Default`, `Catalog`, `Query`, `StockBoundary`) provide
  deterministic data for shared scenarios.
- Integration tests obtain anti-forgery tokens and use HTTP requests; they do not
  call controller methods directly.
- Test data is isolated per test and discarded with its database connection.

## Current automated testcase coverage

| Scenario | Data read from Excel |
|---|---|
| Login, logout, access control | Credentials, account type, feature, and title |
| Inbound receipt | Supplier/warehouse references, quantities, unit prices, line totals |
| Outbound dispatch | Recipient/warehouse references, quantity, and repeat-completion intent |
| Receipt history | Quantity and history expectation |
| Product pagination | Requested page, clamping of invalid/out-of-range pages, and SQLite-backed row window |

The workbook currently contains 204 cases. 71 have an implemented HTTP and
database assertion; 129 still need shared handlers. Four provider-specific or
benchmark cases remain in manual review. The resolver classifies every row;
unimplemented rows are never counted as passed.

`TC_M3_025` and `TC_M4_021` need provider-specific concurrency validation;
`TC_M6_006` needs SQL Server migrations; `TC_M6_010` needs a benchmark
environment and threshold. These remain manual/provider review cases.

SQLite provides a fast, isolated relational test database without Docker or a
SQL Server instance. It is not a substitute for SQL Server compatibility testing;
keep SQL Server-specific migrations, query translation, and deployment checks in
a separate environment.
