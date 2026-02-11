using FluentAssertions;
using VehicleBrands.Domain.Entities;
using VehicleBrands.Infrastructure.Data;
using VehicleBrands.Infrastructure.Repositories;
using VehicleBrands.Tests.Fixtures;

namespace VehicleBrands.Tests.Repositories;

/// <summary>
/// Unit tests for CarBrandRepository.
/// Validates data access behavior using an in-memory database.
/// </summary>
public class CarBrandRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CarBrandRepository _repository;

    public CarBrandRepositoryTests()
    {
        // Each test gets its own isolated database
        _context = InMemoryDbContextFactory.Create();
        _repository = new CarBrandRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSeededBrands()
    {
        // Act - Use large PageSize to get all brands
        var result = await _repository.GetAllAsync(new CarBrandQuery { PageSize = 100 });

        // Assert
        result.Items.Should().NotBeNullOrEmpty();
        result.Items.Should().HaveCountGreaterThanOrEqualTo(56);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnBrandsOrderedByName()
    {
        // Act - Use large PageSize to get all brands
        var result = (await _repository.GetAllAsync(new CarBrandQuery { PageSize = 100 })).Items.ToList();

        // Assert - Database uses culture-aware sorting
        for (int i = 1; i < result.Count; i++)
        {
            var comparison = string.Compare(result[i - 1].Name, result[i].Name, StringComparison.CurrentCulture);
            comparison.Should().BeLessThanOrEqualTo(0,
                $"because {result[i - 1].Name} should come before or equal to {result[i].Name}");
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldContainExpectedBrands()
    {
        // Act - Use large PageSize to get all brands
        var result = (await _repository.GetAllAsync(new CarBrandQuery { PageSize = 100 })).Items.ToList();

        // Assert
        var names = result.Select(b => b.Name);
        names.Should().Contain(new[] { "Toyota", "Ford", "BMW", "Ferrari", "Hyundai" });
    }

    [Fact]
    public async Task GetAllAsync_WithNameFilter_ShouldReturnMatches()
    {
        // Act
        var result = await _repository.GetAllAsync(new CarBrandQuery { Name = "toy" });

        // Assert
        result.Items.Should().ContainSingle(b => b.Name == "Toyota");
    }

    [Fact]
    public async Task GetAllAsync_WithPaging_ShouldReturnSubset()
    {
        // Act
        var result = await _repository.GetAllAsync(new CarBrandQuery { Page = 1, PageSize = 2 });

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(56);
    }

    [Theory]
    [InlineData("Toyota")]
    [InlineData("Ford")]
    [InlineData("BMW")]
    public async Task GetByIdAsync_WithValidId_ShouldReturnCorrectBrand(string brandName)
    {
        // Arrange - Find the brand by name first to get its ID (use large PageSize)
        var allBrands = (await _repository.GetAllAsync(new CarBrandQuery { PageSize = 100 })).Items.ToList();
        var expectedBrand = allBrands.FirstOrDefault(b => b.Name == brandName);
        expectedBrand.Should().NotBeNull($"{brandName} should exist in seeded data");

        // Act
        var result = await _repository.GetByIdAsync(expectedBrand!.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedBrand.Id);
        result.Name.Should().Be(brandName);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_EachBrand_ShouldHaveRequiredProperties()
    {
        // Act
        var result = (await _repository.GetAllAsync(new CarBrandQuery())).Items.ToList();

        // Assert
        result.Should().AllSatisfy(brand =>
        {
            brand.Id.Should().BeGreaterThan(0);
            brand.Name.Should().NotBeNullOrWhiteSpace();
            brand.CountryOfOrigin.Should().NotBeNullOrWhiteSpace();
            brand.FoundedYear.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task AddAsync_ShouldPersistNewBrand()
    {
        // Arrange - Use a unique brand name not in the seeder
        var brand = new CarBrand
        {
            Name = "Pagani",
            CountryOfOrigin = "Italy",
            FoundedYear = 1992,
            Website = "https://www.pagani.com",
            IsLuxury = true,
            Headquarters = "San Cesario sul Panaro"
        };

        // Act
        var created = await _repository.AddAsync(brand);

        // Assert
        created.Id.Should().BeGreaterThan(0);
        var exists = await _repository.ExistsByIdAsync(created.Id);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingBrand()
    {
        // Arrange
        var existing = (await _repository.GetAllAsync(new CarBrandQuery())).Items.First();
        existing.Name = "Updated Brand";

        // Act
        var updated = await _repository.UpdateAsync(existing);

        // Assert
        updated.Name.Should().Be("Updated Brand");
        var reloaded = await _repository.GetByIdAsync(updated.Id);
        reloaded!.Name.Should().Be("Updated Brand");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveBrand()
    {
        // Arrange
        var existing = (await _repository.GetAllAsync(new CarBrandQuery())).Items.First();

        // Act
        var deleted = await _repository.DeleteAsync(existing.Id);

        // Assert
        deleted.Should().BeTrue();
        var shouldBeNull = await _repository.GetByIdAsync(existing.Id);
        shouldBeNull.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkBrandAsInactive()
    {
        // Arrange
        var existing = (await _repository.GetAllAsync(new CarBrandQuery())).Items.First();

        // Act
        var deleted = await _repository.DeleteAsync(existing.Id);

        // Assert
        deleted.Should().BeTrue();
        var inactive = await _repository.GetByIdAsync(existing.Id, includeInactive: true);
        inactive.Should().NotBeNull();
        inactive!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var deleted = await _repository.DeleteAsync(9999);

        // Assert
        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_ForExistingName()
    {
        // Act
        var exists = await _repository.ExistsByNameAsync("Toyota");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenExcludedIdMatches()
    {
        // Arrange - Use PageSize to ensure we get Toyota in results
        var toyota = (await _repository.GetAllAsync(new CarBrandQuery { PageSize = 100 })).Items.First(b => b.Name == "Toyota");

        // Act
        var exists = await _repository.ExistsByNameAsync("Toyota", toyota.Id);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithSortByName_Ascending_ShouldReturnOrderedResults()
    {
        // Act
        var result = await _repository.GetAllAsync(new CarBrandQuery
        {
            SortBy = "name",
            SortDirection = "asc",
            PageSize = 100
        });

        // Assert - Verify culture-aware ascending order
        var items = result.Items.ToList();
        for (int i = 1; i < items.Count; i++)
        {
            var comparison = string.Compare(items[i - 1].Name, items[i].Name, StringComparison.CurrentCulture);
            comparison.Should().BeLessThanOrEqualTo(0,
                $"because {items[i - 1].Name} should come before or equal to {items[i].Name}");
        }
    }

    [Fact]
    public async Task GetAllAsync_WithSortByName_Descending_ShouldReturnOrderedResults()
    {
        // Act
        var result = await _repository.GetAllAsync(new CarBrandQuery
        {
            SortBy = "name",
            SortDirection = "desc",
            PageSize = 100
        });

        // Assert - Verify culture-aware descending order
        var items = result.Items.ToList();
        for (int i = 1; i < items.Count; i++)
        {
            var comparison = string.Compare(items[i - 1].Name, items[i].Name, StringComparison.CurrentCulture);
            comparison.Should().BeGreaterThanOrEqualTo(0,
                $"because {items[i - 1].Name} should come after or equal to {items[i].Name}");
        }
    }

    [Fact]
    public async Task GetAllAsync_WithSortByCountry_Ascending_ShouldReturnOrderedResults()
    {
        // Act
        var result = await _repository.GetAllAsync(new CarBrandQuery
        {
            SortBy = "country",
            SortDirection = "asc"
        });

        // Assert
        result.Items.Should().BeInAscendingOrder(b => b.CountryOfOrigin);
    }

    [Fact]
    public async Task GetAllAsync_WithSortByFoundedYear_Descending_ShouldReturnOrderedResults()
    {
        // Act
        var result = await _repository.GetAllAsync(new CarBrandQuery
        {
            SortBy = "foundedYear",
            SortDirection = "desc"
        });

        // Assert
        result.Items.Should().BeInDescendingOrder(b => b.FoundedYear);
    }

    [Fact]
    public async Task GetAllAsync_WithSortByIsLuxury_Ascending_ShouldReturnOrderedResults()
    {
        // Act
        var result = await _repository.GetAllAsync(new CarBrandQuery
        {
            SortBy = "isLuxury",
            SortDirection = "asc"
        });

        // Assert
        result.Items.Should().BeInAscendingOrder(b => b.IsLuxury);
    }

    [Fact]
    public async Task GetAllAsync_WithDefaultSort_ShouldSortByNameAscending()
    {
        // Act (using defaults: sortBy="name", sortDirection="asc", but larger PageSize)
        var result = await _repository.GetAllAsync(new CarBrandQuery { PageSize = 100 });

        // Assert - Verify culture-aware ascending order
        var items = result.Items.ToList();
        for (int i = 1; i < items.Count; i++)
        {
            var comparison = string.Compare(items[i - 1].Name, items[i].Name, StringComparison.CurrentCulture);
            comparison.Should().BeLessThanOrEqualTo(0,
                $"because {items[i - 1].Name} should come before or equal to {items[i].Name}");
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
