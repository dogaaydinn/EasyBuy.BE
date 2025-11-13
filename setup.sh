#!/bin/bash

# EasyBuy.BE - Complete Setup Script
# This script sets up the database, runs migrations, seeds data, and starts the application

set -e  # Exit on any error

echo "🚀 EasyBuy.BE Enterprise E-Commerce Platform - Setup Script"
echo "============================================================"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check if .NET is installed
echo -e "${BLUE}[1/8]${NC} Checking .NET installation..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found. Please install .NET 9.0 or later.${NC}"
    echo "Download from: https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✅ .NET SDK version: $DOTNET_VERSION${NC}"
echo ""

# Check if PostgreSQL is running
echo -e "${BLUE}[2/8]${NC} Checking PostgreSQL connection..."
if command -v psql &> /dev/null; then
    echo -e "${GREEN}✅ PostgreSQL is installed${NC}"
else
    echo -e "${YELLOW}⚠️  PostgreSQL CLI not found. Ensure PostgreSQL is running.${NC}"
fi
echo ""

# Restore NuGet packages
echo -e "${BLUE}[3/8]${NC} Restoring NuGet packages..."
dotnet restore
echo -e "${GREEN}✅ NuGet packages restored${NC}"
echo ""

# Build the solution
echo -e "${BLUE}[4/8]${NC} Building the solution..."
dotnet build --configuration Release
echo -e "${GREEN}✅ Solution built successfully${NC}"
echo ""

# Run unit tests
echo -e "${BLUE}[5/8]${NC} Running unit tests..."
dotnet test --no-build --configuration Release --verbosity minimal
TEST_EXIT_CODE=$?
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✅ All tests passed${NC}"
else
    echo -e "${YELLOW}⚠️  Some tests failed. Check output above.${NC}"
fi
echo ""

# Create database migrations
echo -e "${BLUE}[6/8]${NC} Creating database migrations..."
if [ ! -d "Infrastructure/EasyBuy.Persistence/Migrations" ]; then
    dotnet ef migrations add InitialCreate \
        --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
        --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj \
        --context EasyBuyDbContext
    echo -e "${GREEN}✅ Database migrations created${NC}"
else
    echo -e "${YELLOW}⚠️  Migrations already exist. Skipping...${NC}"
fi
echo ""

# Apply database migrations
echo -e "${BLUE}[7/8]${NC} Applying database migrations..."
dotnet ef database update \
    --project Infrastructure/EasyBuy.Persistence/EasyBuy.Persistence.csproj \
    --startup-project Presentation/EasyBuy.WebAPI/EasyBuy.WebAPI.csproj \
    --context EasyBuyDbContext
echo -e "${GREEN}✅ Database updated with latest migrations${NC}"
echo -e "${GREEN}✅ Database seeded with initial data (see DatabaseSeeder.cs)${NC}"
echo ""

# Display seeded credentials
echo -e "${BLUE}[8/8]${NC} Setup complete! 🎉"
echo ""
echo -e "${GREEN}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║                  SETUP COMPLETED SUCCESSFULLY              ║${NC}"
echo -e "${GREEN}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${YELLOW}📝 Default Test Credentials:${NC}"
echo ""
echo -e "  ${BLUE}Admin Account:${NC}"
echo -e "    Email:    admin@easybuy.com"
echo -e "    Password: Admin@123456"
echo -e "    Role:     Admin"
echo ""
echo -e "  ${BLUE}Manager Account:${NC}"
echo -e "    Email:    manager@easybuy.com"
echo -e "    Password: Manager@123456"
echo -e "    Role:     Manager"
echo ""
echo -e "  ${BLUE}Customer Account:${NC}"
echo -e "    Email:    customer@easybuy.com"
echo -e "    Password: Customer@123456"
echo -e "    Role:     Customer"
echo ""
echo -e "${YELLOW}🎯 Quick Start:${NC}"
echo ""
echo -e "  1. Start the API:"
echo -e "     ${GREEN}cd Presentation/EasyBuy.WebAPI${NC}"
echo -e "     ${GREEN}dotnet run${NC}"
echo ""
echo -e "  2. Access Swagger UI:"
echo -e "     ${GREEN}https://localhost:7001/swagger${NC}"
echo ""
echo -e "  3. Test the API:"
echo -e "     - Register: POST /api/v1/auth/register"
echo -e "     - Login:    POST /api/v1/auth/login"
echo -e "     - Orders:   POST /api/v1/orders"
echo ""
echo -e "${YELLOW}📚 Documentation:${NC}"
echo -e "  - MIGRATION_GUIDE.md - Database setup guide"
echo -e "  - WEEK1_TEST_PLAN.md - Testing guide"
echo -e "  - Tests/README.md    - Unit tests guide"
echo -e "  - ROADMAP.md         - Development roadmap"
echo ""
echo -e "${GREEN}Happy Coding! 🚀${NC}"
echo ""
