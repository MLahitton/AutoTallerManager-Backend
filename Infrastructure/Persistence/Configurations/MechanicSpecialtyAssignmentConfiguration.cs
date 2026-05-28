using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MechanicSpecialtyAssignmentConfiguration : IEntityTypeConfiguration<MechanicSpecialtyAssignment>
{
    public void Configure(EntityTypeBuilder<MechanicSpecialtyAssignment> builder)
    {
        builder.ToTable("MechanicSpecialtyAssignments");

        builder.HasKey(x => x.AssignmentId);

        builder.Property(x => x.AssignmentId)
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.PersonId, x.SpecialtyId })
            .IsUnique();

        builder.HasOne(x => x.Person)
            .WithMany(x => x.MechanicSpecialtyAssignments)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Specialty)
            .WithMany(x => x.MechanicSpecialtyAssignments)
            .HasForeignKey(x => x.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
