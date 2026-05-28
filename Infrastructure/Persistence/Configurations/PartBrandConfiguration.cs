using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PartBrandConfiguration : IEntityTypeConfiguration<PartBrand>
{
    public void Configure(EntityTypeBuilder<PartBrand> builder)
    {
        builder.ToTable("PartBrands");

        builder.HasKey(x => x.PartBrandId);

        builder.Property(x => x.PartBrandId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
