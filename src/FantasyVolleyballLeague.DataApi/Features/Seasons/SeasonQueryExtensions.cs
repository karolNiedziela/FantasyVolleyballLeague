using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons
{
    internal static class SeasonQueryExtensions
    {
        public static IQueryable<Season> WhereStartYear(this IQueryable<Season> seasons, int? startYear)
        {
            if (!startYear.HasValue)
            {
                return seasons;
            }

            return seasons.Where(x => x.StartYear == startYear.Value);
        }

        public static IQueryable<Season> WhereCurrent(this IQueryable<Season> seasons, bool? current, IQueryable<Season> allSeasons)
        {
            if (current != true)
            {
                return seasons;
            }

            return seasons.Where(x => x.StartYear == allSeasons
                .Where(season => season.LeagueId == x.LeagueId)
                .Max(season => season.StartYear));
        }

        public static IQueryable<SeasonResponse> ProjectToResponse(this IQueryable<Season> seasons, IQueryable<Season> allSeasons)
        {
            return seasons.Select(x => new SeasonResponse
            {
                Id = x.Id,
                Name = x.Name,
                StartYear = x.StartYear,
                EndYear = x.EndYear,
                LeagueName = x.LeagueName,
                LeagueId = x.LeagueId,
                IsCurrent = x.StartYear == allSeasons
                    .Where(season => season.LeagueId == x.LeagueId)
                    .Max(season => season.StartYear)
            });
        }
    }
}
