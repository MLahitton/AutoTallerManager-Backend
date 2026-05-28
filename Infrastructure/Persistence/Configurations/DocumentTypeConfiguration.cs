using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("DocumentTypes");

        builder.HasKey(x => x.DocumentTypeId);

        builder.Property(x => x.DocumentTypeId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
