using VehicleBrands.API.DTOs;
using VehicleBrands.Domain.Entities;

namespace VehicleBrands.API.Mappings;

/// <summary>
/// Extension methods for mapping between domain entities and DTOs.
/// Centralizes transformation logic following the Single Responsibility Principle (SRP).
/// </summary>
public static class CarBrandMappingExtensions
{
    /// <summary>
    /// Converts a CarBrand entity to its corresponding DTO.
    /// </summary>
    public static CarBrandDto ToDto(this CarBrand entity)
    {
        return new CarBrandDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CountryOfOrigin = entity.CountryOfOrigin,
            FoundedYear = entity.FoundedYear,
            Website = entity.Website,
            IsLuxury = entity.IsLuxury,
            Headquarters = entity.Headquarters,
            IsActive = entity.IsActive
        };
    }

    /// <summary>
    /// Converts a collection of CarBrand entities to a collection of DTOs.
    /// </summary>
    public static IEnumerable<CarBrandDto> ToDtoList(this IEnumerable<CarBrand> entities)
    {
        return entities.Select(e => e.ToDto());
    }

    /// <summary>
    /// Converts a create request into a CarBrand entity.
    /// </summary>
    public static CarBrand ToEntity(this CarBrandCreateRequest request)
    {
        return new CarBrand
        {
            Name = request.Name.Trim(),
            CountryOfOrigin = request.CountryOfOrigin.Trim(),
            FoundedYear = request.FoundedYear,
            Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
            IsLuxury = request.IsLuxury,
            Headquarters = string.IsNullOrWhiteSpace(request.Headquarters) ? null : request.Headquarters.Trim(),
            IsActive = true
        };
    }

    /// <summary>
    /// Converts an update request into a CarBrand entity with a known ID.
    /// </summary>
    public static CarBrand ToEntity(this CarBrandUpdateRequest request, int id)
    {
        return new CarBrand
        {
            Id = id,
            Name = request.Name.Trim(),
            CountryOfOrigin = request.CountryOfOrigin.Trim(),
            FoundedYear = request.FoundedYear,
            Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
            IsLuxury = request.IsLuxury,
            Headquarters = string.IsNullOrWhiteSpace(request.Headquarters) ? null : request.Headquarters.Trim()
        };
    }
}
