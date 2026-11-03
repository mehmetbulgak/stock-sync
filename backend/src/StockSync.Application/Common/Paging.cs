using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace StockSync.Application.Common;

public static class Paging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MaxPage = 100_000;
    public const int SearchMaxLength = 100;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize) =>
        (Math.Clamp(page ?? 1, 1, MaxPage), Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize));

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int? page,
        int? pageSize,
        CancellationToken ct)
    {
        var (p, size) = Normalize(page, pageSize);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p - 1) * size).Take(size).ToListAsync(ct);
        return new PagedResult<T>(items, p, size, total);
    }

    public static void ValidPage<T>(this IRuleBuilder<T, int?> rule) =>
        rule.InclusiveBetween(1, MaxPage);

    public static void ValidPageSize<T>(this IRuleBuilder<T, int?> rule) =>
        rule.InclusiveBetween(1, MaxPageSize);

    public static void ValidSearch<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(SearchMaxLength);
}
