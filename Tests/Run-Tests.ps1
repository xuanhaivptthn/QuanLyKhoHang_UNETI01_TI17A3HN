param(
    [string]$TestFilter
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repositoryRoot "QuanLyKhoHang_UNETI01_TI17A3HN.slnx"
$resultsRoot = Join-Path ([System.IO.Path]::GetTempPath()) "QuanLyKhoHang-TestResults"
$runId = "{0}-{1}" -f (Get-Date -Format "yyyyMMdd-HHmmss"), ([guid]::NewGuid().ToString("N"))
$resultsDirectory = Join-Path $resultsRoot $runId
$trxFileName = "test-results.trx"
$trxPath = Join-Path $resultsDirectory $trxFileName

if (-not (Test-Path -LiteralPath $solutionPath)) {
    Write-Error "Solution file not found: $solutionPath"
    exit 1
}

New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null

$dotnetArguments = @(
    "test",
    $solutionPath,
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
