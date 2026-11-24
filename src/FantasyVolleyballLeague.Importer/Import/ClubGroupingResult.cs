namespace FantasyVolleyballLeague.Importer.Import
{
    /// <param name="Clubs">The clubs with their rosters.</param>
    /// <param name="ClubIdsToDelete">The existing clubs that no club kept (left without rosters).</param>
    /// <param name="UnmatchedIds">The ids of <c>clubs.json</c> that match no roster.</param>
    internal sealed record ClubGroupingResult(
        IReadOnlyList<GroupedClub> Clubs,
        IReadOnlyList<Guid> ClubIdsToDelete,
        IReadOnlyList<string> UnmatchedIds);
}
