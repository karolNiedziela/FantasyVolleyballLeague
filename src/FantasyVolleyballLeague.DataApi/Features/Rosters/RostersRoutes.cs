namespace FantasyVolleyballLeague.DataApi.Features.Rosters
{
    internal static class RostersRoutes
    {
        public const string Resource = "/rosters";

        public const string ResourceById = Resource + "/{id:guid}";

        public const string BySeason = "/seasons/{seasonId:guid}/rosters";

        public const string Tag = "Rosters";
    }
}
