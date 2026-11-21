namespace FantasyVolleyballLeague.DataApi.Features.Seasons
{
    internal static class SeasonsRoutes
    {
        public const string Resource = "/seasons";

        public const string ResourceById = Resource + "/{id:guid}";

        public const string ByLeague = "/leagues/{leagueId:guid}/seasons";

        public const string Tag = "Seasons";
    }
}
