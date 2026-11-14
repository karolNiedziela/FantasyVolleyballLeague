using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal static class SortingEndpointExtensions
    {
        public static RouteHandlerBuilder WithSorting<TQuery, TEntity>(this RouteHandlerBuilder builder)
            where TQuery : class, ISortableQuery<TEntity>
            where TEntity : Entity
        {
            builder.AddEndpointFilter<SortValidationFilter<TQuery, TEntity>>();

            var sortMap = TQuery.SortMap;
            var defaultSort = sortMap.DefaultSort is null ? string.Empty : $" Default: {sortMap.DefaultSort}.";
            builder.WithParameterDescription(
                SortingQueryParameters.Sort,
                $"Comma-separated fields, prefix a field with - for descending order. Fields: {string.Join(", ", sortMap.Fields)}.{defaultSort}");

            return builder.ProducesValidationProblemOnce();
        }
    }
}
