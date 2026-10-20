$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$summaryPath = Join-Path $root "testcase_summary.txt"
$dumpPath = Join-Path $root "testcase_dump.txt"
$testProjectPath = Join-Path $PSScriptRoot "QuanLyKhoHang.Tests\QuanLyKhoHang.Tests.csproj"

foreach ($path in @($summaryPath, $dumpPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Required testcase source file not found: $path"
    }
}

$summaryCases = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
Get-Content -LiteralPath $summaryPath -Encoding UTF8 | ForEach-Object {
    if ($_ -match '^\s*ROW\s+\d+:\s+(TC_M\d+_\d+)\s+\|\|') {
        [void]$summaryCases.Add($Matches[1])
    }
}

$dumpCases = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$dumpText = Get-Content -LiteralPath $dumpPath -Raw -Encoding UTF8
[regex]::Matches($dumpText, '\bTC_M\d+_\d+\b') | ForEach-Object {
    [void]$dumpCases.Add($_.Value)
}

$discoveredTests = @(& dotnet test $testProjectPath --list-tests --no-restore 2>&1)
if ($LASTEXITCODE -ne 0) {
    $discoveredTests | ForEach-Object { Write-Host $_ }
    throw "Could not discover testcase IDs from xUnit."
}
$automatedCases = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
[regex]::Matches(($discoveredTests -join "`n"), '\bTC_M\d+_\d+\b') | ForEach-Object {
    [void]$automatedCases.Add($_.Value)
}

$summaryOnly = @($summaryCases | Where-Object { -not $dumpCases.Contains($_) } | Sort-Object)
$dumpOnly = @($dumpCases | Where-Object { -not $summaryCases.Contains($_) } | Sort-Object)
if ($summaryOnly.Count -gt 0 -or $dumpOnly.Count -gt 0) {
    Write-Error "The testcase ID sets in testcase_summary.txt and testcase_dump.txt do not match."
    if ($summaryOnly.Count -gt 0) { Write-Host "Only in summary: $($summaryOnly -join ', ')" }
    if ($dumpOnly.Count -gt 0) { Write-Host "Only in dump: $($dumpOnly -join ', ')" }
    exit 1
}

$covered = @($summaryCases | Where-Object { $automatedCases.Contains($_) } | Sort-Object)
$uncovered = @($summaryCases | Where-Object { -not $automatedCases.Contains($_) } | Sort-Object)
$manualReviewIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
@("TC_M3_025", "TC_M4_021", "TC_M6_006", "TC_M6_010") | ForEach-Object {
    [void]$manualReviewIds.Add($_)
}
$manualReview = @($uncovered | Where-Object { $manualReviewIds.Contains($_) } | Sort-Object)
$needsHandler = @($uncovered | Where-Object { -not $manualReviewIds.Contains($_) })

Write-Host "Testcase source consistency: $($summaryCases.Count) IDs match in both files."
Write-Host "Automated coverage: $($covered.Count)/$($summaryCases.Count)"
Write-Host "Needs shared handler: $($needsHandler.Count); manual review: $($manualReview.Count)"
Write-Host "Workbook classification: Catalog 45; Query/list 31; Warehouse workflows 61; Reports/security 42; Manual/provider review 4."
Write-Host "Manual review IDs: $($manualReview -join ', ')"
Write-Host "Covered IDs: $($covered -join ', ')"
