using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class PlayerEntityTypeConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(p => new { p.GenderId, p.ExternalId })
                .IsUnique()
                .HasFilter("\"ExternalId\" IS NOT NULL");

            builder.HasOne(p => p.Gender)
                .WithMany()
                .HasForeignKey(p => p.GenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
