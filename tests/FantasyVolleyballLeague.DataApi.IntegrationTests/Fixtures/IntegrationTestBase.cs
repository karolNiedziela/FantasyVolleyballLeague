using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures
{
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        private readonly DataApiFactory _factory;

        protected IntegrationTestBase(DataApiFactory factory)
        {
            _factory = factory;
            Client = factory.CreateClient();
        }

        protected HttpClient Client { get; }

        public async ValueTask InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
        }

        public ValueTask DisposeAsync()
        {
            Client.Dispose();
            GC.SuppressFinalize(this);

            return ValueTask.CompletedTask;
        }

        private protected async Task SeedAsync(params IEnumerable<Entity> entities)
        {
            await ExecuteDbContextAsync(async dbContext =>
            {
                dbContext.AddRange(entities);
                await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            });
        }

        private protected async Task ExecuteDbContextAsync(Func<FantasyVolleyballLeagueDbContext, Task> action)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>();

            await action(dbContext);
        }

        private protected async Task<TResult> ExecuteDbContextAsync<TResult>(Func<FantasyVolleyballLeagueDbContext, Task<TResult>> action)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>();

            return await action(dbContext);
        }

        private protected Task ResetDatabaseAsync()
        {
            return _factory.ResetDatabaseAsync();
        }
    }
}
