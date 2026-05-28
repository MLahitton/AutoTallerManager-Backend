using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.RefreshToken)
            .HasColumnType("text");

        builder.Property(x => x.RefreshTokenExpiration)
            .HasColumnType("datetime");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(x => x.PersonId)
            .IsUnique();

        builder.HasOne(x => x.Person)
            .WithOne(x => x.User)
            .HasForeignKey<User>(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
