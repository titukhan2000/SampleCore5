FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /application
EXPOSE 5000

ENV ASPNETCORE_URLS=http://*:5000

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY ./publish .
ENTRYPOINT ["dotnet", "CCCount_DotNet5.dll"]
