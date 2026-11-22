using FantasyVolleyballLeague.Shared.Text;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Common.API.Search
{
    internal static class NameSearchQueryableExtensions
    {
        /// <summary>
        /// Keeps the rows whose normalized name contains every word of <paramref name="query"/>, in any order
        /// ("czyrnianska anna" finds "Anna Rybak-Czyrniańska"). The query goes through the same
        /// <see cref="NameNormalizer"/> as the stored names, so case and diacritics do not matter.
        /// A blank query, or one without letters or digits, filters nothing.
        /// </summary>
        /// <remarks>
        /// One <c>LIKE '%word%'</c> per word, which the trigram index serves (<c>string.Contains</c> would become
        /// <c>strpos</c>, which it cannot). The words hold only letters and digits, so <c>%</c>, <c>_</c> and <c>\</c>
        /// never reach the pattern and need no escaping. A word shorter than 3 characters has no trigrams, so
        /// the index cannot narrow it down.
        /// </remarks>
        public static IQueryable<T> WhereNameMatches<T>(this IQueryable<T> source, string? query)
            where T : class, ISearchableName
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return source;
            }

            var words = NameNormalizer.Tokens(query);
            foreach (var word in words)
            {
                var pattern = $"%{word}%";
                source = source.Where(entity => EF.Functions.Like(entity.NormalizedName, pattern));
            }

            return source;
        }
    }
}
