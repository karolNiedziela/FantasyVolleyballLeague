using FantasyVolleyballLeague.VolleyballData.Database;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    public sealed class MigrationTests
    {
        [Fact]
        public void ModelHasNoChangesMissingFromTheMigrations()
        {
            var options = new DbContextOptionsBuilder<FantasyVolleyballLeagueDbContext>()
                .UseNpgsql("Host=unused.invalid;Database=unused")
                .Options;
            using var dbContext = new FantasyVolleyballLeagueDbContext(options);

            dbContext.Database.HasPendingModelChanges().ShouldBeFalse();
        }
    }
}
