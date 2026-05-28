using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StreetTypeConfiguration : IEntityTypeConfiguration<StreetType>
{
    public void Configure(EntityTypeBuilder<StreetType> builder)
    {
        builder.ToTable("StreetTypes");

        builder.HasKey(x => x.StreetTypeId);

        builder.Property(x => x.StreetTypeId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
