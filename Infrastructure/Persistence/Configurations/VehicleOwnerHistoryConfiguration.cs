using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VehicleOwnerHistoryConfiguration : IEntityTypeConfiguration<VehicleOwnerHistory>
{
    public void Configure(EntityTypeBuilder<VehicleOwnerHistory> builder)
    {
        builder.ToTable("VehicleOwnerHistory");

        builder.HasKey(x => x.VehicleOwnerHistoryId);

        builder.Property(x => x.VehicleOwnerHistoryId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.StartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.EndDate)
            .HasColumnType("date");

        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.VehicleOwnerHistories)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Person)
            .WithMany(x => x.VehicleOwnerHistories)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
