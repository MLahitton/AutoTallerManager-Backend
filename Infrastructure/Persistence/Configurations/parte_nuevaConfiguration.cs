using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class parte_nuevaConfiguration : IEntityTypeConfiguration<parte_nueva>
{
    public void Configure(EntityTypeBuilder<parte_nueva> builder)
    {
        builder.ToTable("parte_nueva");

        builder.HasKey(x => x.parte_nuevaId);

        builder.Property(x => x.parte_nuevaId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
