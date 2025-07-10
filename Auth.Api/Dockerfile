FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o csproj relativo ao contexto ./Auth.Api
COPY ["Auth.Api.csproj", "./"]

RUN dotnet restore "Auth.Api.csproj"

COPY . .

RUN dotnet publish "Auth.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
