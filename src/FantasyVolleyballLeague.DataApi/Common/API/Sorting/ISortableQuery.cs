using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal interface ISortableQuery<TEntity> where TEntity : Entity
    {
        static abstract SortMap<TEntity> SortMap { get; }

        string? Sort { get; }
    }
}
