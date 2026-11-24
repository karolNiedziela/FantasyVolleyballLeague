namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// A club as it should be: <see cref="ExistingClubId"/> is the club to keep, or <c>null</c> when a new club is
    /// needed. <see cref="Name"/> is the name of its newest roster.
    /// </summary>
    internal sealed record GroupedClub(Guid? ExistingClubId, string Name, IReadOnlyList<Guid> RosterIds);
}
