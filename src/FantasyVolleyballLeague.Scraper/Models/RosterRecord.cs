
namespace FantasyVolleyballLeague.Scraper.Models
{
    /// <summary>
    /// Raw roster file (<c>data/raw/rosters/{League}/{YYYY-YYYY}/{ClubId}.json</c>).
    /// </summary>
    internal sealed record RosterRecord
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; init; } = CurrentSchemaVersion;

        public required DateTime ScrapedAtUtc { get; init; }

        public required string League { get; init; }

        public required SeasonReference Season { get; init; }

        public required ClubLink Club { get; init; }

        public IReadOnlyList<PlayerProfile> Players { get; init; } = [];
    }
}
