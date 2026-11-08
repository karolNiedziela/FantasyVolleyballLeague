using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.VolleyballData.Database.EntityTypeConfigurations
{
    internal sealed class RosterPlayerEntityTypeConfiguration : IEntityTypeConfiguration<RosterPlayer>
    {
        public void Configure(EntityTypeBuilder<RosterPlayer> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.RosterId, x.PlayerId })
                .IsUnique();

            builder.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
