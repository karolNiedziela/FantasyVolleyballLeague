namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Models
{
    public sealed record SeasonPhaseRound(
        int RoundNumber,
        string? RoundName,
        List<MatchRecord> Matches);
}
