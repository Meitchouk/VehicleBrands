using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VehicleBrands.Domain.Entities;
using VehicleBrands.Domain.Interfaces;
using VehicleBrands.Infrastructure.Data;

namespace VehicleBrands.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for the CarBrand entity.
/// Applies the Repository pattern to encapsulate data access logic
/// and decouple the domain layer from Entity Framework.
/// </summary>
public class CarBrandRepository : ICarBrandRepository
{
    private readonly ApplicationDbContext _context;

    public CarBrandRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<CarBrand> Items, int TotalCount)> GetAllAsync(
        CarBrandQuery query,
        CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.CarBrands.AsNoTracking().AsQueryable();

        if (!query.IncludeInactive)
        {
            dbQuery = dbQuery.Where(b => b.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(b => b.Name.ToLower()!.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.CountryOfOrigin))
        {
            var country = query.CountryOfOrigin.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(b => b.CountryOfOrigin.ToLower()!.Contains(country));
        }

        if (query.IsLuxury.HasValue)
        {
            dbQuery = dbQuery.Where(b => b.IsLuxury == query.IsLuxury.Value);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // Apply dynamic sorting
        dbQuery = ApplySorting(dbQuery, query.SortBy, query.SortDirection);

        var items = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<CarBrand?> GetByIdAsync(int id, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.CarBrands.AsNoTracking().AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        return await query.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CarBrand> AddAsync(CarBrand brand, CancellationToken cancellationToken = default)
    {
        _context.CarBrands.Add(brand);
        await _context.SaveChangesAsync(cancellationToken);
        return brand;
    }

    /// <inheritdoc />
    public async Task<CarBrand> UpdateAsync(CarBrand brand, CancellationToken cancellationToken = default)
    {
        var existing = await _context.CarBrands
            .FirstOrDefaultAsync(b => b.Id == brand.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException("Car brand not found.");
        }

        existing.Name = brand.Name;
        existing.CountryOfOrigin = brand.CountryOfOrigin;
        existing.FoundedYear = brand.FoundedYear;
        existing.Website = brand.Website;
        existing.IsLuxury = brand.IsLuxury;
        existing.Headquarters = brand.Headquarters;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.CarBrands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (existing is null || !existing.IsActive)
        {
            return false;
        }

        existing.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByIdAsync(int id, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.CarBrands.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        return await query.AnyAsync(b => b.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(
        string name,
        int? excludeId = null,
        bool includeInactive = true,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();

        var query = _context.CarBrands.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        return await query.AnyAsync(b => b.Name.ToLower() == normalized, cancellationToken);
    }

    /// <summary>
    /// Applies dynamic sorting to the query based on the specified column and direction.
    /// </summary>
    private static IQueryable<CarBrand> ApplySorting(IQueryable<CarBrand> query, string sortBy, string sortDirection)
    {
        var normalizedSortBy = sortBy.ToLowerInvariant();
        var isDescending = sortDirection.ToLowerInvariant() == "desc";

        Expression<Func<CarBrand, object>> keySelector = normalizedSortBy switch
        {
            "country" => b => b.CountryOfOrigin,
            "foundedyear" => b => b.FoundedYear,
            "isluxury" => b => b.IsLuxury,
            "headquarters" => b => b.Headquarters ?? string.Empty,
            _ => b => b.Name // Default to Name
        };

        return isDescending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
}
