using System.Text.Json.Serialization;

namespace Devlivery.Infrastructure.WebServer.Models;

/// <summary>
/// Standardized API response wrapper for all operations
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    /// <summary>
    /// Error messages when operation fails
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Errors { get; init; }

    private ApiResponse(bool success, T? data, string[]? errors = null)
    {
        IsSuccess = success;
        Data = data;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> Success(T data)
        => new(true, data);

    /// <summary>
    /// Creates a failed response with error messages
    /// </summary>
    public static ApiResponse<T> Failure(params string[] errors)
        => new(false, default, errors);

    /// <summary>
    /// Creates a failed response with a single error message
    /// </summary>
    public static ApiResponse<T> Failure(string error)
        => new(false, default, [error]);
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

    /// <summary>
    /// Error messages when operation fails
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Errors { get; init; }

    private ApiResponse(bool success, string[]? errors = null)
    {
        IsSuccess = success;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse Success()
        => new(true);

    /// <summary>
    /// Creates a failed response with error messages
    /// </summary>
    public static ApiResponse Failure(params string[] errors)
        => new(false, errors);

    /// <summary>
    /// Creates a failed response with a single error message
    /// </summary>
    public static ApiResponse Failure(string error)
        => new(false, [error]);
}