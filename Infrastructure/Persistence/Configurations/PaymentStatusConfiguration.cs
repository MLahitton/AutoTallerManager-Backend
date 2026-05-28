using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentStatusConfiguration : IEntityTypeConfiguration<PaymentStatus>
{
    public void Configure(EntityTypeBuilder<PaymentStatus> builder)
    {
        builder.ToTable("PaymentStatuses");

        builder.HasKey(x => x.PaymentStatusId);

        builder.Property(x => x.PaymentStatusId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
