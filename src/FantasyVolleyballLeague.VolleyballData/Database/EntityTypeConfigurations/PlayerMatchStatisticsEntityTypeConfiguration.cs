using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.VolleyballData.Database.EntityTypeConfigurations
{
    internal sealed class PlayerMatchStatisticsEntityTypeConfiguration : IEntityTypeConfiguration<PlayerMatchStatistics>
    {
        public void Configure(EntityTypeBuilder<PlayerMatchStatistics> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.MatchId, x.PlayerId })
                .IsUnique();

            builder.Property(x => x.ReceptionPositivePercent)
                .HasPrecision(5, 2);

            builder.Property(x => x.ReceptionPerfectPercent)
                .HasPrecision(5, 2);

            builder.Property(x => x.AttackEfficiencyPercent)
                .HasPrecision(5, 2);

            builder.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Roster)
                .WithMany()
                .HasForeignKey(x => x.RosterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Match)
                .WithMany(x => x.PlayerStatistics)
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
