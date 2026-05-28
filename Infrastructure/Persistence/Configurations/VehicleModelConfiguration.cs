using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> builder)
    {
        builder.ToTable("VehicleModels");

        builder.HasKey(x => x.ModelId);

        builder.Property(x => x.ModelId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ModelName)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasIndex(x => new { x.BrandId, x.ModelName })
            .IsUnique();

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.VehicleModels)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
