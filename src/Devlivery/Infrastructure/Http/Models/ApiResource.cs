using System.Text.Json.Serialization;

using Devlivery.Common.Pagination;

using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Infrastructure.Http.Models;

public sealed record Metadata
{
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext => CurrentPage < TotalPages;
    public bool HasPrevious => CurrentPage > 1;

    public static Metadata FromPaginationResult<T>(PaginatedResult<T> result) => new()
    {
        TotalCount = result.TotalCount,
        PageSize = result.PageSize,
        CurrentPage = result.PageNumber,
        TotalPages = (int)Math.Ceiling(result.TotalCount / (double)result.PageSize)
    };
}

public sealed record ApiResource<T>(
    [property: JsonPropertyName("data")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    T Data,
    [property: JsonPropertyName("meta")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Metadata? Metadata = null);

public sealed class ApiProblemDetails : ProblemDetails
{
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Dictionary<string, string>? Errors { get; init; }
}