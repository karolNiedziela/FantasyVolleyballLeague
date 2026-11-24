namespace FantasyVolleyballLeague.Importer.Records
{
    /// <summary>
    /// An entry of <c>ManualData/club-splits.json</c>: the rosters of the club id <see cref="Id"/> (<c>League:clubId</c>)
    /// from the season starting in <see cref="FromStartYear"/> are another club, e.g. after a licence transfer. In
    /// <c>clubs.json</c> that part is <c>League:clubId@FromStartYear</c>.
    /// </summary>
    internal sealed record ClubSplitFile(string Id, int FromStartYear);
}
