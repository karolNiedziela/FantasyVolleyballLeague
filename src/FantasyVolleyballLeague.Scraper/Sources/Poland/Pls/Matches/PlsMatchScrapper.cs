using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    internal sealed class PlsMatchScrapper
    {
        private readonly PlsWidgetLoader _widgetLoader;

        public PlsMatchScrapper(PlsWidgetLoader widgetLoader)
        {
            _widgetLoader = widgetLoader;
        }

        public async Task<MatchRecord> GetMatchAsync(LeagueOptions league, SeasonReference season, ScheduleEntry entry, PlaywrightSession session)
        {
            var document = await LoadMatchPageAsync(league, entry, session);

            var header = PlsMatchHeaderParser.Parse(document);
            var players = header.Status == MatchStatus.Finished
                ? await GetPlayersAsync(session, document)
                : MatchPlayers.Empty;

            return ToMatchRecord(league, season, entry, header, players);
        }

        public static async Task<MatchRecord> GetMatchWithoutStatisticsAsync(LeagueOptions league, SeasonReference season, ScheduleEntry entry, PlaywrightSession session)
        {
            var document = await LoadMatchPageAsync(league, entry, session);

            return ToMatchRecord(league, season, entry, PlsMatchHeaderParser.Parse(document), MatchPlayers.Empty);
        }

        private static async Task<HtmlDocument> LoadMatchPageAsync(LeagueOptions league, ScheduleEntry entry, PlaywrightSession session)
        {
            var html = await session.GetHtmlAsync(PlsUrls.Match(league, entry.MatchId));

            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }

        private static MatchRecord ToMatchRecord(
            LeagueOptions league, SeasonReference season, ScheduleEntry entry, PlsMatchHeader header, MatchPlayers players)
        {
            return new MatchRecord
            {
                ScrapedAtUtc = DateTime.UtcNow,
                League = league.Key,
                Season = season,
                Phase = entry.Phase,
                Stage = entry.Stage,
                Round = entry.Round,
                MatchId = entry.MatchId,
                Status = header.Status,
                StatisticsLevel = players.Level,
                Date = header.DateUtc,
                IsTimeConfirmed = header.IsTimeConfirmed,
                MatchNumber = header.MatchNumber,
                Attendance = header.Attendance,
                Mvp = header.Mvp,
                FirstReferee = header.FirstReferee,
                SecondReferee = header.SecondReferee,
                Commissioner = header.Commissioner,
                VenueName = header.VenueName,
                VenueCity = header.VenueCity,
                Sets = header.Sets,
                HomeRoster = ToRosterRecord(header.HomeRoster, players.Home),
                AwayRoster = ToRosterRecord(header.AwayRoster, players.Away),
            };
        }

        /// <summary>
        /// Picks the parser of the statistics era the match page belongs to.
        /// </summary>
        private async Task<MatchPlayers> GetPlayersAsync(PlaywrightSession session, HtmlDocument document)
        {
            if (PlsWidgetParser.CanParse(document))
            {
                var homeTask = PlsWidgetParser.GetPlayersAsync(_widgetLoader, session, document, PlsWidgetParser.HomeIframeClass);
                var awayTask = PlsWidgetParser.GetPlayersAsync(_widgetLoader, session, document, PlsWidgetParser.AwayIframeClass);
                return new MatchPlayers(StatisticsLevel.Full, await homeTask, await awayTask);
            }

            if (PlsTableColoredParser.CanParse(document))
            {
                return new MatchPlayers(
                    StatisticsLevel.Full,
                    PlsTableColoredParser.GetPlayers(document, 0),
                    PlsTableColoredParser.GetPlayers(document, 1));
            }

            var home = PlsArchiveTableParser.GetPlayers(document, 0);
            var away = PlsArchiveTableParser.GetPlayers(document, 1);
            var level = home.Count + away.Count > 0 ? StatisticsLevel.LineupOnly : StatisticsLevel.None;

            return new MatchPlayers(level, home, away);
        }

        private static MatchRosterRecord ToRosterRecord(PlsMatchHeaderRoster roster, IReadOnlyList<PlayerMatchStatistics> players)
        {
            return new MatchRosterRecord
            {
                Name = roster.Name,
                ExternalId = roster.ExternalId,
                SetsWon = roster.SetsWon,
                Players = players,
            };
        }

        private sealed record MatchPlayers(
            StatisticsLevel Level,
            IReadOnlyList<PlayerMatchStatistics> Home,
            IReadOnlyList<PlayerMatchStatistics> Away)
        {
            public static readonly MatchPlayers Empty = new(StatisticsLevel.None, [], []);
        }
    }
}
