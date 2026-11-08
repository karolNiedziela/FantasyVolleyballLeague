using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class RoundEntityTypeConfiguration : IEntityTypeConfiguration<Round>
    {
        public void Configure(EntityTypeBuilder<Round> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Stage)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => new { x.PhaseId, x.Stage, x.Name }).IsUnique();

            builder.HasOne(x => x.Phase)
                .WithMany()
                .HasForeignKey(x => x.PhaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
