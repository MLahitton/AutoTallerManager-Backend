using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(x => x.AddressId);

        builder.Property(x => x.AddressId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.MainNumber)
            .HasMaxLength(10);

        builder.Property(x => x.SecondaryNumber)
            .HasMaxLength(10);

        builder.Property(x => x.TertiaryNumber)
            .HasMaxLength(10);

        builder.Property(x => x.Complement)
            .HasMaxLength(150);

        builder.HasOne(x => x.Neighborhood)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.NeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StreetType)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.StreetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
