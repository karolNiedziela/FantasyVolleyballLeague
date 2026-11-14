using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.VolleyballData.Database.EntityTypeConfigurations
{
    internal sealed class LeagueEntityTypeConfiguration : IEntityTypeConfiguration<League>
    {
        public void Configure(EntityTypeBuilder<League> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(100)
                .UseCollation("pl-x-icu");

            builder.ToTable(t => t.HasCheckConstraint("CK_Leagues_DivisionNumber", "\"DivisionNumber\" >= 1"));

            builder.HasIndex(l => new { l.CountryId, l.GenderId, l.DivisionNumber })
                .IsUnique();

            builder.HasOne(l => l.Country)
                .WithMany()
                .HasForeignKey(l => l.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Gender)
                .WithMany()
                .HasForeignKey(l => l.GenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
