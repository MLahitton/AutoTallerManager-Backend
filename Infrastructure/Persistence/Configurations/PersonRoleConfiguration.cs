using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PersonRoleConfiguration : IEntityTypeConfiguration<PersonRole>
{
    public void Configure(EntityTypeBuilder<PersonRole> builder)
    {
        builder.ToTable("PersonRoles");

        builder.HasKey(x => x.PersonRoleId);

        builder.Property(x => x.PersonRoleId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.PersonId, x.RoleId })
            .IsUnique();

        builder.HasOne(x => x.Person)
            .WithMany(x => x.PersonRoles)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.PersonRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
