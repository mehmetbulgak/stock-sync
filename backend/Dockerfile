FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/StockSync.Domain/StockSync.Domain.csproj src/StockSync.Domain/packages.lock.json src/StockSync.Domain/
COPY src/StockSync.Application/StockSync.Application.csproj src/StockSync.Application/packages.lock.json src/StockSync.Application/
COPY src/StockSync.Infrastructure/StockSync.Infrastructure.csproj src/StockSync.Infrastructure/packages.lock.json src/StockSync.Infrastructure/
COPY src/StockSync.Api/StockSync.Api.csproj src/StockSync.Api/packages.lock.json src/StockSync.Api/
RUN dotnet restore src/StockSync.Api/StockSync.Api.csproj --locked-mode

COPY src/ src/
RUN dotnet publish src/StockSync.Api/StockSync.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "StockSync.Api.dll"]
