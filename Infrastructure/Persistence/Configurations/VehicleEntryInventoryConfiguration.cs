using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VehicleEntryInventoryConfiguration : IEntityTypeConfiguration<VehicleEntryInventory>
{
    public void Configure(EntityTypeBuilder<VehicleEntryInventory> builder)
    {
        builder.ToTable("VehicleEntryInventory");

        builder.HasKey(x => x.EntryInventoryId);

        builder.Property(x => x.EntryInventoryId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.HasScratches)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(false);

        builder.Property(x => x.ScratchesDescription)
            .HasColumnType("text");

        builder.Property(x => x.HasToolbox)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(false);

        builder.Property(x => x.ToolboxDescription)
            .HasColumnType("text");

        builder.Property(x => x.OwnershipCardDelivered)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(false);

        builder.Property(x => x.Observations)
            .HasColumnType("text");

        builder.Property(x => x.RegisteredAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.ServiceOrderId)
            .IsUnique();

        builder.HasOne(x => x.ServiceOrder)
            .WithOne(x => x.VehicleEntryInventory)
            .HasForeignKey<VehicleEntryInventory>(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
