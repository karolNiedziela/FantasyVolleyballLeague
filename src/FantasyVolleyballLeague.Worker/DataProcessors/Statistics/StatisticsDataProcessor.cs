using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FantasyVolleyballLeague.Worker.Services;
using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;
using FantasyVolleyballLeague.Worker.StatisticsScrappers;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;

namespace FantasyVolleyballLeague.Worker.DataProcessors.Statistics
{
    public sealed class StatisticsDataProcessor : IStatisticsDataProcessor
    {
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
                            WriteRoundStatisticsInformationToCsv(round, stageDirectoryPath);
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

        private static void WriteRoundStatisticsInformationToCsv(SeasonPhaseRound round, string baseDirectoryPath)
        {
            var roundDirectoryName = string.IsNullOrWhiteSpace(round.RoundName)
                ? round.RoundNumber.ToString(CultureInfo.InvariantCulture)
                : SanitizeDirectoryName(round.RoundName);
            var roundDirectoryPath = Path.Combine(baseDirectoryPath, roundDirectoryName);
            TryCreateDirectory(roundDirectoryPath);

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
            };

            foreach (var matchStatistics in round.Matches)
            {
                var matchFilePath = Path.Combine(roundDirectoryPath, $"{matchStatistics.FirstTeamStatistics.Name}_{matchStatistics.SecondTeamStatistics.Name}_{matchStatistics.ExternalMatchId}");
                TryCreateDirectory(matchFilePath);

                WriteSingleTeamStatistics(matchStatistics.FirstTeamStatistics, csvConfiguration, matchFilePath);
                WriteSingleTeamStatistics(matchStatistics.SecondTeamStatistics, csvConfiguration, matchFilePath);
            }

            SaveProcessedFile(roundDirectoryPath);
        }

        private static void WriteSingleTeamStatistics(MatchTeamStatistics matchTeamStatistics, CsvConfiguration configuration, string matchFilePath)
        {
            var teamStatisticsFilePath = Path.Combine(matchFilePath, $"{matchTeamStatistics.Name}.csv");

            using var writer = new StreamWriter(teamStatisticsFilePath);
            using var csv = new CsvWriter(writer, configuration);
            var players = matchTeamStatistics.PlayersStatistics.ToList();
            csv.WriteRecords(players);
        }
    }
}
