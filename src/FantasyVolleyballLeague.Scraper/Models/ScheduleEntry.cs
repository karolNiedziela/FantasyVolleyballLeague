namespace FantasyVolleyballLeague.Scraper.Models
{
    /// <summary>
    /// A match listed on the season games page together with its place in the season structure.
    /// </summary>
    internal sealed record ScheduleEntry(int MatchId, NamedOrder Phase, NamedOrder Stage, NamedOrder Round);
}
