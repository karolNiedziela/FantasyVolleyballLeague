using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class MatchEntityTypeConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.Venue, venue =>
            {
                venue.Property(v => v.Name)
                    .HasColumnName("VenueName")
                    .HasMaxLength(200)
                    .IsRequired();

                venue.Property(v => v.City)
                    .HasColumnName("VenueCity")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            builder.OwnsOne(x => x.Officials, officials =>
            {
                officials.Property(o => o.FirstReferee)
                    .HasMaxLength(100)
                    .IsRequired();

                officials.Property(o => o.SecondReferee)
                    .HasMaxLength(100)
                    .IsRequired();

                officials.Property(o => o.Commissioner)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.ExternalId)
                .IsRequired(false);

            builder.HasIndex(x => new { x.ExternalId, x.LeagueId })
                .IsUnique();

            builder.HasOne(x => x.League)
                .WithMany()
                .HasForeignKey(x => x.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Mvp)
                .WithMany()
                .HasForeignKey(x => x.MvpId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HomeTeam)
                .WithMany()
                .HasForeignKey(x => x.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AwayTeam)
                .WithMany()
                .HasForeignKey(x => x.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Sets)
                .WithOne(x => x.Match)
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
