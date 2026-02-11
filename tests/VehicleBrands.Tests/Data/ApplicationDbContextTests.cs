using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleBrands.Infrastructure.Data;
using VehicleBrands.Tests.Fixtures;

namespace VehicleBrands.Tests.Data;

/// <summary>
/// Unit tests for ApplicationDbContext.
/// Verifies model configuration, data seed, and table constraints.
/// </summary>
public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextTests()
    {
        // Use the factory to get a seeded context
        _context = InMemoryDbContextFactory.Create();
    }

    [Fact]
    public void DbContext_ShouldCreateCarBrandsDbSet()
    {
        // Assert
        _context.CarBrands.Should().NotBeNull();
    }

    [Fact]
    public async Task DataSeed_ShouldInsertFiveBrands()
    {
        // Act
        var brands = await _context.CarBrands.ToListAsync();

        // Assert - Now seeding 56 brands via intelligent seeder
        brands.Should().HaveCount(56);
    }

    [Fact]
    public async Task DataSeed_ShouldContainToyota()
    {
        // Act
        var toyota = await _context.CarBrands
            .FirstOrDefaultAsync(b => b.Name == "Toyota");

        // Assert
        toyota.Should().NotBeNull();
        toyota!.CountryOfOrigin.Should().Be("Japan");
        toyota.FoundedYear.Should().Be(1937);
        toyota.Website.Should().Be("https://www.toyota.com");
        toyota.IsLuxury.Should().BeFalse();
        toyota.Headquarters.Should().Be("Toyota City, Aichi");
        toyota.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DataSeed_ShouldContainFord()
    {
        // Act
        var ford = await _context.CarBrands
            .FirstOrDefaultAsync(b => b.Name == "Ford");

        // Assert
        ford.Should().NotBeNull();
        ford!.CountryOfOrigin.Should().Be("United States");
        ford.FoundedYear.Should().Be(1903);
    }

    [Fact]
    public async Task DataSeed_ShouldContainBMW()
    {
        // Act
        var bmw = await _context.CarBrands
            .FirstOrDefaultAsync(b => b.Name == "BMW");

        // Assert
        bmw.Should().NotBeNull();
        bmw!.CountryOfOrigin.Should().Be("Germany");
        bmw.FoundedYear.Should().Be(1916);
        bmw.IsLuxury.Should().BeTrue();
    }

    [Fact]
    public async Task DataSeed_ShouldContainFerrari()
    {
        // Act
        var ferrari = await _context.CarBrands
            .FirstOrDefaultAsync(b => b.Name == "Ferrari");

        // Assert
        ferrari.Should().NotBeNull();
        ferrari!.CountryOfOrigin.Should().Be("Italy");
        ferrari.FoundedYear.Should().Be(1947);
        ferrari.IsLuxury.Should().BeTrue();
    }

    [Fact]
    public async Task DataSeed_ShouldContainHyundai()
    {
        // Act
        var hyundai = await _context.CarBrands
            .FirstOrDefaultAsync(b => b.Name == "Hyundai");

        // Assert
        hyundai.Should().NotBeNull();
        hyundai!.CountryOfOrigin.Should().Be("South Korea");
        hyundai.FoundedYear.Should().Be(1967);
    }

    [Fact]
    public async Task DataSeed_AllBrands_ShouldHaveValidData()
    {
        // Act
        var brands = await _context.CarBrands.ToListAsync();

        // Assert
        brands.Should().AllSatisfy(b =>
        {
            b.Id.Should().BeGreaterThan(0);
            b.Name.Should().NotBeNullOrWhiteSpace();
            b.CountryOfOrigin.Should().NotBeNullOrWhiteSpace();
            b.FoundedYear.Should().BeGreaterThan(1800);
            b.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public void DbContext_WithOptions_ShouldBeCreatedSuccessfully()
    {
        // Assert
        _context.Should().NotBeNull();
        _context.Database.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
