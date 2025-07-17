FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os arquivos de projeto
COPY ["Bank.Api/Bank.Api.csproj", "Bank.Api/"]
COPY ["Shared.Messages/Shared.Messages.csproj", "Shared.Messages/"]

# Restore das dependências
RUN dotnet restore "Bank.Api/Bank.Api.csproj"

# Copia todo o código fonte
COPY . .

# Build e publish
WORKDIR "/src/Bank.Api"
RUN dotnet build "Bank.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Bank.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copia a pasta Scripts para o container
COPY --from=build /src/Bank.Api/Data/Scripts ./Scripts

ENTRYPOINT ["dotnet", "Bank.Api.dll"]
