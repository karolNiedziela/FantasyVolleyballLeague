namespace FantasyVolleyballLeague.DataApi.Common.Handlers
{
    internal interface IQueryHandler<in TQuery, TResponse>
    {
        Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
