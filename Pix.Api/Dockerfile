FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Pix.Api.csproj", "./"]

RUN dotnet restore "./Pix.Api.csproj"

COPY . .

WORKDIR "/src"
RUN dotnet build "./Pix.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Pix.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

RUN if [ -d "/src/Data/Scripts" ]; then \
    mkdir -p /app/publish/Scripts && cp -r /src/Data/Scripts/* /app/publish/Scripts/; \
    else echo "Scripts folder not found, skipping..."; \
    fi

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Pix.Api.dll"]
