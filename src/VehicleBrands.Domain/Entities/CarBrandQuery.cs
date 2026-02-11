namespace VehicleBrands.Domain.Entities;

/// <summary>
/// Query parameters for searching and paging car brands.
/// </summary>
public class CarBrandQuery
{
    public string? Name { get; init; }
    public string? CountryOfOrigin { get; init; }
    public bool? IsLuxury { get; init; }
    public bool IncludeInactive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Column to sort by. Valid values: "name", "country", "foundedYear", "isLuxury", "headquarters".
    /// Default: "name".
    /// </summary>
    public string SortBy { get; init; } = "name";

    /// <summary>
    /// Sort direction. Valid values: "asc" or "desc".
    /// Default: "asc".
    /// </summary>
    public string SortDirection { get; init; } = "asc";
}
