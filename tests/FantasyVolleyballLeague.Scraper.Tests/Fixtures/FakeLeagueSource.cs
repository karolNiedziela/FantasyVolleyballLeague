using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources;

namespace FantasyVolleyballLeague.Scraper.Tests.Fixtures
{
    /// <summary>
    /// A source that only has a key. Every other member throws, so a test that calls one fails loudly.
    /// </summary>
    internal sealed class FakeLeagueSource : ILeagueSource
    {
        public FakeLeagueSource(string key)
        {
            Key = key;
        }

        public string Key { get; }

        public Task<IReadOnlyList<Season>> GetMatchSeasonsAsync(LeagueOptions league, PlaywrightSession session)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<ScheduleEntry>> GetScheduleAsync(Season season, PlaywrightSession session)
        {
            throw new NotSupportedException();
        }

        public Task<MatchRecord> GetMatchAsync(LeagueOptions league, SeasonReference season, ScheduleEntry entry, PlaywrightSession session)
        {
            throw new NotSupportedException();
        }

        public Task<MatchRecord> GetMatchWithoutStatisticsAsync(LeagueOptions league, SeasonReference season, ScheduleEntry entry, PlaywrightSession session)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Season>> GetRosterSeasonsAsync(LeagueOptions league, PlaywrightSession session)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<ClubLink>> GetClubsAsync(Season season, PlaywrightSession session)
        {
            throw new NotSupportedException();
        }

        public Task<RosterRecord> GetRosterAsync(LeagueOptions league, Season season, ClubLink club, RosterRecord? stored, PlaywrightSession session)
        {
            throw new NotSupportedException();
        }
    }
}
