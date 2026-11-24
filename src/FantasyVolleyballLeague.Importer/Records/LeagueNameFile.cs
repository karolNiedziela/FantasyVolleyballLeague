namespace FantasyVolleyballLeague.Importer.Records
{
    /// <summary>
    /// An entry of <c>ManualData/league-names.json</c>: the league (<c>Importer:Leagues</c> key) is called
    /// <see cref="Name"/> from the season starting in <see cref="FromStartYear"/> until the next entry of that league.
    /// </summary>
    internal sealed record LeagueNameFile(string League, int FromStartYear, string Name);
}
