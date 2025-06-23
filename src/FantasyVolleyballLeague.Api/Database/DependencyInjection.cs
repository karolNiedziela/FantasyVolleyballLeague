namespace FantasyVolleyballLeague.Api.Database
{
    internal static class DependencyInjection
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDbContext>(serviceProvider => serviceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>());

            services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>());

            return services;
        }
    }
}
