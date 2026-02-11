using VehicleBrands.Domain.Entities;

namespace VehicleBrands.Domain.Interfaces;

/// <summary>
/// Repository interface for the CarBrand entity.
/// Applies the Dependency Inversion Principle (DIP) and the Repository pattern.
/// The domain layer defines the contract; the infrastructure layer implements it.
/// </summary>
public interface ICarBrandRepository
{
    /// <summary>
    /// Retrieves all registered car brands.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A collection of car brands.</returns>
    Task<(IEnumerable<CarBrand> Items, int TotalCount)> GetAllAsync(
        CarBrandQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a car brand by its identifier.
    /// </summary>
    /// <param name="id">The brand identifier.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The car brand if found; otherwise, null.</returns>
    Task<CarBrand?> GetByIdAsync(int id, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new car brand to the data store.
    /// </summary>
    Task<CarBrand> AddAsync(CarBrand brand, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing car brand in the data store.
    /// </summary>
    Task<CarBrand> UpdateAsync(CarBrand brand, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a car brand by its identifier.
    /// Returns true if deleted; false if it does not exist.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a car brand exists by its identifier.
    /// </summary>
    Task<bool> ExistsByIdAsync(int id, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a car brand exists by name. Optionally excludes a specific ID.
    /// </summary>
    Task<bool> ExistsByNameAsync(
        string name,
        int? excludeId = null,
        bool includeInactive = true,
        CancellationToken cancellationToken = default);
}
