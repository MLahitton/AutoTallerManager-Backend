using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.RoleId);

        builder.Property(x => x.RoleId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RoleName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.RoleName)
            .IsUnique();
    }
}
