using FantasyVolleyballLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Infrastructure.Database.Configurations
{
    internal sealed class SeasonConfiguration : IEntityTypeConfiguration<Season>
    {
        public void Configure(EntityTypeBuilder<Season> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(x => x.Name).HasMaxLength(20).IsRequired();
            builder.Property(x => x.StartYear).IsRequired();
            builder.Property(x => x.EndYear).IsRequired();
        }
    }
}
