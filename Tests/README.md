# Automated test model

`TestCase_QuanLyKhoHang_ASPNetCore10.xlsx`, worksheet `Testcase`, is the source
of truth. `ExcelTestCaseLoader` loads testcase rows and the data-driven xUnit
test dispatches rows with an implemented executor. The current automated set is
an explicitly supported subset; every other workbook row is reported as not yet
automated instead of being treated as passed.

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
- Integration tests obtain anti-forgery tokens and use HTTP requests; they do not
  call controller methods directly.
- Test data is isolated per test and discarded with its database connection.

## Current automated testcase coverage

| Test file | Testcase IDs |
|---|---|
| `AuthenticationIntegrationTests.cs` | TC_M1_001, TC_M1_003, TC_M1_008, TC_M1_010, TC_M1_012, TC_M1_020 |
| `InboundWorkflowIntegrationTests.cs` | TC_M3_008, TC_M3_012, TC_M3_018, TC_M3_019 |
| `OutboundWorkflowIntegrationTests.cs` | TC_M4_006, TC_M4_019, TC_M4_020, TC_M5_016 |

The workbook currently contains 204 cases. Only the IDs explicitly listed in
`ExcelTestCaseIntegrationTests` have executable scenario handlers; the
remaining rows are reported as not yet automated. Add handlers and assertions
incrementally rather than interpreting arbitrary natural-language steps as
executable commands.

SQLite provides a fast, isolated relational test database without Docker or a
SQL Server instance. It is not a substitute for SQL Server compatibility testing;
keep SQL Server-specific migrations, query translation, and deployment checks in
a separate environment.
