namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters
{
    internal sealed record PlsRosterEntry(int ExternalId, string FullName, int? ShirtNumber, string? Position, string ProfileUrl);
}
