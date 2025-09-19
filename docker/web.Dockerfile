# Device Tracking Inventory System - Web Dashboard Dockerfile
# Multi-stage build for optimal image size and security

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and restore dependencies
COPY ["DeviceTracking.Inventory.sln", "."]
COPY ["DeviceTracking.Inventory.Web/DeviceTracking.Inventory.Web.csproj", "DeviceTracking.Inventory.Web/"]
COPY ["DeviceTracking.Inventory.Application/DeviceTracking.Inventory.Application.csproj", "DeviceTracking.Inventory.Application/"]
COPY ["DeviceTracking.Inventory.Infrastructure/DeviceTracking.Inventory.Infrastructure.csproj", "DeviceTracking.Inventory.Infrastructure/"]
COPY ["DeviceTracking.Inventory.Shared/DeviceTracking.Inventory.Shared.csproj", "DeviceTracking.Inventory.Shared/"]
COPY ["DeviceTracking.Inventory.QuickBooks/DeviceTracking.Inventory.QuickBooks.csproj", "DeviceTracking.Inventory.QuickBooks/"]
COPY ["DeviceTracking.Inventory.Sync/DeviceTracking.Inventory.Sync.csproj", "DeviceTracking.Inventory.Sync/"]

# Restore NuGet packages
RUN dotnet restore "DeviceTracking.Inventory.Web/DeviceTracking.Inventory.Web.csproj"

# Copy source code
COPY ["DeviceTracking.Inventory.Web/", "DeviceTracking.Inventory.Web/"]
COPY ["DeviceTracking.Inventory.Application/", "DeviceTracking.Inventory.Application/"]
COPY ["DeviceTracking.Inventory.Infrastructure/", "DeviceTracking.Inventory.Infrastructure/"]
COPY ["DeviceTracking.Inventory.Shared/", "DeviceTracking.Inventory.Shared/"]
COPY ["DeviceTracking.Inventory.QuickBooks/", "DeviceTracking.Inventory.QuickBooks/"]
COPY ["DeviceTracking.Inventory.Sync/", "DeviceTracking.Inventory.Sync/"]

# Build application
WORKDIR "/src/DeviceTracking.Inventory.Web"
RUN dotnet build "DeviceTracking.Inventory.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "DeviceTracking.Inventory.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser:appuser /app
USER appuser

# Copy published application
COPY --from=publish --chown=appuser:appuser /app/publish .

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/healthz || exit 1

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Entry point
ENTRYPOINT ["dotnet", "DeviceTracking.Inventory.Web.dll"]
