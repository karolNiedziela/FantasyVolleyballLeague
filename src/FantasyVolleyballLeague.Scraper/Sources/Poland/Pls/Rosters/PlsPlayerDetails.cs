namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters
{
    internal sealed record PlsPlayerDetails(
        DateOnly? DateOfBirth,
        string? Position,
        int? Height,
        int? Weight,
        int? AttackReach,
        int? ShirtNumber);
}
