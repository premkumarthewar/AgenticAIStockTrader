# Not currently used to run the app locally (no Docker Desktop in this environment) -
# written for future CI/deployment use. Build from the `backend/` directory as the
# context, e.g.:
#   docker build -f src/StockTrader.Api/Dockerfile -t stocktrader-api .
# so that every project's relative ProjectReference paths resolve correctly.

# ---- Build stage --------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# global.json first so the SDK feature-band pin is honored before anything else runs.
COPY global.json ./
COPY StockTrader.slnx ./

# Copy only the .csproj/solution files first so `dotnet restore` is cached in its own
# Docker layer and only re-runs when a project reference or package actually changes,
# not on every source-code edit.
COPY src/StockTrader.AI/StockTrader.AI.csproj src/StockTrader.AI/
COPY src/StockTrader.Api/StockTrader.Api.csproj src/StockTrader.Api/
COPY src/StockTrader.Application/StockTrader.Application.csproj src/StockTrader.Application/
COPY src/StockTrader.Contracts/StockTrader.Contracts.csproj src/StockTrader.Contracts/
COPY src/StockTrader.Domain/StockTrader.Domain.csproj src/StockTrader.Domain/
COPY src/StockTrader.Infrastructure/StockTrader.Infrastructure.csproj src/StockTrader.Infrastructure/
COPY src/StockTrader.Persistence/StockTrader.Persistence.csproj src/StockTrader.Persistence/
COPY src/StockTrader.Shared/StockTrader.Shared.csproj src/StockTrader.Shared/

RUN dotnet restore src/StockTrader.Api/StockTrader.Api.csproj

# Now copy everything else (tests/ is excluded via .dockerignore) and publish.
COPY src/ src/

RUN dotnet publish src/StockTrader.Api/StockTrader.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ---- Runtime stage -------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Run as a non-root user rather than the image's default root.
RUN adduser --disabled-password --gecos "" --uid 5678 appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StockTrader.Api.dll"]
