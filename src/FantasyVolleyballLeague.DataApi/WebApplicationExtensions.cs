using FantasyVolleyballLeague.VolleyballData.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FantasyVolleyballLeague.DataApi
{
    internal static class WebApplicationExtensions
    {
        public static async Task ConfigureDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>();

            await EnsureDatabaseAsync(context);
            await RunMigrationsAsync(context);
        }

        private static async Task EnsureDatabaseAsync(FantasyVolleyballLeagueDbContext context)
        {
            var dbCreator = context.GetService<IRelationalDatabaseCreator>();

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                if (!await dbCreator.ExistsAsync())
                {
                    await dbCreator.CreateAsync();
                }
            });
        }

        private static async Task RunMigrationsAsync(FantasyVolleyballLeagueDbContext context)
        {
            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(() => context.Database.MigrateAsync());
        }
    }
}
