using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal static class SortQueryableExtensions
    {
        public static IQueryable<TEntity> ApplySort<TQuery, TEntity>(this IQueryable<TEntity> source, TQuery query)
            where TQuery : ISortableQuery<TEntity>
            where TEntity : Entity
        {
            return TQuery.SortMap.Parse(query.Sort).Apply(source);
        }
    }
}
