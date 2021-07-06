FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS runtime-env
WORKDIR /application
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env
WORKDIR /src
COPY . /src
RUN ls -ltr
RUN dotnet restore
COPY . .
RUN dotnet build CCCount_DotNet5.sln
RUN cd CCCount_DotNet5
RUN ls -ltr

FROM build-env AS publish
RUN dotnet publish src/CCCount_DotNet5/bin/Release/net5.0/CCCount_DotNet5.dll -c Release -o /application/publish

FROM runtime-env AS finalimage
WORKDIR /application
COPY --from=build-env /application/publish .
ENTRYPOINT ["dotnet", "CCCount_DotNet5.dll"]

