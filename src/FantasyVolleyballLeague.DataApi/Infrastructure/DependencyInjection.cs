using FantasyVolleyballLeague.VolleyballData.Database;

namespace FantasyVolleyballLeague.DataApi.Infrastructure
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
