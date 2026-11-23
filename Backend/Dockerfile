FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json ./
COPY Backend/src/CookEase.Domain/CookEase.Domain.csproj Backend/src/CookEase.Domain/
COPY Backend/src/CookEase.Application/CookEase.Application.csproj Backend/src/CookEase.Application/
COPY Backend/src/CookEase.Infrastructure/CookEase.Infrastructure.csproj Backend/src/CookEase.Infrastructure/
COPY Backend/src/CookEase.Api/CookEase.Api.csproj Backend/src/CookEase.Api/
RUN dotnet restore Backend/src/CookEase.Api/CookEase.Api.csproj

COPY Backend/src/ Backend/src/
RUN dotnet publish Backend/src/CookEase.Api/CookEase.Api.csproj \
    --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CookEase.Api.dll"]
