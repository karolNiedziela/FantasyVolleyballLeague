namespace FantasyVolleyballLeague.Worker.TeamScrappers.Models
{
    public sealed record SeasonInformation(
        string Name,
        int StartYear, 
        int EndYear, 
        Uri Url);
    
}
