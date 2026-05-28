using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MechanicSpecialtyConfiguration : IEntityTypeConfiguration<MechanicSpecialty>
{
    public void Configure(EntityTypeBuilder<MechanicSpecialty> builder)
    {
        builder.ToTable("MechanicSpecialties");

        builder.HasKey(x => x.SpecialtyId);

        builder.Property(x => x.SpecialtyId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
