using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentCardConfiguration : IEntityTypeConfiguration<PaymentCard>
{
    public void Configure(EntityTypeBuilder<PaymentCard> builder)
    {
        builder.ToTable("PaymentCards");

        builder.HasKey(x => x.PaymentCardId);

        builder.Property(x => x.PaymentCardId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LastFourDigits)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(x => x.CardHolder)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AuthorizationCode)
            .HasMaxLength(100);

        builder.HasIndex(x => x.PaymentId)
            .IsUnique();

        builder.HasOne(x => x.Payment)
            .WithOne(x => x.PaymentCard)
            .HasForeignKey<PaymentCard>(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CardType)
            .WithMany(x => x.PaymentCards)
            .HasForeignKey(x => x.CardTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
