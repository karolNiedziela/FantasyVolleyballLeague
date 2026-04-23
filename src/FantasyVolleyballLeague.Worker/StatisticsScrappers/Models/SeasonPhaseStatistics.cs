namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Models
{
    public sealed record SeasonPhaseStatistics(
        string PhaseName,
        List<SeasonPhaseStage> Stages);
}
