namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Models
{
    public sealed record SeasonPhaseStage(
        int StageNumber,
        string StageName,
        List<SeasonPhaseRound> Rounds);
}
