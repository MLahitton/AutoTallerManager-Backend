using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuditActionTypeConfiguration : IEntityTypeConfiguration<AuditActionType>
{
    public void Configure(EntityTypeBuilder<AuditActionType> builder)
    {
        builder.ToTable("AuditActionTypes");

        builder.HasKey(x => x.AuditActionTypeId);

        builder.Property(x => x.AuditActionTypeId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
