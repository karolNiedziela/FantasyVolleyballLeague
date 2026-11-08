namespace FantasyVolleyballLeague.Api.Infrastructure.Database
{
    internal static class DependencyInjection
    {
        internal static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddScoped<IDbContext>(sp => sp.GetRequiredService<FantasyVolleyballLeagueDbContext>());

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FantasyVolleyballLeagueDbContext>());

            return services;
        }
    }
}
