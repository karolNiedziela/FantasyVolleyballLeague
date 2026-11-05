using System.Globalization;
using System.Text.RegularExpressions;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls
{
    /// <summary>
    /// Extracts stable identifiers used by the PLS websites from their links,
    /// e.g. <c>/players/id/1516/tour/23.html</c> or <c>/teams/id/18/tour/53.html</c>.
    /// </summary>
    internal static partial class PlsExternalIds
    {
        public static int? FromPlayerUrl(string? url)
        {
            return Match(PlayerIdRegex(), url);
        }

        public static int? FromTeamUrl(string? url)
        {
            return Match(TeamIdRegex(), url);
        }

        public static int? FromTourUrl(string? url)
        {
            return Match(TourIdRegex(), url);
        }

        private static int? Match(Regex regex, string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var match = regex.Match(url);
            return match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : null;
        }

        [GeneratedRegex(@"/players/id/(\d+)")]
        private static partial Regex PlayerIdRegex();

        [GeneratedRegex(@"/teams/id/(\d+)")]
        private static partial Regex TeamIdRegex();

        [GeneratedRegex(@"/tour/(\d+)")]
        private static partial Regex TourIdRegex();
    }
}
