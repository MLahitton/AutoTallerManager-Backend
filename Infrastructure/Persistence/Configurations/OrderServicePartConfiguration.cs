using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderServicePartConfiguration : IEntityTypeConfiguration<OrderServicePart>
{
    public void Configure(EntityTypeBuilder<OrderServicePart> builder)
    {
        builder.ToTable("OrderServiceParts");

        builder.HasKey(x => x.OrderServicePartId);

        builder.Property(x => x.OrderServicePartId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.AppliedUnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.CustomerApproved)
            .HasColumnType("tinyint(1)");

        builder.Property(x => x.ApprovalDate)
            .HasColumnType("datetime");

        builder.HasIndex(x => new { x.OrderServiceId, x.PartId })
            .IsUnique();

        builder.HasOne(x => x.OrderService)
            .WithMany(x => x.OrderServiceParts)
            .HasForeignKey(x => x.OrderServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Part)
            .WithMany(x => x.OrderServiceParts)
            .HasForeignKey(x => x.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
