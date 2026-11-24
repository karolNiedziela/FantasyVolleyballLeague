namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// A roster as <see cref="ClubGrouping"/> sees it. <see cref="ClubId"/> is the club the roster belongs to now.
    /// </summary>
    internal sealed record ClubRoster(
        Guid RosterId,
        string LeagueKey,
        int? ExternalId,
        string NormalizedName,
        Guid ClubId,
        int StartYear,
        string Name);
}
