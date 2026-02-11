namespace VehicleBrands.API.DTOs;

/// <summary>
/// Standard API response envelope.
/// Wraps all responses in a consistent structure with metadata,
/// making the API predictable and easier to consume for clients.
/// </summary>
/// <typeparam name="T">Type of the data payload.</typeparam>
public record ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool Success { get; init; } = true;

    /// <summary>
    /// The response data payload.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Optional message providing additional context.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Optional validation or error details keyed by field name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    /// <summary>
    /// UTC timestamp of the response.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a success response with data.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    /// <summary>
    /// Creates a not-found response.
    /// </summary>
    public static ApiResponse<T> NotFound(string message) => new()
    {
        Success = false,
        Data = default,
        Message = message
    };

    /// <summary>
    /// Creates a validation error response.
    /// </summary>
    public static ApiResponse<T> ValidationError(string message, IReadOnlyDictionary<string, string[]> errors) => new()
    {
        Success = false,
        Data = default,
        Message = message,
        Errors = errors
    };

    /// <summary>
    /// Creates a conflict response.
    /// </summary>
    public static ApiResponse<T> Conflict(string message) => new()
    {
        Success = false,
        Data = default,
        Message = message
    };
}

/// <summary>
/// Specialized response for paginated/list endpoints.
/// Adds a count field so clients know how many items were returned.
/// </summary>
/// <typeparam name="T">Type of each item in the collection.</typeparam>
public record ApiListResponse<T> : ApiResponse<IEnumerable<T>>
{
    /// <summary>
    /// Number of items in the response.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Total items before paging.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Creates a success list response with automatic count.
    /// </summary>
    public static new ApiListResponse<T> Ok(IEnumerable<T> data, string? message = null)
    {
        var list = data.ToList();
        return new ApiListResponse<T>
        {
            Success = true,
            Data = list,
            Count = list.Count,
            Message = message
        };
    }

    /// <summary>
    /// Creates a success list response with paging metadata.
    /// </summary>
    public static ApiListResponse<T> OkPaged(IEnumerable<T> data, int totalCount, int page, int pageSize, string? message = null)
    {
        var list = data.ToList();
        return new ApiListResponse<T>
        {
            Success = true,
            Data = list,
            Count = list.Count,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Message = message
        };
    }
}
