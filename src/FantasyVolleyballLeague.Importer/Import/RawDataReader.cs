using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// Reads the raw JSON layer written by the scraper (<c>{DataRoot}/raw/{matches|rosters}/{League}/{YYYY-YYYY}/*.json</c>).
    /// </summary>
    internal sealed class RawDataReader
    {
        public const int SupportedSchemaVersion = 1;

        private const string ManifestFileName = "_manifest.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public RawDataReader(IOptions<StorageOptions> options)
        {
            DataRoot = options.Value.ResolveDataRoot();
        }

        public string DataRoot { get; }

        public IReadOnlyList<string> GetFiles(string kind, string leagueKey)
        {
            var leagueDirectory = Path.Combine(DataRoot, "raw", kind, leagueKey);
            if (!Directory.Exists(leagueDirectory))
            {
                return [];
            }

            return [.. Directory.EnumerateDirectories(leagueDirectory)
                .Order(StringComparer.Ordinal)
                .SelectMany(season => Directory.EnumerateFiles(season, "*.json").Order(StringComparer.Ordinal))
                .Where(file => !string.Equals(Path.GetFileName(file), ManifestFileName, StringComparison.OrdinalIgnoreCase))];
        }

        public static async Task<T> ReadAsync<T>(string path, Func<T, int> schemaVersion, CancellationToken cancellationToken)
        {
            await using var stream = File.OpenRead(path);
            var record = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
                ?? throw new InvalidDataException("The file is empty.");

            var version = schemaVersion(record);
            if (version != SupportedSchemaVersion)
            {
                throw new InvalidDataException($"Unsupported schema version {version} (expected {SupportedSchemaVersion}).");
            }

            return record;
        }
    }
}
