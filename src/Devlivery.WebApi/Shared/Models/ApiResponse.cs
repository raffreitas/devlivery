namespace Devlivery.WebApi.Shared.Models;

/// <summary>
/// Standardized API response wrapper for successful operations
/// </summary>
/// <typeparam name="T">Type of the data being returned</typeparam>
public sealed record ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The actual data returned by the operation
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Optional message providing additional context
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; init; }

    private ApiResponse(bool success, T? data, string? message)
    {
        Success = success;
        Data = data;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
        => new(true, data, message);

    /// <summary>
    /// Creates a successful response without data
    /// </summary>
    public static ApiResponse<T> Ok(string message)
        => new(true, default, message);
}

/// <summary>
/// Standardized API response for operations without return data
/// </summary>
public sealed record ApiResponse
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Message providing context about the operation
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; init; }

    private ApiResponse(bool success, string message)
    {
        Success = success;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a successful response with a message
    /// </summary>
    public static ApiResponse Ok(string message)
        => new(true, message);
}