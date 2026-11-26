using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FantasyVolleyballLeague.VolleyballData.Database.EntityTypeConfigurations
{
    internal sealed class StaffMemberEntityTypeConfiguration : IEntityTypeConfiguration<StaffMember>
    {
        public void Configure(EntityTypeBuilder<StaffMember> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.FullName)
                .IsRequired()
                .HasMaxLength(100)
                .UseCollation("pl-x-icu");

            builder.Property(s => s.NormalizedName)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(s => s.NormalizedName)
                .IsUnique();

            builder.HasIndex(s => s.NormalizedName, "IX_StaffMember_NormalizedName_Trigram")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
        }
    }
}
