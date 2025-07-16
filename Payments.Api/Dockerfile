# Estágio 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia o arquivo .csproj e restaura as dependências primeiro para otimizar o cache do Docker
COPY ["Payments.Api.csproj", "./"]
RUN dotnet restore "./Payments.Api.csproj"

# Copia todo o resto do código fonte
COPY . .
WORKDIR "/src"
RUN dotnet build "./Payments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio 2: Publicação da aplicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Payments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Copia a pasta de Scripts, se ela existir (exatamente como no seu Pix.Api)
RUN if [ -d "/src/Data/Scripts" ]; then \
    mkdir -p /app/publish/Scripts && cp -r /src/Data/Scripts/* /app/publish/Scripts/; \
    else echo "Scripts folder not found, skipping..."; \
    fi

# Estágio 3: Imagem final, otimizada para execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
USER $APP_UID
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payments.Api.dll"]