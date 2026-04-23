using CsvHelper;
using FantasyVolleyballLeague.Worker.Services;
using FantasyVolleyballLeague.Worker.TeamScrappers;
using FantasyVolleyballLeague.Worker.TeamScrappers.Models;

namespace FantasyVolleyballLeague.Worker.DataProcessors.Teams
{
    public sealed class TeamDataProcessor : ITeamDataProcessor
    {
        private readonly ISeasonScrapper _seasonScrapper;
        private readonly ITeamScrapper _teamScrapper;
        private readonly PlaywrightFactory _playwrightFactory;

        public TeamDataProcessor(ISeasonScrapper seasonScrapper, ITeamScrapper teamScrapper, PlaywrightFactory playwrightFactory)
        {
            _seasonScrapper = seasonScrapper;
            _teamScrapper = teamScrapper;
            _playwrightFactory = playwrightFactory;
        }

        public async Task AcquireAndSaveAsync()
        {
            // One shared session for the entire run: season listing + all team/player pages.
            // Pages within a context are independent, so concurrent season tasks share safely.
            await using var session = await _playwrightFactory.SetupBrowserContextAsync();

            var teamSeasons = await _seasonScrapper.GetSeasons(new Uri(UrlConstants.PlusLigaTeams), session);

            using var semaphore = new SemaphoreSlim(3);
            var seasonTasks = new List<Task>();
            foreach (var season in teamSeasons)
            {
                await semaphore.WaitAsync();
                var task = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"Processing season: {season.StartYear}-{season.EndYear}");
                        var seasonDirectoryPath = Path.Combine("Files", "Seasons", "Teams", $"{season.StartYear}-{season.EndYear}");
                        if (IsSeasonAlreadyProcessed(seasonDirectoryPath))
                        {
                            Console.WriteLine($"Season {season.StartYear}-{season.EndYear} is already processed. Skipping.");
                            return;
                        }

                        TryCreateSeasonDirectory(seasonDirectoryPath);

                        var teamInformationList = await _teamScrapper.GetTeamDataAsync(season.Url, session);
                        foreach (var teamInformation in teamInformationList)
                        {
                            Console.WriteLine($"Processing team: {teamInformation.Name} in season: {season.StartYear}-{season.EndYear}");
                            WriteTeamInformationToCsv(teamInformation, seasonDirectoryPath);
                            Console.WriteLine($"Processed team: {teamInformation.Name} in season: {season.StartYear}-{season.EndYear}");
                        }

                        SaveProcessedSeason(seasonDirectoryPath);
                        Console.WriteLine($"Finished processing season: {season.StartYear}-{season.EndYear}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                seasonTasks.Add(task);
            }

            await Task.WhenAll(seasonTasks);
        }

        private static bool IsSeasonAlreadyProcessed(string seasonDirectoryPath)
        {
            var processedFileName = "processed.txt";
            var processedFilePath = Path.Combine(seasonDirectoryPath, processedFileName);

            return File.Exists(processedFilePath) && new DirectoryInfo(seasonDirectoryPath).GetFiles().Length > 0;
        }

        private static void TryCreateSeasonDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private static void SaveProcessedSeason(string seasonDirectoryPath)
        {
            var processedFileName = "processed.txt";
            var processedFilePath = Path.Combine(seasonDirectoryPath, processedFileName);
            File.Create(processedFilePath).Dispose();
        }

        private static void WriteTeamInformationToCsv(TeamInformation teamInformation, string seasonDirectoryPath)
        {
            var teamDirectoryFilePath = Path.Combine(seasonDirectoryPath, $"{teamInformation.Name}.csv");
            if (File.Exists(teamDirectoryFilePath))
            {
                return;
            }

            var csvConfiguration = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
            };
            using var writer = new StreamWriter(teamDirectoryFilePath);
            using var csv = new CsvWriter(writer, csvConfiguration);
            var players = teamInformation.Players.ToList();
            csv.WriteRecords(players);
        }
    }
}
