using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class PlayerMatchStatisticsEntityTypeConfiguration : IEntityTypeConfiguration<PlayerMatchStatistics>
    {
        public void Configure(EntityTypeBuilder<PlayerMatchStatistics> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReceptionPositivePercent)
                .HasPrecision(5, 2)
                .IsRequired(false);

            builder.Property(x => x.ReceptionPerfectPercent)
                .HasPrecision(5, 2)
                .IsRequired(false);

            builder.Property(x => x.AttackEfficiencyPercent)
                .HasPrecision(5, 2)
                .IsRequired(false);

            builder.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Match)
                .WithMany(x => x.PlayerStatistics)
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
