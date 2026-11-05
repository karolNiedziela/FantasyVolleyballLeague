using System.Text.Encodings.Web;
using System.Text.Json;

namespace FantasyVolleyballLeague.Importer.Import
{
    internal sealed record UnresolvedPlayer(string League, string Season, int MatchId, string Roster, string Name, int? Number);

    internal sealed record FileError(string Path, string Message);

    /// <summary>
    /// Summary of an import run, written to <c>{DataRoot}/import-report.json</c>.
    /// </summary>
    internal sealed class ImportReport
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public DateTime StartedAtUtc { get; } = DateTime.UtcNow;

        public int RostersImported { get; set; }

        public int MatchesInserted { get; set; }

        public int MatchesUpdated { get; set; }

        public int MatchesSkipped { get; set; }

        public int MatchesWithoutMvp { get; set; }

        /// <summary>
        /// Players matched by roster shirt number or name (widget era, no player id in statistics).
        /// </summary>
        public int PlayersResolvedByRoster { get; set; }

        public List<UnresolvedPlayer> UnresolvedPlayers { get; } = [];

        public List<string> UnresolvedMvps { get; } = [];

        public List<FileError> FileErrors { get; } = [];

        public async Task<string> WriteAsync(string dataRoot, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(dataRoot);
            var path = Path.Combine(dataRoot, "import-report.json");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(this, JsonOptions), cancellationToken);
            return path;
        }
    }
}
