using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyVolleyballLeague.Infrastructure.Database
{
    internal static class DependencyInjection
    {
        internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDbContext>(serviceProvider => serviceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>());

            services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>());

            return services;
        }
    }
}
