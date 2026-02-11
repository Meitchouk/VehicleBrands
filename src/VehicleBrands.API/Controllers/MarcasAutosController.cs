using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using VehicleBrands.API.DTOs;
using VehicleBrands.API.Mappings;
using VehicleBrands.Domain.Interfaces;
using VehicleBrands.Domain.Entities;

namespace VehicleBrands.API.Controllers;

/// <summary>
/// REST controller for managing car brands.
/// Named "MarcasAutosController" as required by the specification.
/// Follows the Single Responsibility Principle: only handles HTTP requests
/// and delegates business logic to the injected repository.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class MarcasAutosController : ControllerBase
{
    private readonly ICarBrandRepository _repository;
    private readonly ILogger<MarcasAutosController> _logger;

    public MarcasAutosController(
        ICarBrandRepository repository,
        ILogger<MarcasAutosController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all registered car brands.
    /// </summary>
    /// <param name="name">Optional brand name filter.</param>
    /// <param name="country">Optional country filter.</param>
    /// <param name="isLuxury">Optional luxury filter.</param>
    /// <param name="includeInactive">Include inactive brands.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="sortBy">Column to sort by. Valid: name, country, foundedYear, isLuxury, headquarters. Default: name.</param>
    /// <param name="sortDirection">Sort direction: asc or desc. Default: asc.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of car brands.</returns>
    /// <response code="200">Returns the list of car brands.</response>
    /// <response code="400">Invalid paging or sorting parameters.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiListResponse<CarBrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiListResponse<CarBrandDto>>> GetAll(
        [FromQuery] string? name = null,
        [FromQuery] string? country = null,
        [FromQuery] bool? isLuxury = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        // Validate paging parameters
        if (page < 1 || pageSize < 1 || pageSize > 50)
        {
            return BadRequest(ApiResponse<CarBrandDto>.ValidationError(
                "Invalid paging parameters.",
                new Dictionary<string, string[]>
                {
                    { "page", new[] { "Page must be greater than or equal to 1." } },
                    { "pageSize", new[] { "PageSize must be between 1 and 50." } }
                }));
        }

        // Validate sorting parameters
        var validSortColumns = new[] { "name", "country", "foundedyear", "isluxury", "headquarters" };
        var validSortDirections = new[] { "asc", "desc" };

        if (!validSortColumns.Contains(sortBy.ToLowerInvariant()))
        {
            return BadRequest(ApiResponse<CarBrandDto>.ValidationError(
                "Invalid sorting parameter.",
                new Dictionary<string, string[]>
                {
                    { "sortBy", new[] { $"SortBy must be one of: {string.Join(", ", validSortColumns)}." } }
                }));
        }

        if (!validSortDirections.Contains(sortDirection.ToLowerInvariant()))
        {
            return BadRequest(ApiResponse<CarBrandDto>.ValidationError(
                "Invalid sorting parameter.",
                new Dictionary<string, string[]>
                {
                    { "sortDirection", new[] { "SortDirection must be 'asc' or 'desc'." } }
                }));
        }

        _logger.LogInformation("Retrieving all car brands");

        var query = new CarBrandQuery
        {
            Name = name,
            CountryOfOrigin = country,
            IsLuxury = isLuxury,
            IncludeInactive = includeInactive,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var (brands, totalCount) = await _repository.GetAllAsync(query, cancellationToken);
        var brandsDto = brands.ToDtoList();

        _logger.LogInformation("Retrieved {Count} car brands", brandsDto.Count());

        return Ok(ApiListResponse<CarBrandDto>.OkPaged(brandsDto, totalCount, page, pageSize));
    }

    /// <summary>
    /// Retrieves a car brand by its identifier.
    /// </summary>
    /// <param name="id">The brand identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested car brand.</returns>
    /// <response code="200">Returns the requested car brand.</response>
    /// <response code="404">Car brand with the specified ID was not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CarBrandDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching for car brand with ID: {Id}", id);

        var brand = await _repository.GetByIdAsync(id, includeInactive: false, cancellationToken);

        if (brand is null)
        {
            _logger.LogWarning("Car brand with ID {Id} not found", id);
            return NotFound(ApiResponse<CarBrandDto>.NotFound(
                $"Car brand with ID {id} was not found."));
        }

        return Ok(ApiResponse<CarBrandDto>.Ok(brand.ToDto()));
    }

    /// <summary>
    /// Creates a new car brand.
    /// </summary>
    /// <param name="request">Brand data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created car brand.</returns>
    /// <response code="201">Returns the created car brand.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="409">Brand name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CarBrandDto>>> Create(
        [FromBody] CarBrandCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<CarBrandDto>.ValidationError(
                "Validation failed.", GetModelErrors(ModelState)));
        }

        if (await _repository.ExistsByNameAsync(request.Name, null, includeInactive: true, cancellationToken))
        {
            return Conflict(ApiResponse<CarBrandDto>.Conflict(
                $"Car brand with name '{request.Name}' already exists."));
        }

        var entity = request.ToEntity();
        var created = await _repository.AddAsync(entity, cancellationToken);

        var apiVersion = ControllerContext?.RouteData?.Values["version"]?.ToString() ?? "1.0";
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id, version = apiVersion },
            ApiResponse<CarBrandDto>.Ok(created.ToDto()));
    }

    /// <summary>
    /// Updates an existing car brand.
    /// </summary>
    /// <param name="id">Brand identifier.</param>
    /// <param name="request">Updated brand data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated car brand.</returns>
    /// <response code="200">Returns the updated car brand.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Brand not found.</response>
    /// <response code="409">Brand name already exists.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CarBrandDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CarBrandDto>>> Update(
        int id,
        [FromBody] CarBrandUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<CarBrandDto>.ValidationError(
                "Validation failed.", GetModelErrors(ModelState)));
        }

        if (!await _repository.ExistsByIdAsync(id, includeInactive: false, cancellationToken))
        {
            return NotFound(ApiResponse<CarBrandDto>.NotFound(
                $"Car brand with ID {id} was not found."));
        }

        if (await _repository.ExistsByNameAsync(request.Name, id, includeInactive: true, cancellationToken))
        {
            return Conflict(ApiResponse<CarBrandDto>.Conflict(
                $"Car brand with name '{request.Name}' already exists."));
        }

        var entity = request.ToEntity(id);
        var updated = await _repository.UpdateAsync(entity, cancellationToken);

        return Ok(ApiResponse<CarBrandDto>.Ok(updated.ToDto()));
    }

    /// <summary>
    /// Deletes a car brand by its identifier.
    /// </summary>
    /// <param name="id">Brand identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion confirmation.</returns>
    /// <response code="200">Deleted successfully.</response>
    /// <response code="404">Brand not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponse<string>.NotFound(
                $"Car brand with ID {id} was not found."));
        }

        return Ok(ApiResponse<string>.Ok("Deleted"));
    }

    private static IReadOnlyDictionary<string, string[]> GetModelErrors(ModelStateDictionary modelState)
    {
        return modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Invalid value."
                        : error.ErrorMessage)
                    .ToArray());
    }
}
