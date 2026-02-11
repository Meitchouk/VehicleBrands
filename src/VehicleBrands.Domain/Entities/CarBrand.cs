namespace VehicleBrands.Domain.Entities;

/// <summary>
/// Represents a car brand entity in the domain layer.
/// Follows the Single Responsibility Principle (SRP) by containing only domain data.
/// </summary>
public class CarBrand
{
    /// <summary>
    /// Unique identifier for the car brand.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the car brand.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Country where the brand was founded.
    /// </summary>
    public string CountryOfOrigin { get; set; } = string.Empty;

    /// <summary>
    /// Year the brand was founded.
    /// </summary>
    public int FoundedYear { get; set; }

    /// <summary>
    /// Official website URL of the brand.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Indicates whether the brand is classified as luxury.
    /// </summary>
    public bool IsLuxury { get; set; }

    /// <summary>
    /// City or region where the brand's headquarters is located.
    /// </summary>
    public string? Headquarters { get; set; }

    /// <summary>
    /// Indicates whether the brand is active (soft delete flag).
    /// </summary>
    public bool IsActive { get; set; } = true;
}
