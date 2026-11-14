using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.Importer
{
    internal sealed class LeagueImportOptions
    {
        /// <summary>
        /// League key used by the scraper in data folder names (e.g. "PlusLiga").
        /// </summary>
        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public short DivisionNumber { get; set; }

        public LeagueCountry ResolveCountry()
        {
            return LeagueCountry.All.FirstOrDefault(c => string.Equals(c.Name, Country, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Unknown country '{Country}' for league '{Key}'.");
        }

        public LeagueGender ResolveGender()
        {
            return LeagueGender.All.FirstOrDefault(g => string.Equals(g.Name, Gender, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Unknown gender '{Gender}' for league '{Key}'.");
        }
    }
}
