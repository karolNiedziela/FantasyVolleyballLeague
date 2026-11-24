using System.Text.Json;

namespace FantasyVolleyballLeague.Importer.Import
{
    internal sealed class ManualDataReader
    {
        public const string LeagueNamesFileName = "league-names.json";

        public const string ClubsFileName = "clubs.json";

        public const string ClubSplitsFileName = "club-splits.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ManualDataReader(string manualDataDirectory)
        {
            ManualDataDirectory = manualDataDirectory;
        }

        public string ManualDataDirectory { get; }

        public async Task<IReadOnlyList<T>> ReadAsync<T>(string fileName, CancellationToken cancellationToken)
        {
            await using var stream = File.OpenRead(GetPath(fileName));
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions, cancellationToken)
                ?? throw new InvalidDataException("The file holds null instead of an array.");
        }

        /// <summary>
        /// Reads the file and validates its entries. A file that cannot be read (missing, invalid JSON, <c>null</c>)
        /// gives one error; a file with validation errors gives those errors. In both cases there are no entries.
        /// </summary>
        public async Task<ManualDataResult<T>> ReadValidatedAsync<T>(
            string fileName,
            Func<IReadOnlyList<T>, IReadOnlyList<string>> validate,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<T> entries;
            try
            {
                entries = await ReadAsync<T>(fileName, cancellationToken);
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException or IOException)
            {
                return new ManualDataResult<T>(null, [ex.Message]);
            }

            var errors = validate(entries);
            return new ManualDataResult<T>(errors.Count == 0 ? entries : null, errors);
        }

        public string GetPath(string fileName)
        {
            return Path.Combine(ManualDataDirectory, fileName);
        }
    }
}
