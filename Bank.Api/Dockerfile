FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bank.Api.csproj", "./"]
RUN dotnet restore "Bank.Api.csproj"
COPY . .
RUN dotnet build "Bank.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Bank.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY appsettings.json ./appsettings.json
ENTRYPOINT ["dotnet", "Bank.Api.dll"]

# Copia o script para o container
COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh
