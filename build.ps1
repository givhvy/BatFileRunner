Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Building BatRunner WPF Application" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Try to find MSBuild
$msbuildPaths = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)

$msbuild = $null
foreach ($path in $msbuildPaths) {
    if (Test-Path $path) {
        $msbuild = $path
        break
    }
}

# Try to find MSBuild recursively if not found
if (-not $msbuild) {
    Write-Host "Searching for MSBuild..." -ForegroundColor Yellow
    $found = Get-ChildItem "C:\Program Files" -Recurse -Filter "MSBuild.exe" -ErrorAction SilentlyContinue |
             Where-Object { $_.FullName -like "*\Current\Bin\*" } |
             Select-Object -First 1

    if ($found) {
        $msbuild = $found.FullName
    }
}

if (-not $msbuild) {
    Write-Host "ERROR: MSBuild not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Visual Studio 2019 or 2022 with .NET desktop development workload." -ForegroundColor Yellow
    Write-Host "Or install .NET SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Found MSBuild at: $msbuild" -ForegroundColor Green
Write-Host ""

# Clean previous build
Write-Host "Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path "bin\Release") {
    Remove-Item "bin\Release" -Recurse -Force
}
if (Test-Path "obj\Release") {
    Remove-Item "obj\Release" -Recurse -Force
}
Write-Host ""

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
& $msbuild "BatRunner.csproj" /p:Configuration=Release /p:Platform=AnyCPU /t:Rebuild /v:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "====================================" -ForegroundColor Red
    Write-Host "BUILD FAILED!" -ForegroundColor Red
    Write-Host "====================================" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host "BUILD SUCCESS!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Executable location:" -ForegroundColor Cyan
Write-Host "bin\Release\net8.0-windows\BatRunner.exe" -ForegroundColor White
Write-Host ""
Write-Host "Running application..." -ForegroundColor Yellow
Start-Process "bin\Release\net8.0-windows\BatRunner.exe"
Write-Host ""
Read-Host "Press Enter to exit"
