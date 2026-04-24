using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using FantasyVolleyballLeague.Worker.Services;
using FantasyVolleyballLeague.Worker.StatisticsScrappers;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;

namespace FantasyVolleyballLeague.Worker.DataProcessors.Statistics
{
    public sealed class StatisticsDataProcessor : IStatisticsDataProcessor
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly ISeasonScrapper _seasonScrapper;
        private readonly ISeasonMatchScrapper _seasonMatchScrapper;
        private readonly PlaywrightFactory _playwrightFactory;

        public StatisticsDataProcessor(ISeasonScrapper seasonScrapper, ISeasonMatchScrapper seasonMatchScrapper, PlaywrightFactory playwrightFactory)
        {
            _seasonScrapper = seasonScrapper;
            _seasonMatchScrapper = seasonMatchScrapper;
            _playwrightFactory = playwrightFactory;
        }

        public async Task AcquireAndSaveAsync()
        {
            await using var session = await _playwrightFactory.SetupBrowserContextAsync();

            var seasons = await _seasonScrapper.GetSeasons(new Uri(UrlConstants.PlusLigaGames), session);

            foreach (var season in seasons)
            {
                Console.WriteLine($"Processing season: {season.StartYear}-{season.EndYear}");
                var seasonDirectoryPath = Path.Combine("Files", "Seasons", "Matches", $"{season.StartYear}-{season.EndYear}");
                if (IsAlreadyProcessed(seasonDirectoryPath))
                {
                    Console.WriteLine($"Season {season.StartYear}-{season.EndYear} is already processed. Skipping.");
                    continue;
                }

                TryCreateDirectory(seasonDirectoryPath);

                var page = await session.Context.NewPageAsync();
                await page.GotoAsync(season.Url.AbsoluteUri);

                var allPhases = await _seasonMatchScrapper.GetAllPhasesMatchStatisticsAsync(page, session);
                await page.CloseAsync();

                if (allPhases.Count == 0)
                {
                    Console.WriteLine($"No data found for season: {season.StartYear}-{season.EndYear}. Skipping.");
                    continue;
                }

                foreach (var phase in allPhases)
                {
                    var phaseDirectoryPath = Path.Combine(seasonDirectoryPath, SanitizeDirectoryName(phase.PhaseName));
                    TryCreateDirectory(phaseDirectoryPath);

                    var hasMultipleStages = phase.Stages.Count > 1;

                    foreach (var stage in phase.Stages)
                    {
                        var stageDirectoryPath = hasMultipleStages
                            ? Path.Combine(phaseDirectoryPath, SanitizeDirectoryName(stage.StageName))
                            : phaseDirectoryPath;

                        if (hasMultipleStages)
                        {
                            TryCreateDirectory(stageDirectoryPath);
                        }

                        foreach (var round in stage.Rounds)
                        {
                            var roundLabel = round.RoundName ?? round.RoundNumber.ToString(CultureInfo.InvariantCulture);
                            Console.WriteLine($"Processing phase: {phase.PhaseName}, stage: {stage.StageName}, round: {roundLabel} in season: {season.StartYear}-{season.EndYear}");
                            WriteRoundStatisticsToJson(round, stageDirectoryPath);
                            Console.WriteLine($"Processed phase: {phase.PhaseName}, stage: {stage.StageName}, round: {roundLabel} in season: {season.StartYear}-{season.EndYear}");
                        }
                    }

                    SaveProcessedFile(phaseDirectoryPath);
                }

                Console.WriteLine($"Finished processing season: {season.StartYear}-{season.EndYear}");
            }
        }

        private static string SanitizeDirectoryName(string name)
            => string.Concat(name.Split(Path.GetInvalidFileNameChars()));

        private static bool IsAlreadyProcessed(string directoryPath)
        {
            var processedFilePath = Path.Combine(directoryPath, "processed.txt");
            return File.Exists(processedFilePath) && new DirectoryInfo(directoryPath).GetFiles().Length > 0;
        }

        private static void TryCreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private static void SaveProcessedFile(string directoryPath)
        {
            var processedFilePath = Path.Combine(directoryPath, "processed.txt");
            File.Create(processedFilePath).Dispose();
        }

        private static void WriteRoundStatisticsToJson(SeasonPhaseRound round, string baseDirectoryPath)
        {
            var roundDirectoryName = string.IsNullOrWhiteSpace(round.RoundName)
                ? round.RoundNumber.ToString(CultureInfo.InvariantCulture)
                : SanitizeDirectoryName(round.RoundName);
            var roundDirectoryPath = Path.Combine(baseDirectoryPath, roundDirectoryName);
            TryCreateDirectory(roundDirectoryPath);

            foreach (var match in round.Matches)
            {
                var matchFilePath = Path.Combine(roundDirectoryPath, $"{match.MatchId}.json");
                File.WriteAllText(matchFilePath, JsonSerializer.Serialize(match, JsonOptions));
            }

            SaveProcessedFile(roundDirectoryPath);
        }
    }
}
