using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleBrands.Domain.Interfaces;
using VehicleBrands.Infrastructure;
using VehicleBrands.Infrastructure.Data;
using VehicleBrands.Infrastructure.Repositories;

namespace VehicleBrands.Tests.Infrastructure;

/// <summary>
/// Unit tests for the infrastructure DependencyInjection extension.
/// Verifies that services are registered correctly in the DI container.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetService<ApplicationDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterICarBrandRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<ICarBrandRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<CarBrandRepository>();
    }

    [Fact]
    public void AddInfrastructure_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    /// <summary>
    /// Creates a test configuration with a mock connection string.
    /// </summary>
    private static IConfiguration CreateTestConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", "Host=localhost;Database=TestDb;Username=test;Password=test" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }
}
