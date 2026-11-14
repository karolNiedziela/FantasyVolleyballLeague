using Microsoft.OpenApi;

namespace FantasyVolleyballLeague.DataApi.Common.API
{
    internal static class OpenApiParameterExtensions
    {
        public static RouteHandlerBuilder WithParameterDescription(this RouteHandlerBuilder builder, string parameterName, string description)
        {
            return builder.AddOpenApiOperationTransformer((operation, _, _) =>
            {
                var parameter = operation.Parameters?.FirstOrDefault(
                    candidate => string.Equals(candidate.Name, parameterName, StringComparison.Ordinal));

                if (parameter is OpenApiParameter openApiParameter)
                {
                    openApiParameter.Description = description;
                }

                return Task.CompletedTask;
            });
        }
    }
}
