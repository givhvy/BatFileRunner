@echo off
echo ====================================
echo Building BatRunner WPF Application
echo ====================================
echo.

REM Find MSBuild
set "MSBUILD="

REM Try common locations
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
)
if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
)
if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)

REM Try Visual Studio 2019
if "%MSBUILD%"=="" (
    if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
        set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    )
)
if "%MSBUILD%"=="" (
    if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
        set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
    )
)

if "%MSBUILD%"=="" (
    echo ERROR: MSBuild not found!
    echo.
    echo Please install Visual Studio 2019 or 2022 with .NET desktop development workload.
    echo Or install .NET SDK from: https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

echo Found MSBuild at: %MSBUILD%
echo.

REM Clean previous build
echo Cleaning previous build...
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj\Release" rmdir /s /q "obj\Release"
echo.

REM Build the project
echo Building project...
"%MSBUILD%" BatRunner.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Rebuild /v:minimal

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ====================================
    echo BUILD FAILED!
    echo ====================================
    pause
    exit /b 1
)

echo.
echo ====================================
echo BUILD SUCCESS!
echo ====================================
echo.
echo Executable location:
echo bin\Release\net8.0-windows\BatRunner.exe
echo.
echo Running application...
start "" "bin\Release\net8.0-windows\BatRunner.exe"
pause
