using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class SeasonEntityTypeConfiguration : IEntityTypeConfiguration<Season>
    {
        public void Configure(EntityTypeBuilder<Season> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => new { x.LeagueId, x.StartYear, x.EndYear }).IsUnique();
            
            builder.HasOne(x => x.League)
                .WithMany()
                .HasForeignKey(x => x.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
