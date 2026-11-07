using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Steps
{
    internal sealed class MatchImportContext
    {
        public MatchImportContext(MatchFile file, string path, Guid leagueId, Guid matchId)
        {
            File = file;
            Path = path;
            LeagueId = leagueId;
            MatchId = matchId;
        }

        public MatchFile File { get; }

        public string Path { get; }

        public Guid LeagueId { get; }

        public Guid MatchId { get; }

        public HashSet<Guid> Participants { get; } = [];

        /// <summary>
        /// The roster each participant played for in this match.
        /// </summary>
        public Dictionary<Guid, Guid> ParticipantRosters { get; } = [];

        /// <summary>
        /// Day of the match (UTC), used to tell which roster entries were valid then. <c>null</c> when the date is unknown.
        /// </summary>
        public DateOnly? Date => File.Date is null ? null : DateOnly.FromDateTime(File.Date.Value);

        public Dictionary<string, Guid> ParticipantsByNameKey { get; } = [];
    }
}
