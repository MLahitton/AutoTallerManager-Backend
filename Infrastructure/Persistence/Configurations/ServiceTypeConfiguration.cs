using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
{
    public void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        builder.ToTable("ServiceTypes");

        builder.HasKey(x => x.ServiceTypeId);

        builder.Property(x => x.ServiceTypeId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EstimatedDays)
            .IsRequired()
            .HasDefaultValue(1);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
