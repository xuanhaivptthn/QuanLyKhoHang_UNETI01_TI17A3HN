param(
    [string]$TestCase,
    [string]$TestFilter
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$testProjectPath = Join-Path $PSScriptRoot "QuanLyKhoHang.Tests\QuanLyKhoHang.Tests.csproj"
$testcaseWorkbookPath = Join-Path $repositoryRoot "TestCase_QuanLyKhoHang_ASPNetCore10.xlsx"
$resultsRoot = Join-Path ([System.IO.Path]::GetTempPath()) "QuanLyKhoHang-TestResults"
$runId = "{0}-{1}" -f (Get-Date -Format "yyyyMMdd-HHmmss"), ([guid]::NewGuid().ToString("N"))
$resultsDirectory = Join-Path $resultsRoot $runId
$trxFileName = "test-results.trx"
$trxPath = Join-Path $resultsDirectory $trxFileName
$manualReviewIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
@("TC_M3_025", "TC_M4_021", "TC_M6_006", "TC_M6_010") | ForEach-Object {
    [void]$manualReviewIds.Add($_)
}

function Read-ZipEntryText {
    param(
        [System.IO.Compression.ZipArchive]$Archive,
        [string]$EntryPath
    )

    $entry = $Archive.GetEntry($EntryPath)
    if ($null -eq $entry) {
        throw "Workbook entry not found: $EntryPath"
    }

    $stream = $entry.Open()
    try {
        $reader = [System.IO.StreamReader]::new($stream)
        try {
            return $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Get-ColumnNumber {
    param([string]$CellReference)

    $letters = [regex]::Match($CellReference, '^[A-Z]+').Value
    $column = 0
    foreach ($letter in $letters.ToCharArray()) {
        $column = ($column * 26) + ([int]$letter - [int][char]'A' + 1)
    }
    return $column
}

function ConvertTo-NormalizedHeader {
    param([string]$Header)

    $normalized = $Header.Normalize([Text.NormalizationForm]::FormD)
    $normalized = [regex]::Replace($normalized, '\p{Mn}', '')
    return $normalized.Replace([string][char]273, 'd').Replace([string][char]272, 'D').Normalize([Text.NormalizationForm]::FormC).Trim()
}

function Read-WorkbookTestCases {
    param([string]$Path)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        [xml]$workbookXml = Read-ZipEntryText -Archive $archive -EntryPath "xl/workbook.xml"
        [xml]$relationshipsXml = Read-ZipEntryText -Archive $archive -EntryPath "xl/_rels/workbook.xml.rels"
        $sheet = $workbookXml.SelectSingleNode("//*[local-name()='sheet' and @name='Testcase']")
        if ($null -eq $sheet) {
            throw "Worksheet 'Testcase' was not found in $Path."
        }

        $relationshipId = $sheet.GetAttribute(
            "id",
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships")
        $relationship = $relationshipsXml.SelectSingleNode(
            "//*[local-name()='Relationship' and @Id='$relationshipId']")
        if ($null -eq $relationship) {
            throw "Could not resolve the 'Testcase' worksheet in $Path."
        }

        $worksheetPath = [string]$relationship.GetAttribute("Target")
        if ($worksheetPath.StartsWith("/")) {
            $worksheetPath = $worksheetPath.TrimStart("/")
        }
        else {
            $worksheetPath = "xl/$worksheetPath"
        }

        $sharedStrings = @()
        $sharedStringsEntry = $archive.GetEntry("xl/sharedStrings.xml")
        if ($null -ne $sharedStringsEntry) {
            [xml]$sharedStringsXml = Read-ZipEntryText -Archive $archive -EntryPath "xl/sharedStrings.xml"
            $sharedStrings = @(
                $sharedStringsXml.SelectNodes("//*[local-name()='si']") | ForEach-Object {
                    ($_.SelectNodes(".//*[local-name()='t']") | ForEach-Object { $_.InnerText }) -join ""
                }
            )
        }

        [xml]$worksheetXml = Read-ZipEntryText -Archive $archive -EntryPath $worksheetPath
        $worksheetRows = @($worksheetXml.SelectNodes("//*[local-name()='sheetData']/*[local-name()='row']"))
        if ($worksheetRows.Count -lt 2) {
            throw "Worksheet 'Testcase' contains no testcase rows."
        }

        $headerByColumn = @{}
        foreach ($cell in $worksheetRows[0].SelectNodes("./*[local-name()='c']")) {
            $columnNumber = Get-ColumnNumber -CellReference $cell.GetAttribute("r")
            $headerByColumn[$columnNumber] = Get-ExcelCellValue -Cell $cell -SharedStrings $sharedStrings
        }
        $columnByHeader = @{}
        foreach ($columnNumber in $headerByColumn.Keys) {
            $header = ConvertTo-NormalizedHeader -Header ([string]$headerByColumn[$columnNumber])
            if (-not [string]::IsNullOrWhiteSpace($header)) {
                $columnByHeader[$header] = [int]$columnNumber
            }
        }

        $requiredHeaders = @("ID", "Module", "Chuc nang", "Loai test", "Tieu de / Mo ta", "Du lieu test")
        $missingHeaders = @($requiredHeaders | Where-Object {
            -not $columnByHeader.ContainsKey((ConvertTo-NormalizedHeader -Header $_))
        })
        if ($missingHeaders.Count -gt 0) {
            throw "Worksheet 'Testcase' is missing required columns: $($missingHeaders -join ', ')."
        }

        $testCases = [System.Collections.Generic.List[object]]::new()
        foreach ($row in $worksheetRows | Select-Object -Skip 1) {
            $valuesByColumn = @{}
            foreach ($cell in $row.SelectNodes("./*[local-name()='c']")) {
                $columnNumber = Get-ColumnNumber -CellReference $cell.GetAttribute("r")
                $valuesByColumn[$columnNumber] = Get-ExcelCellValue -Cell $cell -SharedStrings $sharedStrings
            }

            $id = [string]$valuesByColumn[$columnByHeader["ID"]]
            if ([string]::IsNullOrWhiteSpace($id)) {
                continue
            }

            $testCases.Add([pscustomobject]@{
                Id = $id.Trim()
                Module = [string]$valuesByColumn[$columnByHeader["Module"]]
                Feature = [string]$valuesByColumn[$columnByHeader[(ConvertTo-NormalizedHeader -Header "Chuc nang")]]
                Type = [string]$valuesByColumn[$columnByHeader[(ConvertTo-NormalizedHeader -Header "Loai test")]]
                Title = [string]$valuesByColumn[$columnByHeader[(ConvertTo-NormalizedHeader -Header "Tieu de / Mo ta")]]
                TestData = [string]$valuesByColumn[$columnByHeader[(ConvertTo-NormalizedHeader -Header "Du lieu test")]]
            })
        }

        return $testCases.ToArray()
    }
    finally {
        $archive.Dispose()
    }
}

function Get-ExcelCellValue {
    param(
        [System.Xml.XmlElement]$Cell,
        [object[]]$SharedStrings
    )

    $type = $Cell.GetAttribute("t")
    if ($type -eq "inlineStr") {
        return ($Cell.SelectNodes(".//*[local-name()='t']") | ForEach-Object { $_.InnerText }) -join ""
    }

    $valueNode = $Cell.SelectSingleNode("./*[local-name()='v']")
    if ($null -eq $valueNode) {
        return ""
    }
    if ($type -eq "s") {
        return [string]$SharedStrings[[int]$valueNode.InnerText]
    }
    return [string]$valueNode.InnerText
}

function Get-DiscoveredExcelTestIds {
    param([string]$Solution)

    $output = @(& dotnet test $Solution --list-tests --no-restore 2>&1)
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $output | ForEach-Object { Write-Host $_ }
        throw "Could not discover Excel-driven tests. dotnet test exited with code $exitCode."
    }

    $ids = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($match in [regex]::Matches(($output -join "`n"), '\bTC_M\d+_\d+\b')) {
        [void]$ids.Add($match.Value)
    }
    return ,$ids
}

if (-not [string]::IsNullOrWhiteSpace($TestCase) -and -not [string]::IsNullOrWhiteSpace($TestFilter)) {
    Write-Error "Use either -TestCase or -TestFilter, not both."
    exit 1
}

if (-not (Test-Path -LiteralPath $testProjectPath)) {
    Write-Error "Test project not found: $testProjectPath"
    exit 1
}

if (([string]::IsNullOrWhiteSpace($TestFilter)) -and -not (Test-Path -LiteralPath $testcaseWorkbookPath)) {
    Write-Error "Testcase workbook not found: $testcaseWorkbookPath"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($TestFilter)) {
    try {
        $allTestCases = @(Read-WorkbookTestCases -Path $testcaseWorkbookPath)
        $discoveredIds = Get-DiscoveredExcelTestIds -Solution $testProjectPath
    }
    catch {
        Write-Error $_.Exception.Message
        exit 1
    }

    $unsupportedCases = @($allTestCases | Where-Object { -not $discoveredIds.Contains($_.Id) })
    $manualCases = @($unsupportedCases | Where-Object { $manualReviewIds.Contains($_.Id) })
    $needsHandler = $unsupportedCases.Count - $manualCases.Count
    Write-Host "Excel testcases: $($allTestCases.Count) total; $($allTestCases.Count - $unsupportedCases.Count) executable; $needsHandler need a shared handler; $($manualCases.Count) require manual/provider review."

    if ($unsupportedCases.Count -gt 0) {
        Write-Host "Not yet automated:"
        $unsupportedCases | ForEach-Object { Write-Host ("- {0}: {1}" -f $_.Id, $_.Title) }
    }

    if (-not [string]::IsNullOrWhiteSpace($TestCase)) {
        $matchingTestCases = @($allTestCases | Where-Object {
            $case = $_
            @($case.Id, $case.Module, $case.Feature, $case.Type, $case.Title, $case.TestData) |
                Where-Object { $_.IndexOf($TestCase, [StringComparison]::OrdinalIgnoreCase) -ge 0 }
        })
        if ($matchingTestCases.Count -eq 0) {
            Write-Error "No testcase in the Excel workbook matched '$TestCase'."
            exit 1
        }

        $automatedIds = @($matchingTestCases |
            Where-Object { $discoveredIds.Contains($_.Id) } |
            Select-Object -ExpandProperty Id -Unique)
        Write-Host "Testcases matching '$TestCase':"
        $matchingTestCases | ForEach-Object {
            $coverage = if ($discoveredIds.Contains($_.Id)) { "automated" }
                elseif ($manualReviewIds.Contains($_.Id)) { "manual/provider review" }
                else { "needs a shared handler" }
            Write-Host ("- {0}: {1} [{2}]" -f $_.Id, $_.Title, $coverage)
        }

        if ($automatedIds.Count -eq 0) {
            Write-Error "Matching Excel testcase(s) have no executable shared scenario; review the status above. No unrelated tests were run."
            exit 1
        }

        $dynamicTestName = "QuanLyKhoHang.Tests.ExcelTestCaseIntegrationTests.Execute_testcase_from_workbook"
        $TestFilter = ($automatedIds | ForEach-Object {
            "(FullyQualifiedName~$dynamicTestName&DisplayName~$_)"
        }) -join "|"
    }
}

New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null

$dotnetArguments = @(
    "test",
    $testProjectPath,
    "--logger",
    "trx;LogFileName=$trxFileName",
    "--results-directory",
    $resultsDirectory
)
if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
    $dotnetArguments += @("--filter", $TestFilter)
}

Write-Host "Running: dotnet $($dotnetArguments -join ' ')"
try {
    & dotnet @dotnetArguments
    $testExitCode = $LASTEXITCODE
}
catch {
    Write-Error "Could not start dotnet test: $($_.Exception.Message)"
    exit 1
}

if (-not (Test-Path -LiteralPath $trxPath)) {
    Write-Host ""
    Write-Host "Could not find TRX report: $trxPath"
    Write-Host "dotnet test exit code: $testExitCode"
    exit $(if ($testExitCode -eq 0) { 1 } else { $testExitCode })
}

try {
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
    $testResults = @($trx.SelectNodes("//*[local-name()='UnitTestResult']"))
}
catch {
    Write-Error "Could not read TRX report '$trxPath': $($_.Exception.Message)"
    exit 1
}

$failed = @($testResults | Where-Object { $_.GetAttribute("outcome") -eq "Failed" })
$passed = @($testResults | Where-Object { $_.GetAttribute("outcome") -eq "Passed" })
$skipped = @($testResults | Where-Object {
    $_.GetAttribute("outcome") -in @("NotExecuted", "Skipped")
})

Write-Host ""
Write-Host "========== TEST RESULTS =========="
Write-Host ("Total: {0} | Succeeded: {1} | Failed: {2} | Skipped: {3}" -f `
    $testResults.Count, $passed.Count, $failed.Count, $skipped.Count)

if ($failed.Count -gt 0) {
    Write-Host ""
    Write-Host "FAILED - error details:" -ForegroundColor Red
    foreach ($result in $failed) {
        $name = $result.GetAttribute("testName")
        $message = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='Message']")
        $stackTrace = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='StackTrace']")

        Write-Host ("- {0}" -f $name) -ForegroundColor Red
        if ($message -and -not [string]::IsNullOrWhiteSpace($message.InnerText)) {
            Write-Host ("  Error: {0}" -f $message.InnerText.Trim())
        }
        if ($stackTrace -and -not [string]::IsNullOrWhiteSpace($stackTrace.InnerText)) {
            Write-Host "  Stack trace:"
            Write-Host $stackTrace.InnerText.Trim()
        }
    }
}

if ($passed.Count -gt 0) {
    Write-Host ""
    Write-Host "SUCCEEDED:" -ForegroundColor Green
    $passed | ForEach-Object { Write-Host ("- {0}" -f $_.GetAttribute("testName")) }
}

if ($skipped.Count -gt 0) {
    Write-Host ""
    Write-Host "SKIPPED:" -ForegroundColor Yellow
    foreach ($result in $skipped) {
        $name = $result.GetAttribute("testName")
        $reason = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='Message']")
        if (-not $reason) {
            $reason = $result.SelectSingleNode(".//*[local-name()='Text']")
        }

        if ($reason -and -not [string]::IsNullOrWhiteSpace($reason.InnerText)) {
            Write-Host ("- {0} - {1}" -f $name, $reason.InnerText.Trim()) -ForegroundColor Yellow
        }
        else {
            Write-Host ("- {0}" -f $name) -ForegroundColor Yellow
        }
    }
}

if ($testResults.Count -eq 0) {
    Write-Host "No test cases were executed."
}

Write-Host ""
Write-Host "TRX report: $trxPath"

if ($testExitCode -ne 0) {
    Write-Host "dotnet test exited with code $testExitCode." -ForegroundColor Red
    exit $testExitCode
}

if ($failed.Count -gt 0) {
    exit 1
}

exit 0
