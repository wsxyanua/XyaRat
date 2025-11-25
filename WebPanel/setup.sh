#!/bin/bash

# XyaRat Web Panel - Quick Start Script

echo "🚀 XyaRat Web Panel - Quick Start"
echo "=================================="

# Check prerequisites
echo "📋 Checking prerequisites..."

if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 6.0 or later."
    exit 1
fi

if ! command -v node &> /dev/null; then
    echo "❌ Node.js not found. Please install Node.js 18+ and npm."
    exit 1
fi

echo "✅ Prerequisites OK"

# Backend setup
echo ""
echo "🔧 Setting up backend..."
cd "$(dirname "$0")"

dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Failed to restore NuGet packages"
    exit 1
fi

echo "✅ Backend setup complete"

# Frontend setup
echo ""
echo "🎨 Setting up frontend..."
cd ClientApp

npm install
if [ $? -ne 0 ]; then
    echo "❌ Failed to install npm packages"
    exit 1
fi

npm run build
if [ $? -ne 0 ]; then
    echo "❌ Failed to build frontend"
    exit 1
fi

echo "✅ Frontend setup complete"

cd ..

# Display instructions
echo ""
echo "✨ Setup complete!"
echo ""
echo "📚 Quick Start:"
echo ""
echo "  1. Start the server:"
echo "     cd WebPanel"
echo "     dotnet run"
echo ""
echo "  2. Open browser:"
echo "     http://localhost:5000"
echo ""
echo "  3. Login with default credentials:"
echo "     Username: admin"
echo "     Password: admin123"
echo ""
echo "⚠️  IMPORTANT: Change default password after first login!"
echo ""
echo "🔗 For development mode with hot reload:"
echo "   Terminal 1: cd WebPanel && dotnet run"
echo "   Terminal 2: cd WebPanel/ClientApp && npm run dev"
echo "   Access at: http://localhost:3000"
echo ""
