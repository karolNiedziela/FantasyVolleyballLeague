namespace FantasyVolleyballLeague.Importer.Steps
{
    internal sealed record ImportedMatch(Guid Id, DateTimeOffset SourceScrapedAt);
}
