using System.Globalization;
using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters
{
    internal sealed class PlsRosterScrapper
    {
        private readonly PlaywrightOptions _playwrightOptions;

        public PlsRosterScrapper(IOptions<PlaywrightOptions> playwrightOptions)
        {
            _playwrightOptions = playwrightOptions.Value;
        }

        public static async Task<IReadOnlyList<ClubLink>> GetClubsAsync(Season season, PlaywrightSession session)
        {
            var document = await LoadAsync(session, season.Url.AbsoluteUri);
            return PlsRosterParser.ParseClubs(document);
        }

        public async Task<RosterRecord> GetRosterAsync(LeagueOptions league, Season season, ClubLink club, PlaywrightSession session)
        {
            var teamUrl = PlsUrls.Team(league, club.ExternalId, season.Tour);

            var teamPage = PlsRosterParser.ParseTeamPage(await LoadAsync(session, teamUrl));

            using var semaphore = new SemaphoreSlim(_playwrightOptions.MaxConcurrentTasks);
            var players = await Task.WhenAll(teamPage.Players.Select(async entry =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await GetPlayerAsync(league, entry, session);
                }
                finally
                {
                    semaphore.Release();
                }
            }));

            return new RosterRecord
            {
                ScrapedAtUtc = DateTime.UtcNow,
                League = league.Key,
                Season = season.Reference,
                Club = new ClubLink(club.ExternalId, teamPage.Name ?? club.Name),
                Players = players,
            };
        }

        /// <summary>
        /// Enriches the team page entry with the profile page. A failed profile page keeps the team page data only.
        /// </summary>
        private static async Task<PlayerProfile> GetPlayerAsync(LeagueOptions league, PlsRosterEntry entry, PlaywrightSession session)
        {
            PlsPlayerDetails? details = null;
            try
            {
                details = PlsRosterParser.ParsePlayerProfile(await LoadAsync(session, $"{league.BaseUrl}{entry.ProfileUrl}"));
            }
            catch (Exception ex) when (ex is PlaywrightException or TimeoutException)
            {
                Console.WriteLine($"  Player {entry.ExternalId} ({entry.FullName}): profile page failed ({ex.Message}), using team page data only.");
            }

            return new PlayerProfile
            {
                ExternalId = entry.ExternalId,
                FullName = entry.FullName,
                Position = entry.Position ?? details?.Position,
                ShirtNumber = entry.ShirtNumber ?? details?.ShirtNumber,
                DateOfBirth = details?.DateOfBirth,
                Height = details?.Height,
                Weight = details?.Weight,
                AttackReach = details?.AttackReach,
            };
        }

        private static async Task<HtmlDocument> LoadAsync(PlaywrightSession session, string url)
        {
            var document = new HtmlDocument();
            document.LoadHtml(await session.GetHtmlAsync(url));
            return document;
        }
    }
}
