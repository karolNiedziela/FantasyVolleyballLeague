using FantasyVolleyballLeague.DataApi.Common.API;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons
{
    internal static class SeasonEndpointExtensions
    {
        public static RouteHandlerBuilder WithLeagueFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(SeasonQueryParameters.LeagueId, "Id of the league.");
        }

        public static RouteHandlerBuilder WithStartYearFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(SeasonQueryParameters.StartYear, "Start year of the season.");
        }

        public static RouteHandlerBuilder WithCurrentSeasonFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(SeasonQueryParameters.Current, "When true, returns only the latest season of each league.");
        }
    }
}
