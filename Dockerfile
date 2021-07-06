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
RUN pwd
RUN ls -ltr

FROM build-env AS publish
RUN pwd
RUN ls -ltr
RUN dotnet publish /src -c Release -o /application/publish

FROM runtime-env AS finalimage
WORKDIR /application
RUN ls -ltr
COPY --from=publish /application/publish .
ENTRYPOINT ["dotnet", "CCCount_DotNet5.dll"]

