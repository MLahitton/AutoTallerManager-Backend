using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NeighborhoodConfiguration : IEntityTypeConfiguration<Neighborhood>
{
    public void Configure(EntityTypeBuilder<Neighborhood> builder)
    {
        builder.ToTable("Neighborhoods");

        builder.HasKey(x => x.NeighborhoodId);

        builder.Property(x => x.NeighborhoodId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.City)
            .WithMany(x => x.Neighborhoods)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
