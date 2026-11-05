namespace FantasyVolleyballLeague.Importer
{
    internal interface IImportStep
    {
        int Order { get; }

        string Name { get; }

        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
