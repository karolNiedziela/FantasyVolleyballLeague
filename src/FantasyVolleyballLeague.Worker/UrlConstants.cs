namespace FantasyVolleyballLeague.Worker
{
    public static class UrlConstants
    {
        public const string PlusLiga = "https://www.plusliga.pl";

        public const string PlusLigaTeams = "https://www.plusliga.pl/teams.html";

        public const string PlusLigaGames = "https://www.plusliga.pl/games.html";

#pragma warning disable S1075 // URIs should not be hardcoded
        public const string PlusLigaMatchDetailsUrl = "https://www.plusliga.pl/games/action/show/id/";
#pragma warning restore S1075 // URIs should not be hardcoded
    }
}
