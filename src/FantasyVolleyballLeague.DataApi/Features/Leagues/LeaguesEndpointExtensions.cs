using FantasyVolleyballLeague.DataApi.Common.API;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues
{
    internal static class LeaguesEndpointExtensions
    {
        public static RouteHandlerBuilder WithGenderFilter(this RouteHandlerBuilder builder)
        {
            var genders = string.Join(", ", LeagueGender.All.Select(gender => $"{gender.Id} = {gender.Name}"));

            return builder.WithParameterDescription(LeaguesQueryParameters.Gender, $"Gender id: {genders}.");
        }

        public static RouteHandlerBuilder WithDivisionFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(LeaguesQueryParameters.Division, "Division number, 1 is the top division.");
        }
    }
}
