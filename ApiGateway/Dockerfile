FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os arquivos .csproj com estrutura correta (contexto é a raiz)
COPY ApiGateway/ApiGateway.csproj ApiGateway/
# Se o ApiGateway usar Shared.Messages, descomente a linha abaixo:
# COPY Shared.Messages/Shared.Messages.csproj Shared.Messages/

# Restore das dependências
RUN dotnet restore ApiGateway/ApiGateway.csproj

# Copia todo o código fonte
COPY . .

# Build e publish
WORKDIR /src/ApiGateway
RUN dotnet build ApiGateway.csproj -c Release -o /app/build
RUN dotnet publish ApiGateway.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ApiGateway.dll"]