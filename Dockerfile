# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and all project files first (layer-cache friendly restore)
COPY SareeGrace.sln .
COPY NuGet.Config .
COPY SareeGrace.Domain/SareeGrace.Domain.csproj             SareeGrace.Domain/
COPY SareeGrace.Application/SareeGrace.Application.csproj   SareeGrace.Application/
COPY SareeGrace.Infrastructure/SareeGrace.Infrastructure.csproj SareeGrace.Infrastructure/
COPY SareeGrace.API/SareeGrace.API.csproj                   SareeGrace.API/

RUN dotnet restore SareeGrace.sln

# Copy all source code
COPY SareeGrace.Domain/       SareeGrace.Domain/
COPY SareeGrace.Application/  SareeGrace.Application/
COPY SareeGrace.Infrastructure/ SareeGrace.Infrastructure/
COPY SareeGrace.API/          SareeGrace.API/

# Publish the API project (references are resolved via solution restore above)
RUN dotnet publish SareeGrace.API/SareeGrace.API.csproj \
    --no-restore -c Release -o /app/out

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SareeGrace.API.dll"]
