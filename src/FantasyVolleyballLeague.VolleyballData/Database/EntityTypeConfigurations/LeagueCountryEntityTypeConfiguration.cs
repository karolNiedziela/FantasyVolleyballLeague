using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.VolleyballData.Database.EntityTypeConfigurations
{
    internal sealed class LeagueCountryEntityTypeConfiguration : IEntityTypeConfiguration<LeagueCountry>
    {
        public void Configure(EntityTypeBuilder<LeagueCountry> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .HasMaxLength(2)
                .IsRequired();

            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasData(LeagueCountry.All);
        }
    }
}
