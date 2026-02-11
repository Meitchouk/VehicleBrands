using System.ComponentModel.DataAnnotations;

namespace VehicleBrands.API.DTOs;

/// <summary>
/// Request model for updating an existing car brand.
/// Uses full update semantics (PUT).
/// </summary>
public record CarBrandUpdateRequest
{
    /// <summary>
    /// Name of the car brand.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Country where the brand was founded.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CountryOfOrigin { get; init; } = string.Empty;

    /// <summary>
    /// Year the brand was founded.
    /// </summary>
    [Range(1800, 2100)]
    public int FoundedYear { get; init; }

    /// <summary>
    /// Official website URL of the brand.
    /// </summary>
    [Url]
    [StringLength(200)]
    public string? Website { get; init; }

    /// <summary>
    /// Indicates whether the brand is classified as luxury.
    /// </summary>
    public bool IsLuxury { get; init; }

    /// <summary>
    /// City or region where the brand's headquarters is located.
    /// </summary>
    [StringLength(150)]
    public string? Headquarters { get; init; }
}
