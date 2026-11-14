namespace FantasyVolleyballLeague.DataApi.Common.API.Paging
{
    internal sealed class PagingValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            // A missing argument is a wiring mistake (for example a query that is not bound with [AsParameters]).
            // Failing here is better than letting the request through without paging validation.
            var query = context.Arguments.OfType<IPagedQuery>().FirstOrDefault()
                ?? throw new InvalidOperationException(
                    $"{nameof(PagingValidationFilter)} requires an endpoint argument that implements {nameof(IPagedQuery)}.");

            var errors = Validate(query);
            if (errors.Count > 0)
            {
                return TypedResults.ValidationProblem(errors);
            }

            return await next(context);
        }

        private static Dictionary<string, string[]> Validate(IPagedQuery query)
        {
            var errors = new Dictionary<string, string[]>();

            if (query.Offset < 0)
            {
                errors[PagingQueryParameters.Offset] = ["The offset must be 0 or greater."];
            }

            if (query.Limit < 1 || query.Limit > PagingDefaults.MaxLimit)
            {
                errors[PagingQueryParameters.Limit] = [$"The limit must be between 1 and {PagingDefaults.MaxLimit}."];
            }

            return errors;
        }
    }
}
