using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal sealed class SortValidationFilter<TQuery, TEntity> : IEndpointFilter
        where TQuery : class, ISortableQuery<TEntity>
        where TEntity : Entity
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var query = context.Arguments.OfType<TQuery>().FirstOrDefault()
                ?? throw new InvalidOperationException(
                    $"{nameof(SortValidationFilter<TQuery, TEntity>)} requires an endpoint argument of type {typeof(TQuery).Name}.");

            var parsed = TQuery.SortMap.Parse(query.Sort);
            if (!parsed.IsValid)
            {
                var errors = parsed.Errors.ToDictionary(error => error.Key, error => error.Value);

                return TypedResults.ValidationProblem(errors);
            }

            return await next(context);
        }
    }
}
