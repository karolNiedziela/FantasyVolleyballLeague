using FantasyVolleyballLeague.Scraper.Models;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    internal sealed record PlsMatchPlayers(
        StatisticsLevel Level,
        IReadOnlyList<PlayerMatchStatistics> Home,
        IReadOnlyList<PlayerMatchStatistics> Away)
    {
        public static readonly PlsMatchPlayers Empty = new(StatisticsLevel.None, [], []);
    }
}
