namespace FantasyVolleyballLeague.Worker.TeamScrappers.Models
{
    public sealed record PlayerInformation(
        string Fullname,
        string Position,
        string DateOfBirth,
        int Height,
        int Weight,
        int AttackRange,
        int ShirtNumber,
        string LinkToDetailsSuffix);
}
