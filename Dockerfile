# ─── Build Stage ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project and restore (caching layer)
COPY ShoppingApp.csproj .
RUN dotnet restore ShoppingApp.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish ShoppingApp.csproj -c Release -o /app/publish

# ─── Runtime Stage ────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Non-root user for security
RUN useradd -m -u 1000 appuser
COPY --from=build /app/publish .
RUN mkdir -p /app/logs && chown -R appuser:appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "ShoppingApp.dll"]