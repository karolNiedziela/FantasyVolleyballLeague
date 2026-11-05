namespace FantasyVolleyballLeague.Scraper.DataProcessors.Statistics
{
    internal interface IStatisticsDataProcessor
    {
        Task AcquireAndSaveAsync(CancellationToken cancellationToken);
    }
}
