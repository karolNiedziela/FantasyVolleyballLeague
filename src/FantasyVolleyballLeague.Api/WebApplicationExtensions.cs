using FantasyVolleyballLeague.Api.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FantasyVolleyballLeague.Api
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

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                await context.Database.MigrateAsync();

                await transaction.CommitAsync();
            });
        }
    }
}
