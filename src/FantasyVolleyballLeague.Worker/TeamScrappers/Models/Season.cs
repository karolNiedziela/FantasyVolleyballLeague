namespace FantasyVolleyballLeague.Worker.TeamScrappers.Models
{
    public sealed record Season(
        string Name,
        int StartYear,
        int EndYear,
        Uri Url);
}
