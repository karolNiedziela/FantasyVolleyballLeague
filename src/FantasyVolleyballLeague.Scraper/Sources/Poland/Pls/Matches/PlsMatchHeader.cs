using FantasyVolleyballLeague.Scraper.Models;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    internal sealed record PlsMatchHeaderRoster(string Name, int? ExternalId, int? SetsWon);

    /// <summary>
    /// Match data available on every match page regardless of the statistics era.
    /// </summary>
    internal sealed record PlsMatchHeader
    {
        public required MatchStatus Status { get; init; }

        public DateTime? DateUtc { get; init; }

        public bool IsTimeConfirmed { get; init; }

        public required PlsMatchHeaderRoster HomeRoster { get; init; }

        public required PlsMatchHeaderRoster AwayRoster { get; init; }

        public IReadOnlyList<SetScore> Sets { get; init; } = [];

        public string? MatchNumber { get; init; }

        public int? Attendance { get; init; }

        public PlayerReference? Mvp { get; init; }

        public string? FirstReferee { get; init; }

        public string? SecondReferee { get; init; }

        public string? Commissioner { get; init; }

        public string? VenueName { get; init; }

        public string? VenueCity { get; init; }
    }
}
