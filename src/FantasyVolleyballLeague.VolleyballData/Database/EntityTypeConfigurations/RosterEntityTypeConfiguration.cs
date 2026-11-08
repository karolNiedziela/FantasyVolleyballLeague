using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class RosterEntityTypeConfiguration : IEntityTypeConfiguration<Roster>
    {
        public void Configure(EntityTypeBuilder<Roster> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasIndex(r => new { r.Name, r.LeagueId, r.SeasonId }).IsUnique();

            builder.HasIndex(r => new { r.SeasonId, r.ExternalId })
                .IsUnique()
                .HasFilter("\"ExternalId\" IS NOT NULL");

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(r => r.League)
                .WithMany()
                .HasForeignKey(r => r.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Season)
                .WithMany()
                .HasForeignKey(r => r.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.Players)
                .WithOne(rp => rp.Roster)
                .HasForeignKey(rp => rp.RosterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
