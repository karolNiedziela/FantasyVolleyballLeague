namespace FantasyVolleyballLeague.Scraper.DataProcessors.Rosters
{
    internal interface IRosterDataProcessor
    {
        Task AcquireAndSaveAsync(CancellationToken cancellationToken);
    }
}
