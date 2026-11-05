using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FantasyVolleyballLeague.Scraper.Models;
using Microsoft.Extensions.Options;

namespace FantasyVolleyballLeague.Scraper.Storage
{
    /// <summary>
    /// Reads and writes the raw JSON layer:
    /// <c>{DataRoot}/raw/matches/{League}/{YYYY-YYYY}/{matchId}.json</c>,
    /// <c>{DataRoot}/raw/matches/{League}/{YYYY-YYYY}/_manifest.json</c> and
    /// <c>{DataRoot}/raw/rosters/{League}/{YYYY-YYYY}/{clubId}.json</c>.
    /// </summary>
    internal sealed class RawDataStore
    {
        private const string ManifestFileName = "_manifest.json";

        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };

        public RawDataStore(IOptions<StorageOptions> options)
        {
            Root = options.Value.ResolveDataRoot();
        }

        public string Root { get; }

        public string GetMatchesDirectory(string leagueKey, SeasonReference season)
        {
            return Path.Combine(Root, "raw", "matches", leagueKey, season.FolderName);
        }

        public string GetRostersDirectory(string leagueKey, SeasonReference season)
        {
            return Path.Combine(Root, "raw", "rosters", leagueKey, season.FolderName);
        }

        public MatchRecord? TryReadMatch(string leagueKey, SeasonReference season, int matchId)
        {
            return TryRead<MatchRecord>(GetMatchPath(leagueKey, season, matchId));
        }

        public bool MatchExists(string leagueKey, SeasonReference season, int matchId)
        {
            return File.Exists(GetMatchPath(leagueKey, season, matchId));
        }

        public Task WriteMatchAsync(MatchRecord match, CancellationToken cancellationToken)
        {
            return WriteAsync(GetMatchPath(match.League, match.Season, match.MatchId), match, cancellationToken);
        }

        public SeasonManifest? TryReadManifest(string leagueKey, SeasonReference season)
        {
            return TryRead<SeasonManifest>(Path.Combine(GetMatchesDirectory(leagueKey, season), ManifestFileName));
        }

        public Task WriteManifestAsync(string leagueKey, SeasonReference season, SeasonManifest manifest, CancellationToken cancellationToken)
        {
            return WriteAsync(Path.Combine(GetMatchesDirectory(leagueKey, season), ManifestFileName), manifest, cancellationToken);
        }

        public bool RosterExists(string leagueKey, SeasonReference season, int clubId)
        {
            return File.Exists(GetRosterPath(leagueKey, season, clubId));
        }

        public Task WriteRosterAsync(RosterRecord roster, CancellationToken cancellationToken)
        {
            return WriteAsync(GetRosterPath(roster.League, roster.Season, roster.Club.ExternalId), roster, cancellationToken);
        }

        private string GetMatchPath(string leagueKey, SeasonReference season, int matchId)
        {
            return Path.Combine(GetMatchesDirectory(leagueKey, season), $"{matchId.ToString(CultureInfo.InvariantCulture)}.json");
        }

        private string GetRosterPath(string leagueKey, SeasonReference season, int clubId)
        {
            return Path.Combine(GetRostersDirectory(leagueKey, season), $"{clubId.ToString(CultureInfo.InvariantCulture)}.json");
        }

        private static T? TryRead<T>(string path)
            where T : class
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions);
            }
            catch (JsonException)
            {
                // A corrupted or outdated file is treated as missing, so it gets scraped again.
                return null;
            }
        }

        /// <summary>
        /// Writes to a temporary file first, so an interrupted run never leaves a half-written JSON file.
        /// </summary>
        private static async Task WriteAsync<T>(string path, T value, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var temporaryPath = $"{path}.tmp";
            await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(value, JsonOptions), cancellationToken);
            File.Move(temporaryPath, path, overwrite: true);
        }
    }
}
