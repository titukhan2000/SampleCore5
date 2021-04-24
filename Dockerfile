FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ../SampleCore5/CCCount_DotNet5/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ../SampleCore5 ./
RUN dotnet build CCCount_DotNet5.sln -c Release -o out
