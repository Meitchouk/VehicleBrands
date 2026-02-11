# Stage 1: Restore dependencies (layer caching)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

# Copy project files first to leverage Docker layer caching
COPY nuget.config ./
COPY VehicleBrands.sln ./
COPY src/VehicleBrands.Domain/VehicleBrands.Domain.csproj src/VehicleBrands.Domain/
COPY src/VehicleBrands.Infrastructure/VehicleBrands.Infrastructure.csproj src/VehicleBrands.Infrastructure/
COPY src/VehicleBrands.API/VehicleBrands.API.csproj src/VehicleBrands.API/
COPY tests/VehicleBrands.Tests/VehicleBrands.Tests.csproj tests/VehicleBrands.Tests/

RUN dotnet restore VehicleBrands.sln

# Stage 2: Build the application
FROM restore AS build
COPY . .
RUN dotnet build VehicleBrands.sln -c Release --no-restore

# Stage 3: Publish the API
FROM build AS publish
RUN dotnet publish src/VehicleBrands.API/VehicleBrands.API.csproj -c Release --no-build -o /app/publish

# Stage 4: Final optimized image (runtime only)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Configure non-root user for security
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=publish /app/publish .

# Expose HTTP port
EXPOSE 8080

# ASP.NET Core environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "VehicleBrands.API.dll"]
