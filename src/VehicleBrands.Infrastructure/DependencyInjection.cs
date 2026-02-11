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
    /// Resolves the PostgreSQL connection string from env vars or appsettings.
    /// Call this before AddInfrastructure to get the connection string for health checks, etc.
    /// </summary>
    public static string ResolveConnectionString(IConfiguration configuration, ILoggerFactory? loggerFactory = null)
    {
        var logger = loggerFactory?.CreateLogger("VehicleBrands.Infrastructure.DependencyInjection");
        return GetConnectionString(configuration, logger);
    }

    /// <summary>
    /// Registers the DbContext and infrastructure repositories.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? resolvedConnectionString = null)
    {
        // Build service provider to get logger for connection string parsing
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("VehicleBrands.Infrastructure.DependencyInjection");

        var connectionString = resolvedConnectionString ?? GetConnectionString(configuration, logger);

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
    /// Gets the PostgreSQL connection string from environment variables or appsettings.json.
    /// Priority: DATABASE_URL > DATABASE_PRIVATE_URL > PGHOST individual vars > appsettings.json
    /// </summary>
    private static string GetConnectionString(IConfiguration configuration, ILogger? logger)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL");

        if (!string.IsNullOrEmpty(databaseUrl))
        {
            var source = Environment.GetEnvironmentVariable("DATABASE_URL") != null
                ? "DATABASE_URL" : "DATABASE_PRIVATE_URL";

            try
            {
                logger?.LogInformation("{Source} detected, parsing connection string...", source);

                // Support both postgres:// and postgresql:// schemes
                var normalizedUrl = databaseUrl.StartsWith("postgres://")
                    ? databaseUrl.Replace("postgres://", "postgresql://")
                    : databaseUrl;

                var uri = new Uri(normalizedUrl);
                var userInfo = uri.UserInfo?.Split(':') ?? Array.Empty<string>();

                if (userInfo.Length < 2)
                {
                    throw new InvalidOperationException(
                        $"{source} format is invalid. Expected 'user:password' in URI, got: '{uri.UserInfo}'");
                }

                var builder = new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Database = uri.LocalPath.TrimStart('/'),
                    Username = userInfo[0],
                    Password = userInfo[1],
                    SslMode = Npgsql.SslMode.Require,
                    Pooling = true,
                    MinPoolSize = 0,
                    MaxPoolSize = 20
                };

                logger?.LogInformation("Parsed {Source} -> Host: {Host}, Port: {Port}, Database: {Database}",
                    source, builder.Host, builder.Port, builder.Database);

                return builder.ToString();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to parse {Source}. Trying next fallback...", source);
            }
        }

        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT");
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

        if (!string.IsNullOrEmpty(pgHost))
        {
            logger?.LogInformation("PGHOST detected, building connection string from PG* variables...");

            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = pgHost,
                Port = int.TryParse(pgPort, out var port) ? port : 5432,
                Database = pgDatabase ?? "railway",
                Username = pgUser ?? "postgres",
                Password = pgPassword ?? "",
                SslMode = Npgsql.SslMode.Require,
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = 20
            };

            logger?.LogInformation("Built from PG* vars -> Host: {Host}, Port: {Port}, Database: {Database}",
                builder.Host, builder.Port, builder.Database);

            return builder.ToString();
        }

        // 3) Fallback to appsettings.json
        logger?.LogInformation("No DATABASE_URL or PG* variables found, trying appsettings.json...");

        var configConnectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(configConnectionString))
        {
            var envVars = string.Join(", ",
                new[] { "DATABASE_URL", "DATABASE_PRIVATE_URL", "PGHOST" }
                    .Select(v => $"{v}={(Environment.GetEnvironmentVariable(v) != null ? "set" : "NOT SET")}"));

            logger?.LogError("No connection string found anywhere. Env vars: {EnvVars}", envVars);

            throw new InvalidOperationException(
                "No database connection string found. " +
                "Set DATABASE_URL, DATABASE_PRIVATE_URL, or PGHOST environment variables, " +
                "or configure ConnectionStrings:DefaultConnection in appsettings.json. " +
                $"Current env check: {envVars}");
        }

        logger?.LogInformation("Using connection string from appsettings.json");
        return configConnectionString;
    }
}
