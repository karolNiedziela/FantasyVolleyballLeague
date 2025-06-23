using FantasyVolleyballLeague.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Database.Configurations
{
    internal sealed class TeamEntityTypeConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(x => x.League)
                .WithMany()
                .HasForeignKey(x => x.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
