namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// Players of all rosters of one gender and season, by name. A roster lists the players at the end of the
    /// season, so a player who moved mid-season (or a club from another division, e.g. a play-off opponent) is
    /// missing from the roster of the club in his early matches, but is on another roster of the same season.
    /// </summary>
    internal sealed class SeasonNameIndex
    {
        private readonly Dictionary<(int GenderId, int StartYear, string NameKey), HashSet<Guid>> _players = [];

        public void Add(int genderId, int startYear, string nameKey, Guid playerId)
        {
            var key = (genderId, startYear, nameKey);
            if (!_players.TryGetValue(key, out var players))
            {
                players = [];
                _players[key] = players;
            }

            players.Add(playerId);
        }

        public Guid? Find(int genderId, int startYear, string nameKey)
        {
            return _players.TryGetValue((genderId, startYear, nameKey), out var players) && players.Count == 1
                ? players.First()
                : null;
        }
    }
}
