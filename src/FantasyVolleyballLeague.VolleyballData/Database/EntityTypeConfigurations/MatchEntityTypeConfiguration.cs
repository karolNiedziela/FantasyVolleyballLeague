using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.VolleyballData.Database.EntityTypeConfigurations
{
    internal sealed class MatchEntityTypeConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.HasKey(x => x.Id);

            builder.ComplexProperty(x => x.Venue, venue =>
            {
                venue.Property(v => v.Name)
                    .HasColumnName("VenueName")
                    .HasMaxLength(200);

                venue.Property(v => v.City)
                    .HasColumnName("VenueCity")
                    .HasMaxLength(100);
            });

            builder.ComplexProperty(x => x.Officials, officials =>
            {
                officials.Property(o => o.FirstReferee)
                    .HasColumnName("Officials_FirstReferee")
                    .HasMaxLength(100);

                officials.Property(o => o.SecondReferee)
                    .HasColumnName("Officials_SecondReferee")
                    .HasMaxLength(100);

                officials.Property(o => o.Commissioner)
                    .HasColumnName("Officials_Commissioner")
                    .HasMaxLength(100);
            });

            builder.Property(x => x.MatchNumber)
                .HasMaxLength(20);

            builder.HasIndex(x => new { x.ExternalId, x.LeagueId })
                .IsUnique();

            builder.HasOne(x => x.Status)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.StatisticsLevel)
                .WithMany()
                .HasForeignKey(x => x.StatisticsLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.League)
                .WithMany()
                .HasForeignKey(x => x.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Mvp)
                .WithMany()
                .HasForeignKey(x => x.MvpId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HomeRoster)
                .WithMany()
                .HasForeignKey(x => x.HomeRosterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AwayRoster)
                .WithMany()
                .HasForeignKey(x => x.AwayRosterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Sets)
                .WithOne(x => x.Match)
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Round)
                .WithMany()
                .HasForeignKey(x => x.RoundId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
