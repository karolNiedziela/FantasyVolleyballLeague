using System.Text.Encodings.Web;
using System.Text.Json;
using FantasyVolleyballLeague.Worker.Services;
using FantasyVolleyballLeague.Worker.TeamScrappers;
using FantasyVolleyballLeague.Worker.TeamScrappers.Models;

namespace FantasyVolleyballLeague.Worker.DataProcessors.Teams
{
    public sealed class TeamDataProcessor : ITeamDataProcessor
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly ISeasonScrapper _seasonScrapper;
        private readonly ITeamScrapper _teamScrapper;
        private readonly PlaywrightFactory _playwrightFactory;
        private readonly IReadOnlyList<LeagueOptions> _leagues;

        public TeamDataProcessor(ISeasonScrapper seasonScrapper, ITeamScrapper teamScrapper, PlaywrightFactory playwrightFactory, IReadOnlyList<LeagueOptions> leagues)
        {
            _seasonScrapper = seasonScrapper;
            _teamScrapper = teamScrapper;
            _playwrightFactory = playwrightFactory;
            _leagues = leagues;
        }

        public async Task AcquireAndSaveAsync()
        {
            await using var session = await _playwrightFactory.SetupBrowserContextAsync();

            foreach (var league in _leagues)
            {
                var teamSeasons = await _seasonScrapper.GetTeamSeasonsAsync(league, session);

                foreach (var season in teamSeasons)
                {
                    Console.WriteLine($"Processing season: {season.StartYear}-{season.EndYear}");
                    var seasonDirectoryPath = Path.Combine("Files", "Seasons", "Teams", league.Name, $"{season.StartYear}-{season.EndYear}");
                    if (IsAlreadyProcessed(seasonDirectoryPath))
                    {
                        Console.WriteLine($"Season {season.StartYear}-{season.EndYear} is already processed. Skipping.");
                        continue;
                    }

                    TryCreateDirectory(seasonDirectoryPath);

                    var rosters = await _teamScrapper.GetTeamRosterAsync(season.Url, league.Url, session);
                    foreach (var roster in rosters)
                    {
                        Console.WriteLine($"Processing team: {roster.Name} in season: {season.StartYear}-{season.EndYear}");
                        WriteRosterToJson(roster, seasonDirectoryPath);
                        Console.WriteLine($"Processed team: {roster.Name} in season: {season.StartYear}-{season.EndYear}");
                    }

                    SaveProcessedFile(seasonDirectoryPath);
                    Console.WriteLine($"Finished processing season: {season.StartYear}-{season.EndYear}");
                }
            }
        }

        private static bool IsAlreadyProcessed(string directoryPath)
        {
            var processedFilePath = Path.Combine(directoryPath, "processed.txt");
            return File.Exists(processedFilePath);
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

        private static void WriteRosterToJson(TeamRoster roster, string seasonDirectoryPath)
        {
            var filePath = Path.Combine(seasonDirectoryPath, $"{roster.Name}.json");
            if (File.Exists(filePath))
            {
                return;
            }

            File.WriteAllText(filePath, JsonSerializer.Serialize(roster, JsonOptions));
        }
    }
}
