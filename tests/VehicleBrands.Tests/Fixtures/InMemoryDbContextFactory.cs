using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleBrands.Infrastructure.Data;

namespace VehicleBrands.Tests.Fixtures;

/// <summary>
/// Factory for creating ApplicationDbContext instances with an in-memory database.
/// Provides isolated database contexts for each test to prevent side effects.
/// </summary>
public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Creates an ApplicationDbContext configured with an in-memory database.
    /// Each invocation generates an isolated database using a unique name.
    /// The database is seeded with 56 car brands using the intelligent seeder.
    /// </summary>
    /// <param name="databaseName">Optional name for the in-memory database.</param>
    /// <returns>A DbContext instance with seeded data.</returns>
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        // Ensure the database is created
        context.Database.EnsureCreated();

        // Seed the database with 56 car brands
        var logger = NullLogger.Instance;
        DatabaseSeeder.SeedAsync(context, logger).GetAwaiter().GetResult();

        return context;
    }
}
