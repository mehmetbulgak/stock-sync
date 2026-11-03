FROM node:24-alpine AS web
WORKDIR /src/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src/backend
COPY backend/global.json backend/Directory.Build.props backend/Directory.Packages.props backend/.editorconfig ./
COPY backend/src/StockSync.Domain/StockSync.Domain.csproj backend/src/StockSync.Domain/packages.lock.json src/StockSync.Domain/
COPY backend/src/StockSync.Application/StockSync.Application.csproj backend/src/StockSync.Application/packages.lock.json src/StockSync.Application/
COPY backend/src/StockSync.Infrastructure/StockSync.Infrastructure.csproj backend/src/StockSync.Infrastructure/packages.lock.json src/StockSync.Infrastructure/
COPY backend/src/StockSync.Api/StockSync.Api.csproj backend/src/StockSync.Api/packages.lock.json src/StockSync.Api/
RUN dotnet restore src/StockSync.Api/StockSync.Api.csproj --locked-mode
COPY backend/src/ src/
RUN dotnet publish src/StockSync.Api/StockSync.Api.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS demo
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-16 nginx supervisor gettext-base \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=api /out ./
COPY --from=web /src/frontend/dist ./wwwroot
COPY deploy/render/ ./deploy/
COPY frontend/nginx.conf ./deploy/server.conf.template
RUN chmod +x deploy/*.sh \
    && chmod -R a-w /app
ENV PATH="/usr/lib/postgresql/16/bin:${PATH}" \
    PORT=10000 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://127.0.0.1:8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_gcServer=0 \
    PGDATA=/tmp/stocksync-postgres \
    PGHOST=/tmp/stocksync-postgres-socket \
    ConnectionStrings__Default="Host=/tmp/stocksync-postgres-socket;Database=stocksync;Username=stocksync;Maximum Pool Size=10" \
    Database__MigrateOnStartup=true \
    Database__SeedDemoData=true \
    ReverseProxy__Enabled=true
USER $APP_UID
EXPOSE 10000
ENTRYPOINT ["/app/deploy/start-demo.sh"]
