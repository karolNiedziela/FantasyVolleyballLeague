namespace FantasyVolleyballLeague.Api.Domain
{
    /// <summary>
    /// How detailed the player statistics of a match are. Archive seasons (2000/01–2007/08) only list lineups.
    /// </summary>
    internal sealed class StatisticsLevel
    {
        public static readonly StatisticsLevel None = new(1, "None");
        public static readonly StatisticsLevel LineupOnly = new(2, "LineupOnly");
        public static readonly StatisticsLevel Full = new(3, "Full");

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        private StatisticsLevel() { }

        private StatisticsLevel(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public static IReadOnlyCollection<StatisticsLevel> All => [None, LineupOnly, Full];

        public static StatisticsLevel? FromName(string name)
        {
            return All.FirstOrDefault(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
