# Automated test model

The test cases in `testcase_summary.txt` (also represented in `testcase_dump.txt`)
are the source of truth. Automated tests use xUnit traits named `TC`, with each
trait value matching a testcase ID so the test runner can filter by spreadsheet
ID, for example:

```powershell
dotnet test .\QuanLyKhoHang_UNETI01_TI17A3HN.slnx --filter "TC=TC_M1_001"
```

Run the current automated suite from the repository root. The wrapper shows
each succeeded/skipped test by name and prints error messages and stack traces
for failures:

```powershell
.\tests\Run-Tests.ps1
```

To run selected tests, pass a VSTest filter:

```powershell
.\tests\Run-Tests.ps1 -TestFilter "TC=TC_M1_001"
```

Compare the IDs in both input files and see which cases have an xUnit mapping:

```powershell
.\tests\Show-TestcaseCoverage.ps1
```

## Structure

- `QuanLyKhoHang.Tests` runs the real MVC pipeline with `WebApplicationFactory`.
- Each test gets an isolated SQLite in-memory database and deterministic seed data.
- Integration tests obtain anti-forgery tokens and use HTTP requests; they do not
  call controller methods directly.
- Test data is isolated per test and discarded with its database connection.

## Initial testcase coverage

| Test file | Testcase IDs |
|---|---|
| `AuthenticationIntegrationTests.cs` | TC_M1_001, TC_M1_003, TC_M1_008, TC_M1_010, TC_M1_012, TC_M1_020 |
| `InboundWorkflowIntegrationTests.cs` | TC_M3_008, TC_M3_012, TC_M3_018, TC_M3_019 |
| `OutboundWorkflowIntegrationTests.cs` | TC_M4_006, TC_M4_019, TC_M4_020, TC_M5_016 |

The catalog contains 204 cases, but this is the first runnable slice, not full
automation of all 204. Add tests and testcase traits incrementally; do not mark
an unmapped spreadsheet case as automated or passed.

SQLite provides a fast, isolated relational test database without Docker or a
SQL Server instance. It is not a substitute for SQL Server compatibility testing;
keep SQL Server-specific migrations, query translation, and deployment checks in
a separate environment.
