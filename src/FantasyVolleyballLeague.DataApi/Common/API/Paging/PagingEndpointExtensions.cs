namespace FantasyVolleyballLeague.DataApi.Common.API.Paging
{
    internal static class PagingEndpointExtensions
    {
        public static RouteHandlerBuilder WithPaging(this RouteHandlerBuilder builder)
        {
            builder.AddEndpointFilter<PagingValidationFilter>();
            builder.WithParameterDescription(PagingQueryParameters.Offset, "Number of items to skip.");
            builder.WithParameterDescription(PagingQueryParameters.Limit, $"Page size, from 1 to {PagingDefaults.MaxLimit}.");

            return builder.ProducesValidationProblemOnce();
        }
    }
}
