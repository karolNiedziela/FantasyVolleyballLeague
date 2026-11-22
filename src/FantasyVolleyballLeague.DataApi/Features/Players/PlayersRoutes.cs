namespace FantasyVolleyballLeague.DataApi.Features.Players
{
    internal static class PlayersRoutes
    {
        public const string Resource = "/players";

        public const string ResourceById = Resource + "/{id:guid}";

        public const string RostersOfPlayer = Resource + "/{playerId:guid}/rosters";

        public const string BySeason = "/seasons/{seasonId:guid}/players";

        public const string Tag = "Players";
    }
}
