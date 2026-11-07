namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters
{
    internal sealed record PlsTeamPage(string? Name, IReadOnlyList<PlsRosterEntry> Players);
}
