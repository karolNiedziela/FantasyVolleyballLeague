using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace FantasyVolleyballLeague.DataApi.Common.API.Lookups
{
    internal static class CodeFilterEndpointExtensions
    {
        /// <summary>
        /// Accepts only the given lookup codes in the query parameter (400 otherwise) and lists them as an enum in OpenAPI.
        /// </summary>
        public static RouteHandlerBuilder WithCodeFilter(
            this RouteHandlerBuilder builder,
            string parameterName,
            string description,
            IEnumerable<string> codes)
        {
            var allowedCodes = codes.ToList();

            builder.AddEndpointFilter(new CodeValidationFilter(parameterName, allowedCodes));
            builder.WithParameterDescription(parameterName, $"{description} Values: {string.Join(", ", allowedCodes)}.");
            builder.AddOpenApiOperationTransformer((operation, _, _) =>
            {
                var parameter = operation.Parameters?.FirstOrDefault(
                    candidate => string.Equals(candidate.Name, parameterName, StringComparison.Ordinal));

                if (parameter?.Schema is OpenApiSchema schema)
                {
                    schema.Enum = allowedCodes.Select(code => (JsonNode)JsonValue.Create(code)).ToList();
                }

                return Task.CompletedTask;
            });

            return builder.ProducesValidationProblemOnce();
        }
    }
}
