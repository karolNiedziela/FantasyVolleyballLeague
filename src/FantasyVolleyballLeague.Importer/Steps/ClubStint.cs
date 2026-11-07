namespace FantasyVolleyballLeague.Importer.Steps
{
    /// <summary>
    /// A stay of a player in a club other than the roster file's club, taken from the player's career.
    /// </summary>
    internal sealed record ClubStint(
        Guid SeasonId,
        int ClubExternalId,
        Guid PlayerId,
        string FullName,
        int? ShirtNumber,
        int? PositionId,
        DateOnly? From,
        DateOnly? To);
}
