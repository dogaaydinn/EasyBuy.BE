@echo off
REM EasyBuy.BE - Complete Setup Script for Windows
REM This script sets up the database, runs migrations, seeds data

echo.
echo ========================================
echo EasyBuy.BE Enterprise E-Commerce Platform
echo Complete Setup Script
echo ========================================
echo.

REM Check if .NET is installed
echo [1/8] Checking .NET installation...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found. Please install .NET 9.0 or later.
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo SUCCESS: .NET SDK version: %DOTNET_VERSION%
echo.

REM Restore NuGet packages
echo [2/8] Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore NuGet packages
    pause
    exit /b 1
)
echo SUCCESS: NuGet packages restored
echo.

REM Build the solution
echo [3/8] Building the solution...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo SUCCESS: Solution built successfully
echo.

REM Run unit tests
echo [4/8] Running unit tests...
dotnet test --no-build --configuration Release --verbosity minimal
if %errorlevel% neq 0 (
    echo WARNING: Some tests failed
) else (
    echo SUCCESS: All tests passed
)
echo.

REM Check if migrations exist
echo [5/8] Checking for existing migrations...
if exist "Infrastructure\EasyBuy.Persistence\Migrations" (
    echo Migrations already exist. Skipping creation...
) else (
    echo Creating database migrations...
    dotnet ef migrations add InitialCreate --project Infrastructure\EasyBuy.Persistence\EasyBuy.Persistence.csproj --startup-project Presentation\EasyBuy.WebAPI\EasyBuy.WebAPI.csproj --context EasyBuyDbContext
    if %errorlevel% neq 0 (
        echo ERROR: Failed to create migrations
        pause
        exit /b 1
    )
    echo SUCCESS: Database migrations created
)
echo.

REM Apply database migrations
echo [6/8] Applying database migrations...
dotnet ef database update --project Infrastructure\EasyBuy.Persistence\EasyBuy.Persistence.csproj --startup-project Presentation\EasyBuy.WebAPI\EasyBuy.WebAPI.csproj --context EasyBuyDbContext
if %errorlevel% neq 0 (
    echo ERROR: Failed to apply migrations
    echo Please ensure PostgreSQL is running and connection string is correct
    pause
    exit /b 1
)
echo SUCCESS: Database updated with latest migrations
echo SUCCESS: Database seeded with initial data
echo.

REM Display completion message
echo [7/8] Setup complete!
echo.
echo ============================================
echo           SETUP COMPLETED SUCCESSFULLY
echo ============================================
echo.
echo Default Test Credentials:
echo.
echo   Admin Account:
echo     Email:    admin@easybuy.com
echo     Password: Admin@123456
echo     Role:     Admin
echo.
echo   Manager Account:
echo     Email:    manager@easybuy.com
echo     Password: Manager@123456
echo     Role:     Manager
echo.
echo   Customer Account:
echo     Email:    customer@easybuy.com
echo     Password: Customer@123456
echo     Role:     Customer
echo.
echo Quick Start:
echo.
echo   1. Start the API:
echo      cd Presentation\EasyBuy.WebAPI
echo      dotnet run
echo.
echo   2. Access Swagger UI:
echo      https://localhost:7001/swagger
echo.
echo   3. Test the API:
echo      - Register: POST /api/v1/auth/register
echo      - Login:    POST /api/v1/auth/login
echo      - Orders:   POST /api/v1/orders
echo.
echo Documentation:
echo   - MIGRATION_GUIDE.md - Database setup guide
echo   - WEEK1_TEST_PLAN.md - Testing guide
echo   - Tests/README.md    - Unit tests guide
echo   - ROADMAP.md         - Development roadmap
echo.
echo Happy Coding!
echo.
pause
