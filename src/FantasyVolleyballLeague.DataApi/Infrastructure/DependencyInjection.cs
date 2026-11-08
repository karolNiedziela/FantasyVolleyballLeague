using FantasyVolleyballLeague.Api.Infrastructure.Database;

namespace FantasyVolleyballLeague.Api.Infrastructure
{
    internal static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDatabase();

            return services;
        }
    }
}
