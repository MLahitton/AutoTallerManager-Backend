using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(x => x.SupplierId);

        builder.Property(x => x.SupplierId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.TaxId)
            .HasMaxLength(30);

        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasMaxLength(120);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(true);

        builder.HasIndex(x => x.TaxId)
            .IsUnique();
    }
}
