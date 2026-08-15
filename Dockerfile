# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["ShoppingApp.csproj", "."]
RUN dotnet restore "ShoppingApp.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "ShoppingApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ShoppingApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user
RUN groupadd --gid 1000 appgroup && \
    useradd --uid 1000 --gid appgroup --shell /bin/bash --create-home appuser

# Copy published app
COPY --from=publish /app/publish .

# Create logs directory with proper permissions
RUN mkdir -p /app/logs && chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "ShoppingApp.dll"]