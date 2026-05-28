using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PartCategoryConfiguration : IEntityTypeConfiguration<PartCategory>
{
    public void Configure(EntityTypeBuilder<PartCategory> builder)
    {
        builder.ToTable("PartCategories");

        builder.HasKey(x => x.PartCategoryId);

        builder.Property(x => x.PartCategoryId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
