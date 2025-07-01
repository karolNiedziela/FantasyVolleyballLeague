namespace FantasyVolleyballLeague.Worker.StatisticsScrapper.Models
{
    public sealed record StatisticsMatchDetails(
        string FirstTeamName,
        string SecondTeamName,
        string FinalScore,
        int TotalSets,
        List<string> SetResults);      
}
