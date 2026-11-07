namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters
{
    /// <summary>
    /// A row of the player's career table. <see cref="Tour"/> is the season id of the stint.
    /// </summary>
    internal sealed record PlsCareerRow(int ClubExternalId, int Tour, DateOnly From, DateOnly To, int? ShirtNumber, string? Position);
}
