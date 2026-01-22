namespace Devlivery.Common.Pagination;

public record PaginatedResult<T>(T[] Items, int TotalCount, int PageNumber, int PageSize);