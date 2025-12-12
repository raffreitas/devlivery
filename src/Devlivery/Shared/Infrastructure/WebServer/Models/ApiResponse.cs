using System.Text.Json.Serialization;

namespace Devlivery.Shared.Infrastructure.WebServer.Models;

/// <summary>
/// Standardized API response wrapper for successful operations
/// </summary>
/// <typeparam name="T">Type of the data being returned</typeparam>
public sealed record ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The actual data returned by the operation
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }

    private ApiResponse(bool success, T? data)
    {
        IsSuccess = success;
        Data = data;
    }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> Success(T data)
    {
        return new(true, data);
    }

    /// <summary>
    /// Creates a failed response without data
    /// </summary>
    public static ApiResponse<T> Failure()
        => new(false, default);
}

/// <summary>
/// Standardized API response for operations without return data
/// </summary>
public sealed record ApiResponse
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool IsSuccess { get; init; }

    private ApiResponse(bool success)
    {
        IsSuccess = success;
    }

    /// <summary>
    /// Creates a successful response
    /// </summary>
    /// 
    public static ApiResponse Success()
        => new(true);
}