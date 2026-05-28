using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EmailDomainConfiguration : IEntityTypeConfiguration<EmailDomain>
{
    public void Configure(EntityTypeBuilder<EmailDomain> builder)
    {
        builder.ToTable("EmailDomains");

        builder.HasKey(x => x.EmailDomainId);

        builder.Property(x => x.EmailDomainId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Domain)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Domain)
            .IsUnique();
    }
}
