using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
    {
        builder.ToTable("InvoiceDetails");

        builder.HasKey(x => x.InvoiceDetailId);

        builder.Property(x => x.InvoiceDetailId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Concept)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.Subtotal)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.LineType)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.InvoiceDetails)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourcePart)
            .WithMany(x => x.InvoiceDetails)
            .HasForeignKey(x => x.SourcePartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
