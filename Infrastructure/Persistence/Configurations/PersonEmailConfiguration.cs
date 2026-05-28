using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PersonEmailConfiguration : IEntityTypeConfiguration<PersonEmail>
{
    public void Configure(EntityTypeBuilder<PersonEmail> builder)
    {
        builder.ToTable("PersonEmails");

        builder.HasKey(x => x.PersonEmailId);

        builder.Property(x => x.PersonEmailId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.EmailUser)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsPrimary)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.EmailUser, x.EmailDomainId })
            .IsUnique();

        builder.HasOne(x => x.Person)
            .WithMany(x => x.PersonEmails)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EmailDomain)
            .WithMany(x => x.PersonEmails)
            .HasForeignKey(x => x.EmailDomainId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
