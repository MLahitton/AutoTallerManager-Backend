using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("ServiceOrders");

        builder.HasKey(x => x.ServiceOrderId);

        builder.Property(x => x.ServiceOrderId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.EntryDate)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.EstimatedDeliveryDate)
            .HasColumnType("datetime");

        builder.Property(x => x.GeneralDescription)
            .HasColumnType("text");

        builder.Property(x => x.CancellationReason)
            .HasColumnType("text");

        builder.Property(x => x.CancellationDate)
            .HasColumnType("datetime");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.ServiceOrders)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OrderStatus)
            .WithMany(x => x.ServiceOrders)
            .HasForeignKey(x => x.OrderStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
