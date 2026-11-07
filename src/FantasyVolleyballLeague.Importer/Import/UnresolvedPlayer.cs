namespace FantasyVolleyballLeague.Importer.Import
{
    internal sealed record UnresolvedPlayer(string League, string Season, int MatchId, string Roster, string Name, int? Number);
}
