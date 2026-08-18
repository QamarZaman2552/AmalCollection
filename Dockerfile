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

# Non-root user (Choreo requires non-root; uses documented UID 10014)
RUN groupadd -g 10014 choreo && useradd --no-create-home --uid 10014 --gid 10014 choreo
COPY --from=build /app/publish .
RUN mkdir -p /app/logs && chown -R choreo:choreo /app
USER choreo

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "ShoppingApp.dll"]