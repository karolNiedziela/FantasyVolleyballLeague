using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.VolleyballData.Database.EntityTypeConfigurations
{
    internal sealed class RosterStaffMemberEntityTypeConfiguration : IEntityTypeConfiguration<RosterStaffMember>
    {
        public void Configure(EntityTypeBuilder<RosterStaffMember> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.RosterId, x.StaffMemberId, x.RoleId })
                .IsUnique();

            builder.HasOne(x => x.StaffMember)
                .WithMany()
                .HasForeignKey(x => x.StaffMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
