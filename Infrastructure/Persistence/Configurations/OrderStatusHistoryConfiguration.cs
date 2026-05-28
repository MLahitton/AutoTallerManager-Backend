using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory");

        builder.HasKey(x => x.OrderStatusHistoryId);

        builder.Property(x => x.OrderStatusHistoryId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Observation)
            .HasColumnType("text");

        builder.Property(x => x.ChangedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.ServiceOrder)
            .WithMany(x => x.OrderStatusHistories)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PreviousOrderStatus)
            .WithMany(x => x.PreviousOrderStatusHistories)
            .HasForeignKey(x => x.PreviousOrderStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NewOrderStatus)
            .WithMany(x => x.NewOrderStatusHistories)
            .HasForeignKey(x => x.NewOrderStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ChangedByUser)
            .WithMany(x => x.OrderStatusHistories)
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
