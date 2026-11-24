namespace FantasyVolleyballLeague.Importer.Records
{
    /// <summary>
    /// An entry of <c>ManualData/clubs.json</c>: the club ids in <see cref="Ids"/> (<c>League:clubId</c>, the league
    /// is an <c>Importer:Leagues</c> key) are one club. <see cref="Key"/> only names the entry in the file.
    /// </summary>
    internal sealed record ClubGroupFile(string Key, IReadOnlyList<string> Ids);
}
