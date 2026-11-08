using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.VolleyballData.Database.EntityTypeConfigurations
{
    internal sealed class MatchStatusEntityTypeConfiguration : IEntityTypeConfiguration<MatchStatus>
    {
        public void Configure(EntityTypeBuilder<MatchStatus> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasData(MatchStatus.All);
        }
    }
}
