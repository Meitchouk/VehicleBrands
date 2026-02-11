using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VehicleBrands.Domain.Interfaces;
using VehicleBrands.Infrastructure.Data;
using VehicleBrands.Infrastructure.Repositories;

namespace VehicleBrands.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure layer services
/// in the dependency injection container.
/// Follows the Open/Closed Principle (OCP) by allowing new registrations
/// without modifying existing code.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the DbContext and infrastructure repositories.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Build service provider to get logger for connection string parsing
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("VehicleBrands.Infrastructure.DependencyInjection");

        var connectionString = GetConnectionString(configuration, logger);

        // Configure DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    // Enable retry on failure for connection resilience
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    // Command timeout for slow Railway connections
                    npgsqlOptions.CommandTimeout(60);
                }));

        // Register repositories (Repository pattern + DI)
        services.AddScoped<ICarBrandRepository, CarBrandRepository>();

        return services;
    }

    /// <summary>
    /// Gets the PostgreSQL connection string from either DATABASE_URL (Railway/Heroku)
    /// or ConnectionStrings:DefaultConnection (appsettings.json)
    /// </summary>
    private static string GetConnectionString(IConfiguration configuration, ILogger? logger)
    {
        // Railway and Heroku provide DATABASE_URL in format:
        // postgres://user:password@host:port/database OR
        // postgresql://user:password@host:port/database
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrEmpty(databaseUrl))
        {
            try
            {
                logger?.LogInformation("DATABASE_URL detected, parsing connection string...");

                // Support both postgres:// and postgresql:// schemes
                var normalizedUrl = databaseUrl.StartsWith("postgres://")
                    ? databaseUrl.Replace("postgres://", "postgresql://")
                    : databaseUrl;

                var uri = new Uri(normalizedUrl);
                var userInfo = uri.UserInfo?.Split(':') ?? Array.Empty<string>();

                if (userInfo.Length < 2)
                {
                    throw new InvalidOperationException($"DATABASE_URL format is invalid. UserInfo expected 'user:password', got: '{uri.UserInfo}'");
                }

                var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Database = uri.LocalPath.TrimStart('/'),
                    Username = userInfo[0],
                    Password = userInfo[1],
                    SslMode = Npgsql.SslMode.Require, // Railway requires SSL
                    Pooling = true,
                    MinPoolSize = 0,
                    MaxPoolSize = 20
                };

                var connectionString = connectionStringBuilder.ToString();

                logger?.LogInformation("Successfully parsed DATABASE_URL. Host: {Host}, Port: {Port}, Database: {Database}, SSL: {SslMode}",
                    connectionStringBuilder.Host,
                    connectionStringBuilder.Port,
                    connectionStringBuilder.Database,
                    connectionStringBuilder.SslMode);

                return connectionString;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to parse DATABASE_URL environment variable. Using fallback connection string from configuration.");
                // Fall through to appsettings.json
            }
        }
        else
        {
            logger?.LogInformation("DATABASE_URL not found, using connection string from appsettings.json");
        }

        // Fallback to traditional connection string from appsettings.json
        var configConnectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(configConnectionString))
        {
            throw new InvalidOperationException(
                "No database connection string found. Set DATABASE_URL environment variable or configure ConnectionStrings:DefaultConnection in appsettings.json");
        }

        logger?.LogInformation("Using connection string from configuration");
        return configConnectionString;
    }
}
