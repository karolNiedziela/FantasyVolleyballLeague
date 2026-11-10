using FantasyVolleyballLeague.DataApi.Common.Versioning;
using System.Reflection;

namespace FantasyVolleyballLeague.DataApi.Common.Endpoints
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

            var v1 = app.NewVersionedApi()
                .MapGroup("/api/v{version:apiVersion}")
                .HasApiVersion(ApiVersions.V1);

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(v1);
            }

            return app;
        }
    }
}
