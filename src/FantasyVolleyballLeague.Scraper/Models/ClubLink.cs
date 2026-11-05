namespace FantasyVolleyballLeague.Scraper.Models
{
    /// <param name="ExternalId">Club id from the league website (stable across seasons).</param>
    internal sealed record ClubLink(int ExternalId, string Name);
}
