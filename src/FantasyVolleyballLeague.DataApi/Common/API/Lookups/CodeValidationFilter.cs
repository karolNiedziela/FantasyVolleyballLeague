namespace FantasyVolleyballLeague.DataApi.Common.API.Lookups
{
    internal sealed class CodeValidationFilter : IEndpointFilter
    {
        private readonly string _parameterName;
        private readonly IReadOnlyCollection<string> _codes;

        public CodeValidationFilter(string parameterName, IReadOnlyCollection<string> codes)
        {
            _parameterName = parameterName;
            _codes = codes;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var values = context.HttpContext.Request.Query[_parameterName];
            var unknownValues = values
                .Where(value => !_codes.Contains(value, StringComparer.Ordinal))
                .ToList();

            if (unknownValues.Count > 0)
            {
                var errors = unknownValues
                    .Select(value => $"Unknown value '{value}'. Allowed values: {string.Join(", ", _codes)}.")
                    .ToArray();

                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [_parameterName] = errors });
            }

            return await next(context);
        }
    }
}
