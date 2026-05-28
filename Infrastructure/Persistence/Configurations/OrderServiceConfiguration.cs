using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderServiceConfiguration : IEntityTypeConfiguration<OrderService>
{
    public void Configure(EntityTypeBuilder<OrderService> builder)
    {
        builder.ToTable("OrderServices");

        builder.HasKey(x => x.OrderServiceId);

        builder.Property(x => x.OrderServiceId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Description)
            .HasColumnType("text");

        builder.Property(x => x.WorkPerformed)
            .HasColumnType("text");

        builder.Property(x => x.LaborCost)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.CustomerApproved)
            .HasColumnType("tinyint(1)");

        builder.Property(x => x.ApprovalDate)
            .HasColumnType("datetime");

        builder.HasOne(x => x.ServiceOrder)
            .WithMany(x => x.OrderServices)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServiceType)
            .WithMany(x => x.OrderServices)
            .HasForeignKey(x => x.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
