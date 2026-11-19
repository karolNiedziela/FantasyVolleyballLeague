namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    /// <summary>
    /// How detailed the player statistics of a match are. Archive seasons (2000/01–2007/08) only list lineups.
    /// </summary>
    internal sealed class StatisticsLevel
    {
        public static readonly StatisticsLevel None = new(1, "none", "None");
        public static readonly StatisticsLevel LineupOnly = new(2, "lineup_only", "LineupOnly");
        public static readonly StatisticsLevel Full = new(3, "full", "Full");

        public int Id { get; private set; }

        /// <summary>
        /// The value the API uses for this level. Part of the public contract, so it never changes.
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;

        private StatisticsLevel() { }

        private StatisticsLevel(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public static IReadOnlyCollection<StatisticsLevel> All => [None, LineupOnly, Full];

        public static StatisticsLevel? FromName(string name)
        {
            return All.FirstOrDefault(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static StatisticsLevel? FromCode(string code)
        {
            return All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.Ordinal));
        }
    }
}
