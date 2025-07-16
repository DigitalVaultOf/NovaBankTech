FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia os arquivos .csproj com estrutura correta (contexto é a raiz)
COPY Pix.Api/Pix.Api.csproj Pix.Api/
COPY Shared.Messages/Shared.Messages.csproj Shared.Messages/

# Restore das dependências
RUN dotnet restore Pix.Api/Pix.Api.csproj

# Copia todo o código fonte
COPY . .

# Build
WORKDIR /src/Pix.Api
RUN dotnet build Pix.Api.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Pix.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Copia scripts se existirem
RUN if [ -d "/src/Pix.Api/Data/Scripts" ]; then \
    mkdir -p /app/publish/Scripts && cp -r /src/Pix.Api/Data/Scripts/* /app/publish/Scripts/; \
    else echo "Scripts folder not found, skipping..."; \
    fi

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Pix.Api.dll"]