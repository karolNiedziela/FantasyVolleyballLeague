namespace FantasyVolleyballLeague.Scraper.Sources
{
    /// <summary>
    /// Picks the <see cref="ILeagueSource"/> of a league by <see cref="LeagueOptions.Source"/>, from all registered sources.
    /// </summary>
    internal sealed class LeagueSourceResolver
    {
        private readonly Dictionary<string, ILeagueSource> _sources;

        public LeagueSourceResolver(IEnumerable<ILeagueSource> sources)
        {
            _sources = sources.ToDictionary(source => source.Key, StringComparer.OrdinalIgnoreCase);
        }

        public ILeagueSource Resolve(LeagueOptions league)
        {
            if (string.IsNullOrWhiteSpace(league.Source))
            {
                throw new InvalidOperationException($"League '{league.Key}' has no source configured (Leagues:Source).");
            }

            return _sources.TryGetValue(league.Source, out var source)
                ? source
                : throw new InvalidOperationException(
                    $"League '{league.Key}' uses unknown source '{league.Source}'. Known sources: {string.Join(", ", _sources.Keys)}.");
        }
    }
}
