using FantasyVolleyballLeague.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Database.Configurations
{
    internal sealed class PlayerStatisticsEntityTypeConfiguration : IEntityTypeConfiguration<PlayerStatistics>
    {
        public void Configure(EntityTypeBuilder<PlayerStatistics> builder)
        {
            builder.HasKey(ps => ps.Id);

            builder.Property(ps => ps.PointsScored)
                .IsRequired();

            builder.Property(ps => ps.Blocks)
                .IsRequired();

            builder.Property(ps => ps.Aces)
                .IsRequired();

            builder.Property(ps => ps.Errors)
                .IsRequired();
        }
    }
}
