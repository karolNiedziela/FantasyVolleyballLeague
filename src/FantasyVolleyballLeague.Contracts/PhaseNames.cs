namespace FantasyVolleyballLeague.Contracts
{
    /// <summary>
    /// The phase names every source writes to the raw JSON (a source maps its own wording to one of them),
    /// matched by the Importer and seeded in the database. A new kind of phase gets a new value here.
    /// Changing a value breaks the data already scraped and stored.
    /// </summary>
    public static class PhaseNames
    {
        public const string RegularSeason = "Regular Season";

        public const string Playoff = "Playoff";

        public const string Relegation = "Relegation";
    }
}
