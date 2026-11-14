namespace FantasyVolleyballLeague.DataApi.Common.API.Paging
{
    internal sealed class PagedResponse<T>
    {
        public PagedResponse(IReadOnlyList<T> items, int offset, int limit, int totalCount, PageLinks? links = null)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
            if (items.Count > limit)
            {
                throw new ArgumentOutOfRangeException(nameof(items), items.Count, $"A page cannot have more items than its limit ({limit}).");
            }

            Items = items;
            Offset = offset;
            Limit = limit;
            TotalCount = totalCount;
            Links = links ?? PageLinks.Empty;
        }

        public IReadOnlyList<T> Items { get; }

        public int Offset { get; }

        public int Limit { get; }

        public int TotalCount { get; }

        public PageLinks Links { get; }

        public PagedResponse<T> WithLinks(PageLinks links)
        {
            return new PagedResponse<T>(Items, Offset, Limit, TotalCount, links);
        }
    }
}
