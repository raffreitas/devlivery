using Microsoft.AspNetCore.Mvc;

namespace Devlivery.Infrastructure.Http.Models;

public sealed record PaginationParameters(
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "pageSize")] int PageSize = 50)
{
    public const int MaximumPageSize = 100;
}