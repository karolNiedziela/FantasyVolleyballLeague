namespace FantasyVolleyballLeague.DataApi.Features.Players
{
    internal static class PlayersRoutes
    {
        public const string Resource = "/players";

        public const string BySeason = "/seasons/{seasonId:guid}/players";

        public const string Tag = "Players";
    }
}
