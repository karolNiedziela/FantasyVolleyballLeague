using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Seasons;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls
{
    /// <summary>
    /// Source for the leagues run by PLS (Polska Liga Siatkówki S.A.), the company operating the Polish
    /// professional volleyball leagues: TAURON Liga (formerly PlusLiga), TAURON Liga Kobiet, TAURON 1. Liga
    /// and TAURON 1. Liga Kobiet. All their websites (tauronliga.pl, tauron1liga.pl, ...) run on the same
    /// PLS CMS ("PLS S.A." in the footer, pls.pl), so they share the HTML structure, URL patterns and match ids.
    /// </summary>
    internal sealed class PlsLeagueSource : ILeagueSource
    {
        public const string SourceKey = "Pls";

        private readonly PlsMatchScrapper _matchScrapper;
        private readonly PlsRosterScrapper _rosterScrapper;

        public PlsLeagueSource(PlsMatchScrapper matchScrapper, PlsRosterScrapper rosterScrapper)
        {
            _matchScrapper = matchScrapper;
            _rosterScrapper = rosterScrapper;
        }

        public string Key => SourceKey;

        public Task<IReadOnlyList<Season>> GetMatchSeasonsAsync(LeagueOptions league, PlaywrightSession session)
        {
            return PlsSeasonScrapper.GetSeasonsAsync(league, session);
        }

        public Task<IReadOnlyList<ScheduleEntry>> GetScheduleAsync(Season season, PlaywrightSession session)
        {
            return PlsScheduleScrapper.GetScheduleAsync(season, session);
        }

        public Task<MatchRecord> GetMatchAsync(LeagueOptions league, SeasonReference season, ScheduleEntry entry, PlaywrightSession session)
        {
            return _matchScrapper.GetMatchAsync(league, season, entry, session);
        }

        public Task<MatchRecord> GetMatchWithoutStatisticsAsync(LeagueOptions league, SeasonReference season, ScheduleEntry entry, PlaywrightSession session)
        {
            return PlsMatchScrapper.GetMatchWithoutStatisticsAsync(league, season, entry, session);
        }

        public Task<IReadOnlyList<Season>> GetRosterSeasonsAsync(LeagueOptions league, PlaywrightSession session)
        {
            return PlsSeasonScrapper.GetTeamSeasonsAsync(league, session);
        }

        public Task<IReadOnlyList<ClubLink>> GetClubsAsync(Season season, PlaywrightSession session)
        {
            return PlsRosterScrapper.GetClubsAsync(season, session);
        }

        public Task<RosterRecord> GetRosterAsync(LeagueOptions league, Season season, ClubLink club, PlaywrightSession session)
        {
            return _rosterScrapper.GetRosterAsync(league, season, club, session);
        }
    }
}
