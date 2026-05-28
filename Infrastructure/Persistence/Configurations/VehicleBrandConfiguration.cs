using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VehicleBrandConfiguration : IEntityTypeConfiguration<VehicleBrand>
{
    public void Configure(EntityTypeBuilder<VehicleBrand> builder)
    {
        builder.ToTable("VehicleBrands");

        builder.HasKey(x => x.BrandId);

        builder.Property(x => x.BrandId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BrandName)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasIndex(x => x.BrandName)
            .IsUnique();
    }
}
