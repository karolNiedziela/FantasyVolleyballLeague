using FantasyVolleyballLeague.DataApi.Infrastructure;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

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
        public void UnorderedPagingIsOnlyAWarningWithoutTheExtension()
        {
            using var context = CreateContext(throwOnUnorderedPaging: false);

            Should.NotThrow(() => context.Set<League>().Skip(25).Take(25).ToQueryString());
        }

        private static FantasyVolleyballLeagueDbContext CreateContext(bool throwOnUnorderedPaging)
        {
            var options = new DbContextOptionsBuilder<FantasyVolleyballLeagueDbContext>()
                .UseNpgsql("Host=localhost;Database=unused");

            if (throwOnUnorderedPaging)
            {
                options.ThrowOnUnorderedPaging();
            }

            return new FantasyVolleyballLeagueDbContext(options.Options);
        }
    }
}
