using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PartPurchaseConfiguration : IEntityTypeConfiguration<PartPurchase>
{
    public void Configure(EntityTypeBuilder<PartPurchase> builder)
    {
        builder.ToTable("PartPurchases");

        builder.HasKey(x => x.PartPurchaseId);

        builder.Property(x => x.PartPurchaseId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.PurchaseDate)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.Total)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.PartPurchases)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
