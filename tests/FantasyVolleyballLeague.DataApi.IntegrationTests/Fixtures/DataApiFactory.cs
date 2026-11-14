using FantasyVolleyballLeague.Contracts;
using FantasyVolleyballLeague.VolleyballData.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures
{
    public sealed class DataApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer =
            new PostgreSqlBuilder($"{DatabaseResource.PostgresImage}:{DatabaseResource.PostgresImageTag}").Build();

        private Respawner? _respawner;

        public async ValueTask InitializeAsync()
        {
            await _postgresContainer.StartAsync();
            await ApplyMigrationsAsync();
            _respawner = await CreateRespawnerAsync();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await _postgresContainer.DisposeAsync();
        }

        internal async Task ResetDatabaseAsync()
        {
            if (_respawner is null)
            {
                throw new InvalidOperationException("The database is not initialized yet.");
            }

            await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                ConfigurationPath.Combine("ConnectionStrings", DatabaseResource.Name),
                _postgresContainer.GetConnectionString());
        }

        private async Task ApplyMigrationsAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>();
            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(() => dbContext.Database.MigrateAsync());
        }

        private async Task<Respawner> CreateRespawnerAsync()
        {
            await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            await connection.OpenAsync();

            return await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = [new Table(HistoryRepository.DefaultTableName), .. GetSeededTables()],
            });
        }

        private List<Table> GetSeededTables()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FantasyVolleyballLeagueDbContext>();
            var designTimeModel = dbContext.GetService<IDesignTimeModel>().Model;

            return designTimeModel.GetEntityTypes()
                .Where(entityType => entityType.GetSeedData().Any())
                .Select(entityType => new Table(entityType.GetTableName()!))
                .ToList();
        }
    }
}
