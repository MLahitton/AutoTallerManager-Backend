using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MechanicAssignmentConfiguration : IEntityTypeConfiguration<MechanicAssignment>
{
    public void Configure(EntityTypeBuilder<MechanicAssignment> builder)
    {
        builder.ToTable("MechanicAssignments");

        builder.HasKey(x => x.MechanicAssignmentId);

        builder.Property(x => x.MechanicAssignmentId)
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.OrderServiceId, x.MechanicPersonId })
            .IsUnique();

        builder.HasOne(x => x.OrderService)
            .WithMany(x => x.MechanicAssignments)
            .HasForeignKey(x => x.OrderServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MechanicPerson)
            .WithMany(x => x.MechanicAssignments)
            .HasForeignKey(x => x.MechanicPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Specialty)
            .WithMany(x => x.MechanicAssignments)
            .HasForeignKey(x => x.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
