namespace VehicleBrands.API.DTOs;

/// <summary>
/// Data Transfer Object for exposing car brand data through the API.
/// Decouples the domain entity from the HTTP response representation,
/// protecting the domain layer from API-level changes.
/// </summary>
public record CarBrandDto
{
    /// <summary>
    /// Unique identifier of the car brand.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Name of the car brand.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Country where the brand was founded.
    /// </summary>
    public string CountryOfOrigin { get; init; } = string.Empty;

    /// <summary>
    /// Year the brand was founded.
    /// </summary>
    public int FoundedYear { get; init; }

    /// <summary>
    /// Official website URL of the brand.
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Indicates whether the brand is classified as luxury.
    /// </summary>
    public bool IsLuxury { get; init; }

    /// <summary>
    /// City or region where the brand's headquarters is located.
    /// </summary>
    public string? Headquarters { get; init; }

    /// <summary>
    /// Indicates whether the brand is active.
    /// </summary>
    public bool IsActive { get; init; }
}
