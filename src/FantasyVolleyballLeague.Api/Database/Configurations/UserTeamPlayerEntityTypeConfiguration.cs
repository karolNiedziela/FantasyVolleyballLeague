using FantasyVolleyballLeague.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.Api.Database.Configurations
{
    internal sealed class UserTeamPlayerEntityTypeConfiguration : IEntityTypeConfiguration<UserTeamPlayer>
    {
        public void Configure(EntityTypeBuilder<UserTeamPlayer> builder)
        {
            builder.HasKey(x => new { x.UserTeamId, x.PlayerId });

            builder.HasOne(x => x.UserTeam)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.UserTeamId)
                .OnDelete(DeleteBehavior.Cascade);
           
            builder.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
