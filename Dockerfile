FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy everything else and build
COPY ../engine/examples ./
RUN dotnet build CCCount_DotNet5.sln -c Release -o out