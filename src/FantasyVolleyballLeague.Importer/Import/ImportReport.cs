using System.Text.Encodings.Web;
using System.Text.Json;

namespace FantasyVolleyballLeague.Importer.Import
{
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

        /// <summary>
        /// Players missing from the roster of the club in the match but found by name on another roster of the same
        /// gender and season (a transfer during the season, or a club of another division).
        /// </summary>
        public int PlayersResolvedBySeason { get; set; }

        /// <summary>
        /// Roster entries created or completed from the career of a player who moved during the season.
        /// </summary>
        public int RosterEntriesFromCareer { get; set; }

        public List<UnresolvedPlayer> UnresolvedPlayers { get; } = [];

        public List<string> UnresolvedMvps { get; } = [];

        public List<FileError> FileErrors { get; } = [];

        /// <summary>
        /// Data the import could repair but that is wrong on the website, e.g. one player id used for both squads of a match.
        /// </summary>
        public List<FileError> Warnings { get; } = [];

        public async Task<string> WriteAsync(string dataRoot, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(dataRoot);
            var path = Path.Combine(dataRoot, "import-report.json");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(this, JsonOptions), cancellationToken);
            return path;
        }
    }
}
