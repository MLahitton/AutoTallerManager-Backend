using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");

        builder.HasKey(x => x.PersonId);

        builder.Property(x => x.PersonId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DocumentNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.MiddleName)
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SecondLastName)
            .HasMaxLength(50);

        builder.Property(x => x.BirthDate)
            .HasColumnType("date");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.DocumentNumber)
            .IsUnique();

        builder.HasOne(x => x.DocumentType)
            .WithMany(x => x.Persons)
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Gender)
            .WithMany(x => x.Persons)
            .HasForeignKey(x => x.GenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Address)
            .WithMany(x => x.Persons)
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
