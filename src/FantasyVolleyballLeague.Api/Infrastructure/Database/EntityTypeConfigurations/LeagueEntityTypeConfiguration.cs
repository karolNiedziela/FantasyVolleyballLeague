using FantasyVolleyballLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.EntityTypeConfigurations
{
    internal sealed class LeagueEntityTypeConfiguration : IEntityTypeConfiguration<League>
    {
        public void Configure(EntityTypeBuilder<League> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.ToTable(t => t.HasCheckConstraint("CK_Leagues_DivisionNumber", "\"DivisionNumber\" >= 1"));

            builder.HasOne(l => l.Country)
                .WithMany()
                .HasForeignKey(l => l.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
