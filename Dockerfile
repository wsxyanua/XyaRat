# Docker image to build XyaRat on Linux
FROM mcr.microsoft.com/dotnet/sdk:9.0

# Install Mono and build tools
RUN apt-get update && apt-get install -y \
    mono-complete \
    mono-xbuild \
    nuget \
    wget \
    unzip \
    curl \
    gnupg \
    zip \
    && rm -rf /var/lib/apt/lists/*

# Install Node.js 20
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs

# Set working directory
WORKDIR /build

# Copy project files
COPY . .

# Build script
RUN chmod +x /build/build.sh

# Default command
CMD ["/bin/bash", "-c", "./build.sh && echo 'Build complete! Output in Binaries/Release/'"]
