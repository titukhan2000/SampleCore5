FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app
# Copy csproj and restore as distinct layers
#COPY *.csproj . /app
#RUN dotnet restore

# Copy everything else and build
#COPY ../engine/examples ./
RUN dotnet build CCCount_DotNet5.sln

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "aspnetapp.dll"]

