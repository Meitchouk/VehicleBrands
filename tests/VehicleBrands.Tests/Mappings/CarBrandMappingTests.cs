using FluentAssertions;
using VehicleBrands.API.DTOs;
using VehicleBrands.API.Mappings;
using VehicleBrands.Domain.Entities;

namespace VehicleBrands.Tests.Mappings;

/// <summary>
/// Unit tests for car brand mapping extensions.
/// Verifies that the transformation between entities and DTOs is correct.
/// </summary>
public class CarBrandMappingTests
{
    [Fact]
    public void ToDto_ShouldMapAllProperties()
    {
        // Arrange
        var entity = new CarBrand
        {
            Id = 1,
            Name = "Toyota",
            CountryOfOrigin = "Japan",
            FoundedYear = 1937,
            Website = "https://www.toyota.com",
            IsLuxury = false,
            Headquarters = "Toyota City, Aichi",
            IsActive = false
        };

        // Act
        var dto = entity.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entity.Id);
        dto.Name.Should().Be(entity.Name);
        dto.CountryOfOrigin.Should().Be(entity.CountryOfOrigin);
        dto.FoundedYear.Should().Be(entity.FoundedYear);
        dto.Website.Should().Be(entity.Website);
        dto.IsLuxury.Should().Be(entity.IsLuxury);
        dto.Headquarters.Should().Be(entity.Headquarters);
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ToDtoList_ShouldMapAllEntities()
    {
        // Arrange
        var entities = new List<CarBrand>
        {
            new() { Id = 1, Name = "Toyota", CountryOfOrigin = "Japan", FoundedYear = 1937, Website = "https://www.toyota.com", IsLuxury = false },
            new() { Id = 2, Name = "Ford", CountryOfOrigin = "United States", FoundedYear = 1903, Website = "https://www.ford.com", IsLuxury = false }
        };

        // Act
        var dtos = entities.ToDtoList().ToList();

        // Assert
        dtos.Should().HaveCount(2);
        dtos[0].Name.Should().Be("Toyota");
        dtos[1].Name.Should().Be("Ford");
    }

    [Fact]
    public void ToDtoList_WithEmptyCollection_ShouldReturnEmpty()
    {
        // Arrange
        var entities = Enumerable.Empty<CarBrand>();

        // Act
        var dtos = entities.ToDtoList();

        // Assert
        dtos.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ShouldReturnCorrectType()
    {
        // Arrange
        var entity = new CarBrand { Id = 1, Name = "Test", CountryOfOrigin = "Test", FoundedYear = 2000 };

        // Act
        var dto = entity.ToDto();

        // Assert
        dto.Should().BeOfType<CarBrandDto>();
    }

    [Fact]
    public void CreateRequest_ToEntity_ShouldMapAllFields()
    {
        // Arrange
        var request = new CarBrandCreateRequest
        {
            Name = "Mazda",
            CountryOfOrigin = "Japan",
            FoundedYear = 1920,
            Website = "https://www.mazda.com",
            IsLuxury = false,
            Headquarters = "Hiroshima"
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        entity.Name.Should().Be("Mazda");
        entity.CountryOfOrigin.Should().Be("Japan");
        entity.FoundedYear.Should().Be(1920);
        entity.Website.Should().Be("https://www.mazda.com");
        entity.IsLuxury.Should().BeFalse();
        entity.Headquarters.Should().Be("Hiroshima");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateRequest_ToEntity_ShouldMapAllFieldsAndId()
    {
        // Arrange
        var request = new CarBrandUpdateRequest
        {
            Name = "Mazda",
            CountryOfOrigin = "Japan",
            FoundedYear = 1920,
            Website = "https://www.mazda.com",
            IsLuxury = false,
            Headquarters = "Hiroshima"
        };

        // Act
        var entity = request.ToEntity(10);

        // Assert
        entity.Id.Should().Be(10);
        entity.Name.Should().Be("Mazda");
        entity.CountryOfOrigin.Should().Be("Japan");
        entity.FoundedYear.Should().Be(1920);
        entity.Website.Should().Be("https://www.mazda.com");
        entity.IsLuxury.Should().BeFalse();
        entity.Headquarters.Should().Be("Hiroshima");
    }
}
