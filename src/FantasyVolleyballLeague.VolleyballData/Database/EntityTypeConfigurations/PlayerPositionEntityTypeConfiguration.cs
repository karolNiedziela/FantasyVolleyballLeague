using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class PlayerPositionEntityTypeConfiguration : IEntityTypeConfiguration<PlayerPosition>
    {
        public void Configure(EntityTypeBuilder<PlayerPosition> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Abbreviation)
                .IsRequired()
                .HasMaxLength(3);

            builder.HasData(PlayerPosition.All);
        }
    }
}
