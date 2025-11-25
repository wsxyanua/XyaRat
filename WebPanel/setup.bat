@echo off
REM XyaRat Web Panel - Quick Start Script (Windows)

echo.
echo ======================================
echo  XyaRat Web Panel - Quick Start
echo ======================================
echo.

REM Check prerequisites
echo Checking prerequisites...

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found. Please install .NET 6.0 or later.
    pause
    exit /b 1
)

where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Node.js not found. Please install Node.js 18+ and npm.
    pause
    exit /b 1
)

echo [OK] Prerequisites OK
echo.

REM Backend setup
echo Setting up backend...
cd /d "%~dp0"

dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to restore NuGet packages
    pause
    exit /b 1
)

echo [OK] Backend setup complete
echo.

REM Frontend setup
echo Setting up frontend...
cd ClientApp

call npm install
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to install npm packages
    pause
    exit /b 1
)

call npm run build
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to build frontend
    pause
    exit /b 1
)

echo [OK] Frontend setup complete
echo.

cd ..

REM Display instructions
echo ======================================
echo  Setup complete!
echo ======================================
echo.
echo Quick Start:
echo.
echo   1. Start the server:
echo      cd WebPanel
echo      dotnet run
echo.
echo   2. Open browser:
echo      http://localhost:5000
echo.
echo   3. Login with default credentials:
echo      Username: admin
echo      Password: admin123
echo.
echo   WARNING: Change default password after first login!
echo.
echo For development mode with hot reload:
echo   Terminal 1: cd WebPanel ^&^& dotnet run
echo   Terminal 2: cd WebPanel\ClientApp ^&^& npm run dev
echo   Access at: http://localhost:3000
echo.

pause
