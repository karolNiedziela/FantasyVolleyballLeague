using FantasyVolleyballLeague.DataApi.Infrastructure;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Infrastructure
{
    public sealed class DbContextOptionsExtensionsTests
    {
        [Fact]
        public void ThrowOnUnorderedPagingRejectsSkipAndTakeWithoutOrderBy()
        {
            using var context = CreateContext(throwOnUnorderedPaging: true);

            Should.Throw<InvalidOperationException>(() => context.Set<League>().Skip(25).Take(25).ToQueryString());
        }

        [Fact]
        public void ThrowOnUnorderedPagingAllowsOrderedPaging()
        {
            using var context = CreateContext(throwOnUnorderedPaging: true);

            Should.NotThrow(() => context.Set<League>().OrderBy(league => league.Id).Skip(25).Take(25).ToQueryString());
        }

        [Fact]
        public void ThrowOnUnorderedPagingAlsoRejectsABareTake()
        {
            using var context = CreateContext(throwOnUnorderedPaging: true);
            var id = Guid.CreateVersion7();

            Should.Throw<InvalidOperationException>(() => context.Set<League>().Where(league => league.Id == id).Take(1).ToQueryString());
        }

        [Fact]
        public void ThrowOnUnorderedPagingAllowsFirstOrDefaultLookups()
        {         
            using var context = CreateContext(throwOnUnorderedPaging: true, "Host=127.0.0.1;Port=1;Database=unused;Timeout=1");
            var id = Guid.CreateVersion7();

            var exception = Should.Throw<Exception>(() => context.Set<League>().FirstOrDefault(league => league.Id == id));

            exception.Message.ShouldNotContain(nameof(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
        }

        [Fact]
        public void UnorderedPagingIsOnlyAWarningWithoutTheExtension()
        {
            using var context = CreateContext(throwOnUnorderedPaging: false);

            Should.NotThrow(() => context.Set<League>().Skip(25).Take(25).ToQueryString());
        }

        private static FantasyVolleyballLeagueDbContext CreateContext(
            bool throwOnUnorderedPaging,
            string connectionString = "Host=localhost;Database=unused")
        {
            var options = new DbContextOptionsBuilder<FantasyVolleyballLeagueDbContext>()
                .UseNpgsql(connectionString);

            if (throwOnUnorderedPaging)
            {
                options.ThrowOnUnorderedPaging();
            }

            return new FantasyVolleyballLeagueDbContext(options.Options);
        }
    }
}
