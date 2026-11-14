using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Common.API.Paging
{
    internal static class PagedQueryableExtensions
    {
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(this IQueryable<T> query, int offset, int limit, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

            var items = await query
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var isOnlyPage = offset == 0 && items.Count < limit;
            var totalCount = isOnlyPage ? items.Count : await query.CountAsync(cancellationToken);

            return new PagedResponse<T>(items, offset, limit, totalCount);
        }
    }
}
