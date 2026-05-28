using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.PaymentId);

        builder.Property(x => x.PaymentId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.PaymentDate)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.Reference)
            .HasMaxLength(100);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentMethod)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentStatus)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.PaymentStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
