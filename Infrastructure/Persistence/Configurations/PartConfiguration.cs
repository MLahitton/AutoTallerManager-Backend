using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.ToTable("Parts");

        builder.HasKey(x => x.PartId);

        builder.Property(x => x.PartId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Stock)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.MinimumStock)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(x => x.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasOne(x => x.PartCategory)
            .WithMany(x => x.Parts)
            .HasForeignKey(x => x.PartCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PartBrand)
            .WithMany(x => x.Parts)
            .HasForeignKey(x => x.PartBrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
