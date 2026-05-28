using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PersonPhoneConfiguration : IEntityTypeConfiguration<PersonPhone>
{
    public void Configure(EntityTypeBuilder<PersonPhone> builder)
    {
        builder.ToTable("PersonPhones");

        builder.HasKey(x => x.PersonPhoneId);

        builder.Property(x => x.PersonPhoneId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.IsPrimary)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.CountryId, x.PhoneNumber })
            .IsUnique();

        builder.HasOne(x => x.Person)
            .WithMany(x => x.PersonPhones)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.PersonPhones)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
