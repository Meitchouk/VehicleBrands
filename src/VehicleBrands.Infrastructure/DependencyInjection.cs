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
        // Configure DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    // Enable retry on failure for connection resilience
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        // Register repositories (Repository pattern + DI)
        services.AddScoped<ICarBrandRepository, CarBrandRepository>();

        return services;
    }
}
