namespace FantasyVolleyballLeague.Importer
{
    internal sealed class ImporterOptions
    {
        public const string SectionName = "Importer";

        public List<LeagueImportOptions> Leagues { get; set; } = [];
    }
}
