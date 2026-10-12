$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$summaryPath = Join-Path $root "testcase_summary.txt"
$dumpPath = Join-Path $root "testcase_dump.txt"
$testSourcePath = Join-Path $PSScriptRoot "QuanLyKhoHang.Tests"

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

$automatedCases = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
Get-ChildItem -LiteralPath $testSourcePath -Recurse -Filter *.cs |
    Select-String -Pattern 'Trait\("TC",\s*"(TC_M\d+_\d+)"\)' -AllMatches |
    ForEach-Object {
        foreach ($match in $_.Matches) {
            [void]$automatedCases.Add($match.Groups[1].Value)
        }
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

Write-Host "Testcase source consistency: $($summaryCases.Count) IDs match in both files."
Write-Host "Automated coverage: $($covered.Count)/$($summaryCases.Count)"
Write-Host "Not yet automated: $($uncovered.Count)"
Write-Host "Covered IDs: $($covered -join ', ')"
