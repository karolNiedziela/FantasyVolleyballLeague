using Microsoft.AspNetCore.Http.Metadata;

namespace FantasyVolleyballLeague.DataApi.Common.API
{
    internal static class ValidationProblemEndpointExtensions
    {
        public static RouteHandlerBuilder ProducesValidationProblemOnce(this RouteHandlerBuilder builder)
        {
            builder.Add(endpointBuilder =>
            {
                var alreadyDocumented = endpointBuilder.Metadata
                    .OfType<IProducesResponseTypeMetadata>()
                    .Any(metadata => metadata.StatusCode == StatusCodes.Status400BadRequest);

                if (!alreadyDocumented)
                {
                    endpointBuilder.Metadata.Add(new ProducesResponseTypeMetadata(
                        StatusCodes.Status400BadRequest,
                        typeof(HttpValidationProblemDetails),
                        ["application/problem+json"]));
                }
            });

            return builder;
        }
    }
}
