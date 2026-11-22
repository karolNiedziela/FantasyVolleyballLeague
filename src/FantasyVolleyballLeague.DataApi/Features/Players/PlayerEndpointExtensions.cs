using FantasyVolleyballLeague.DataApi.Common.API;
using FantasyVolleyballLeague.DataApi.Common.API.Lookups;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Players
{
    internal static class PlayerEndpointExtensions
    {
        public static RouteHandlerBuilder WithGenderFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithCodeFilter(
                PlayerQueryParameters.Gender,
                "Gender of the player.",
                LeagueGender.All.Select(gender => gender.Code));
        }

        public static RouteHandlerBuilder WithPositionFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithCodeFilter(
                PlayerQueryParameters.Position,
                "Position of the player, the same value as the position in the response: from the current roster player, the most recent one among the roster players selected by rosterId, seasonId and leagueId (all of them when none is given). A current roster player without a position never matches.",
                PlayerPosition.All.Select(position => position.Code));
        }

        public static RouteHandlerBuilder WithRosterFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(
                PlayerQueryParameters.RosterId,
                "Id of a roster. Keeps the players on that roster.");
        }

        public static RouteHandlerBuilder WithSeasonFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(
                PlayerQueryParameters.SeasonId,
                "Id of a season. Keeps the players on any roster of that season.");
        }

        public static RouteHandlerBuilder WithLeagueFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(
                PlayerQueryParameters.LeagueId,
                "Id of a league. Keeps the players on any roster of that league, in any season.");
        }

        public static RouteHandlerBuilder WithNameSearch(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(
                PlayerQueryParameters.Query,
                "Words of the player name: every word must occur in the name, in any order. Case, diacritics and punctuation are ignored.");
        }
    }
}
