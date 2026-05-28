using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.ToTable("Audits");

        builder.HasKey(x => x.AuditId);

        builder.Property(x => x.AuditId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.AffectedEntity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.User)
            .WithMany(x => x.Audits)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AuditActionType)
            .WithMany(x => x.Audits)
            .HasForeignKey(x => x.AuditActionTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
