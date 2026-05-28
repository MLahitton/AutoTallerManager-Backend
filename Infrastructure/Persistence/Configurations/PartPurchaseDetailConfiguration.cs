using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PartPurchaseDetailConfiguration : IEntityTypeConfiguration<PartPurchaseDetail>
{
    public void Configure(EntityTypeBuilder<PartPurchaseDetail> builder)
    {
        builder.ToTable("PartPurchaseDetails");

        builder.HasKey(x => x.PartPurchaseDetailId);

        builder.Property(x => x.PartPurchaseDetailId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.HasIndex(x => new { x.PartPurchaseId, x.PartId })
            .IsUnique();

        builder.HasOne(x => x.PartPurchase)
            .WithMany(x => x.PartPurchaseDetails)
            .HasForeignKey(x => x.PartPurchaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Part)
            .WithMany(x => x.PartPurchaseDetails)
            .HasForeignKey(x => x.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
