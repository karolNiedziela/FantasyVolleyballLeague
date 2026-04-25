namespace FantasyVolleyballLeague.Worker.TeamScrappers.Models
{
    public sealed record PlayerProfile(
        string FullName,
        string Position,
        string DateOfBirth,
        int Height,
        int Weight,
        int AttackReach,
        int ShirtNumber,
        string ProfileUrlSuffix);
}
