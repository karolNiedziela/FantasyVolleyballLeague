using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class SetScoreEntityTypeConfiguration : IEntityTypeConfiguration<SetScore>
    {
        public void Configure(EntityTypeBuilder<SetScore> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.MatchId, x.SetNumber })
                .IsUnique();
        }
    }
}
