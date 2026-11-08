using System.Reflection;

namespace FantasyVolleyballLeague.Api.Common.Endpoints
{
    internal static class EndpointExtensions
    {
        public static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            var endpointTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in endpointTypes)
            {
                services.AddTransient(typeof(IEndpoint), type);
            }

            return services;
        }

        public static WebApplication MapEndpoints(this WebApplication app)
        {
            var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(app);
            }

            return app;
        }
    }
}
