using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Database
{
    public sealed class UnitOfWorkTests : IntegrationTestBase
    {
        public UnitOfWorkTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_Should_CommitTheChanges_When_TheOperationCompletes()
        {
            await ExecuteDbContextAsync(dbContext => dbContext.ExecuteInTransactionAsync(
                async token =>
                {
                    dbContext.Add(CreateLeague());
                    await dbContext.SaveChangesAsync(token);
                },
                TestContext.Current.CancellationToken));

            (await CountLeaguesAsync()).ShouldBe(1);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_Should_RollBackAndRethrow_When_TheOperationThrows()
        {
            await Should.ThrowAsync<InvalidOperationException>(() => ExecuteDbContextAsync(dbContext => dbContext.ExecuteInTransactionAsync(
                async token =>
                {
                    dbContext.Add(CreateLeague());
                    await dbContext.SaveChangesAsync(token);
                    throw new InvalidOperationException("The operation failed after saving.");
                },
                TestContext.Current.CancellationToken)));

            (await CountLeaguesAsync()).ShouldBe(0);
        }

        private static League CreateLeague()
        {
            return new League("Men's league", 1, LeagueCountry.Poland.Id, LeagueGender.Men.Id);
        }

        private Task<int> CountLeaguesAsync()
        {
            return ExecuteDbContextAsync(dbContext => dbContext.Set<League>().CountAsync(TestContext.Current.CancellationToken));
        }
    }
}
