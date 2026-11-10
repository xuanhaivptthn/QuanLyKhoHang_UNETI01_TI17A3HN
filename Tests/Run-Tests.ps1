param(
    [string]$Module,
    [string]$TestCase,
    [string]$TestFilter,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$testProjectPath = Join-Path $PSScriptRoot "QuanLyKhoHang.Automation.csproj"

$filterArgs = @()

if (-not [string]::IsNullOrWhiteSpace($TestCase)) {
    $filterArgs += "FullyQualifiedName~$TestCase"
}

if (-not [string]::IsNullOrWhiteSpace($Module)) {
    $filterArgs += "TestCategory=$Module"
}

if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
    $filterArgs += $TestFilter
}

$dotnetArgs = @("test", $testProjectPath)

if ($NoBuild) {
    $dotnetArgs += "--no-build"
}

if ($filterArgs.Count -gt 0) {
    $combinedFilter = $filterArgs -join "&"
    $dotnetArgs += "--filter"
    $dotnetArgs += $combinedFilter
}

$dotnetArgs += "--logger"
$dotnetArgs += "console;verbosity=normal"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  QUAN LY KHO HANG - AUTOMATION TEST RUNNER (.NET 10)      " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Project: $testProjectPath" -ForegroundColor Gray
if ($filterArgs.Count -gt 0) {
    Write-Host "Filter : $($dotnetArgs[($dotnetArgs.IndexOf('--filter') + 1)])" -ForegroundColor Yellow
}
Write-Host ""

& dotnet @dotnetArgs
exit $LASTEXITCODE
