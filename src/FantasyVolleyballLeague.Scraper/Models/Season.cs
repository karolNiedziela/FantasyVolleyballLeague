using FantasyVolleyballLeague.Scraper.Models;

namespace FantasyVolleyballLeague.Scraper.Models
{
    internal sealed record Season(
        string Name,
        int StartYear,
        int EndYear,
        int? Tour,
        Uri Url)
    {
        public SeasonReference Reference => new(StartYear, EndYear, Tour);
    }
}
