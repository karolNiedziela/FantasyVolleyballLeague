namespace FantasyVolleyballLeague.Worker.DataProcessors.Teams
{
    public interface ITeamDataProcessor 
    {
        Task AcquireAndSaveAsync();
    }
}
