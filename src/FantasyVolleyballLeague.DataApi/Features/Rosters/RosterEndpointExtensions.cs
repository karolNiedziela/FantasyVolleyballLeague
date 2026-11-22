using FantasyVolleyballLeague.DataApi.Common.API;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters
{
    internal static class RosterEndpointExtensions
    {
        public static RouteHandlerBuilder WithLeagueFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(RosterQueryParameters.LeagueId, "Id of the league.");
        }

        public static RouteHandlerBuilder WithSeasonFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(RosterQueryParameters.SeasonId, "Id of the season.");
        }

        public static RouteHandlerBuilder WithNameSearch(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(
                RosterQueryParameters.Query,
                "Words of the roster name: every word must occur in the name, in any order. Case, diacritics and punctuation are ignored.");
        }
    }
}
