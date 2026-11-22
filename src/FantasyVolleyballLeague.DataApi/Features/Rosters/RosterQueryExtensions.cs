using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters
{
    internal static class RosterQueryExtensions
    {
        public static IQueryable<Roster> WhereLeague(this IQueryable<Roster> rosters, Guid? leagueId)
        {
            if (!leagueId.HasValue)
            {
                return rosters;
            }

            return rosters.Where(x => x.LeagueId == leagueId.Value);
        }

        public static IQueryable<Roster> WhereSeason(this IQueryable<Roster> rosters, Guid? seasonId)
        {
            if (!seasonId.HasValue)
            {
                return rosters;
            }

            return rosters.Where(x => x.SeasonId == seasonId.Value);
        }

        public static IQueryable<RosterResponse> ProjectToResponse(this IQueryable<Roster> rosters)
        {
            return rosters.Select(x => new RosterResponse
            {
                Id = x.Id,
                Name = x.Name,
                LeagueId = x.LeagueId,
                Season = new SeasonSummaryResponse
                {
                    Id = x.Season.Id,
                    Name = x.Season.Name,
                    StartYear = x.Season.StartYear,
                    EndYear = x.Season.EndYear
                }
            });
        }
    }
}
