using FantasyVolleyballLeague.Scraper.Models;

namespace FantasyVolleyballLeague.Scraper.Sources
{
    /// <summary>
    /// A website (or family of websites sharing one system) that league data is scraped from. Every source knows
    /// its own HTML structure and URL patterns and returns the shared raw models (<see cref="MatchRecord"/>,
    /// <see cref="RosterRecord"/>). A league selects its source with <see cref="LeagueOptions.Source"/>
    /// (see <see cref="LeagueSourceResolver"/>); implementations live in <c>Sources/{Country}/{Provider}/</c>.
    /// </summary>
    internal interface ILeagueSource
    {
        /// <summary>
        /// Value of <see cref="LeagueOptions.Source"/> that selects this source, e.g. "Pls".
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Seasons with match schedules, newest first.
        /// </summary>
        Task<IReadOnlyList<Season>> GetMatchSeasonsAsync(LeagueOptions league, PlaywrightSession session);

        /// <summary>
        /// All matches of the season (finished and scheduled) with their phase, stage and round.
        /// </summary>
        Task<IReadOnlyList<ScheduleEntry>> GetScheduleAsync(Season season, PlaywrightSession session);

        Task<MatchRecord> GetMatchAsync(LeagueOptions league, SeasonReference season, ScheduleEntry entry, PlaywrightSession session);

        /// <summary>
        /// Seasons with rosters, newest first.
        /// </summary>
        Task<IReadOnlyList<Season>> GetRosterSeasonsAsync(LeagueOptions league, PlaywrightSession session);

        /// <summary>
        /// Clubs playing in the season.
        /// </summary>
        Task<IReadOnlyList<ClubLink>> GetClubsAsync(Season season, PlaywrightSession session);

        Task<RosterRecord> GetRosterAsync(LeagueOptions league, Season season, ClubLink club, PlaywrightSession session);
    }
}
