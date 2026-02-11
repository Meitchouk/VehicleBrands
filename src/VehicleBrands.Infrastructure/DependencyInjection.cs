using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var connectionString = GetConnectionString(configuration);

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
    private static string GetConnectionString(IConfiguration configuration)
    {
        // Railway and Heroku provide DATABASE_URL in format:
        // postgresql://user:password@host:port/database
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            // Parse DATABASE_URL format (Railway/Heroku style)
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            
            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.LocalPath.TrimStart('/'),
                Username = userInfo[0],
                Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
                SslMode = Npgsql.SslMode.Require // Railway requires SSL
            };
            
            return builder.ToString();
        }
        
        // Fallback to traditional connection string from appsettings.json
        return configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException(
                "No database connection string found. Set DATABASE_URL environment variable or configure ConnectionStrings:DefaultConnection in appsettings.json");
    }
}
