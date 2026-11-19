using FantasyVolleyballLeague.DataApi.Common.API;
using FantasyVolleyballLeague.DataApi.Common.API.Lookups;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Leagues
{
    internal static class LeaguesEndpointExtensions
    {
        public static RouteHandlerBuilder WithGenderFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithCodeFilter(
                LeaguesQueryParameters.Gender,
                "Gender of the league.",
                LeagueGender.All.Select(gender => gender.Code));
        }

        public static RouteHandlerBuilder WithDivisionFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithParameterDescription(LeaguesQueryParameters.Division, "Division number, 1 is the top division.");
        }

        public static RouteHandlerBuilder WithCountryFilter(this RouteHandlerBuilder builder)
        {
            return builder.WithCodeFilter(
                LeaguesQueryParameters.Country,
                "Country of the league, ISO 3166-1 alpha-2 code in lowercase.",
                LeagueCountry.All.Select(country => country.Code));
        }
    }
}
