namespace FantasyVolleyballLeague.Worker.StatisticsExtractor.Models
{
    public sealed record PlayerMatchStatistics(
      int Number,
      string Name,
      int PointsTotal,
      int ServeTotal,
      int ServeErrors,
      int Aces,
      int ReceptionTotal,
      int ReceptionErrors,
      string ReceptionPositivePercent,
      string ReceptionPerfectPercent,
      int AttackTotal,
      int AttackErrors,
      int AttackBlocked,
      int AttackPoints,
      string AttackEfficiencyPercent,
      int Blocks
  );
}
