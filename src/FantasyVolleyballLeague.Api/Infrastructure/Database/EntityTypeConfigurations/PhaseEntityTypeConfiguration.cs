using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class PhaseEntityTypeConfiguration : IEntityTypeConfiguration<Phase>
    {
        public void Configure(EntityTypeBuilder<Phase> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => new {x.SeasonId, x.Name}).IsUnique();

            builder.HasOne(t => t.Season)
                .WithMany()
                .HasForeignKey(t => t.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
