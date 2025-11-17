namespace Devlivery.WebApi.Shared.Presentation.Models;

/// <summary>
/// Standardized API response for error scenarios
/// </summary>
public sealed record ErrorResponse(string Message, int StatusCode, object? Errors = null, string? TraceId = null)
{
    /// <summary>
    /// Indicates the operation failed (always false)
    /// </summary>
    public bool Success { get; init; } = false;

    /// <summary>
    /// Error message describing what went wrong
    /// </summary>
    public string Message { get; init; } = Message;

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; init; } = StatusCode;

    /// <summary>
    /// Detailed error information (e.g., validation errors)
    /// </summary>
    public object? Errors { get; init; } = Errors;

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier for tracking this error (optional)
    /// </summary>
    public string? TraceId { get; init; } = TraceId;
}