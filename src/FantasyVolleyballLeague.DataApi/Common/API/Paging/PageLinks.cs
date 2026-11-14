using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Globalization;

namespace FantasyVolleyballLeague.DataApi.Common.API.Paging
{
    internal sealed record PageLinks(string Self, string First, string? Prev, string? Next, string Last)
    {
        public static PageLinks Empty { get; } = new(string.Empty, string.Empty, null, null, string.Empty);

        public static PageLinks Create(string url, int offset, int limit, int totalCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

            var queryStart = url.IndexOf('?', StringComparison.Ordinal);
            var urlWithoutQuery = queryStart < 0 ? url : url[..queryStart];
            var query = queryStart < 0 ? string.Empty : url[queryStart..];

            var otherParameters = QueryHelpers.ParseQuery(query)
                .Where(parameter => !IsPagingParameter(parameter.Key))
                .ToList();

            var lastOffset = totalCount > 0 ? (totalCount - 1) / limit * limit : 0;

            string? previous = null;
            if (offset > 0)
            {
                var previousOffset = Math.Min(Math.Max(offset - limit, 0), lastOffset);
                previous = CreateLink(urlWithoutQuery, otherParameters, previousOffset, limit);
            }

            string? next = null;
            if ((long)offset + limit < totalCount)
            {
                next = CreateLink(urlWithoutQuery, otherParameters, offset + limit, limit);
            }

            return new PageLinks(
                Self: CreateLink(urlWithoutQuery, otherParameters, offset, limit),
                First: CreateLink(urlWithoutQuery, otherParameters, 0, limit),
                Prev: previous,
                Next: next,
                Last: CreateLink(urlWithoutQuery, otherParameters, lastOffset, limit));
        }

        private static bool IsPagingParameter(string name)
        {
            return name.Equals(PagingQueryParameters.Offset, StringComparison.OrdinalIgnoreCase)
                || name.Equals(PagingQueryParameters.Limit, StringComparison.OrdinalIgnoreCase);
        }

        private static string CreateLink(
            string urlWithoutQuery,
            List<KeyValuePair<string, StringValues>> otherParameters,
            int offset,
            int limit)
        {
            var parameters = new List<KeyValuePair<string, StringValues>>(otherParameters)
            {
                new(PagingQueryParameters.Offset, offset.ToString(CultureInfo.InvariantCulture)),
                new(PagingQueryParameters.Limit, limit.ToString(CultureInfo.InvariantCulture)),
            };

            return QueryHelpers.AddQueryString(urlWithoutQuery, parameters);
        }
    }
}
