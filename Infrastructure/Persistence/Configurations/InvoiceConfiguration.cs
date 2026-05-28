using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.InvoiceId);

        builder.Property(x => x.InvoiceId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.InvoiceDate)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.Subtotal)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.Tax)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.Total)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.Observations)
            .HasColumnType("text");

        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();

        builder.HasIndex(x => x.ServiceOrderId)
            .IsUnique();

        builder.HasOne(x => x.ServiceOrder)
            .WithOne(x => x.Invoice)
            .HasForeignKey<Invoice>(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InvoiceStatus)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.InvoiceStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
