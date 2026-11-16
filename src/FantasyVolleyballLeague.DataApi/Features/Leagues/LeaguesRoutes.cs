namespace FantasyVolleyballLeague.DataApi.Features.Leagues
{
    internal static class LeaguesRoutes
    {
        public const string Resource = "/leagues";

        public const string ResourceById = Resource + "/{id:guid}";

        public const string Tag = "Leagues";
    }
}
