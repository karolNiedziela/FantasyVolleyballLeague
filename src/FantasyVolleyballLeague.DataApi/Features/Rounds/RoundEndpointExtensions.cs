using FantasyVolleyballLeague.DataApi.Common.API;

namespace FantasyVolleyballLeague.DataApi.Features.Rounds
{
    internal static class RoundEndpointExtensions
    {
        public static RouteHandlerBuilder WithSeasonFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(RoundQueryParameters.SeasonId, "Id of the season. Required: the rounds are listed per season.");
        }

        public static RouteHandlerBuilder WithPhaseFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(RoundQueryParameters.PhaseId, "Id of a phase of the season, to list only its rounds.");
        }
    }
}
