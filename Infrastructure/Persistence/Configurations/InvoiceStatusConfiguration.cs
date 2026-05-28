using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class InvoiceStatusConfiguration : IEntityTypeConfiguration<InvoiceStatus>
{
    public void Configure(EntityTypeBuilder<InvoiceStatus> builder)
    {
        builder.ToTable("InvoiceStatuses");

        builder.HasKey(x => x.InvoiceStatusId);

        builder.Property(x => x.InvoiceStatusId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
