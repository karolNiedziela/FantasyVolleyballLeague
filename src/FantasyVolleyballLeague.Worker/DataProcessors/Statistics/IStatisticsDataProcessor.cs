namespace FantasyVolleyballLeague.Worker.DataProcessors.Statistics
{
    public interface IStatisticsDataProcessor
    {
        Task AcquireAndSaveAsync();
    }
}
