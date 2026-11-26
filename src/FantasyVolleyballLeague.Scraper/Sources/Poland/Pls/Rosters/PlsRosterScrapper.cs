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

        public async Task<RosterRecord> GetRosterAsync(LeagueOptions league, Season season, ClubLink club, RosterRecord? stored, PlaywrightSession session)
        {
            var teamUrl = PlsUrls.Team(league, club.ExternalId, season.Tour);

            var teamPage = PlsRosterParser.ParseTeamPage(await LoadAsync(session, teamUrl));

            using var semaphore = new SemaphoreSlim(_playwrightOptions.MaxConcurrentTasks);
            var players = await Task.WhenAll(teamPage.Players.Select(async entry =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await GetPlayerAsync(league, season.Tour, entry, session);
                }
                finally
                {
                    semaphore.Release();
                }
            }));

            var formerPlayers = new List<PlayerProfile>();
            var storedFormerPlayers = FormerPlayers(teamPage.Players, stored);
            foreach (var storedPlayer in storedFormerPlayers)
            {
                var formerPlayer = await GetFormerPlayerAsync(league, season, club, storedPlayer, session);
                if (formerPlayer is not null)
                {
                    formerPlayers.Add(formerPlayer);
                }
            }

            return new RosterRecord
            {
                ScrapedAtUtc = DateTime.UtcNow,
                League = league.Key,
                Season = season.Reference,
                Club = new ClubLink(club.ExternalId, teamPage.Name ?? club.Name),
                Players = [.. players, .. formerPlayers],
                Staff = teamPage.Staff,
            };
        }

        /// <summary>
        /// Enriches the team page entry with the profile page. A failed profile page keeps the team page data only.
        /// </summary>
        private static async Task<PlayerProfile> GetPlayerAsync(LeagueOptions league, int? tour, PlsRosterEntry entry, PlaywrightSession session)
        {
            PlsPlayerDetails? details = null;
            IReadOnlyList<PlayerCareerEntry> career = [];
            try
            {
                var profile = await LoadAsync(session, $"{league.BaseUrl}{entry.ProfileUrl}");
                details = PlsRosterParser.ParsePlayerProfile(profile);
                career = ToCareerEntries(PlsRosterParser.ParseCareer(profile), tour);
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
                Career = career,
            };
        }

        /// <summary>
        /// Players of the stored roster file who are no longer on the team page: they left the club during the season.
        /// </summary>
        internal static IReadOnlyList<PlayerProfile> FormerPlayers(IReadOnlyList<PlsRosterEntry> teamPagePlayers, RosterRecord? stored)
        {
            if (stored is null)
            {
                return [];
            }

            var listedIds = teamPagePlayers.Select(player => player.ExternalId).ToHashSet();
            return [.. stored.Players.Where(player => !listedIds.Contains(player.ExternalId))];
        }

        /// <summary>
        /// A former player stays while his career still has a stint in the club in this season (the stint then ends
        /// on the day he left); otherwise the website corrected the roster and he is dropped.
        /// </summary>
        internal static PlayerProfile? KeepFormerPlayer(PlayerProfile stored, PlsPlayerDetails details, IReadOnlyList<PlayerCareerEntry> career, int clubExternalId)
        {
            if (!career.Any(entry => entry.ClubExternalId == clubExternalId))
            {
                return null;
            }

            return stored with
            {
                Position = details.Position ?? stored.Position,
                ShirtNumber = details.ShirtNumber ?? stored.ShirtNumber,
                DateOfBirth = details.DateOfBirth ?? stored.DateOfBirth,
                Height = details.Height ?? stored.Height,
                Weight = details.Weight ?? stored.Weight,
                AttackReach = details.AttackReach ?? stored.AttackReach,
                Career = career,
            };
        }

        /// <summary>
        /// A profile page that fails to load keeps the stored player as he was.
        /// </summary>
        private static async Task<PlayerProfile?> GetFormerPlayerAsync(LeagueOptions league, Season season, ClubLink club, PlayerProfile stored, PlaywrightSession session)
        {
            try
            {
                var profile = await LoadAsync(session, PlsUrls.Player(league, stored.ExternalId, season.Tour));
                var career = ToCareerEntries(PlsRosterParser.ParseCareer(profile), season.Tour);
                return KeepFormerPlayer(stored, PlsRosterParser.ParsePlayerProfile(profile), career, club.ExternalId);
            }
            catch (Exception ex) when (ex is PlaywrightException or TimeoutException)
            {
                Console.WriteLine($"  Former player {stored.ExternalId} ({stored.FullName}): profile page failed ({ex.Message}), keeping the stored data.");
                return stored;
            }
        }

        /// <summary>
        /// Keeps the stints of the roster's season (the career table lists every season of the player).
        /// </summary>
        internal static IReadOnlyList<PlayerCareerEntry> ToCareerEntries(IReadOnlyList<PlsCareerRow> rows, int? tour)
        {
            var seasonRows = rows.Where(row => row.Tour == tour);

            return seasonRows
                .OrderBy(row => row.From)
                .Select(row => new PlayerCareerEntry
                {
                    ClubExternalId = row.ClubExternalId,
                    From = row.From,
                    To = row.To,
                    ShirtNumber = row.ShirtNumber,
                    Position = row.Position,
                })
                .ToList();
        }

        private static async Task<HtmlDocument> LoadAsync(PlaywrightSession session, string url)
        {
            var document = new HtmlDocument();
            document.LoadHtml(await session.GetHtmlAsync(url));
            return document;
        }
    }
}
